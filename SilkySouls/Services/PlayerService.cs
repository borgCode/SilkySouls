using System;
using System.Collections.Generic;
using System.Threading;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class PlayerService

    {
        private readonly MemoryIo _memoryIo;

        private readonly Dictionary<int, int> _lowLevelSoulRequirements = new Dictionary<int, int>
        {
            { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
            { 11, 829 },
        };

        public PlayerService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }


        public int GetPlayerStat(GameDataMan.PlayerGameData stat)
        {
            var statsBasePtr = (IntPtr)_memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) +
                                                           (int)GameDataMan.GameDataOffsets.PlayerGameData);
            return _memoryIo.ReadInt32(statsBasePtr + (int)stat);
        }

        public void SetPlayerStat(GameDataMan.PlayerGameData statType, int newValue)
        {
            var statPtr = _memoryIo.FollowPointers(GameDataMan.Base, new[]
                { (int)GameDataMan.GameDataOffsets.PlayerGameData, (int)statType }, false);

            int currentValue = _memoryIo.ReadInt32(statPtr);
            if (currentValue == newValue) return;

            switch (statType)
            {
                case GameDataMan.PlayerGameData.Souls:
                    HandleSoulEdit(statPtr, newValue, currentValue);
                    return;
                case GameDataMan.PlayerGameData.Humanity:
                    var validatedHumanity = newValue;
                    if (validatedHumanity < 1) validatedHumanity = 1;
                    if (validatedHumanity > 99) validatedHumanity = 99;
                    _memoryIo.WriteInt32(statPtr, validatedHumanity);
                    return;

                default:
                    var validatedStat = newValue;
                    if (validatedStat < 1) validatedStat = 1;
                    if (validatedStat > 99) validatedStat = 99;
                    if (validatedStat != currentValue)
                    {
                        _memoryIo.WriteInt32(statPtr, validatedStat);
                        UpdatePlayerStats(validatedStat - currentValue);
                    }

                    return;
            }
        }

        private void HandleSoulEdit(IntPtr statPtr, int newValue, int oldValue)
        {
            if (newValue < oldValue)
            {
                _memoryIo.WriteInt32(statPtr, newValue);
                return;
            }

            int difference = newValue - oldValue;
            var totalSoulsPtr = _memoryIo.FollowPointers(GameDataMan.Base, new[]
                {
                    (int)GameDataMan.GameDataOffsets.PlayerGameData,
                    (int)GameDataMan.PlayerGameData.TotalSouls
                },
                false);
            int currentTotalSouls = _memoryIo.ReadInt32(totalSoulsPtr);

            _memoryIo.WriteInt32(totalSoulsPtr, difference + currentTotalSouls);
            _memoryIo.WriteInt32(statPtr, newValue);
        }

        private void UpdatePlayerStats(int difference)
        {
            var allStatsPtr =
                _memoryIo.FollowPointers(GameDataMan.Base,
                    new[] { (int)GameDataMan.GameDataOffsets.PlayerGameData }, true);

            int originalSouls = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls);

            int currentLevel = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.SoulLevel);
            int newLevel = currentLevel + difference;


            int[] stats = new int[9];
            stats[0] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Vitality);
            stats[1] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Attunement);
            stats[2] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Endurance);
            stats[3] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Strength);
            stats[4] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Dexterity);
            stats[5] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Resistance);
            stats[6] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Intelligence);
            stats[7] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Faith);
            stats[8] = _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Humanity);

            if (CallLevelUpFunction(newLevel, stats))
            {
                if (newLevel < currentLevel)
                {
                    _memoryIo.WriteInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls, originalSouls);
                    return;
                }

                int totalSoulsRequired = CalculateTotalSoulsRequired(currentLevel, newLevel);
                int currentTotalSouls =
                    _memoryIo.ReadInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.TotalSouls);
                _memoryIo.WriteInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.TotalSouls,
                    totalSoulsRequired + currentTotalSouls);
                _memoryIo.WriteInt32(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls, originalSouls);
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
            bytes = BitConverter.GetBytes(LevelUpFunc);
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

        public void SetHp(int hp) =>
            _memoryIo.WriteInt32(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Health), hp);


        public int GetHp() =>
            _memoryIo.ReadInt32(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Health));

        public int GetMaxHp() =>
            _memoryIo.ReadInt32(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.MaxHealth));

        public IntPtr GetPlayerInsPointer(int finalOffset)
        {
            var ptr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    finalOffset
                }, false);

            return ptr;
        }

        public void SavePos(int index)
        {
            var coordsPtr = GetPlayerCoordinatePtr(WorldChrMan.Coords.X);

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

        public IntPtr GetPlayerCoordinatePtr(WorldChrMan.Coords coordinateType)
        {
            return _memoryIo.FollowPointers(WorldChrMan.Base, new[]
            {
                (int)WorldChrMan.BaseOffsets.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                WorldChrMan.CoordsPtr2,
                WorldChrMan.CoordsPtr3,
                WorldChrMan.CoordsPtr4,
                (int)coordinateType
            }, false);
        }

        private byte[] _lastKnownCoordsBytes;

        public void RestorePos(int index)
        {
            var coordsUpdate = (IntPtr)Hooks.UpdateCoords;
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
            _memoryIo.WriteBytes(coordsUpdate, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            _memoryIo.WriteBytes(coordsUpdate + 0x252, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

            byte[] positionBytes;
            if (index == 0)
            {
                positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, 12);
            }
            else
            {
                positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, 12);
            }

            var coordsPtr = GetPlayerCoordinatePtr(WorldChrMan.Coords.X);

            _memoryIo.WriteBytes(coordsPtr, positionBytes);
            Thread.Sleep(15);
            _memoryIo.WriteBytes(coordsUpdate, originBytes);
            _memoryIo.WriteBytes(coordsUpdate + 0x252, new byte[] { 0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00 });
        }

        public void RestoreSpellCasts()
        {
            var magicDataPtr = _memoryIo.FollowPointers(GameDataMan.Base,
                new[]
                {
                    (int)GameDataMan.GameDataOffsets.PlayerGameData,
                    (int)GameDataMan.PlayerGameData.EquipMagicData
                }, true);
            byte[] restoreBytes = AsmLoader.GetAsmBytes("RestoreSpellCasts");
            byte[] bytes = BitConverter.GetBytes(magicDataPtr.ToInt64());
            Array.Copy(bytes, 0, restoreBytes, 2, 8);
            bytes = BitConverter.GetBytes(RestoreCastsFunc);
            Array.Copy(bytes, 0, restoreBytes, 16, 8);
            _memoryIo.AllocateAndExecute(restoreBytes);
        }

        public void ToggleNoDeath(int value)
        {
            var noDeathPtr = DebugFlags.Base + DebugFlags.NoDeath;
            _memoryIo.WriteByte(noDeathPtr, value);
        }


        public void ToggleNoDamage(bool setValue)
        {
            var noDamagePtr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns, (int)WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = WorldChrMan.NoDamage;
            _memoryIo.SetBitValue(noDamagePtr, flagMask, setValue);
        }

        public void ToggleInfiniteStamina(bool setValue)
        {
            var infiniteStamPtr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);

            var flagMask = (byte)WorldChrMan.ChrFlags.InfiniteStam;
            _memoryIo.SetBitValue(infiniteStamPtr, flagMask, setValue);
        }

        public void ToggleNoGoodsConsume(bool setValue)
        {
            var noGoodsConsumePtr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.NoGoodsConsume
                }, false);
            var flagMask = WorldChrMan.NoGoodsConsume;
            _memoryIo.SetBitValue(noGoodsConsumePtr, flagMask, setValue);
        }

        public void ToggleInfiniteCasts(int value)
        {
            var infiniteCastsPtr = DebugFlags.Base + DebugFlags.InfiniteCasts;
            _memoryIo.WriteByte(infiniteCastsPtr, value);
        }

        public void ToggleOneShot(int value)
        {
            var oneShotPtr = DebugFlags.Base + DebugFlags.OneShot;
            _memoryIo.WriteByte(oneShotPtr, value);
        }

        public void ToggleInvisible(int value)
        {
            var invisiblePtr = DebugFlags.Base + DebugFlags.Invisible;
            _memoryIo.WriteByte(invisiblePtr, value);
        }

        public void ToggleSilent(int value)
        {
            var silentPtr = DebugFlags.Base + DebugFlags.Silent;
            _memoryIo.WriteByte(silentPtr, value);
        }

        public void ToggleNoAmmoConsume(int value)
        {
            var noAmmoConsumePtr = DebugFlags.Base + DebugFlags.NoAmmoConsume;
            _memoryIo.WriteByte(noAmmoConsumePtr, value);
        }

        public void ToggleInfinitePoise(bool setValue)
        {
            var infinitePoisePtr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.InfinitePoise
                }, false);
            var flagMask = WorldChrMan.InfinitePoise;
            _memoryIo.SetBitValue(infinitePoisePtr, flagMask, setValue);
        }

        public bool IsNoDeathOn()
        {
            var noDeathPtr = DebugFlags.Base + DebugFlags.NoDeath;
            return _memoryIo.ReadBytes(noDeathPtr, 1)[0] == 1;
        }

        public void ToggleInfiniteDurability(bool isInfiniteDurabilityEnabled)
        {
            if (isInfiniteDurabilityEnabled) _memoryIo.WriteByte(Patches.InfiniteDurabilityPatch + 0x1, 0x89);
            else _memoryIo.WriteByte(Patches.InfiniteDurabilityPatch + 0x1, 0x88);
        }

        public int GetSp() =>
            _memoryIo.ReadInt32(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Stamina));

        public void SetSp(int sp) =>
            _memoryIo.WriteInt32(GetPlayerInsPointer((int)WorldChrMan.PlayerInsOffsets.Stamina), sp);

        public void ToggleNoRoll(bool isNoRollEnabled)
        {
            var noRollPatchPtr = Patches.NoRollPatch;
            var noBackstepPatchPtr = noRollPatchPtr + 0xFF;
            if (isNoRollEnabled)
            {
                _memoryIo.WriteByte(noRollPatchPtr + 0x6, 0);
                _memoryIo.WriteByte(noRollPatchPtr + 0xD, 0);
                _memoryIo.WriteByte(noBackstepPatchPtr + 0x6, 0);
                _memoryIo.WriteByte(noBackstepPatchPtr + 0xD, 0);
            }
            else
            {
                _memoryIo.WriteByte(noRollPatchPtr + 0x6, 1);
                _memoryIo.WriteByte(noRollPatchPtr + 0xD, 1);
                _memoryIo.WriteByte(noBackstepPatchPtr + 0x6, 1);
                _memoryIo.WriteByte(noBackstepPatchPtr + 0xD, 1);
            }
        }

        public int GetNewGame() =>
            _memoryIo.ReadInt32((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) + (int)GameDataMan.GameDataOffsets.Ng);

        public void SetNewGame(int value) =>
            _memoryIo.WriteInt32((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) + (int)GameDataMan.GameDataOffsets.Ng,
                value);

        public void GiveSouls()
        {
            var soulsPtr = _memoryIo.FollowPointers(GameDataMan.Base, new[]
                {
                    (int)GameDataMan.GameDataOffsets.PlayerGameData,
                    (int)GameDataMan.PlayerGameData.Souls
                },
                false);
            int currentVal = _memoryIo.ReadInt32(soulsPtr);
            HandleSoulEdit(soulsPtr, currentVal + 10000, currentVal);
        }

        public float GetPlayerSpeed() => _memoryIo.ReadFloat(GetPlayerSpeedPtr());

        public void SetPlayerSpeed(float speed) => _memoryIo.WriteFloat(GetPlayerSpeedPtr(), speed);

        private IntPtr GetPlayerSpeedPtr()
        {
            return _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.PlayerCtrl, WorldChrMan.ChrAnim,
                    WorldChrMan.ChrAnimSpeed
                }, false);
        }

        public (float x, float y, float z) GetReadOnlyCoords()
        {
            var playerInsPtr = (IntPtr)_memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(WorldChrMan.Base) +
                                                           (int)WorldChrMan.BaseOffsets.PlayerIns);

            var coordBytes = _memoryIo.ReadBytes(playerInsPtr + (int)WorldChrMan.PlayerInsOffsets.ReadOnlyCoords, 12);
            float x = BitConverter.ToSingle(coordBytes, 0);
            float z = BitConverter.ToSingle(coordBytes, 4);
            float y = BitConverter.ToSingle(coordBytes, 8);
            return (x, y, z); 
        }

        public void SetAxis(WorldChrMan.Coords coords, float value) =>
            _memoryIo.WriteFloat(GetPlayerCoordinatePtr(coords), value);

        public void BreakWeapon()
        {
            var playerGameData = _memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) +
                                                     (int)GameDataMan.GameDataOffsets.PlayerGameData);
            int equippedWep = _memoryIo.ReadInt32((IntPtr)playerGameData + (int)GameDataMan.PlayerGameData.RightHandWeapon);

            var equipGameData = _memoryIo.ReadInt64((IntPtr)playerGameData + (int)GameDataMan.PlayerGameData.EquipGameData);
            var bytes = AsmLoader.GetAsmBytes("BreakRightHandWep");
            AsmHelper.WriteAbsoluteAddresses64(bytes, new []
            {
                (equipGameData, 0x0 + 2),
                (equippedWep, 0x12 + 2),
                (Funcs.GetInventoryIndexByCatAndId, 0x20 + 2)
            });
            
            _memoryIo.AllocateAndExecute(bytes);
        }
    }
    
    
}