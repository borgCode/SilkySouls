using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkySouls");
            Directory.CreateDirectory(appData);
            string savePath = Path.Combine(appData, "backup_addresses.txt");
            
            Dictionary<string, long> saved = new Dictionary<string, long>();
            if (File.Exists(savePath))
            {
                foreach (string line in File.ReadAllLines(savePath))
                {
                    string[] parts = line.Split('=');
                    saved[parts[0]] = Convert.ToInt64(parts[1], 16);
                }
            }
            
            
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
            Offsets.MenuMan.Base = FindAddressByPattern(Patterns.MenuMan);
            Offsets.EventFlagMan.Base = FindAddressByPattern(Patterns.EventFlagMan);
            Offsets.LevelUpFunc = FindAddressByPattern(Patterns.LevelUpFunc).ToInt64();
            Offsets.RestoreCastsFunc = FindAddressByPattern(Patterns.RestoreCastsFunc).ToInt64();
            Offsets.HgDraw.Base = FindAddressByPattern(Patterns.HgDraw);
            Offsets.WarpEvent = FindAddressByPattern(Patterns.WarpEvent);
            Offsets.WarpFunc = FindAddressByPattern(Patterns.WarpFunc).ToInt64();
            Offsets.SoloParamMan.Base = FindAddressByPattern(Patterns.SoloParamMan);
            Offsets.OpenEnhanceShopWeapon = FindAddressByPattern(Patterns.OpenEnhanceShop).ToInt64();
            Offsets.OpenEnhanceShopArmor = Offsets.OpenEnhanceShopWeapon - 0x40;
            Offsets.WorldAiMan.Base = FindAddressByPattern(Patterns.WorldAiMan);
            Offsets.EmkEventIns.Base = FindAddressByPattern(Patterns.EmkEventIns);
            Offsets.DebugEventMan.Base = FindAddressByPattern(Patterns.DebugEventMan);

            // Hooks
            TryPatternWithFallback("LastLockedTarget", Patterns.LastLockedTarget,
                addr => Offsets.Hooks.LastLockedTarget = addr.ToInt64(), saved);
            TryPatternWithFallback("AllNoDamage", Patterns.AllNoDamage,
                addr => Offsets.Hooks.AllNoDamage = addr.ToInt64(), saved);
            TryPatternWithFallback("ItemSpawn", Patterns.ItemSpawnHook,
                addr => Offsets.Hooks.ItemSpawn = addr.ToInt64(), saved);
            TryPatternWithFallback("Draw", Patterns.DrawHook, addr => Offsets.Hooks.Draw = addr.ToInt64(), saved);
            TryPatternWithFallback("TargetingView", Patterns.TargetingView,
                addr => Offsets.Hooks.TargetingView = addr.ToInt64(), saved);
            TryPatternWithFallback("InAirTimer", Patterns.InAirTimer, addr => Offsets.Hooks.InAirTimer = addr.ToInt64(),
                saved);
            TryPatternWithFallback("Keyboard", Patterns.Keyboard, addr => Offsets.Hooks.Keyboard = addr.ToInt64(),
                saved);
            TryPatternWithFallback("ControllerR2", Patterns.ControllerR2,
                addr => Offsets.Hooks.ControllerR2 = addr.ToInt64(), saved);
            TryPatternWithFallback("ControllerL2", Patterns.ControllerL2,
                addr => Offsets.Hooks.ControllerL2 = addr.ToInt64(), saved);
            TryPatternWithFallback("UpdateCoords", Patterns.UpdateCoords,
                addr => Offsets.Hooks.UpdateCoords = addr.ToInt64(), saved);
            TryPatternWithFallback("WarpCoords", Patterns.WarpCoords, addr => Offsets.Hooks.WarpCoords = addr.ToInt64(),
                saved);
            TryPatternWithFallback("LuaIfCase", Patterns.LuaIfElseHook,
                addr => Offsets.Hooks.LuaIfCase = addr.ToInt64(), saved);
            TryPatternWithFallback("LuaSwitchCase", Patterns.LuaOpCodeSwitch,
                addr => Offsets.Hooks.LuaSwitchCase = addr.ToInt64(), saved);
            TryPatternWithFallback("BattleActivate", Patterns.BattleActivateHook,
                addr => Offsets.Hooks.BattleActivate = addr.ToInt64(), saved); 
            TryPatternWithFallback("Emevd", Patterns.EmevdCommandHook,
                addr => Offsets.Hooks.Emevd = addr.ToInt64(), saved);

// Patches
            TryPatternWithFallback("FourKingsPatch", Patterns.FourKingsPatch,
                addr => Offsets.Patches.FourKingsPatch = addr, saved);
            TryPatternWithFallback("NoRollPatch", Patterns.NoRollPatch, addr => Offsets.Patches.NoRollPatch = addr,
                saved);
            TryPatternWithFallback("InfiniteDurabilityPatch", Patterns.InfiniteDurabilityPatch,
                addr => Offsets.Patches.InfiniteDurabilityPatch = addr, saved);
            TryPatternWithFallback("DrawEventPatch", Patterns.DrawEventPatch,
                addr => Offsets.Patches.DrawEventPatch = addr, saved);
            TryPatternWithFallback("DrawSoundViewPatch", Patterns.DrawSoundViewPatch,
                addr => Offsets.Patches.DrawSoundViewPatch = addr, saved);
            TryPatternWithFallback("QuitoutPatch", Patterns.QuitoutPatch, addr => Offsets.Patches.QuitoutPatch = addr,
                saved);
            
            using (var writer = new StreamWriter(savePath))
            {
                foreach (var pair in saved)
                    writer.WriteLine($"{pair.Key}={pair.Value:X}");
            }
            
            
            Offsets.Funcs.SetEvent = FindAddressByPattern(Patterns.SetEvent).ToInt64();
            Offsets.Funcs.ShopParamSave = FindAddressByPattern(Patterns.ShopParamSave).ToInt64();
            Offsets.Funcs.OpenRegularShop = FindAddressByPattern(Patterns.OpenRegularShop).ToInt64();
            Offsets.Funcs.ProcessEmevdCommand = FindAddressByPattern(Patterns.ProcessEmevdCommand).ToInt64();
            Offsets.Funcs.OpenAttunement = FindAddressByPattern(Patterns.OpenAttunement).ToInt64();
            Offsets.Funcs.AttunementWindowPrep = FindAddressByPattern(Patterns.AttunementWindowPrep).ToInt64();
            
            
            #if DEBUG
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
            Console.WriteLine($"DrawEventPatch: 0x{Offsets.Patches.DrawEventPatch.ToInt64():X}");
            Console.WriteLine($"DrawSoundViewPatch: 0x{Offsets.Patches.DrawSoundViewPatch.ToInt64():X}");
            Console.WriteLine($"MenuMan.Base: 0x{Offsets.MenuMan.Base.ToInt64():X}");
            Console.WriteLine($"EventFlagMan.Base: 0x{Offsets.EventFlagMan.Base.ToInt64():X}");
            Console.WriteLine($"LevelUpFunc: 0x{Offsets.LevelUpFunc:X}");
            Console.WriteLine($"RestoreCastsFunc: 0x{Offsets.RestoreCastsFunc:X}");
            Console.WriteLine($"HgDraw.Base: 0x{Offsets.HgDraw.Base.ToInt64():X}");
            Console.WriteLine($"WarpEvent: 0x{Offsets.WarpEvent.ToInt64():X}");
            Console.WriteLine($"WarpFunc: 0x{Offsets.WarpFunc:X}");
            Console.WriteLine($"FastQuitout: 0x{Offsets.Patches.QuitoutPatch.ToInt64():X}");
            Console.WriteLine($"WorldAiMan: 0x{Offsets.WorldAiMan.Base.ToInt64():X}");
            Console.WriteLine($"EmkEventIns: 0x{Offsets.EmkEventIns.Base.ToInt64():X}");
            Console.WriteLine($"DebugEventMan: 0x{Offsets.DebugEventMan.Base.ToInt64():X}");
            
            Console.WriteLine($"Weapon: 0x{Offsets.OpenEnhanceShopWeapon:X}");
            Console.WriteLine($"Armor: 0x{Offsets.OpenEnhanceShopArmor:X}");
            
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
            Console.WriteLine($"Hooks.LuaIfElse: 0x{Offsets.Hooks.LuaIfCase:X}");
            Console.WriteLine($"Hooks.Emevd: 0x{Offsets.Hooks.Emevd:X}");
            
            Console.WriteLine($"Funcs.SetEvent: 0x{Offsets.Funcs.SetEvent:X}");
            Console.WriteLine($"Funcs.ShopParamSave: 0x{Offsets.Funcs.ShopParamSave:X}");
            Console.WriteLine($"Funcs.OpenRegularShop: 0x{Offsets.Funcs.OpenRegularShop:X}");
            Console.WriteLine($"Funcs.ProcessEmevdCommand: 0x{Offsets.Funcs.ProcessEmevdCommand:X}");
            Console.WriteLine($"Funcs.OpenAttunement: 0x{Offsets.Funcs.OpenAttunement:X}");
            Console.WriteLine($"Funcs.AttunementWindowPrep: 0x{Offsets.Funcs.AttunementWindowPrep:X}");
#endif
        }
        
        private void TryPatternWithFallback(string name, Pattern pattern, Action<IntPtr> setter, Dictionary<string, long> saved)
        {
            var addr = FindAddressByPattern(pattern);
    
            if (addr == IntPtr.Zero && saved.TryGetValue(name, out var value))
                addr = new IntPtr(value);
            else if (addr != IntPtr.Zero)
                saved[name] = addr.ToInt64();

            setter(addr);
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

                case RipType.Mov64:
                    // e.g. 48 8B 05/0D - Standard mov rax/rcx,[rip+offset]
                    int stdOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                    return IntPtr.Add(instructionAddress, stdOffset + 7);

                case RipType.Cmp:
                    // e.g. 80 3D - cmp byte ptr [rip+offset],imm
                    int cmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 2));
                    return IntPtr.Add(instructionAddress, cmpOffset + 7);
                case RipType.QwordCmp:
                    int qwordCmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                    return IntPtr.Add(instructionAddress, qwordCmpOffset + 7);
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


            if (bestActs.Count != 0) return bestActs.ToArray();

            var worldAiBase = (IntPtr)_memoryIo.ReadUInt64(Offsets.WorldAiMan.Base);
            var scriptModuleStart = _memoryIo.GetModuleStart(worldAiBase) + 0x8E20000;

            var rangesToScan = new (IntPtr Start, IntPtr End)[]
            {
                (scriptModuleStart, (IntPtr)0x10000000),
                ((IntPtr)0x1A000000, (IntPtr)0x1D000000)
            };
            var cts = new CancellationTokenSource();
            var tasks = rangesToScan.Select((range, index) =>
                ScanMemoryRange(
                    range.Start, range.End, index, actAobScan, chunkSize, enemyId, cts.Token)).ToList();

            try
            {
                while (tasks.Count > 0)
                {
                    var completedIndex = Task.WaitAny(tasks.Cast<Task>().ToArray());
                    var completedTask = tasks[completedIndex];
                    var result = completedTask.Result;
                    tasks.Remove(completedTask);

                    if (result.Acts.Count <= 0) continue;
                    bestActs = result.Acts;
                    cts.Cancel();
                    break;
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
            }


            return bestActs.ToArray();
        }

        private Task<(List<int> Acts, int RegionIndex)> ScanMemoryRange(
            IntPtr rangeStart, IntPtr rangeEnd, int regionIndex, byte[] actAobScan, int chunkSize, string enemyId,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var fallbackActs = new List<int>();
                IntPtr currentScanAddr = rangeStart;

                while (currentScanAddr.ToInt64() < rangeEnd.ToInt64() && !cancellationToken.IsCancellationRequested)
                {
                    int bytesRemaining = (int)(rangeEnd.ToInt64() - currentScanAddr.ToInt64());
                    int bytesToRead = Math.Min(bytesRemaining, chunkSize);

                    if (bytesToRead < actAobScan.Length) break;

                    byte[] buffer;
                    try
                    {
                        buffer = _memoryIo.ReadBytes(currentScanAddr, bytesToRead);
                    }
                    catch
                    {
                        currentScanAddr = IntPtr.Add(currentScanAddr, bytesToRead - actAobScan.Length + 1);
                        continue;
                    }

                    for (int i = 0; i <= bytesToRead - actAobScan.Length; i++)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        if (!IsPatternMatch(buffer, i, actAobScan)) continue;

                        IntPtr foundAddress = IntPtr.Add(currentScanAddr, i);
                        try
                        {
                            byte[] resultBuffer = _memoryIo.ReadBytes(foundAddress, 10000);
                            string content = Encoding.ASCII.GetString(resultBuffer);

                            var matches = Regex.Matches(content, @"Act(\d+)Per");
                            int lastNum = -1;

                            foreach (Match match in matches)
                            {
                                int actNum = int.Parse(match.Groups[1].Value);

                                if (lastNum != -1 && actNum <= lastNum) break;

                                fallbackActs.Add(actNum);
                                lastNum = actNum;
                            }

                            if (fallbackActs.Count > 0)
                            {
                                return (fallbackActs, regionIndex);
                            }

                            var alternativeMatches = Regex.Matches(content, $@"{enemyId}_Act(\d+)");
                            lastNum = -1;

                            foreach (Match match in alternativeMatches)
                            {
                                int actNum = int.Parse(match.Groups[1].Value);

                                if (actNum > 0 && actNum < 1000)
                                {
                                    if (lastNum != -1 && actNum <= lastNum) break;

                                    fallbackActs.Add(actNum);
                                    lastNum = actNum;
                                }
                            }

                            if (fallbackActs.Count > 0)
                            {
                                return (fallbackActs, regionIndex);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"Region {regionIndex}: Error reading fallback data at 0x{foundAddress.ToInt64():X}: {ex.Message}");
                        }
                    }

                    currentScanAddr = IntPtr.Add(currentScanAddr, bytesToRead - actAobScan.Length + 1);
                }

                return (fallbackActs, regionIndex);
            }, cancellationToken);
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