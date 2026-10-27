// 

using System;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class UtilityService(IMemoryService memoryService, HookManager hookManager) : IUtilityService
{
    public const float DefaultNoClipSpeedScale = 0.2f;

    public void ShowMenu(int offset, int val) =>
        memoryService.Write(memoryService.Read<nint>(MenuMan.Base) + offset, val);

    
    public void ToggleNoClip(bool isEnabled)
    {
        var inAirTimerCode = CodeCaveOffsets.Base + CodeCaveOffsets.InAirTimer;
        var kbCode = CodeCaveOffsets.Base + CodeCaveOffsets.Kb;
        var rightTriggerCode = CodeCaveOffsets.Base + CodeCaveOffsets.TriggerR2;
        var leftTriggerCode = CodeCaveOffsets.Base + CodeCaveOffsets.TriggerL2;
        var updateCoordsCode = CodeCaveOffsets.Base + CodeCaveOffsets.UpdateCoords;
        
        if (isEnabled)
        {
            WriteInAirTimer(inAirTimerCode);
            WriteKeyboardHook(kbCode);
            WriteRightTriggerCode(rightTriggerCode);
            WriteLeftTriggerCode(leftTriggerCode);
            WriteUpdateCoordsCode(updateCoordsCode);

            hookManager.InstallHook(inAirTimerCode, Hooks.InAirTimer, [0xF3, 0x0F, 0x58, 0x9B, 0xB0, 0x01, 0x00, 0x00]);
            hookManager.InstallHook(kbCode, Hooks.Keyboard, [0xC6, 0x43, 0xF0, 0x01, 0xC6, 0x00, 0x01]);
            hookManager.InstallHook(rightTriggerCode, Hooks.ControllerR2, [0x0F, 0xB6, 0x44, 0x24, 0x27]);
            hookManager.InstallHook(leftTriggerCode, Hooks.ControllerL2, [0x0F, 0xB6, 0x44, 0x24, 0x26]);
            hookManager.InstallHook(updateCoordsCode, Hooks.UpdateCoords, [0x0F, 0x14, 0xDA, 0x0F, 0x29, 0x5B, 0x10]);
        }
        else
        {
            hookManager.UninstallHook(inAirTimerCode);
            hookManager.UninstallHook(kbCode);
            hookManager.UninstallHook(rightTriggerCode);
            hookManager.UninstallHook(leftTriggerCode);
            hookManager.UninstallHook(updateCoordsCode);
        }
    }

    public void WriteNoClipSpeed(float speedScale)
    {
        var ptr = CodeCaveOffsets.Base + CodeCaveOffsets.SpeedScale;
        memoryService.Write(ptr, DefaultNoClipSpeedScale * speedScale);
    }

    private void WriteInAirTimer(nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_InAirTimer);
        codeBytes[0x21] = (byte)ChrIns.ChrCtrl; // the second mov rax,[rax+0x68]: player -> ChrCtrl
        AsmHelper.WriteRelativeOffsets(codeBytes, [
            (code + 0x9, WorldChrMan.Base, 7, 0x9 + 3),
            (code + 0x31, Hooks.InAirTimer + 8, 5, 0x31 + 1)
        ]);

        memoryService.WriteBytes(code, codeBytes);
    }

    private void WriteKeyboardHook(nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_KB);
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(codeBytes, [
            (code + 0x16, Hooks.Keyboard + 7, 5, 0x16 + 1),
            (code + 0x1b, zDirection, 7, 0x1B + 2),
            (code + 0x29, Hooks.Keyboard + 7, 5, 0x29 + 1),
            (code + 0x2E, zDirection, 7, 0x2E + 2),
            (code + 0x3C, Hooks.Keyboard + 7, 5, 0x3C + 1),
        ]);

        memoryService.WriteBytes(code, codeBytes);
    }

    private void WriteRightTriggerCode(nint code)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_R2);
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, Hooks.ControllerR2 + 5, 6, 0x7 + 2),
            (code + 0xD, zDirection, 7, 0xD + 2),
            (code + 0x16, Hooks.ControllerR2 + 5, 5, 0x16 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteLeftTriggerCode(nint code)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_ZDirection_L2);
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x7, Hooks.ControllerL2 + 5, 6, 0x7 + 2),
            (code + 0xD, zDirection, 7, 0xD + 2),
            (code + 0x16, Hooks.ControllerL2 + 5, 5, 0x16 + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteUpdateCoordsCode(nint code)
    {
        var codeBytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_UpdateCoords);
        codeBytes[0x24] = (byte)ChrIns.ChrCtrl; // the second mov rax,[rax+0x68]: player -> ChrCtrl
        var zDirection = CodeCaveOffsets.Base + CodeCaveOffsets.ZDirection;
        var speedScale = CodeCaveOffsets.Base + CodeCaveOffsets.SpeedScale;

        AsmHelper.WriteRelativeOffsets(codeBytes, [
            (code + 0x4, WorldChrMan.Base, 7, 0x4 + 3),
            (code + 0x79, PadMan.Base, 7, 0x79 + 3),
            (code + 0x84, DbgMapWalkPadVtable, 7, 0x84 + 3),
            (code + 0xA3, Functions.GetYMovement, 5, 0xA3 + 1),
            (code + 0xB2, Functions.GetXMovement, 5, 0xB2 + 1),
            (code + 0xE6, FieldArea.Base, 7, 0xE6 + 3),
            (code + 0xF9, Functions.MatrixVectorProduct, 5, 0xF9 + 1),
            (code + 0x135, speedScale, 9, 0x135 + 5),
            (code + 0x14B, zDirection, 6, 0x14B + 2),
            (code + 0x175, zDirection, 7, 0x175 + 2),
            (code + 0x1A0, Hooks.UpdateCoords + 7, 5, 0x1A0 + 1)
        ]);
        memoryService.WriteBytes(code, codeBytes);
    }

    public void ToggleDeathCamera(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(WorldChrMan.Base) + (int)WorldChrMan.BaseOffsets.DeathCam, isEnabled);

    public bool HasTemporalAntiAliasing()
    {
        var pcOptionData =
            memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) + (int)GameDataMan.GameDataOffsets.PcOptionData);
        return memoryService.Read<int>(pcOptionData + (int)GameDataMan.PcOptionData.AntiAliasingMode) == 3;
    }

    public void SetGuaranteedBkhDrop(bool isEnabled)
    {
        var bkhPtr = memoryService.FollowPointers(memoryService.Read<nint>(SoloParamMan.Base), new[]
        {
            SoloParamMan.ParamResCap,
            SoloParamMan.ItemLot,
            SoloParamMan.BkhItemLotEntry
        }, false);

        if (isEnabled)
        {
            memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Nothing, (byte)0);
            memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bkh, (byte)0x64);
            memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bks, (byte)0);
        }
        else
        {
            memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Nothing, (byte)0x4B);
            memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bkh, (byte)0x14);
            memoryService.Write(bkhPtr + (int)SoloParamMan.BkhDropRateSlots.Bks, (byte)0x5);
        }
    }

    public void ToggleFilter(bool isEnabled)
    {
        if (isEnabled)
        {
            var filterPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
            memoryService.Write(filterPtr, (byte)1);
            var brightnessPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                { FieldArea.RenderPtr, FieldArea.Brightness }, false);
            var bytes = new byte[12];
            var floatBytes = BitConverter.GetBytes(5.0f);
            Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
            Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
            Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

            memoryService.WriteBytes(brightnessPtr, bytes);
        }
        else
        {
            var filterPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
            memoryService.Write(filterPtr, (byte)0);
            var brightnessPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), new[]
                { FieldArea.RenderPtr, FieldArea.Brightness }, false);
            var bytes = new byte[12];
            var floatBytes = BitConverter.GetBytes(1.0f);
            Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
            Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
            Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

            memoryService.WriteBytes(brightnessPtr, bytes);
        }
    }
}