using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            Offsets.WorldAiMan.Base = FindAddressByPattern(Patterns.WorldAiMan);

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
            Offsets.Hooks.LuaIfElse = FindAddressByPattern(Patterns.LuaIfElseHook).ToInt64();
            Offsets.Hooks.LuaOpcodeSwitch = FindAddressByPattern(Patterns.LuaOpCodeSwitch).ToInt64();
            Offsets.Hooks.BattleActivate = FindAddressByPattern(Patterns.BattleActivateHook).ToInt64();

            Console.WriteLine($"WorldChrMan.Base: 0x{Offsets.WorldChrMan.Base.ToInt64():X}");
            Console.WriteLine($"DebugFlags.Base: 0x{Offsets.DebugFlags.Base.ToInt64():X}");
            Console.WriteLine($"Cam.Base: 0x{Offsets.Cam.Base.ToInt64():X}");
            Console.WriteLine($"GameDataMan.Base: 0x{Offsets.GameDataMan.Base.ToInt64():X}");
            Console.WriteLine($"ItemGet: 0x{Offsets.ItemGet:X}");
            Console.WriteLine($"ItemGetMenuMan: 0x{Offsets.ItemGetMenuMan.ToInt64():X}");
            Console.WriteLine($"ItemDlgFunc: 0x{Offsets.ItemDlgFunc:X}");
            Console.WriteLine($"FieldArea.Base: 0x{Offsets.FieldArea.Base.ToInt64():X}");
            Console.WriteLine($"GameMan.Base: 0x{Offsets.GameMan.Base.ToInt64():X}");
            Console.WriteLine($"DamageMan.Base: 0x{Offsets.DamageMan.Base.ToInt64():X}");
            Console.WriteLine($"DrawEventPatch: 0x{Offsets.DrawEventPatch.ToInt64():X}");
            Console.WriteLine($"DrawSoundViewPatch: 0x{Offsets.DrawSoundViewPatch.ToInt64():X}");
            Console.WriteLine($"MenuMan.Base: 0x{Offsets.MenuMan.Base.ToInt64():X}");
            Console.WriteLine($"EventFlagMan.Base: 0x{Offsets.EventFlagMan.Base.ToInt64():X}");
            Console.WriteLine($"LevelUpFunc: 0x{Offsets.LevelUpFunc:X}");
            Console.WriteLine($"RestoreCastsFunc: 0x{Offsets.RestoreCastsFunc:X}");
            Console.WriteLine($"HgDraw.Base: 0x{Offsets.HgDraw.Base.ToInt64():X}");
            Console.WriteLine($"WarpEvent: 0x{Offsets.WarpEvent.ToInt64():X}");
            Console.WriteLine($"WarpFunc: 0x{Offsets.WarpFunc:X}");
            Console.WriteLine($"FastQuitout: 0x{Offsets.QuitoutPatch.ToInt64():X}");
            Console.WriteLine($"WorldAiMan: 0x{Offsets.WorldAiMan.Base.ToInt64():X}");

            Console.WriteLine($"Weapon: 0x{Offsets.OpenEnhanceShopWeapon:X}");
            Console.WriteLine($"Weapon: 0x{Offsets.OpenEnhanceShopArmor:X}");

            Console.WriteLine($"Hooks.LastLockedTarget: 0x{Offsets.Hooks.LastLockedTarget:X}");
            Console.WriteLine($"Hooks.AllNoDamage: 0x{Offsets.Hooks.AllNoDamage:X}");
            Console.WriteLine($"Hooks.ItemSpawn: 0x{Offsets.Hooks.ItemSpawn:X}");
            Console.WriteLine($"Hooks.Draw: 0x{Offsets.Hooks.Draw:X}");
            Console.WriteLine($"Hooks.TargetingView: 0x{Offsets.Hooks.TargetingView:X}");
            Console.WriteLine($"Hooks.InAirTimer: 0x{Offsets.Hooks.InAirTimer:X}");
            Console.WriteLine($"Hooks.Keyboard: 0x{Offsets.Hooks.Keyboard:X}");
            Console.WriteLine($"Hooks.ControllerR2: 0x{Offsets.Hooks.ControllerR2:X}");
            Console.WriteLine($"Hooks.ControllerL2: 0x{Offsets.Hooks.ControllerL2:X}");
            Console.WriteLine($"Hooks.UpdateCoords: 0x{Offsets.Hooks.UpdateCoords:X}");
            Console.WriteLine($"Hooks.WarpCoords: 0x{Offsets.Hooks.WarpCoords:X}");
            Console.WriteLine($"Hooks.LuaIfElse: 0x{Offsets.Hooks.LuaIfElse:X}");
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

        public int[] DoActScan(IntPtr luaModule, string enemyId)
        {
            const int chunkSize = 4096 * 16;
            const int maxMatches = 2;
            const int maxReadSize = 50002;

            byte[] actAobScan = Encoding.ASCII.GetBytes(enemyId)
                .Concat(Encoding.ASCII.GetBytes("Battle_Activate"))
                .ToArray();

            IntPtr currentAddress = luaModule;
            IntPtr endAddress = IntPtr.Add(currentAddress, 0x400000);

            List<int> bestActs = new List<int>();
            bool bestIsOrdered = false;
            int matchCount = 0;

            while (currentAddress.ToInt64() < endAddress.ToInt64() && matchCount < maxMatches)
            {
                int bytesRemaining = (int)(endAddress.ToInt64() - currentAddress.ToInt64());
                int bytesToRead = Math.Min(bytesRemaining, chunkSize);

                if (bytesToRead < actAobScan.Length) break;

                byte[] buffer;
                try
                {
                    buffer = _memoryIo.ReadBytes(currentAddress, bytesToRead);
                }
                catch (Exception ex)
                {
                    currentAddress = IntPtr.Add(currentAddress, bytesToRead - actAobScan.Length + 1);
                    continue;
                }

                for (int i = 0; i <= bytesToRead - actAobScan.Length; i++)
                {
                    if (!IsPatternMatch(buffer, i, actAobScan)) continue;


                    IntPtr foundAddress = IntPtr.Add(currentAddress, i);

                    try
                    {
                        var acts = ParseActsFromMemory(foundAddress, enemyId, maxReadSize);

                        if (acts.Count > 0)
                        {
                            bool isOrdered = IsOrdered(acts);

                            if (acts.Count > bestActs.Count ||
                                (acts.Count == bestActs.Count && isOrdered && !bestIsOrdered))
                            {
                                bestActs = acts;
                                bestIsOrdered = isOrdered;
                            }
                        }

                        if (++matchCount >= maxMatches) break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading result data: {ex.Message}");
                    }
                }

                currentAddress = IntPtr.Add(currentAddress, bytesToRead - actAobScan.Length + 1);
            }
            Console.WriteLine("AOB scan pattern bytes:");
            for (int i = 0; i < actAobScan.Length; i++)
            {
                Console.Write($"{actAobScan[i]:X2} ");
                if ((i + 1) % 16 == 0) Console.WriteLine(); // New line every 16 bytes for readability
            }
            
            Console.WriteLine("Best acts content:");
            for (int i = 0; i < bestActs.Count; i++)
            {
                Console.WriteLine($"  Index {i}: {bestActs[i]}");
            }
            
            if (bestActs.Count == 0)
            {
                Console.WriteLine($"Best acts empty, initiating fallback scan. Current best acts: [{string.Join(", ", bestActs)}]");
    
                var scriptModuleStart = _memoryIo.GetModuleStart((IntPtr)_memoryIo.ReadUInt64(Offsets.WorldAiMan.Base));
                Console.WriteLine($"Script module address: 0x{scriptModuleStart.ToInt64():X}");
    
                var scanEndAddress = IntPtr.Add(scriptModuleStart, 79000000);
                Console.WriteLine($"Scan end address: 0x{scanEndAddress.ToInt64():X}");

                IntPtr currentScanAddr = scriptModuleStart;
                List<int> fallbackActs = new List<int>();
                bool foundMatch = false;

                while (currentScanAddr.ToInt64() < scanEndAddress.ToInt64() && !foundMatch)
                {
                    int bytesRemaining = (int)(scanEndAddress.ToInt64() - currentScanAddr.ToInt64());
                    int bytesToRead = Math.Min(bytesRemaining, chunkSize);

                    Console.WriteLine(
                        $"Scanning fallback location at 0x{currentScanAddr.ToInt64():X} - remaining bytes: {bytesRemaining}");

                    if (bytesToRead < actAobScan.Length)
                    {
                        Console.WriteLine("Bytes to read less than pattern length, ending scan");
                        break;
                    }

                    byte[] buffer;
                    try
                    {
                        buffer = _memoryIo.ReadBytes(currentScanAddr, bytesToRead);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading memory at 0x{currentScanAddr.ToInt64():X}: {ex.Message}");
                        currentScanAddr = IntPtr.Add(currentScanAddr, bytesToRead - actAobScan.Length + 1);
                        continue;
                    }

                    for (int i = 0; i <= bytesToRead - actAobScan.Length; i++)
                    {
                        if (!IsPatternMatch(buffer, i, actAobScan)) continue;

                        IntPtr foundAddress = IntPtr.Add(currentScanAddr, i);
                        Console.WriteLine(
                            $"Pattern match found at address 0x{foundAddress.ToInt64():X}, reading 4000 bytes");

                        try
                        {
                            byte[] resultBuffer = _memoryIo.ReadBytes(foundAddress, 4000);
                            string content = Encoding.ASCII.GetString(resultBuffer);

                            Console.WriteLine("Parsing content with regex for Act patterns");
                            var matches = Regex.Matches(content, @"Act(\d+)Per");
                            Console.WriteLine($"Found {matches.Count} regex matches");

                            int lastNum = -1;

                            foreach (Match match in matches)
                            {
                                int actNum = int.Parse(match.Groups[1].Value);
                                Console.WriteLine($"Found Act{actNum}Per pattern");

                                if (lastNum != -1 && actNum <= lastNum)
                                {
                                    Console.WriteLine(
                                        $"Unordered sequence detected (previous: {lastNum}, current: {actNum}), stopping collection");
                                    break;
                                }

                                fallbackActs.Add(actNum);
                                lastNum = actNum;
                                Console.WriteLine(
                                    $"Added Act {actNum} to fallback list, total count: {fallbackActs.Count}");
                            }

                            if (fallbackActs.Count > 0)
                            {
                                Console.WriteLine(
                                    $"Successfully found {fallbackActs.Count} ordered acts in fallback scan");
                                foundMatch = true;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("No valid act sequences found at this location");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"Error reading fallback data at 0x{foundAddress.ToInt64():X}: {ex.Message}");
                        }
                    }

                    currentScanAddr = IntPtr.Add(currentScanAddr, bytesToRead - actAobScan.Length + 1);
                    Console.WriteLine($"Moving to next chunk at 0x{currentScanAddr.ToInt64():X}");
                }

                if (fallbackActs.Count > 0)
                {
                    Console.WriteLine($"Using fallback acts: {string.Join(", ", fallbackActs)}");
                }
                else
                {
                    Console.WriteLine("No fallback acts found in secondary scan");
                }

                bestActs = fallbackActs;
            }

            return bestActs.ToArray();
        }

        private bool IsPatternMatch(byte[] buffer, int startIndex, byte[] pattern)
        {
            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[startIndex + j] != pattern[j]) return false;
            }

            return true;
        }

        private List<int> ParseActsFromMemory(IntPtr address, string enemyId, int readSize)
        {
            string result = Encoding.ASCII.GetString(_memoryIo.ReadBytes(address, readSize));
            var pattern = $@"{enemyId}_Act(\d+)";
            return Regex.Matches(result, pattern)
                .Cast<Match>()
                .Select(m => int.Parse(m.Groups[1].Value))
                .Distinct()
                .ToList();
        }

        bool IsOrdered(List<int> list)
        {
            for (int i = 1; i < list.Count; i++)
                if (list[i] < list[i - 1])
                    return false;
            return true;
        }
    }
}