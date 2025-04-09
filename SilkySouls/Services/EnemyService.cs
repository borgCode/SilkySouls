using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SilkySouls.memory;
using SilkySouls.Memory;
using SilkySouls.Utilities;

namespace SilkySouls.Services
{
    public class EnemyService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        private readonly AoBScanner _aoBScanner;

        private IntPtr _lockedTargetPtr;
        private IntPtr _lastTargetBlock;

        private bool _isHookInstalled;

        private long _lockedTargetOrigin;
        private readonly byte[] _lockedTargetOriginBytes = { 0x48, 0x8D, 0x54, 0x24, 0x38 };
        private bool _isRepeatActCodeWritten;
        private bool _hasWrittenEnemyId;
        private bool _isRepeatActHookInstalled;
        private List<long> _repeatActHooks = new List<long>();

        public EnemyService(MemoryIo memoryIo, HookManager hookManager, AoBScanner aobScanner)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
            _aoBScanner = aobScanner;
        }

        internal void TryInstallTargetHook()
        {
            if (_isHookInstalled) return;
            if (IsHookInstalled()) return;
            if (!IsTargetOriginInitialized()) return;

            _lockedTargetPtr = CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr;
            _lastTargetBlock = CodeCaveOffsets.Base + CodeCaveOffsets.LockedTarget;

            byte[] lockedTargetBytes = AsmLoader.GetAsmBytes("LastLockedTarget");


            byte[] bytes = BitConverter.GetBytes(Offsets.WorldChrMan.Base.ToInt64());
            Array.Copy(bytes, 0, lockedTargetBytes, 4, bytes.Length);

            bytes = BitConverter.GetBytes(13); // Jump to exit if player
            Array.Copy(bytes, 0, lockedTargetBytes, 21, bytes.Length);
            bytes = BitConverter.GetBytes(_lockedTargetPtr.ToInt64());
            Array.Copy(bytes, 0, lockedTargetBytes, 30, bytes.Length);

            int originOffset = (int)(_lockedTargetOrigin + 5 - (_lastTargetBlock.ToInt64() + 50));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, lockedTargetBytes, 46, bytes.Length);

            _memoryIo.WriteBytes(_lastTargetBlock, lockedTargetBytes);
            _hookManager.InstallHook(_lastTargetBlock.ToInt64(), _lockedTargetOrigin, _lockedTargetOriginBytes);
            _isHookInstalled = true;
        }

        internal void ResetHooks()
        {
            _isHookInstalled = false;
        }

        private bool IsHookInstalled()
        {
            byte[] codeCaveBytes = _memoryIo.ReadBytes(_lastTargetBlock, 2);
            byte[] expectedSignature = { 0x50, 0x51 };
            if (codeCaveBytes.SequenceEqual(expectedSignature))
            {
                _isHookInstalled = true;
                return true;
            }

            return false;
        }

        private bool IsTargetOriginInitialized()
        {
            _lockedTargetOrigin = Offsets.Hooks.LastLockedTarget;
            var originBytes = _memoryIo.ReadBytes((IntPtr)_lockedTargetOrigin, _lockedTargetOriginBytes.Length);
            return originBytes.SequenceEqual(_lockedTargetOriginBytes);
        }

        public int GetTargetHp()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetHp);
        }

        public int GetTargetMaxHp()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetMaxHp);
        }

        public void SetTargetHp(int value)
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            _memoryIo.WriteInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetHp, value);
        }

        public float GetTargetPoise()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.CurrentPoise);
        }

        public float GetTargetMaxPoise()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.MaxPoise);
        }

        public float GetTargetPoiseTimer()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.PoiseTimer);
        }

        public int GetImmunitySpEffect()
        {
            var spEffectPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.LockedTarget.NpcSpEffectEquipCtrl, Offsets.SpEffectPtr1, Offsets.SpEffectPtr2,
                    Offsets.SpEffectOffset
                }, false);

            return _memoryIo.ReadInt32(spEffectPtr);
        }

        public int GetTargetBleed()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.BleedCurrent);
        }

        public int GetTargetMaxBleed()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.BleedMax);
        }

        public int GetTargetPoison()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.PoisonCurrent);
        }

        public int GetTargetMaxPoison()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.PoisonMax);
        }

        public int GetTargetToxic()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.ToxicCurrent);
        }

        public int GetTargetMaxToxic()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                                       CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.ToxicMax);
        }

        public ulong GetTargetId()
        {
            return _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                        CodeCaveOffsets.LockedTargetPtr);
        }

        public float[] GetTargetPos()
        {
            var targetPosPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base +
                                                        CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.LockedTarget.Coords
                }, false);

            float[] position = new float[3];

            position[0] = _memoryIo.ReadFloat(targetPosPtr);
            position[1] = _memoryIo.ReadFloat(targetPosPtr + 0x4);
            position[2] = _memoryIo.ReadFloat(targetPosPtr + 0x8);

            return position;
        }

        public void SetTargetSpeed(float value)
        {
            var lockedTargetBase = CodeCaveOffsets.Base +
                                   CodeCaveOffsets.LockedTargetPtr;
            var targetSpeedPtr = _memoryIo.FollowPointers(lockedTargetBase,
                new[]
                {
                    (int)Offsets.LockedTarget.EnemyCtrl, Offsets.WorldChrMan.ChrAnim, Offsets.WorldChrMan.ChrAnimSpeed
                }, false);

            _memoryIo.WriteFloat(targetSpeedPtr, value);
        }

        public float GetTargetSpeed()
        {
            var lockedTargetBase = CodeCaveOffsets.Base +
                                   CodeCaveOffsets.LockedTargetPtr;
            var targetSpeedPtr = _memoryIo.FollowPointers(lockedTargetBase,
                new[]
                {
                    (int)Offsets.LockedTarget.EnemyCtrl, Offsets.WorldChrMan.ChrAnim, Offsets.WorldChrMan.ChrAnimSpeed
                }, false);

            return _memoryIo.ReadFloat(targetSpeedPtr);
        }

        public void ToggleTargetAi(bool setValue)
        {
            var disableTargetAiPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base +
                                                              CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);
            var flagMask = (byte)Offsets.WorldChrMan.ChrFlags.NoUpdate;
            _memoryIo.SetBitValue(disableTargetAiPtr, flagMask, setValue);
        }

        public bool IsTargetAiDisabled()
        {
            var disableTargetAiPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base +
                                                              CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);
            var flagMask = (byte)Offsets.WorldChrMan.ChrFlags.NoUpdate;
            return _memoryIo.IsBitSet(disableTargetAiPtr, flagMask);
        }

        public void ToggleTargetNoDamage(bool setValue)
        {
            var disableTargetDamagePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base +
                                                                  CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = Offsets.WorldChrMan.NoDamage;
            _memoryIo.SetBitValue(disableTargetDamagePtr, flagMask, setValue);
        }

        public bool IsTargetNoDamageEnabled()
        {
            var disableTargetDamagePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base +
                                                                  CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = Offsets.WorldChrMan.NoDamage;
            return _memoryIo.IsBitSet(disableTargetDamagePtr, flagMask);
        }

        public int[] GetActs()
        {
            var luaModulePtr = _memoryIo.FollowPointers(Offsets.WorldAiMan.Base,
                new[]
                {
                    Offsets.WorldAiMan.DLLuaPtr,
                    Offsets.WorldAiMan.DLLua,
                    Offsets.WorldAiMan.LuaModule
                }, true);

            var enemyBattleIdPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    Offsets.BattleGoalIdPtr1,
                    Offsets.BattleGoalIdPtr2,
                    Offsets.BattleGoalIdOffset
                }, false);

            string enemyId = _memoryIo.ReadInt32(enemyBattleIdPtr).ToString();
            return _aoBScanner.DoActScan(luaModulePtr, enemyId);
        }

        public void RepeatAct(int actLabelIndex, int finalActIndex)
        {
            var ifManipulationCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaIfManipulationCode;
            var luaSwitchCheckCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaSwitchCheckCode;
            var luaIfCaseHook = Offsets.Hooks.LuaIfCase;
            var luaSwitchCaseHook = Offsets.Hooks.LuaSwitchCase;
            var battleActivateHook = Offsets.Hooks.BattleActivate;

            if (actLabelIndex == 0)
            {
                _hookManager.UninstallHook(ifManipulationCode.ToInt64());
                _hasWrittenEnemyId = false;
                _isRepeatActHookInstalled = false;
                return;
            }

            //For enemies with DbgForceAct 
            var forceAct = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.LockedTarget.ForceActPtr,
                    Offsets.ForceActOffset
                }, false);
            _memoryIo.WriteByte(forceAct, actLabelIndex);

            var enemyIdLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyId;
            var enemyIdLengthPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdLength;
            if (!_hasWrittenEnemyId)
            {
                var enemyBattleIdPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                    new[]
                    {
                        Offsets.BattleGoalIdPtr1,
                        Offsets.BattleGoalIdPtr2,
                        Offsets.BattleGoalIdOffset
                    }, false);

                string enemyId = _memoryIo.ReadInt32(enemyBattleIdPtr).ToString();
                byte[] enemyIdBytes = Encoding.ASCII.GetBytes(enemyId);

                _memoryIo.WriteBytes(enemyIdLoc, enemyIdBytes);
                _memoryIo.WriteInt32(enemyIdLengthPtr, enemyIdBytes.Length);
                _hasWrittenEnemyId = true;
            }

            var targetActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.TargetActIndex;
            var finalActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.FinalActIndex;
            _memoryIo.WriteInt32(targetActLoc, actLabelIndex - 1);
            _memoryIo.WriteInt32(finalActLoc, finalActIndex - 1);

            var switchPatternMatchFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaSwitchPatternMatchFlag;
            var enemyRaxIdentifier = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyRaxIdentifier;
            var enemyIdentifierCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdentifierCode;
            var luaSwitchHistory = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaSwitchHistory;
            var originalCallOffset = Offsets.Hooks.LuaIfCase + 0x906;
            var luaIfCounter = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LuaIfCounter;
            var ifConditionFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.IfConditionFlag;

            if (!_isRepeatActCodeWritten)
            {
                byte[] enemyIdCheckBytes = AsmLoader.GetAsmBytes("RepeatActIdCheck");
                AsmHelper.WriteRelativeOffsets(enemyIdCheckBytes, new[]
                {
                    (enemyIdentifierCode.ToInt64() + 0x4B, enemyIdLoc.ToInt64(), 7, 0x4B + 3),
                    (enemyIdentifierCode.ToInt64() + 0x52, enemyIdLengthPtr.ToInt64(), 7, 0x52 + 3),
                    (enemyIdentifierCode.ToInt64() + 0x70, enemyRaxIdentifier.ToInt64(), 7, 0x70 + 3)
                });

                Byte[] bytes = BitConverter.GetBytes((int)battleActivateHook + 8 - (enemyIdentifierCode.ToInt64() + 0x8B));
                Array.Copy(bytes, 0, enemyIdCheckBytes, 0x86 + 1, 4);
                _memoryIo.WriteBytes(enemyIdentifierCode, enemyIdCheckBytes);

                byte[] switchCheckBytes = AsmLoader.GetAsmBytes("RepeatActFlagSet");
                AsmHelper.WriteRelativeOffsets(switchCheckBytes, new[]
                {
                    (luaSwitchCheckCode.ToInt64(), switchPatternMatchFlag.ToInt64(), 7, 0x2),
                    (luaSwitchCheckCode.ToInt64() + 0xE, switchPatternMatchFlag.ToInt64(), 7, 0xE + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x16, luaSwitchHistory.ToInt64() + 0x4, 6, 0x16 + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x1C, luaSwitchHistory.ToInt64(), 6, 0x1C + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x22, luaSwitchHistory.ToInt64() + 0x8, 6, 0x22 + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x28, luaSwitchHistory.ToInt64() + 0x4, 6, 0x28 + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x2E, luaSwitchHistory.ToInt64() + 0xC, 6, 0x2E + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x34, luaSwitchHistory.ToInt64() + 0x8, 6, 0x34 + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x3A, luaSwitchHistory.ToInt64() + 0xC, 6, 0x3A + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x40, luaSwitchHistory.ToInt64(), 6, 0x40 + 2), // history[1]
                    (luaSwitchCheckCode.ToInt64() + 0x4B, luaSwitchHistory.ToInt64() + 0x4, 6, 0x4B + 2), // history[2]
                    (luaSwitchCheckCode.ToInt64() + 0x56, luaSwitchHistory.ToInt64() + 0x8, 6, 0x56 + 2), // history[3]
                    (luaSwitchCheckCode.ToInt64() + 0x61, luaSwitchHistory.ToInt64() + 0xC, 6, 0x61 + 2), // history[4]
                    (luaSwitchCheckCode.ToInt64() + 0x6C, switchPatternMatchFlag.ToInt64(), 7, 0x6C + 2),
                    (luaSwitchCheckCode.ToInt64() + 0x75, luaSwitchHistory.ToInt64() + 0x4, 6, 0x75 + 2), // history[2]
                    (luaSwitchCheckCode.ToInt64() + 0x80, luaSwitchHistory.ToInt64() + 0x8, 6, 0x80 + 2), // history[3]
                    (luaSwitchCheckCode.ToInt64() + 0x8B, luaSwitchHistory.ToInt64() + 0xC, 6, 0x8B + 2), // history[4]
                    (luaSwitchCheckCode.ToInt64() + 0x96, switchPatternMatchFlag.ToInt64(), 7, 0x96 + 2),
                });

                bytes = BitConverter.GetBytes((int)luaSwitchCaseHook + 7 - (luaSwitchCheckCode.ToInt64() + 0xAA));
                Array.Copy(bytes, 0, switchCheckBytes, 0xA5 + 1, 4);

                _memoryIo.WriteBytes(luaSwitchCheckCode, switchCheckBytes);

                byte[] ifManipBytes = AsmLoader.GetAsmBytes("RepeatAct");
                AsmHelper.WriteRelativeOffsets(ifManipBytes, new[]
                {
                    (ifManipulationCode.ToInt64(), switchPatternMatchFlag.ToInt64(), 7, 0x2),
                    (ifManipulationCode.ToInt64() + 0xA, enemyRaxIdentifier.ToInt64(), 7, 0xA + 3),
                    (ifManipulationCode.ToInt64() + 0x17, originalCallOffset, 5, 0x17 + 1),
                    (ifManipulationCode.ToInt64() + 0x1E, luaIfCounter.ToInt64(), 6, 0x1E + 2),
                    (ifManipulationCode.ToInt64() + 0x24, targetActLoc.ToInt64(), 6, 0x24 + 2),
                    (ifManipulationCode.ToInt64() + 0x2E, ifConditionFlag.ToInt64(), 7, 0x2E + 2),
                    (ifManipulationCode.ToInt64() + 0x37, luaIfCounter.ToInt64(), 6, 0x37 + 2),
                    (ifManipulationCode.ToInt64() + 0x3D, ifConditionFlag.ToInt64(), 6, 0x3D + 2),
                    (ifManipulationCode.ToInt64() + 0x45, ifConditionFlag.ToInt64(), 7, 0x45 + 2),
                    (ifManipulationCode.ToInt64() + 0x55, finalActLoc.ToInt64(), 6, 0x55 + 2),
                    (ifManipulationCode.ToInt64() + 0x5F, luaIfCounter.ToInt64(), 6, 0x5F + 2),
                    (ifManipulationCode.ToInt64() + 0x65, ifConditionFlag.ToInt64(), 6, 0x65 + 2),
                    (ifManipulationCode.ToInt64() + 0x75, originalCallOffset, 5, 0x75 + 1)
                });

                var hookJumpOffsets = new[]
                {
                    (0x53, 0x4E + 1),
                    (0x74, 0x6F + 1),
                    (0x82, 0x7D + 1)
                };

                foreach (var (target, offset) in hookJumpOffsets)
                {
                    var jumpOffset = BitConverter.GetBytes((int)luaIfCaseHook + 8 - (ifManipulationCode.ToInt64() + target));
                    Array.Copy(jumpOffset, 0, ifManipBytes, offset, 4);
                }

                _memoryIo.WriteBytes(ifManipulationCode, ifManipBytes);
                _isRepeatActCodeWritten = true;
            }

            if (_isRepeatActHookInstalled) return;
            _repeatActHooks.Add(_hookManager.InstallHook(enemyIdentifierCode.ToInt64(), battleActivateHook,
                new byte[] { 0x48, 0x8B, 0x45, 0x18, 0x48, 0x2B, 0x45, 0x10 }));
            _repeatActHooks.Add(_hookManager.InstallHook(luaSwitchCheckCode.ToInt64(), luaSwitchCaseHook,
                new byte[] { 0x44, 0x8B, 0xF8, 0x4F, 0x8D, 0x34, 0xEC }));
            _repeatActHooks.Add(_hookManager.InstallHook(ifManipulationCode.ToInt64(), luaIfCaseHook,
                    new byte[] { 0xE8, 0x01, 0x09, 0x00, 0x00, 0x41, 0x3B, 0xC7 }));
            _isRepeatActHookInstalled = true;
        }

        public void DisableRepeatAct()
        {
            foreach (var hookAddr in _repeatActHooks)
            {
                _hookManager.UninstallHook(hookAddr);
            }
            _repeatActHooks.Clear();
            _hasWrittenEnemyId = false;
            _isRepeatActCodeWritten = false;
            _isRepeatActHookInstalled = false;
            _memoryIo.WriteBytes(CodeCaveOffsets.Base + (int) CodeCaveOffsets.RepeatAct.TargetActIndex, new byte[780]);
        }

        public int GetCurrentRepeatEnemyId()
        {
            try
            {
                var enemyIdBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyId, 8);
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

        public int GetEnemyBattleId()
        {
            var enemyBattleIdPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    Offsets.BattleGoalIdPtr1,
                    Offsets.BattleGoalIdPtr2,
                    Offsets.BattleGoalIdOffset
                }, false);
            return _memoryIo.ReadInt32(enemyBattleIdPtr);
        }

        public void ToggleAllNoDamage(int value)
        {
            var allNoDamagePtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.AllNoDamage;
            _memoryIo.WriteInt32(allNoDamagePtr, value);

            var codeBlock = CodeCaveOffsets.Base + CodeCaveOffsets.AllNoDamage;
            if (value == 1)
            {
                long origin = Offsets.Hooks.AllNoDamage;

                byte[] restoreHealthBytes = AsmLoader.GetAsmBytes("AllNoDamage");
                byte[] jumpBytes = BitConverter.GetBytes(origin + 7 - (codeBlock.ToInt64() + 26));
                Array.Copy(jumpBytes, 0, restoreHealthBytes, 22, 4);
                _memoryIo.WriteBytes(codeBlock, restoreHealthBytes);
                _hookManager.InstallHook(codeBlock.ToInt64(), origin,
                    new byte[] { 0xF6, 0x81, 0x54, 0x01, 0x00, 0x00, 0x28 });
            }
            else
            {
                _hookManager.UninstallHook(codeBlock.ToInt64());
            }
        }

        public void ToggleAllNoDeath(int value)
        {
            var allNoDeathPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.AllNoDeath;
            _memoryIo.WriteInt32(allNoDeathPtr, value);
        }

        public void ToggleAi(int value)
        {
            var disableAiPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.DisableAi;
            _memoryIo.WriteInt32(disableAiPtr, value);
        }

        public void Toggle4KingsTimer(bool is4KingsTimerStopped)
        {
            var patchLocation = Offsets.FourKingsPatch;
            if (is4KingsTimerStopped) _memoryIo.WriteBytes(patchLocation, new byte[] {0x90, 0x90, 0x90, 0x90, 0x90});
            else _memoryIo.WriteBytes(patchLocation, new byte[]{ 0xF3, 0x0F, 0x11, 0x47, 0x10 });
        }
    }
}