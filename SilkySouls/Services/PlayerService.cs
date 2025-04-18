﻿using System;
using System.Collections.Generic;
using System.Threading;
using SilkySouls.memory;
using SilkySouls.Memory;
using SilkySouls.Utilities;

namespace SilkySouls.Services
{
    public class PlayerService

    {
        private readonly MemoryIo _memoryIo;
        
        private readonly int[] _hpOffsets =
            { (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns, (int)Offsets.WorldChrMan.PlayerInsOffsets.Health };

        private readonly int[] _maxHpOffsets =
            { (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns, (int)Offsets.WorldChrMan.PlayerInsOffsets.MaxHealth };

        private readonly Dictionary<int, int> _lowLevelSoulRequirements = new Dictionary<int, int>
        {
            { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
            { 11, 829 },
        };

        public PlayerService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }

        public int? GetSoulLevel()
        {
            var soulLevelPtr = _memoryIo.FollowPointers(Offsets.GameDataMan.Base, new[]
                {(int)Offsets.GameDataMan.GameDataOffsets.PlayerGameData, (int)Offsets.GameDataMan.PlayerGameData.SoulLevel }, false);
            return _memoryIo.ReadInt32(soulLevelPtr);
        }

        public int GetSetPlayerStat(Offsets.GameDataMan.PlayerGameData statType, int? newValue = null)
        {
            var statPtr = _memoryIo.FollowPointers(Offsets.GameDataMan.Base, new[]
                {(int)Offsets.GameDataMan.GameDataOffsets.PlayerGameData, (int)statType }, false);

            
            int currentValue = _memoryIo.ReadInt32(statPtr);
            

            if (!newValue.HasValue)
            {
                return currentValue;
            }

            if (currentValue == newValue.Value)
            {
                return currentValue;
            }
            
            switch (statType)
            {
                case Offsets.GameDataMan.PlayerGameData.Souls:
                    return HandleSoulEdit(statPtr, newValue.Value, currentValue);
                case  Offsets.GameDataMan.PlayerGameData.Humanity:
                    var validatedHumanity = newValue.Value;
                    if (validatedHumanity < 1) validatedHumanity = 1;
                    if (validatedHumanity > 99) validatedHumanity = 99;
                    _memoryIo.WriteInt32(statPtr, validatedHumanity);
                    return validatedHumanity;

                default:
                    var validatedStat = newValue.Value;
                    if (validatedStat < 1) validatedStat = 1;
                    if (validatedStat > 99) validatedStat = 99;

                    bool wasNoDeathEnabled = IsNoDeathOn();
                    ToggleNoDeath(1);
                    if (validatedStat != currentValue)
                    {
                        _memoryIo.WriteInt32(statPtr, validatedStat);
                        UpdatePlayerStats(validatedStat - currentValue);
                    }
                    ToggleNoDeath(wasNoDeathEnabled ? 1 : 0);
                    return validatedStat;
            }
        }

        private int HandleSoulEdit(IntPtr statPtr, int newValue, int oldValue)
        {
            if (newValue < oldValue)
            {
                _memoryIo.WriteInt32(statPtr, newValue);
                return newValue;
            }

            int difference = newValue - oldValue;
            var totalSoulsPtr = _memoryIo.FollowPointers(Offsets.GameDataMan.Base,new[]
                {
                   (int)Offsets.GameDataMan.GameDataOffsets.PlayerGameData, (int) Offsets.GameDataMan.PlayerGameData.TotalSouls
                },
                false);
            int currentTotalSouls = _memoryIo.ReadInt32(totalSoulsPtr);

            _memoryIo.WriteInt32(totalSoulsPtr, difference + currentTotalSouls);
            _memoryIo.WriteInt32(statPtr, newValue);
            return newValue;
        }

        private void UpdatePlayerStats(int difference)
        {
            var allStatsPtr =
                _memoryIo.FollowPointers(Offsets.GameDataMan.Base, new[] {(int)Offsets.GameDataMan.GameDataOffsets.PlayerGameData }, true);

            int originalSouls = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Souls);

            int currentLevel = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.SoulLevel);
            int newLevel = currentLevel + difference;


            int[] stats = new int[9];
            stats[0] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Vitality);
            stats[1] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Attunement);
            stats[2] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Endurance);
            stats[3] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Strength);
            stats[4] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Dexterity);
            stats[5] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Resistance);
            stats[6] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Intelligence);
            stats[7] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Faith);
            stats[8] = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Humanity);

            if (CallLevelUpFunction(newLevel, stats))
            {
                if (newLevel < currentLevel)
                {
                    _memoryIo.WriteInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Souls, originalSouls);
                    return;
                }
            
                int totalSoulsRequired = CalculateTotalSoulsRequired(currentLevel, newLevel);
                int currentTotalSouls = _memoryIo.ReadInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.TotalSouls);
                _memoryIo.WriteInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.TotalSouls,
                    totalSoulsRequired + currentTotalSouls);
                _memoryIo.WriteInt32(allStatsPtr + (int) Offsets.GameDataMan.PlayerGameData.Souls, originalSouls);
            }
        }

        private bool CallLevelUpFunction(int newLevel, int[] stats)
        {
            var statArrayAddress = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.StatsArray;
            var tempStatArray = statArrayAddress;
            for (int i = 0; i < 9; i++)
            {
                _memoryIo.WriteInt32(tempStatArray, stats[i]);
                tempStatArray += 0x4;
            }
        
            var codeStart = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.CodeBlock;
            var soulsPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.SoulsPtr;
        
            byte[] codeBytes = AsmLoader.GetAsmBytes("LevelUp");
            byte[] bytes = BitConverter.GetBytes(statArrayAddress.ToInt64());
            Array.Copy(bytes, 0, codeBytes, 2, 8);
            bytes = BitConverter.GetBytes(soulsPtr.ToInt64());
            Array.Copy(bytes, 0, codeBytes, 15, 8);
            bytes = BitConverter.GetBytes(Offsets.LevelUpFunc);
            Array.Copy(bytes, 0, codeBytes, 32, 8);
            _memoryIo.WriteBytes(codeStart, codeBytes);
        
            var newLevelAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.NewLevel;
            _memoryIo.WriteInt32(newLevelAddr, newLevel);
            var requiredSoulsAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.RequiredSouls;
            _memoryIo.WriteInt32(requiredSoulsAddr, 0);
        
            var currSoulsAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.CurrentSouls;
            _memoryIo.WriteInt32(currSoulsAddr, 9999999);
        
            return _memoryIo.RunThreadAndWaitForCompletion(codeStart);
        }

        private int CalculateTotalSoulsRequired(int startLevel, int endLevel)
        {
            
            startLevel = Math.Max(1, startLevel);
            double totalSouls = 0;
            for (int level = startLevel + 1; level <= endLevel; level++)
            {
                if (level <= 11)
                {
                    totalSouls += _lowLevelSoulRequirements[level];
                }
                else
                {
                    double x = level;
                    double levelCost = 0.02 * Math.Pow(x, 3) + 3.06 * Math.Pow(x, 2) + 105.6 * x - 895;
                    totalSouls += Math.Round(levelCost);
                }
            }

            return (int)totalSouls;
        }

        public void SetHp(int hp)
        {
            var hpPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns, (int)Offsets.WorldChrMan.PlayerInsOffsets.Health
                }, false);
            _memoryIo.WriteInt32(hpPtr, hp);
        }

        public int? GetHp()
        {
            var hpPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base, _hpOffsets, false);

            return _memoryIo.ReadInt32(hpPtr);
        }

        public int? GetMaxHp()
        {
            var hpPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base, _maxHpOffsets, false);

            return _memoryIo.ReadInt32(hpPtr);
        }

        public void SavePos(int index)
        {
           
            var coordsPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base, new[]
            {
                (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                (int)Offsets.WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                Offsets.WorldChrMan.CoordsPtr2,
                Offsets.WorldChrMan.CoordsPtr3,
                Offsets.WorldChrMan.CoordsPtr4,
                (int)Offsets.WorldChrMan.Coords.X
            }, false);

            byte[] positionBytes = _memoryIo.ReadBytes(coordsPtr, 12);

            if (index == 0)
            {
                _memoryIo.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, positionBytes);
            }
            else
            {
                _memoryIo.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, positionBytes);
            }
        }

        private byte[] _lastKnownCoordsBytes;
        public void RestorePos(int index)
        {
            var coordsUpdate = (IntPtr) Offsets.Hooks.UpdateCoords;
            byte[] originBytes = _memoryIo.ReadBytes(coordsUpdate, 7);
            bool allNops = true;
            for (int i = 0; i < originBytes.Length; i++)
            {
                if (originBytes[i] != 0x90)
                {
                    allNops = false;
                    break;
                }
            }
            if (allNops)
            {
                originBytes = _lastKnownCoordsBytes;
            }

            _lastKnownCoordsBytes = originBytes;
            _memoryIo.WriteBytes(coordsUpdate, new byte[]{ 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            _memoryIo.WriteBytes(coordsUpdate + 0x252, new byte[]{ 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            
            byte[] positionBytes;
            if (index == 0)
            {
                positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, 12);
            }
            else
            {
                positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, 12);
            }

            var coordsPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base, new[]
            {
                (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                (int)Offsets.WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                Offsets.WorldChrMan.CoordsPtr2,
                Offsets.WorldChrMan.CoordsPtr3,
                Offsets.WorldChrMan.CoordsPtr4,
                (int)Offsets.WorldChrMan.Coords.X
            }, false);

            _memoryIo.WriteBytes(coordsPtr, positionBytes);
            Thread.Sleep(15);
            _memoryIo.WriteBytes(coordsUpdate, originBytes);
            _memoryIo.WriteBytes(coordsUpdate + 0x252, new byte[]{ 0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00 });
        }

        public int GetSetNewGame(int? value)
        {
            var newGamePtr = _memoryIo.FollowPointers(Offsets.GameDataMan.Base,new[]
                { (int)Offsets.GameDataMan.GameDataOffsets.Ng }, false);

            int currentValue = _memoryIo.ReadInt32(newGamePtr);

            if (!value.HasValue)
            {
                return currentValue;
            }

            if (currentValue == value.Value)
            {
                return currentValue;
            }

            _memoryIo.WriteInt32(newGamePtr, value.Value);
            return value.Value;
        }

        public void RestoreSpellCasts()
        {
            var magicDataPtr = _memoryIo.FollowPointers(Offsets.GameDataMan.Base,
                new[]
                {
                    (int)Offsets.GameDataMan.GameDataOffsets.PlayerGameData,
                    (int)Offsets.GameDataMan.PlayerGameData.EquipMagicData
                }, true);
            byte[] restoreBytes = AsmLoader.GetAsmBytes("RestoreSpellCasts");
            byte[] bytes = BitConverter.GetBytes(magicDataPtr.ToInt64());
            Array.Copy(bytes, 0, restoreBytes, 2, 8);
            bytes = BitConverter.GetBytes(Offsets.RestoreCastsFunc);
            Array.Copy(bytes, 0, restoreBytes, 16, 8);
            _memoryIo.AllocateAndExecute(restoreBytes);
        }

        public void ToggleNoDeath(int value)
        {
            var noDeathPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.NoDeath;
            _memoryIo.WriteByte(noDeathPtr, value);
        }


        public void ToggleNoDamage(bool setValue)
        {
            var noDamagePtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns, (int)Offsets.WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = Offsets.WorldChrMan.NoDamage;
            _memoryIo.SetBitValue(noDamagePtr, flagMask, setValue);
        }

        public void ToggleInfiniteStamina(bool setValue)
        {
            var infiniteStamPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);

            var flagMask = (byte) Offsets.WorldChrMan.ChrFlags.InfiniteStam;
            _memoryIo.SetBitValue(infiniteStamPtr, flagMask, setValue);
        }

        public void ToggleNoGoodsConsume(bool setValue)
        {
            var noGoodsConsumePtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.NoGoodsConsume
                }, false);
            var flagMask = Offsets.WorldChrMan.NoGoodsConsume;
            _memoryIo.SetBitValue(noGoodsConsumePtr, flagMask, setValue);
        }

        public void ToggleInfiniteCasts(int value)
        {
            var infiniteCastsPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.InfiniteCasts;
            _memoryIo.WriteByte(infiniteCastsPtr, value);
        }

        public void ToggleOneShot(int value)
        {
            var oneShotPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.OneShot;
            _memoryIo.WriteByte(oneShotPtr, value);
        }

        public void ToggleInvisible(int value)
        {
            var invisiblePtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.Invisible;
            _memoryIo.WriteByte(invisiblePtr, value);
        }

        public void ToggleSilent(int value)
        {
            var silentPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.Silent;
            _memoryIo.WriteByte(silentPtr, value);
        }

        public void ToggleNoAmmoConsume(int value)
        {
            var noAmmoConsumePtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.NoAmmoConsume;
            _memoryIo.WriteByte(noAmmoConsumePtr, value);
        }

        public void ToggleInfinitePoise(bool setValue)
        {
            var infinitePoisePtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.InfinitePoise
                }, false);
            var flagMask = Offsets.WorldChrMan.InfinitePoise;
            _memoryIo.SetBitValue(infinitePoisePtr, flagMask, setValue) ;
        }

        public float GetSetPlayerSpeed(float? value)
        {
            var playerSpeedPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.PlayerCtrl, Offsets.WorldChrMan.ChrAnim, Offsets.WorldChrMan.ChrAnimSpeed
                }, false);

            float currentSpeed = _memoryIo.ReadFloat(playerSpeedPtr);

            if (!value.HasValue)
            {
                return currentSpeed;
            }

            _memoryIo.WriteFloat(playerSpeedPtr, value.Value);
            return value.Value;
        }

        public bool IsNoDeathOn()
        {
            var noDeathPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.NoDeath;
            return _memoryIo.ReadBytes(noDeathPtr, 1)[0] == 1;
        }

        public void ToggleInfiniteDurability(bool isInfiniteDurabilityEnabled)
        {
            if(isInfiniteDurabilityEnabled) _memoryIo.WriteByte(Offsets.Patches.InfiniteDurabilityPatch + 0x1, 0x89);
            else _memoryIo.WriteByte(Offsets.Patches.InfiniteDurabilityPatch + 0x1, 0x88);
        }
    }
}