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
    // In the WarpCoords / WarpAngle scripts: after the 8-byte `movups xmm15,[rip+x]`, the 4-byte opcode of
    // `movups [rax+field],xmm15`, then the field's displacement.
    private const int StoreDisplacement = 8 + 4;

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
        BitConverter.GetBytes(GameMan.WarpCoords).CopyTo(bytes, StoreDisplacement);
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
        BitConverter.GetBytes(GameMan.WarpAngle).CopyTo(bytes, StoreDisplacement);
        
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

        hookManager.InstallHook(coordsCode, Hooks.WarpCoords, StoreIntoField(GameMan.WarpCoords));
        hookManager.InstallHook(angleCode, Hooks.WarpAngle, StoreIntoField(GameMan.WarpAngle));


        if (!WaitForLoadingFlag(loadingFlagAddr, 1))
        {
        }

        hookManager.UninstallHook(coordsCode);
        hookManager.UninstallHook(angleCode);
    }

    // The game's own `movdqa XMMWORD PTR [rax+displacement],xmm0` that the warp hooks replace.
    private static byte[] StoreIntoField(int displacement) =>
        [0x66, 0x0F, 0x7F, 0x80, ..BitConverter.GetBytes(displacement)];

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

