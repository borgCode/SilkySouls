// 

using System;
using System.Numerics;
using System.Threading;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class TravelService(IMemoryService memoryService, HookManager hookManager) : ITravelService
{
    public void Warp(int bonfireId)
    {
        var lastBonfirePtr = memoryService.Read<nint>(GameMan.Base) + GameMan.LastBonfire;
        memoryService.Write(lastBonfirePtr, bonfireId);

        var bytes = AsmLoader.GetAsmBytes(AsmScript.Warp);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (EventMan.Base, 2),
            (Functions.Warp, 0x16 + 2)
        ]);

        memoryService.AllocateAndExecute(bytes);
    }

    public void WarpWithCoords(Vector3 coords, float angle, int bonfireId)
    {
        Warp(bonfireId);

        var coordsAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.Coords;
        var coordsCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.CoordCode;

        memoryService.Write(coordsAddr, coords);
        memoryService.Write(coordsAddr + 12, 1.0f);

        byte[] bytes = AsmLoader.GetAsmBytes(AsmScript.WarpCoords);
        AsmHelper.WriteRelativeOffsets(bytes, [
        (coordsCode, coordsAddr, 8, 4),
        (coordsCode + 0x14, Hooks.WarpCoords + 8, 5, 0x14 + 1)
        ]);
        
        memoryService.WriteBytes(coordsCode, bytes);

        var angleAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.Angle;
        var angleCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpCoords.AngleCode;

        memoryService.Write(angleAddr, 0L);     
        memoryService.Write(angleAddr + 4, angle);
        memoryService.Write(angleAddr + 8, 0L); 

        bytes = AsmLoader.GetAsmBytes(AsmScript.WarpAngle);
        
        AsmHelper.WriteRelativeOffsets(bytes, [
            (angleCode, angleAddr, 8, 4),
            (angleCode + 0x14, Hooks.WarpAngle + 8, 5, 0x14 + 1)
        ]);
        
        memoryService.WriteBytes(angleCode, bytes);

        IntPtr loadingFlagAddr =
            memoryService.FollowPointers(memoryService.Read<nint>(MenuMan.Base),
                new[] { MenuMan.LoadedFlag }, false);

        if (!WaitForLoadingFlag(loadingFlagAddr, 0))
        {
            return;
        }

        hookManager.InstallHook(coordsCode, Hooks.WarpCoords, [0x66, 0x0F, 0x7F, 0x80, 0x80, 0x0A, 0x00, 0x00]);
        var hasAngleHook = Hooks.WarpAngle != memoryService.BaseAddress;
        if (hasAngleHook)
            hookManager.InstallHook(angleCode, Hooks.WarpAngle, [0x66, 0x0F, 0x7F, 0x80, 0x90, 0x0A, 0x00, 0x00]);


        if (!WaitForLoadingFlag(loadingFlagAddr, 1))
        {
        }

        hookManager.UninstallHook(coordsCode);
        if (hasAngleHook)
            hookManager.UninstallHook(angleCode);
    }

    private bool WaitForLoadingFlag(IntPtr loadingFlagAddr, int expectedValue)
    {
        int startTime = Environment.TickCount;

        while (Environment.TickCount - startTime < 10000)
        {
            int loadingValue = memoryService.Read<int>(loadingFlagAddr);
            if (loadingValue == expectedValue)
            {
                return true;
            }

            Thread.Sleep(50);
        }

        return false;
    }

    public void UnlockBonfireWarps()
    {
        var bonfireFlagBase = memoryService.FollowPointers(memoryService.Read<nint>(EventFlagMan.Base),
            new[] { EventFlagMan.FlagPtr }, true);

        var bonfireWarpFlagAddr = bonfireFlagBase + EventFlagMan.WarpFlag;
        memoryService.SetBit32(bonfireWarpFlagAddr, EventFlagMan.WarpFlagBit1, true);
        memoryService.SetBit32(bonfireWarpFlagAddr, EventFlagMan.WarpFlagBit2, true);

        var bonfireFlagAddr = bonfireFlagBase + EventFlagMan.BonfireFlags;
        foreach (EventFlagMan.BonfireBitFlag flag in Enum.GetValues(
                     typeof(EventFlagMan.BonfireBitFlag)))
        {
            int bitPosition = (int)flag;
            memoryService.SetBit32(bonfireFlagAddr, bitPosition, true);
        }
    }
}

