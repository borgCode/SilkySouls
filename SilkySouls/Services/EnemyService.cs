using System;
using System.Linq;
using SilkySouls.memory;
using SilkySouls.Memory;
using SilkySouls.Utilities;

namespace SilkySouls.Services
{
    public class EnemyService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        private readonly IntPtr _codeCave;
        private IntPtr _lastTargetBlock;
        private IntPtr _lockedTargetPtr;

        private bool _isHookInstalled;

        private long _lockedTargetOrigin;
        private readonly byte[] _lockedTargetOriginBytes = { 0x48, 0x8D, 0x54, 0x24, 0x38 };

        public EnemyService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;

            _codeCave = CodeCaveOffsets.CodeCave1.Base;
            _lastTargetBlock = _codeCave + CodeCaveOffsets.CodeCave1.LockedTarget;
        }

        internal void TryInstallTargetHook()
        {
            if (_isHookInstalled) return;
            if (IsHookInstalled()) return;
            if (!IsTargetOriginInitialized()) return;
            
            _lockedTargetPtr = _codeCave + CodeCaveOffsets.CodeCave1.LockedTargetPtr;

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
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetHp);
        }

        public int GetTargetMaxHp()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetMaxHp);
        }

        public void SetTargetHp(int value)
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            _memoryIo.WriteInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetHp, value);
        }

        public float GetTargetPoise()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.CurrentPoise);
        }

        public float GetTargetMaxPoise()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.MaxPoise);
        }

        public float GetTargetPoiseTimer()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.PoiseTimer);
        }

        public int GetImmunitySpEffect()
        {
            var spEffectPtr = _memoryIo.FollowPointers(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.LockedTarget.NpcSpEffectEquipCtrl, Offsets.SpEffectPtr1, Offsets.SpEffectPtr2,
                    Offsets.SpEffectOffset
                }, false);
            
            return _memoryIo.ReadInt32(spEffectPtr);
        }

        public int GetTargetBleed()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.BleedCurrent);
        }

        public int GetTargetMaxBleed()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.BleedMax);
        }

        public int GetTargetPoison()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.PoisonCurrent);
        }

        public int GetTargetMaxPoison()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.PoisonMax);
        }

        public int GetTargetToxic()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.ToxicCurrent);
        }

        public int GetTargetMaxToxic()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.ToxicMax);
        }

        public ulong GetTargetId()
        {
            return _memoryIo.ReadUInt64(CodeCaveOffsets.CodeCave1.Base +
                                        CodeCaveOffsets.CodeCave1.LockedTargetPtr);
        }

        public float[] GetTargetPos()
        {
            var targetPosPtr = _memoryIo.FollowPointers(CodeCaveOffsets.CodeCave1.Base +
                                                        CodeCaveOffsets.CodeCave1.LockedTargetPtr,
                new[]
                {
                    (int) Offsets.LockedTarget.Coords
                }, false);
    
            float[] position = new float[3];
            
            position[0] = _memoryIo.ReadFloat(targetPosPtr);     
            position[1] = _memoryIo.ReadFloat(targetPosPtr + 0x4);  
            position[2] = _memoryIo.ReadFloat(targetPosPtr + 0x8); 
    
            return position;
        }

        public void SetTargetSpeed(float value)
        {
            var lockedTargetBase = CodeCaveOffsets.CodeCave1.Base +
                                   CodeCaveOffsets.CodeCave1.LockedTargetPtr;
            var targetSpeedPtr = _memoryIo.FollowPointers(lockedTargetBase,
                new[]
                {
                    (int)Offsets.LockedTarget.EnemyCtrl, Offsets.WorldChrMan.ChrAnim, Offsets.WorldChrMan.ChrAnimSpeed
                }, false);

            _memoryIo.WriteFloat(targetSpeedPtr, value);
        }

        public float GetTargetSpeed()
        {
            var lockedTargetBase = CodeCaveOffsets.CodeCave1.Base +
                                   CodeCaveOffsets.CodeCave1.LockedTargetPtr;
            var targetSpeedPtr = _memoryIo.FollowPointers(lockedTargetBase,
                new[]
                {
                    (int)Offsets.LockedTarget.EnemyCtrl, Offsets.WorldChrMan.ChrAnim, Offsets.WorldChrMan.ChrAnimSpeed
                }, false);

            return _memoryIo.ReadFloat(targetSpeedPtr);
        }

        public void ToggleTargetAi(bool setValue)
        {
            var disableTargetAiPtr = _memoryIo.FollowPointers(CodeCaveOffsets.CodeCave1.Base +
                                                              CodeCaveOffsets.CodeCave1.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);
            var flagMask = (byte)Offsets.WorldChrMan.ChrFlags.NoUpdate;
            _memoryIo.SetBitValue(disableTargetAiPtr, flagMask, setValue);
        }

        public bool IsTargetAiDisabled()
        {
            var disableTargetAiPtr = _memoryIo.FollowPointers(CodeCaveOffsets.CodeCave1.Base +
                                                              CodeCaveOffsets.CodeCave1.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.ChrFlags
                }, false);
            var flagMask = (byte)Offsets.WorldChrMan.ChrFlags.NoUpdate;
            return _memoryIo.IsBitSet(disableTargetAiPtr, flagMask);
        }

        public void ToggleTargetNoDamage(bool setValue)
        {
            var disableTargetDamagePtr = _memoryIo.FollowPointers(CodeCaveOffsets.CodeCave1.Base +
                                                              CodeCaveOffsets.CodeCave1.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = Offsets.WorldChrMan.NoDamage;
            _memoryIo.SetBitValue(disableTargetDamagePtr, flagMask, setValue);
        }

        public bool IsTargetNoDamageEnabled()
        {
            var disableTargetDamagePtr = _memoryIo.FollowPointers(CodeCaveOffsets.CodeCave1.Base +
                                                                  CodeCaveOffsets.CodeCave1.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.NoDamage
                }, false);
            var flagMask = Offsets.WorldChrMan.NoDamage;
            return _memoryIo.IsBitSet(disableTargetDamagePtr, flagMask);
        }

        public void ToggleAi(int value)
        {
            var disableAiPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.DisableAi;
            _memoryIo.WriteInt32(disableAiPtr, value);
        }

        public void ToggleAllNoDamage(int value)
        {
            var allNoDamagePtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.AllNoDamage;
            _memoryIo.WriteInt32(allNoDamagePtr, value);
            
            var codeBlock = _codeCave + CodeCaveOffsets.CodeCave1.AllNoDamage;
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
    }
}