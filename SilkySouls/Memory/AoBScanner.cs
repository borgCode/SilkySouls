using System;
using SilkySouls.memory;

namespace SilkySouls.Memory
{
    public class AoBScanner
    {
        private readonly MemoryIo _memoryIo;

        public AoBScanner(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }

        public void Scan()
        {
            Offsets.WorldChrMan.Base = FindAddressByPattern(Patterns.WorldChrMan);
            Offsets.DebugFlags.Base = FindAddressByPattern(Patterns.DebugFlags);
            Offsets.Cam.Base = FindAddressByPattern(Patterns.CamBase);
            Offsets.GameDataMan.Base = FindAddressByPattern(Patterns.GameDataMan);
            Offsets.ItemGet = FindAddressByPattern(Patterns.ItemGetFunc).ToInt64();
            Offsets.ItemGetMenuMan = FindAddressByPattern(Patterns.ItemGetMenuMan);
            Offsets.ItemDlgFunc = FindAddressByPattern(Patterns.ItemGetDlgFunc).ToInt64();
            Offsets.FieldArea.Base = FindAddressByPattern(Patterns.FieldArea);
            Offsets.GameMan.Base = FindAddressByPattern(Patterns.GameMan);
            Offsets.DamageMan.Base = FindAddressByPattern(Patterns.DamMan);
            Offsets.DrawEventPatch = FindAddressByPattern(Patterns.DrawEventPatch);
            Offsets.DrawSoundViewPatch = FindAddressByPattern(Patterns.DrawSoundViewPatch);
            Offsets.MenuMan.Base = FindAddressByPattern(Patterns.MenuMan);
            Offsets.EventFlagMan.Base = FindAddressByPattern(Patterns.EventFlagMan);
            Offsets.LevelUpFunc = FindAddressByPattern(Patterns.LevelUpFunc).ToInt64();
            Offsets.RestoreCastsFunc = FindAddressByPattern(Patterns.RestoreCastsFunc).ToInt64();
            Offsets.HgDraw.Base = FindAddressByPattern(Patterns.HgDraw);
            Offsets.WarpEvent = FindAddressByPattern(Patterns.WarpEvent);
            Offsets.WarpFunc = FindAddressByPattern(Patterns.WarpFunc).ToInt64();
            Offsets.QuitoutPatch = FindAddressByPattern(Patterns.QuitoutPatch);
            Offsets.SoloParamMan.Base = FindAddressByPattern(Patterns.SoloParamMan);
            Offsets.InfiniteDurabilityPatch = FindAddressByPattern(Patterns.InfiniteDurabilityPatch);
            Offsets.OpenEnhanceShopWeapon = FindAddressByPattern(Patterns.OpenEnhanceShop).ToInt64();
            Offsets.OpenEnhanceShopArmor = Offsets.OpenEnhanceShopWeapon - 0x40;
            
            Offsets.Hooks.LastLockedTarget = FindAddressByPattern(Patterns.LastLockedTarget).ToInt64();
            Offsets.Hooks.AllNoDamage = FindAddressByPattern(Patterns.AllNoDamage).ToInt64();
            Offsets.Hooks.ItemSpawn = FindAddressByPattern(Patterns.ItemSpawnHook).ToInt64();
            Offsets.Hooks.Draw = FindAddressByPattern(Patterns.DrawHook).ToInt64();
            Offsets.Hooks.TargetingView = FindAddressByPattern(Patterns.TargetingView).ToInt64();
            Offsets.Hooks.InAirTimer = FindAddressByPattern(Patterns.InAirTimer).ToInt64();
            Offsets.Hooks.Keyboard = FindAddressByPattern(Patterns.Keyboard).ToInt64();
            Offsets.Hooks.ControllerR2 = FindAddressByPattern(Patterns.ControllerR2).ToInt64();
            Offsets.Hooks.ControllerL2 = FindAddressByPattern(Patterns.ControllerL2).ToInt64();
            Offsets.Hooks.UpdateCoords = FindAddressByPattern(Patterns.UpdateCoords).ToInt64();
            Offsets.Hooks.WarpCoords = FindAddressByPattern(Patterns.WarpCoords).ToInt64();

            // Console.WriteLine($"WorldChrMan.Base: 0x{Offsets.WorldChrMan.Base.ToInt64():X}");
            // Console.WriteLine($"DebugFlags.Base: 0x{Offsets.DebugFlags.Base.ToInt64():X}"); 
            // Console.WriteLine($"Cam.Base: 0x{Offsets.Cam.Base.ToInt64():X}");
            // Console.WriteLine($"GameDataMan.Base: 0x{Offsets.GameDataMan.Base.ToInt64():X}");
            // Console.WriteLine($"ItemGet: 0x{Offsets.ItemGet:X}");
            // Console.WriteLine($"ItemGetMenuMan: 0x{Offsets.ItemGetMenuMan.ToInt64():X}");
            // Console.WriteLine($"ItemDlgFunc: 0x{Offsets.ItemDlgFunc:X}");
            // Console.WriteLine($"FieldArea.Base: 0x{Offsets.FieldArea.Base.ToInt64():X}");
            // Console.WriteLine($"GameMan.Base: 0x{Offsets.GameMan.Base.ToInt64():X}");
            // Console.WriteLine($"DamageMan.Base: 0x{Offsets.DamageMan.Base.ToInt64():X}");
            // Console.WriteLine($"DrawEventPatch: 0x{Offsets.DrawEventPatch.ToInt64():X}");
            // Console.WriteLine($"DrawSoundViewPatch: 0x{Offsets.DrawSoundViewPatch.ToInt64():X}");
            // Console.WriteLine($"MenuMan.Base: 0x{Offsets.MenuMan.Base.ToInt64():X}");
            // Console.WriteLine($"EventFlagMan.Base: 0x{Offsets.EventFlagMan.Base.ToInt64():X}");
            // Console.WriteLine($"LevelUpFunc: 0x{Offsets.LevelUpFunc:X}");
            // Console.WriteLine($"RestoreCastsFunc: 0x{Offsets.RestoreCastsFunc:X}");
            // Console.WriteLine($"HgDraw.Base: 0x{Offsets.HgDraw.Base.ToInt64():X}");
            // Console.WriteLine($"WarpEvent: 0x{Offsets.WarpEvent.ToInt64():X}");
            // Console.WriteLine($"WarpFunc: 0x{Offsets.WarpFunc:X}");
            // Console.WriteLine($"FastQuitout: 0x{Offsets.QuitoutPatch.ToInt64():X}");
            
            Console.WriteLine($"Weapon: 0x{Offsets.OpenEnhanceShopWeapon:X}");
            Console.WriteLine($"Weapon: 0x{Offsets.OpenEnhanceShopArmor:X}");
            //
            // Console.WriteLine($"Hooks.LastLockedTarget: 0x{Offsets.Hooks.LastLockedTarget:X}");
            // Console.WriteLine($"Hooks.AllNoDamage: 0x{Offsets.Hooks.AllNoDamage:X}");
            // Console.WriteLine($"Hooks.ItemSpawn: 0x{Offsets.Hooks.ItemSpawn:X}");
            // Console.WriteLine($"Hooks.Draw: 0x{Offsets.Hooks.Draw:X}");
            // Console.WriteLine($"Hooks.TargetingView: 0x{Offsets.Hooks.TargetingView:X}");
            // Console.WriteLine($"Hooks.InAirTimer: 0x{Offsets.Hooks.InAirTimer:X}");
            // Console.WriteLine($"Hooks.Keyboard: 0x{Offsets.Hooks.Keyboard:X}");
            // Console.WriteLine($"Hooks.ControllerR2: 0x{Offsets.Hooks.ControllerR2:X}");
            // Console.WriteLine($"Hooks.ControllerL2: 0x{Offsets.Hooks.ControllerL2:X}");
            // Console.WriteLine($"Hooks.UpdateCoords: 0x{Offsets.Hooks.UpdateCoords:X}");
            // Console.WriteLine($"Hooks.WarpCoords: 0x{Offsets.Hooks.WarpCoords:X}");
        }

        public IntPtr FindAddressByPattern(Pattern pattern)
        {
            IntPtr patternAddress = PatternScan(pattern.Bytes, pattern.Mask);
            if (patternAddress == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr instructionAddress = IntPtr.Add(patternAddress, pattern.InstructionOffset);

            switch (pattern.RipType)
            {
                case RipType.None:
                    return instructionAddress;

                case RipType.Standard:
                    // e.g. 48 8B 05/0D - Standard mov rax/rcx,[rip+offset]
                    int stdOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                    return IntPtr.Add(instructionAddress, stdOffset + 7);

                case RipType.Comparison:
                    // e.g. 80 3D - cmp byte ptr [rip+offset],imm
                    int cmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 2));
                    return IntPtr.Add(instructionAddress, cmpOffset + 7);
                case RipType.Call:
                    int callOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 1));
                    return IntPtr.Add(instructionAddress, callOffset + 5);

                default:
                    return IntPtr.Zero;
            }
        }

        public IntPtr PatternScan(byte[] pattern, string mask)
        {
            const int chunkSize = 4096 * 16;
            byte[] buffer = new byte[chunkSize];

            IntPtr currentAddress = _memoryIo.BaseAddress;
            IntPtr endAddress = IntPtr.Add(currentAddress, 0x3200000);

            while (currentAddress.ToInt64() < endAddress.ToInt64())
            {
                int bytesRemaining = (int)(endAddress.ToInt64() - currentAddress.ToInt64());
                int bytesToRead = Math.Min(bytesRemaining, buffer.Length);

                if (bytesToRead < pattern.Length)
                    break;

                buffer = _memoryIo.ReadBytes(currentAddress, bytesToRead);

                for (int i = 0; i <= bytesToRead - pattern.Length; i++)
                {
                    bool found = true;

                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (j < mask.Length && mask[j] == '?')
                            continue;

                        if (buffer[i + j] != pattern[j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return IntPtr.Add(currentAddress, i);
                }

                currentAddress = IntPtr.Add(currentAddress, bytesToRead - pattern.Length + 1);
            }

            return IntPtr.Zero;
        }
    }
}