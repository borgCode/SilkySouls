using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
            if (actLabelIndex == 0)
            {
                _hookManager.UninstallHook(actManipCode.ToInt64());
                _hasWrittenEnemyId = false;
                _isRepeatActHookInstalled = false;
                return;
            }

            var forceAct = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.LockedTarget.ForceActPtr,
                    Offsets.ForceActOffset
                }, false);
            _memoryIo.WriteByte(forceAct, actLabelIndex);
            
            var enemyIdLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyId;
            var enemyIdLocV2 = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdV2;
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

                _memoryIo.WriteDouble(enemyIdLoc, _memoryIo.ReadInt32(enemyBattleIdPtr));
                
                string enemyId = _memoryIo.ReadInt32(enemyBattleIdPtr).ToString();
                byte[] enemyIdBytes = Encoding.ASCII.GetBytes(enemyId);

                _memoryIo.WriteBytes(enemyIdLocV2, enemyIdBytes); 
                _memoryIo.WriteInt32(enemyIdLengthPtr, enemyIdBytes.Length);
                _hasWrittenEnemyId = true;
            }

            var desiredActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.DesiredAct;
            _memoryIo.WriteInt32(desiredActLoc, actLabelIndex - 1);
            var lastActLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.LastAct;
            _memoryIo.WriteInt32(lastActLoc, lastAct - 1);

            var repeatActFlagLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.RepeatActFlagLoc;

            var hookLoc = Offsets.Hooks.LuaIfElse;
            var opcodeHookLoc = 0x140DD358F;
         
            //TODO Pattern scan


            // Pattern 44 8B F8 4F

            var enemyIdHookLoc = 0x140DC83C0;
            // E8 ?? ?? ?? ?? BA EF D8 FF FF 48 8B CB E8 ?? ?? ?? ?? 48 8B 45

            var enemySavedPtr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemySavedPtr;
            var enemyIdCheckLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.EnemyIdCheckCode;
            //TODO pattern scan
            if (!_isRepeatActCodeWritten)
            {
                
                byte[] enemyIdCheckBytes = AsmLoader.GetAsmBytes("RepeatActIdCheck");
                byte[] bytes = AsmHelper.GetRelOffsetBytes(enemyIdCheckLoc + 0x4B, enemyIdLocV2, 7);
                Array.Copy(bytes, 0, enemyIdCheckBytes, 0x4B + 3, 4);
                bytes = AsmHelper.GetRelOffsetBytes(enemyIdCheckLoc + 0x52, enemyIdLengthPtr, 7);
                Array.Copy(bytes, 0, enemyIdCheckBytes, 0x52 + 3, 4);
                bytes = AsmHelper.GetRelOffsetBytes(enemyIdCheckLoc + 0x70, enemySavedPtr, 7);
                Array.Copy(bytes, 0, enemyIdCheckBytes, 0x70 + 3, 4);
                bytes = BitConverter.GetBytes((int)enemyIdHookLoc + 8 - (enemyIdCheckLoc.ToInt64() + 0x8B));
                Array.Copy(bytes, 0, enemyIdCheckBytes, 0x86 + 1, 4);

                _memoryIo.WriteBytes(enemyIdCheckLoc, enemyIdCheckBytes);
                
                var opcodeHistoryLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.OpcodeHistory;

                byte[] opcodeCheckBytes = AsmLoader.GetAsmBytes("RepeatActFlagSet");
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode, repeatActFlagLoc, 7);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x2, 4);
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0xE, repeatActFlagLoc, 7);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0xE + 2, 4);

                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x16, opcodeHistoryLoc + 0x4, 6);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x16 + 2, 4);
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x1C, opcodeHistoryLoc, 6);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x1C + 2, 4);

                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x22, opcodeHistoryLoc + 0x8, 6);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x22 + 2, 4);
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x28, opcodeHistoryLoc + 0x4, 6);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x28 + 2, 4);

                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x2E, opcodeHistoryLoc + 0x1C, 6);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x2E + 2, 4);
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x34, opcodeHistoryLoc + 0x8, 6);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x34 + 2, 4);

                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x3A, opcodeHistoryLoc + 0x1C, 6);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x3A + 2, 4);

                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x40, opcodeHistoryLoc, 6); // history[1]
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x40 + 2, 4);
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x4B, opcodeHistoryLoc + 0x4, 6); // history[2]
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x4B + 2, 4);
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x56, opcodeHistoryLoc + 0x8, 6); // history[3]
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x56 + 2, 4);
                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x61, opcodeHistoryLoc + 0x1C, 6); // history[4]
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x61 + 2, 4);

                bytes = AsmHelper.GetRelOffsetBytes(opcodeCheckCode + 0x6C, repeatActFlagLoc, 7);
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x6C + 2, 4);

                bytes = BitConverter.GetBytes((int)opcodeHookLoc + 7 - (opcodeCheckCode.ToInt64() + 0x80));
                Array.Copy(bytes, 0, opcodeCheckBytes, 0x7B + 1, 4);

                _memoryIo.WriteBytes(opcodeCheckCode, opcodeCheckBytes);


                var originalCallOffset = Offsets.Hooks.LuaIfElse + 0x906;
                var count = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.Count;
                var flag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.Flag;

                byte[] repeatActBytes = AsmLoader.GetAsmBytes("RepeatAct");
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode, repeatActFlagLoc, 7);
                Array.Copy(bytes, 0, repeatActBytes, 0x2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0xA, enemySavedPtr, 7);
                Array.Copy(bytes, 0, repeatActBytes, 0xA + 3, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode.ToInt64() + 0x17, originalCallOffset, 5);
                Array.Copy(bytes, 0, repeatActBytes, 0x17 + 1, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x1E, count, 6);
                Array.Copy(bytes, 0, repeatActBytes, 0x1E + 2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x24, desiredActLoc, 6);
                Array.Copy(bytes, 0, repeatActBytes, 0x24 + 2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x2E, flag, 7);
                Array.Copy(bytes, 0, repeatActBytes, 0x2E + 2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x37, count, 6);
                Array.Copy(bytes, 0, repeatActBytes, 0x37 + 2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x3D, flag, 6);
                Array.Copy(bytes, 0, repeatActBytes, 0x3D + 2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x45, flag, 7);
                Array.Copy(bytes, 0, repeatActBytes, 0x45 + 2, 4);
                
                bytes = BitConverter.GetBytes((int)hookLoc + 8 - (actManipCode.ToInt64() + 0x53));
                Array.Copy(bytes, 0, repeatActBytes, 0x4E + 1, 4);
               
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x55, lastActLoc, 6);
                Array.Copy(bytes, 0, repeatActBytes, 0x55 + 2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x5F, count, 6);
                Array.Copy(bytes, 0, repeatActBytes, 0x5F + 2, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode + 0x65, flag, 6);
                Array.Copy(bytes, 0, repeatActBytes, 0x65 + 2, 4);
                
                bytes = BitConverter.GetBytes((int)hookLoc + 8 - (actManipCode.ToInt64() + 0x74));
                Array.Copy(bytes, 0, repeatActBytes, 0x6F + 1, 4);
                
                bytes = AsmHelper.GetRelOffsetBytes(actManipCode.ToInt64() + 0x75, originalCallOffset, 5);
                Array.Copy(bytes, 0, repeatActBytes, 0x75 + 1, 4);
                
                bytes = BitConverter.GetBytes((int)hookLoc + 8 - (actManipCode.ToInt64() + 0x82));
                Array.Copy(bytes, 0, repeatActBytes, 0x7D + 1, 4);

                _memoryIo.WriteBytes(actManipCode, repeatActBytes);
                _isRepeatActCodeWritten = true;
            }

            if (_isRepeatActHookInstalled) return;
            _hookManager.InstallHook(enemyIdCheckLoc.ToInt64(), enemyIdHookLoc,
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