// 

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class TargetService(IMemoryService memoryService, HookManager hookManager) : ITargetService
{
    private bool _isRepeatActCodeWritten;
    private bool _hasWrittenEnemyId;
    private bool _isRepeatActHookInstalled;
    private List<nint> _repeatActHooks = new();

    public void ToggleTargetHook(bool isEnabled)
    {
        var code = CodeCaveOffsets.Base + CodeCaveOffsets.LockedTarget;
        if (isEnabled)
        {
            var hook = Hooks.LastLockedTarget;
            var savedPtr = CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr;
            var bytes = AsmLoader.GetAsmBytes(AsmScript.LockedTarget);
            AsmHelper.WriteRelativeOffsets(bytes, [
                (code + 0x5, WorldChrMan.Base, 7, 0x5 + 3),
                (code + 0x15, savedPtr, 7, 0x15 + 3),
                (code + 0x21, hook + 6, 5, 0x21 + 1)
            ]);
            memoryService.WriteBytes(code, bytes);
            hookManager.InstallHook(code, hook, [0x48, 0x8B, 0xD7, 0x48, 0x8b, 0xCB]);
        }
        else
        {
            hookManager.UninstallHook(code);
        }
    }

    public nint GetChrIns() =>
        memoryService.Read<nint>(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr);

    public int GetHp() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.Health);

    public int GetMaxHp() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.MaxHealth);

    public void SetHp(int value) =>
        memoryService.Write(GetChrIns() + ChrIns.Health, value);

    public Vector3 GetPosition() =>
        memoryService.Read<Vector3>(GetChrIns() + ChrIns.ReadOnlyCoords);

    public bool IsAiDisabled() =>
        memoryService.IsBitSet(GetChrIns() + ChrIns.NoUpdate.Offset, ChrIns.NoUpdate.Bit);

    public void ToggleAi(bool isDisableAiEnabled) =>
        memoryService.SetBitValue(GetChrIns() + ChrIns.NoUpdate.Offset, ChrIns.NoUpdate.Bit,
            isDisableAiEnabled);

    public bool IsNoDamageEnabled() =>
        memoryService.IsBitSet(GetChrIns() + ChrIns.NoDamage.Offset, ChrIns.NoDamage.Bit);

    public void ToggleNoDamage(bool isDisableNoDamageEnabled) =>
        memoryService.SetBitValue(GetChrIns() + ChrIns.NoDamage.Offset, ChrIns.NoDamage.Bit, isDisableNoDamageEnabled);

    public float GetPoise() =>
        memoryService.Read<float>(GetChrIns() + ChrIns.CurrentPoise);

    public float GetMaxPoise() =>
        memoryService.Read<float>(GetChrIns() + ChrIns.MaxPoise);

    public float GetPoiseTimer() =>
        memoryService.Read<float>(GetChrIns() + ChrIns.PoiseTimer);

    public int[] GetActs()
    {
        var battleGoalId = memoryService.Read<int>(GetNpcThinkParamPtr() + BattleGoalId);
        var globalLuaTable =
            memoryService.FollowPointers(memoryService.Read<nint>(WorldAiMan.Base), WorldAiMan.LuaGlobalTable, true);

        var lsizenode = memoryService.Read<byte>(globalLuaTable + 0x0B);
        var nodeBase = memoryService.Read<nint>(globalLuaTable + 0x20);
        var hashSize = 1 << lsizenode;
        var acts = new List<int>();
        var idStr = battleGoalId.ToString();

        for (int i = 0; i < hashSize; i++)
        {
            var n = nodeBase + i * 40;
            var keyTT = memoryService.Read<int>(n);
            if (keyTT != 4) continue;

            var tsPtr = memoryService.Read<nint>(n + 0x8);
            var len = memoryService.Read<int>(tsPtr + 0x10);
            if (len <= 0 || len > 200) continue;

            var bytes = memoryService.ReadBytes(tsPtr + 0x18, len);
            var str = Encoding.ASCII.GetString(bytes);

            var actIndex = str.IndexOf(idStr + "_Act", StringComparison.Ordinal);
            if (actIndex == -1) continue;

            var numStr = str.Substring(actIndex + idStr.Length + 4);
            if (int.TryParse(numStr, out int actNum))
                acts.Add(actNum);
        }

        acts.Sort();
        return acts.ToArray();
    }

    public int GetCurrentBleed() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.BleedCurrent);

    public int GetMaxBleed() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.BleedMax);

    public int GetCurrentPoison() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.PoisonCurrent);

    public int GetMaxPoison() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.PoisonMax);

    public int GetCurrentToxic() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.ToxicCurrent);

    public int GetMaxToxic() =>
        memoryService.Read<int>(GetChrIns() + ChrIns.ToxicMax);

    public float GetSpeed()
    {
        var pChrIns = CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr;
        var targetSpeedPtr = memoryService.FollowPointers(memoryService.Read<nint>(pChrIns), ChrIns.AnimSpeed, false);
        return memoryService.Read<float>(targetSpeedPtr);
    }

    public void SetSpeed(float speed)
    {
        var pChrIns = CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr;
        var targetSpeedPtr = memoryService.FollowPointers(memoryService.Read<nint>(pChrIns), ChrIns.AnimSpeed, false);

        memoryService.Write(targetSpeedPtr, speed);
    }

    public void DisableRepeatAct()
    {
        foreach (var hookAddr in _repeatActHooks)
        {
            hookManager.UninstallHook(hookAddr);
        }

        _repeatActHooks.Clear();
        _hasWrittenEnemyId = false;
        _isRepeatActCodeWritten = false;
        _isRepeatActHookInstalled = false;
        memoryService.WriteBytes(CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.TargetActIndex, new byte[780]);
    }

    public int GetCurrentRepeatEnemyId()
    {
        try
        {
            var enemyIdBytes =
                memoryService.ReadBytes(CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyId, 8);
            if (enemyIdBytes == null || enemyIdBytes.Length == 0) return -1;

            string idString = Encoding.ASCII.GetString(enemyIdBytes).TrimEnd('\0');
            if (string.IsNullOrWhiteSpace(idString)) return -1;

            if (int.TryParse(idString, out int result)) return result;

            return -1;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    public int GetBattleId()
    {
        var enemyBattleIdPtr = memoryService.FollowPointers(
            memoryService.Read<nint>(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr),
            [..ChrIns.NpcThinkParam, BattleGoalId], false);
        return memoryService.Read<int>(enemyBattleIdPtr);
    }

    public int GetImmunitySpEffect() =>
        memoryService.Read<int>(GetNpcParamPtr() + (int)ChrIns.NpcParamOffsets.AuxImmunitySpEffect);

    public void RepeatAct(int actLabelIndex, int finalActIndex)

    {
        var ifManipulationCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaIfManipulationCode;
        var luaSwitchCheckCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaSwitchCheckCode;
        var luaIfCaseHook = Hooks.LuaLowerOrEqual;
        var luaSwitchCaseHook = Hooks.LuaVmSwitch;
        var battleActivateHook = Hooks.BattleActivate;

        //For enemies with DbgForceAct 
        var forceAct = memoryService.FollowPointers(GetChrIns(), [ChrIns.AiPtr, ForceActOffset], false);
        var hasForceAct = GetChrIns() != IntPtr.Zero && forceAct != ForceActOffset;

        if (actLabelIndex == 0)
        {
            if (hasForceAct)
                memoryService.Write(forceAct, (byte)0);
            hookManager.UninstallHook(ifManipulationCode);
            _hasWrittenEnemyId = false;
            _isRepeatActHookInstalled = false;
            return;
        }

        if (hasForceAct)
            memoryService.Write(forceAct, (byte)actLabelIndex);

        var enemyIdLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyId;
        var enemyIdLengthPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdLength;
        if (!_hasWrittenEnemyId)
        {
            var enemyBattleIdPtr = memoryService.FollowPointers(GetChrIns(),
                [..ChrIns.NpcThinkParam, BattleGoalId], false);

            string enemyId = memoryService.Read<int>(enemyBattleIdPtr).ToString();
            byte[] enemyIdBytes = Encoding.ASCII.GetBytes(enemyId);

            memoryService.WriteBytes(enemyIdLoc, enemyIdBytes);
            memoryService.Write(enemyIdLengthPtr, enemyIdBytes.Length);
            _hasWrittenEnemyId = true;
        }

        var targetActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.TargetActIndex;
        var finalActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.FinalActIndex;
        memoryService.Write(targetActLoc, actLabelIndex - 1);
        memoryService.Write(finalActLoc, finalActIndex - 1);

        var switchPatternMatchFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaSwitchPatternMatchFlag;
        var enemyRaxIdentifier = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyRaxIdentifier;
        var enemyIdentifierCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdentifierCode;
        var luaSwitchHistory = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaSwitchHistory;
        var originalCallOffset = Hooks.LuaLowerOrEqual + 0x906;
        var luaIfCounter = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaIfCounter;
        var ifConditionFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.IfConditionFlag;

        if (!_isRepeatActCodeWritten)
        {
            byte[] enemyIdCheckBytes = AsmLoader.GetAsmBytes(AsmScript.RepeatActIdCheck);
            AsmHelper.WriteRelativeOffsets(enemyIdCheckBytes, new[]
            {
                (enemyIdentifierCode + 0x4B, enemyIdLoc, 7, 0x4B + 3),
                (enemyIdentifierCode + 0x52, enemyIdLengthPtr, 7, 0x52 + 3),
                (enemyIdentifierCode + 0x70, enemyRaxIdentifier, 7, 0x70 + 3)
            });

            Byte[] bytes = BitConverter.GetBytes((int)battleActivateHook + 8 - (enemyIdentifierCode + 0x8B));
            Array.Copy(bytes, 0, enemyIdCheckBytes, 0x86 + 1, 4);
            memoryService.WriteBytes(enemyIdentifierCode, enemyIdCheckBytes);

            byte[] switchCheckBytes = AsmLoader.GetAsmBytes(AsmScript.RepeatActFlagSet);
            AsmHelper.WriteRelativeOffsets(switchCheckBytes, new[]
            {
                (luaSwitchCheckCode, switchPatternMatchFlag, 7, 0x2),
                (luaSwitchCheckCode + 0xE, switchPatternMatchFlag, 7, 0xE + 2),
                (luaSwitchCheckCode + 0x16, luaSwitchHistory + 0x4, 6, 0x16 + 2),
                (luaSwitchCheckCode + 0x1C, luaSwitchHistory, 6, 0x1C + 2),
                (luaSwitchCheckCode + 0x22, luaSwitchHistory + 0x8, 6, 0x22 + 2),
                (luaSwitchCheckCode + 0x28, luaSwitchHistory + 0x4, 6, 0x28 + 2),
                (luaSwitchCheckCode + 0x2E, luaSwitchHistory + 0xC, 6, 0x2E + 2),
                (luaSwitchCheckCode + 0x34, luaSwitchHistory + 0x8, 6, 0x34 + 2),
                (luaSwitchCheckCode + 0x3A, luaSwitchHistory + 0xC, 6, 0x3A + 2),
                (luaSwitchCheckCode + 0x40, luaSwitchHistory, 6, 0x40 + 2), // history[1]
                (luaSwitchCheckCode + 0x4B, luaSwitchHistory + 0x4, 6, 0x4B + 2), // history[2]
                (luaSwitchCheckCode + 0x56, luaSwitchHistory + 0x8, 6, 0x56 + 2), // history[3]
                (luaSwitchCheckCode + 0x61, luaSwitchHistory + 0xC, 6, 0x61 + 2), // history[4]
                (luaSwitchCheckCode + 0x6C, switchPatternMatchFlag, 7, 0x6C + 2),
                (luaSwitchCheckCode + 0x75, luaSwitchHistory + 0x4, 6, 0x75 + 2), // history[2]
                (luaSwitchCheckCode + 0x80, luaSwitchHistory + 0x8, 6, 0x80 + 2), // history[3]
                (luaSwitchCheckCode + 0x8B, luaSwitchHistory + 0xC, 6, 0x8B + 2), // history[4]
                (luaSwitchCheckCode + 0x96, switchPatternMatchFlag, 7, 0x96 + 2),
            });

            bytes = BitConverter.GetBytes((int)luaSwitchCaseHook + 7 - (luaSwitchCheckCode + 0xAA));
            Array.Copy(bytes, 0, switchCheckBytes, 0xA5 + 1, 4);

            memoryService.WriteBytes(luaSwitchCheckCode, switchCheckBytes);

            byte[] ifManipBytes = AsmLoader.GetAsmBytes(AsmScript.RepeatAct);
            AsmHelper.WriteRelativeOffsets(ifManipBytes, new[]
            {
                (ifManipulationCode, switchPatternMatchFlag, 7, 0x2),
                (ifManipulationCode + 0xA, enemyRaxIdentifier, 7, 0xA + 3),
                (ifManipulationCode + 0x17, originalCallOffset, 5, 0x17 + 1),
                (ifManipulationCode + 0x1E, luaIfCounter, 6, 0x1E + 2),
                (ifManipulationCode + 0x24, targetActLoc, 6, 0x24 + 2),
                (ifManipulationCode + 0x2E, ifConditionFlag, 7, 0x2E + 2),
                (ifManipulationCode + 0x37, luaIfCounter, 6, 0x37 + 2),
                (ifManipulationCode + 0x3D, ifConditionFlag, 6, 0x3D + 2),
                (ifManipulationCode + 0x45, ifConditionFlag, 7, 0x45 + 2),
                (ifManipulationCode + 0x55, finalActLoc, 6, 0x55 + 2),
                (ifManipulationCode + 0x5F, luaIfCounter, 6, 0x5F + 2),
                (ifManipulationCode + 0x65, ifConditionFlag, 6, 0x65 + 2),
                (ifManipulationCode + 0x75, originalCallOffset, 5, 0x75 + 1)
            });

            var hookJumpOffsets = new[]
            {
                (0x53, 0x4E + 1),
                (0x74, 0x6F + 1),
                (0x82, 0x7D + 1)
            };

            foreach (var (target, offset) in hookJumpOffsets)
            {
                var jumpOffset = BitConverter.GetBytes((int)luaIfCaseHook + 8 - (ifManipulationCode + target));
                Array.Copy(jumpOffset, 0, ifManipBytes, offset, 4);
            }

            memoryService.WriteBytes(ifManipulationCode, ifManipBytes);
            _isRepeatActCodeWritten = true;
        }

        if (_isRepeatActHookInstalled) return;
        _repeatActHooks.Add(hookManager.InstallHook(enemyIdentifierCode, battleActivateHook,
            new byte[] { 0x48, 0x8B, 0x45, 0x18, 0x48, 0x2B, 0x45, 0x10 }));
        _repeatActHooks.Add(hookManager.InstallHook(luaSwitchCheckCode, luaSwitchCaseHook,
            new byte[] { 0x44, 0x8B, 0xF8, 0x4F, 0x8D, 0x34, 0xEC }));
        _repeatActHooks.Add(hookManager.InstallHook(ifManipulationCode, luaIfCaseHook,
            new byte[] { 0xE8, 0x01, 0x09, 0x00, 0x00, 0x41, 0x3B, 0xC7 }));
        _isRepeatActHookInstalled = true;
    }

    private nint GetNpcThinkParamPtr() =>
        memoryService.FollowPointers(GetChrIns(), ChrIns.NpcThinkParam, true);

    private nint GetNpcParamPtr() =>
        memoryService.FollowPointers(GetChrIns(), ChrIns.NpcParam, true);
}