﻿using System;
using System.Collections.Generic;
using DSRForge.memory;
using DSRForge.Memory;
using DSRForge.Utilities;

namespace DSRForge.Services
{
    public class PlayerService

    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        
        private readonly IntPtr _codeCave2;

        private readonly int[] _hpOffsets =
            { Offsets.WorldChrMan, (int)Offsets.WorldChrOffsets.PlayerIns, (int)Offsets.PlayerInsOffsets.Health };

        private readonly int[] _maxHpOffsets =
            { Offsets.WorldChrMan, (int)Offsets.WorldChrOffsets.PlayerIns, (int)Offsets.PlayerInsOffsets.MaxHealth };

        private readonly Dictionary<int, int> _lowLevelSoulRequirements = new Dictionary<int, int>
        {
            { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
            { 11, 829 },
        };
        
        public PlayerService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
            _codeCave2 = memoryIo.BaseAddress + CodeCaveOffsets.CodeCave2.Base;
        }

        public int GetSetPlayerStat(Offsets.PlayerGameData statType, int? newValue = null)
        {
            var statPtr = _memoryIo.FollowPointers(new[]
                { Offsets.GameDataMan, (int)Offsets.GameData.PlayerGameData, (int)statType }, false);
    
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
                case Offsets.PlayerGameData.Souls:
                    return HandleSoulEdit(statPtr, newValue.Value, currentValue);
            
                case Offsets.PlayerGameData.Humanity:
                    var validatedHumanity = newValue.Value;
                    if (validatedHumanity < 1) validatedHumanity = 1;
                    if (validatedHumanity > 99) validatedHumanity = 99;
                    _memoryIo.WriteInt32(statPtr, validatedHumanity);
                    return validatedHumanity;
            
                default:
                    var validatedStat = newValue.Value;
                    if (validatedStat < 1) validatedStat = 1;
                    if (validatedStat > 99) validatedStat = 99;
            
                    if (validatedStat != currentValue)
                    {
                        _memoryIo.WriteInt32(statPtr, validatedStat);
                        UpdatePlayerStats(validatedStat - currentValue);
                    }
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
            var totalSoulsPtr = _memoryIo.FollowPointers(new[]
                { Offsets.GameDataMan, (int)Offsets.GameData.PlayerGameData, (int)Offsets.PlayerGameData.TotalSouls }, false);
            int currentTotalSouls = _memoryIo.ReadInt32(totalSoulsPtr);
                
            _memoryIo.WriteInt32(totalSoulsPtr, difference + currentTotalSouls);
            _memoryIo.WriteInt32(statPtr, newValue);
            return newValue;
        }
        
        private void UpdatePlayerStats(int difference)
        {
            var allStatsPtr =
                _memoryIo.FollowPointers(new[] { Offsets.GameDataMan, (int)Offsets.GameData.PlayerGameData }, true);

            int originalSouls = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Souls);

            int currentLevel = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.SoulLevel);
            int newLevel = currentLevel + difference;


            int[] stats = new int[9];
            stats[0] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Vitality);
            stats[1] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Attunement);
            stats[2] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Endurance);
            stats[3] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Strength);
            stats[4] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Dexterity);
            stats[5] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Resistance);
            stats[6] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Intelligence);
            stats[7] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Faith);
            stats[8] = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.Humanity);

            if (CallLevelUpFunction(newLevel, stats))
            {
                if (newLevel < currentLevel)
                {
                    _memoryIo.WriteInt32(allStatsPtr + (int)Offsets.PlayerGameData.Souls, originalSouls);
                    return;
                }
                int totalSoulsRequired = CalculateTotalSoulsRequired(currentLevel, newLevel);
                int currentTotalSouls = _memoryIo.ReadInt32(allStatsPtr + (int)Offsets.PlayerGameData.TotalSouls);
                _memoryIo.WriteInt32(allStatsPtr + (int)Offsets.PlayerGameData.TotalSouls,
                    totalSoulsRequired + currentTotalSouls);
                _memoryIo.WriteInt32(allStatsPtr + (int)Offsets.PlayerGameData.Souls, originalSouls);
            }
        }

        private bool CallLevelUpFunction(int newLevel, int[] stats)
        {
            var statArrayAddress = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.LevelUp.StatsArray;
            var tempStatArray = statArrayAddress;
            for (int i = 0; i < 9; i++)
            {
                _memoryIo.WriteInt32(tempStatArray, stats[i]);
                tempStatArray += 0x4;
            }

            var codeStart = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.LevelUp.CodeBlock;
            var soulsPtr = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.LevelUp.SoulsPtr;

            byte[] codeBytes = AsmLoader.GetAsmBytes("LevelUp");
            byte[] bytes = BitConverter.GetBytes(statArrayAddress.ToInt64());
            Array.Copy(bytes, 0, codeBytes, 2, 8);
            bytes = BitConverter.GetBytes(soulsPtr.ToInt64());
            Array.Copy(bytes, 0, codeBytes, 15, 8);
            _memoryIo.WriteBytes(codeStart, codeBytes);

            var newLevelAddr = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.LevelUp.NewLevel;
            _memoryIo.WriteInt32(newLevelAddr, newLevel);
            var requiredSoulsAddr = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.LevelUp.RequiredSouls;
            _memoryIo.WriteInt32(requiredSoulsAddr, 0);

            var currSoulsAddr = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.LevelUp.CurrentSouls;
            _memoryIo.WriteInt32(currSoulsAddr, 9999999);

            return _memoryIo.RunThreadAndWaitForCompletion(codeStart);
        }

        private int CalculateTotalSoulsRequired(int startLevel, int endLevel)
        {
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
            var hpPtr = _memoryIo.FollowPointers(
                new[]
                {
                    Offsets.WorldChrMan, (int)Offsets.WorldChrOffsets.PlayerIns, (int)Offsets.PlayerInsOffsets.Health
                }, false);
            _memoryIo.WriteInt32(hpPtr, hp);
            GetSetPlayerStat(Offsets.PlayerGameData.Endurance, 99);
        }

        public int? GetHp()
        {
            var hpPtr = _memoryIo.FollowPointers(_hpOffsets, false);

            return _memoryIo.ReadInt32(hpPtr);
        }

        public int? GetMaxHp()
        {
            var hpPtr = _memoryIo.FollowPointers(_maxHpOffsets, false);

            return _memoryIo.ReadInt32(hpPtr);
        }

        public void SavePos(int index)
        {
            var coordsPtr = _memoryIo.FollowPointers(new[]
            {
                Offsets.WorldChrMan,
                (int)Offsets.WorldChrOffsets.PlayerIns,
                (int)Offsets.PlayerInsOffsets.CoordsPtr1,
                Offsets.CoordsPtr2,
                Offsets.CoordsPtr3,
                Offsets.CoordsPtr4,
                (int)Offsets.Coords.X
            }, false);

            byte[] positionBytes = _memoryIo.ReadBytes(coordsPtr, 12);

            if (index == 0)
            {
                _memoryIo.WriteBytes(_codeCave2 + CodeCaveOffsets.CodeCave2.SavePos1, positionBytes);
            }
            else
            {
                _memoryIo.WriteBytes(_codeCave2 + CodeCaveOffsets.CodeCave2.SavePos2, positionBytes);
            }
        }

        public int GetSetNewGame(int? value)
        {
            var newGamePtr = _memoryIo.FollowPointers(new[]
                { Offsets.GameDataMan, (int)Offsets.GameData.Ng }, false);
    
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

        public void RestorePos(int index)
        {
            byte[] positionBytes;
            if (index == 0)
            {
                positionBytes = _memoryIo.ReadBytes(_codeCave2 + CodeCaveOffsets.CodeCave2.SavePos1, 12);
            }
            else
            {
                positionBytes = _memoryIo.ReadBytes(_codeCave2 + CodeCaveOffsets.CodeCave2.SavePos2, 12);
            }

            var coordsPtr = _memoryIo.FollowPointers(new[]
            {
                Offsets.WorldChrMan,
                (int)Offsets.WorldChrOffsets.PlayerIns,
                (int)Offsets.PlayerInsOffsets.CoordsPtr1,
                Offsets.CoordsPtr2,
                Offsets.CoordsPtr3,
                Offsets.CoordsPtr4,
                (int)Offsets.Coords.X
            }, false);

            _memoryIo.WriteBytes(coordsPtr, positionBytes);
        }

        public void RestoreSpellCasts()
        {
            var magicDataPtr = _memoryIo.FollowPointers(
                new[]
                {
                    Offsets.GameDataMan, (int)Offsets.GameData.PlayerGameData,
                    (int)Offsets.PlayerGameData.EquipMagicData
                }, true);
            byte[] restoreBytes = AsmLoader.GetAsmBytes("RestoreSpellCasts");
            byte[] bytes = BitConverter.GetBytes(magicDataPtr.ToInt64());
            Array.Copy(bytes, 0, restoreBytes, 2, 8);
            _memoryIo.WriteBytes(_codeCave2 + CodeCaveOffsets.CodeCave2.RestoreCasts, restoreBytes);

            _memoryIo.RunThread(_codeCave2 + CodeCaveOffsets.CodeCave2.RestoreCasts);
        }

        public void ToggleNoDeath(int value)
        {
            var noDeathPtr = _memoryIo.BaseAddress + Offsets.NoDeath;
            _memoryIo.WriteByte(noDeathPtr, value);
        }


        public void ToggleNoDamage(bool setValue)
        {
            var noDamagePtr = _memoryIo.FollowPointers(
                new[]
                {
                    Offsets.WorldChrMan, (int)Offsets.WorldChrOffsets.PlayerIns, (int)Offsets.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = Offsets.NoDamage;
            _memoryIo.SetBitValue(noDamagePtr, flagMask, setValue);
        }

        public void ToggleInfiniteStamina(bool setValue)
        {
            var infiniteStamPtr = _memoryIo.FollowPointers(
                new[]
                {
                    Offsets.WorldChrMan, (int)Offsets.WorldChrOffsets.PlayerIns,
                    (int)Offsets.PlayerInsOffsets.InfiniteStam
                }, false);

            var flagMask = Offsets.InfiniteStam;
            _memoryIo.SetBitValue(infiniteStamPtr, flagMask, setValue);
        }

        public void ToggleNoGoodsConsume(bool setValue)
        {
            var noGoodsConsumePtr = _memoryIo.FollowPointers(
                new[]
                {
                    Offsets.WorldChrMan, (int)Offsets.WorldChrOffsets.PlayerIns,
                    (int)Offsets.PlayerInsOffsets.NoGoodsConsume
                }, false);
            var flagMask = Offsets.NoGoodsConsume;
            _memoryIo.SetBitValue(noGoodsConsumePtr, flagMask, setValue);
        }

        public void ToggleInfiniteCasts(int value)
        {
            var infiniteCastsPtr = _memoryIo.BaseAddress + Offsets.InfiniteCasts;
            _memoryIo.WriteByte(infiniteCastsPtr, value);
        }

        public void ToggleOneShot(int value)
        {
            var oneShotPtr = _memoryIo.BaseAddress + Offsets.OneShot;
            _memoryIo.WriteByte(oneShotPtr, value);
        }

        public void ToggleInvisible(int value)
        {
            var invisiblePtr = _memoryIo.BaseAddress + Offsets.Invisible;
            _memoryIo.WriteByte(invisiblePtr, value);
        }

        public void ToggleSilent(int value)
        {
            var silentPtr = _memoryIo.BaseAddress + Offsets.Silent;
            _memoryIo.WriteByte(silentPtr, value);
        }

        public void ToggleNoAmmoConsume(int value)
        {
            var noAmmoConsumePtr = _memoryIo.BaseAddress + Offsets.NoAmmoConsume;
            _memoryIo.WriteByte(noAmmoConsumePtr, value);
        }

        public float GetSetPlayerSpeed(float? value)
        {
            var playerSpeedPtr = _memoryIo.FollowPointers(
                new[]
                {
                    Offsets.WorldChrMan, (int)Offsets.WorldChrOffsets.PlayerIns,
                    (int)Offsets.PlayerInsOffsets.PlayerCtrl, Offsets.PlayerAnim, Offsets.PlayerAnimSpeed
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
            var noDeathPtr = _memoryIo.BaseAddress + Offsets.NoDeath;
            return  _memoryIo.ReadBytes(noDeathPtr, 1)[0] == 1;
        }
    }
}