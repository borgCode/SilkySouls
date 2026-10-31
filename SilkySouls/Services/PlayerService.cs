// 

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Models;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class PlayerService(IMemoryService memoryService, ITravelService travelService) : IPlayerService
{
    private readonly Dictionary<int, int> _lowLevelSoulRequirements = new()
    {
        { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
        { 11, 829 },
    };

    private readonly Dictionary<uint, int> _bonfiresByBlockId = DataLoader.LoadDict<uint, int>("BonfiresByBlockId");

    private readonly Position[] _positions =
    [
        new(0, Vector3.Zero, 0f),
        new(0, Vector3.Zero, 0f)
    ];

    public nint GetPlayerIns() =>
        memoryService.Read<nint>(memoryService.Read<nint>(WorldChrMan.Base) + WorldChrMan.PlayerIns);

    public int GetHp() => memoryService.Read<int>(GetPlayerIns() + ChrIns.Health);

    public int GetMaxHp() => memoryService.Read<int>(GetPlayerIns() + ChrIns.MaxHealth);

    public void SetHp(int hp) => memoryService.Write(GetPlayerIns() + ChrIns.Health, hp);

    public void SetRtsr() => SetHp(GetMaxHp() * 20 / 100 - 1);

    public void SetMaxHp() => SetHp(GetMaxHp());

    public int GetSp() => memoryService.Read<int>(GetPlayerIns() + ChrIns.Stamina);

    public void SetSp(int sp) => memoryService.Write(GetPlayerIns() + ChrIns.Stamina, sp);

    public Vector3 GetPosition() => memoryService.Read<Vector3>(GetPlayerIns() + ChrIns.ReadOnlyCoords);

    public void SavePosition(int index)
    {
        var posToSave = _positions[index];
        var playerIns = GetPlayerIns();
        var blockIdPtr = memoryService.FollowPointers(playerIns, WorldChrMan.CurrentBlockId, false);
        var physicsModule = memoryService.FollowPointers(playerIns, ChrIns.PhysicsModule, true);
        var havokCoords = memoryService.FollowPointers(physicsModule, ChrIns.HavokCoords, false);

        posToSave.BlockId = memoryService.Read<uint>(blockIdPtr);
        posToSave.Coords = memoryService.Read<Vector3>(havokCoords);
        posToSave.Coords = memoryService.Read<Vector3>(physicsModule + ChrIns.Coords);
        posToSave.Angle = memoryService.Read<float>(physicsModule + ChrIns.Angle);
    }

    public void RestorePositon(int index)
    {
        var savedPos = _positions[index];
        var blockIdPtr = memoryService.FollowPointers(GetPlayerIns(), WorldChrMan.CurrentBlockId, false);
        var currentBlockId = memoryService.Read<uint>(blockIdPtr);

        if (currentBlockId != savedPos.BlockId)
        {
            var bonfireId = _bonfiresByBlockId[savedPos.BlockId];
            _ = Task.Run(() => travelService.WarpWithCoords(savedPos.Coords, savedPos.Angle, bonfireId));
        }
        else
        {
            var physicsModule = memoryService.FollowPointers(GetPlayerIns(), ChrIns.PhysicsModule, true);
            var havokCoords = memoryService.FollowPointers(physicsModule, ChrIns.HavokCoords, false);
            memoryService.Write(havokCoords, savedPos.Coords);
            memoryService.Write(physicsModule + ChrIns.Coords, savedPos.Coords);
            memoryService.Write(physicsModule + ChrIns.Angle, savedPos.Angle);
        }
    }

    public int GetNewGame() =>
        memoryService.Read<int>(memoryService.Read<nint>(GameDataMan.Base) + (int)GameDataMan.GameDataOffsets.Ng);

    public void SetNewGame(int value) =>
        memoryService.Write(memoryService.Read<nint>(GameDataMan.Base) + (int)GameDataMan.GameDataOffsets.Ng, value);

    public float GetSpeed()
    {
        var targetSpeedPtr = memoryService.FollowPointers(GetPlayerIns(), ChrIns.AnimSpeed, false);
        return memoryService.Read<float>(targetSpeedPtr);
    }

    public void SetSpeed(float speed)
    {
        var targetSpeedPtr = memoryService.FollowPointers(GetPlayerIns(), ChrIns.AnimSpeed, false);

        memoryService.Write(targetSpeedPtr, speed);
    }

    public void ToggleChrDebugFlag(int offset, bool isEnabled) =>
        memoryService.Write(DebugFlags.Base + offset, isEnabled);

    public void ToggleNoDamage(bool isEnabled) =>
        memoryService.SetBitValue(GetPlayerIns() + ChrIns.NoDamage.Offset, ChrIns.NoDamage.Bit, isEnabled);

    public void ToggleInfiniteStamina(bool isEnabled) =>
        memoryService.SetBitValue(GetPlayerIns() + ChrIns.InfiniteStam.Offset, ChrIns.InfiniteStam.Bit, isEnabled);

    public void ToggleNoGoodsConsume(bool isEnabled) =>
        memoryService.SetBitValue(GetPlayerIns() + ChrIns.NoGoodsConsume.Offset, ChrIns.NoGoodsConsume.Bit, isEnabled);

    public void ToggleInfinitePoise(bool isEnabled) =>
        memoryService.SetBitValue(GetPlayerIns() + ChrIns.InfinitePoise.Offset, ChrIns.InfinitePoise.Bit, isEnabled);

    public void ToggleInfiniteDurability(bool isEnabled)
    {
        if (isEnabled) memoryService.Write(Patches.InfiniteDurability + 0x1, (byte)0x89);
        else memoryService.Write(Patches.InfiniteDurability + 0x1, (byte)0x88);
    }

    public void ToggleNoRoll(bool isEnabled)
    {
        var noRollPatchPtr = Patches.NoRoll;
        var noBackStepPatchPtr = Patches.NoBackStep;
        if (isEnabled)
        {
            memoryService.Write(noRollPatchPtr + 0x6, (byte)0);
            memoryService.Write(noRollPatchPtr + 0xD, (byte)0);
            memoryService.Write(noBackStepPatchPtr + 0x6, (byte)0);
            memoryService.Write(noBackStepPatchPtr + 0xD, (byte)0);
        }
        else
        {
            memoryService.Write(noRollPatchPtr + 0x6, (byte)1);
            memoryService.Write(noRollPatchPtr + 0xD, (byte)1);
            memoryService.Write(noBackStepPatchPtr + 0x6, (byte)1);
            memoryService.Write(noBackStepPatchPtr + 0xD, (byte)1);
        }
    }

    public void RestoreSpellCasts()
    {
        var equipMagicData = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base),
        [
            (int)GameDataMan.GameDataOffsets.PlayerGameData,
            (int)GameDataMan.PlayerGameData.EquipMagicData
        ], true);
        var bytes = AsmLoader.GetAsmBytes(AsmScript.RestoreSpellCasts);

        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (equipMagicData, 2),
            (Functions.RestoreCastsFunc, 0xE + 2)
        ]);

        memoryService.AllocateAndExecute(bytes);
    }

    public void GiveSouls()
    {
        var soulsPtr = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base), new[]
            {
                (int)GameDataMan.GameDataOffsets.PlayerGameData,
                (int)GameDataMan.PlayerGameData.Souls
            },
            false);
        int currentVal = memoryService.Read<int>(soulsPtr);
        HandleSoulEdit(soulsPtr, currentVal + 10000, currentVal);
    }

    public int GetPlayerStat(GameDataMan.PlayerGameData stat)
    {
        var statsBasePtr = memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) +
                                                    (int)GameDataMan.GameDataOffsets.PlayerGameData);
        return memoryService.Read<int>(statsBasePtr + (int)stat);
    }

    public void SetPlayerStat(GameDataMan.PlayerGameData statType, int newValue)
    {
        var statPtr = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base), [
            (int)GameDataMan.GameDataOffsets.PlayerGameData, (int)statType
        ], false);

        int currentValue = memoryService.Read<int>(statPtr);
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
                memoryService.Write(statPtr, validatedHumanity);
                return;

            default:
                var validatedStat = newValue;
                if (validatedStat < 1) validatedStat = 1;
                if (validatedStat > 99) validatedStat = 99;
                if (validatedStat != currentValue)
                {
                    memoryService.Write(statPtr, validatedStat);
                    UpdatePlayerStats(validatedStat - currentValue);
                }

                return;
        }
    }

    public void BreakWeapon(int slotOffset)
    {
        var playerGameData = memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) +
                                                      (int)GameDataMan.GameDataOffsets.PlayerGameData);
        int equippedWep = memoryService.Read<int>(playerGameData + slotOffset);

        var equipGameData = memoryService.Read<nint>(playerGameData + (int)GameDataMan.PlayerGameData.EquipGameData);
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BreakRightHandWep);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (equipGameData, 0x0 + 2),
            (equippedWep, 0x12 + 2),
            (Functions.GetInventoryIndexByCatAndId, 0x20 + 2)
        ]);

        memoryService.AllocateAndExecute(bytes);
    }

    private void UpdatePlayerStats(int difference)
    {
        var allStatsPtr =
            memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base),
                [(int)GameDataMan.GameDataOffsets.PlayerGameData], true);

        int originalSouls = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls);

        int currentLevel = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.SoulLevel);
        int newLevel = currentLevel + difference;


        int[] stats = new int[9];
        stats[0] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Vitality);
        stats[1] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Attunement);
        stats[2] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Endurance);
        stats[3] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Strength);
        stats[4] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Dexterity);
        stats[5] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Resistance);
        stats[6] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Intelligence);
        stats[7] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Faith);
        stats[8] = memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.Humanity);

        if (CallLevelUpFunction(newLevel, stats))
        {
            if (newLevel < currentLevel)
            {
                memoryService.Write(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls, originalSouls);
                return;
            }

            int totalSoulsRequired = CalculateTotalSoulsRequired(currentLevel, newLevel);
            int currentTotalSouls =
                memoryService.Read<int>(allStatsPtr + (int)GameDataMan.PlayerGameData.TotalSouls);
            memoryService.Write(allStatsPtr + (int)GameDataMan.PlayerGameData.TotalSouls,
                totalSoulsRequired + currentTotalSouls);
            memoryService.Write(allStatsPtr + (int)GameDataMan.PlayerGameData.Souls, originalSouls);
        }
    }

    private bool CallLevelUpFunction(int newLevel, int[] stats)
    {
        var statArrayAddress = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.StatsArray;
        var tempStatArray = statArrayAddress;
        for (int i = 0; i < 9; i++)
        {
            memoryService.Write(tempStatArray, stats[i]);
            tempStatArray += 0x4;
        }

        var codeStart = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.CodeBlock;
        var soulsPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.LevelUp.SoulsPtr;

        byte[] codeBytes = AsmLoader.GetAsmBytes(AsmScript.LevelUp);
        byte[] bytes = BitConverter.GetBytes(statArrayAddress);
        Array.Copy(bytes, 0, codeBytes, 2, 8);
        bytes = BitConverter.GetBytes(soulsPtr);
        Array.Copy(bytes, 0, codeBytes, 15, 8);
        bytes = BitConverter.GetBytes(Functions.LevelUpFunc);
        Array.Copy(bytes, 0, codeBytes, 32, 8);
        memoryService.WriteBytes(codeStart, codeBytes);

        var newLevelAddr = soulsPtr + LevelUpData.NewLevel;
        memoryService.Write(newLevelAddr, newLevel);
        var requiredSoulsAddr = soulsPtr + LevelUpData.RequiredSouls;
        memoryService.Write(requiredSoulsAddr, 0);

        var currSoulsAddr = soulsPtr + LevelUpData.CurrentSouls;
        memoryService.Write(currSoulsAddr, 9999999);

        return memoryService.RunThreadAndWaitForCompletion(codeStart);
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

    private void HandleSoulEdit(nint statPtr, int newValue, int oldValue)
    {
        if (newValue < oldValue)
        {
            memoryService.Write(statPtr, newValue);
            return;
        }

        int difference = newValue - oldValue;
        var totalSoulsPtr = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base), new[]
            {
                (int)GameDataMan.GameDataOffsets.PlayerGameData,
                (int)GameDataMan.PlayerGameData.TotalSouls
            },
            false);
        int currentTotalSouls = memoryService.Read<int>(totalSoulsPtr);

        memoryService.Write(totalSoulsPtr, difference + currentTotalSouls);
        memoryService.Write(statPtr, newValue);
    }
}