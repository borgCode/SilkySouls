using System;
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

        public void RepeatAct(int actLabelIndex, int lastAct)
        {
            var actManipCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.ActManipCode;
            var opcodeCheckCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.OpcodeCheckCode;
            var hookLoc = Offsets.Hooks.LuaIfElse;
            var opcodeHookLoc = Offsets.Hooks.LuaOpcodeSwitch;
            var battleActivateHook = Offsets.Hooks.BattleActivate;
            
            if (actLabelIndex == 0)
            {
                _hookManager.UninstallHook(actManipCode.ToInt64());
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
            
            var enemyIdLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdV2;
            var enemyIdLengthPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdLen;
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

            var desiredActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.DesiredAct;
            var lastActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LastAct;
            _memoryIo.WriteInt32(desiredActLoc, actLabelIndex - 1);
            _memoryIo.WriteInt32(lastActLoc, lastAct - 1);

            var repeatActFlagLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.RepeatActFlagLoc;
            var enemySavedPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemySavedPtr;
            var enemyIdCheckLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdCheckCode;
            var opcodeHistoryLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.OpcodeHistory;
            var originalCallOffset = Offsets.Hooks.LuaIfElse + 0x906;
            var count = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.Count;
            var flag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.Flag;
            
            if (!_isRepeatActCodeWritten)
            {
                
                byte[] enemyIdCheckBytes = AsmLoader.GetAsmBytes("RepeatActIdCheck");
                AsmHelper.WriteRelativeOffsets(enemyIdCheckBytes, new[] { 
                    (enemyIdCheckLoc.ToInt64() + 0x4B, enemyIdLoc.ToInt64(), 7, 0x4B + 3),
                    (enemyIdCheckLoc.ToInt64() + 0x52, enemyIdLengthPtr.ToInt64(), 7, 0x52 + 3),
                    (enemyIdCheckLoc.ToInt64() + 0x70, enemySavedPtr.ToInt64(), 7, 0x70 + 3)
                });
                
                Byte[] bytes = BitConverter.GetBytes((int)battleActivateHook + 8 - (enemyIdCheckLoc.ToInt64() + 0x8B));
                Array.Copy(bytes, 0, enemyIdCheckBytes, 0x86 + 1, 4);
                _memoryIo.WriteBytes(enemyIdCheckLoc, enemyIdCheckBytes);
                
                byte[] opcodeCheckBytes = AsmLoader.GetAsmBytes("RepeatActFlagSet");
                AsmHelper.WriteRelativeOffsets(opcodeCheckBytes, new[]
                {
                    (opcodeCheckCode.ToInt64(), repeatActFlagLoc.ToInt64(), 7, 0x2),
                    (opcodeCheckCode.ToInt64() + 0xE, repeatActFlagLoc.ToInt64(), 7, 0xE + 2),
                    (opcodeCheckCode.ToInt64() + 0x16, opcodeHistoryLoc.ToInt64() + 0x4, 6, 0x16 + 2),
                    (opcodeCheckCode.ToInt64() + 0x1C, opcodeHistoryLoc.ToInt64(), 6, 0x1C + 2),
                    (opcodeCheckCode.ToInt64() + 0x22, opcodeHistoryLoc.ToInt64() + 0x8, 6, 0x22 + 2),
                    (opcodeCheckCode.ToInt64() + 0x28, opcodeHistoryLoc.ToInt64() + 0x4, 6, 0x28 + 2),
                    (opcodeCheckCode.ToInt64() + 0x2E, opcodeHistoryLoc.ToInt64() + 0x1C, 6, 0x2E + 2),
                    (opcodeCheckCode.ToInt64() + 0x34, opcodeHistoryLoc.ToInt64() + 0x8, 6, 0x34 + 2),
                    (opcodeCheckCode.ToInt64() + 0x3A, opcodeHistoryLoc.ToInt64() + 0x1C, 6, 0x3A + 2),
                    (opcodeCheckCode.ToInt64() + 0x40, opcodeHistoryLoc.ToInt64(), 6, 0x40 + 2),      // history[1]
                    (opcodeCheckCode.ToInt64() + 0x4B, opcodeHistoryLoc.ToInt64() + 0x4, 6, 0x4B + 2),// history[2]
                    (opcodeCheckCode.ToInt64() + 0x56, opcodeHistoryLoc.ToInt64() + 0x8, 6, 0x56 + 2),// history[3]
                    (opcodeCheckCode.ToInt64() + 0x61, opcodeHistoryLoc.ToInt64() + 0x1C, 6, 0x61 + 2),// history[4]
                    (opcodeCheckCode.ToInt64() + 0x6C, repeatActFlagLoc.ToInt64(), 7, 0x6C + 2)
                });
                
                bytes = BitConverter.GetBytes((int)opcodeHookLoc + 7 - (opcodeCheckCode.ToInt64() + 0x80));
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x7B + 1, 4);

                _memoryIo.WriteBytes(opcodeCheckCode, opcodeCheckBytes);
                
                byte[] repeatActBytes = AsmLoader.GetAsmBytes("RepeatAct");
                AsmHelper.WriteRelativeOffsets(repeatActBytes, new[]
                {
                    (actManipCode.ToInt64(), repeatActFlagLoc.ToInt64(), 7, 0x2),
                    (actManipCode.ToInt64() + 0xA, enemySavedPtr.ToInt64(), 7, 0xA + 3),
                    (actManipCode.ToInt64() + 0x17, originalCallOffset, 5, 0x17 + 1),
                    (actManipCode.ToInt64() + 0x1E, count.ToInt64(), 6, 0x1E + 2),
                    (actManipCode.ToInt64() + 0x24, desiredActLoc.ToInt64(), 6, 0x24 + 2),
                    (actManipCode.ToInt64() + 0x2E, flag.ToInt64(), 7, 0x2E + 2),
                    (actManipCode.ToInt64() + 0x37, count.ToInt64(), 6, 0x37 + 2),
                    (actManipCode.ToInt64() + 0x3D, flag.ToInt64(), 6, 0x3D + 2),
                    (actManipCode.ToInt64() + 0x45, flag.ToInt64(), 7, 0x45 + 2),
                    (actManipCode.ToInt64() + 0x55, lastActLoc.ToInt64(), 6, 0x55 + 2),
                    (actManipCode.ToInt64() + 0x5F, count.ToInt64(), 6, 0x5F + 2),
                    (actManipCode.ToInt64() + 0x65, flag.ToInt64(), 6, 0x65 + 2),
                    (actManipCode.ToInt64() + 0x75, originalCallOffset, 5, 0x75 + 1)
                });
                
                var hookJumpOffsets = new[]
                {
                    (0x53, 0x4E + 1),
                    (0x74, 0x6F + 1),
                    (0x82, 0x7D + 1)
                };

                foreach (var (target, offset) in hookJumpOffsets)
                {
                    var jumpOffset = BitConverter.GetBytes((int)hookLoc + 8 - (actManipCode.ToInt64() + target));
                    Array.Copy(jumpOffset, 0, repeatActBytes, offset, 4);
                }

                _memoryIo.WriteBytes(actManipCode, repeatActBytes);
                _isRepeatActCodeWritten = true;
            }

            if (_isRepeatActHookInstalled) return;
            _hookManager.InstallHook(enemyIdCheckLoc.ToInt64(), battleActivateHook,
                new byte[] { 0x48, 0x8B, 0x45, 0x18, 0x48, 0x2B, 0x45, 0x10 });
            _hookManager.InstallHook(opcodeCheckCode.ToInt64(), opcodeHookLoc,
                new byte[] { 0x44, 0x8B, 0xF8, 0x4F, 0x8D, 0x34, 0xEC });
            _hookManager.InstallHook(actManipCode.ToInt64(), hookLoc,
                new byte[] { 0xE8, 0x01, 0x09, 0x00, 0x00, 0x44, 0x39, 0xF8 });
            _isRepeatActHookInstalled = true;
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
            _hookManager.UninstallHook(_lastTargetBlock.ToInt64());
            var allNoDeathPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.AllNoDeath;
            _memoryIo.WriteInt32(allNoDeathPtr, value);
        }

        public void ToggleAi(int value)
        {
            var disableAiPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.DisableAi;
            _memoryIo.WriteInt32(disableAiPtr, value);
        }
    }
}