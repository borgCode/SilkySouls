using System;
using System.Linq;
using DSRForge.memory;
using DSRForge.Memory;
using DSRForge.Utilities;

namespace DSRForge.Services
{
    public class EnemyService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        private readonly IntPtr _codeCave;
        private IntPtr _lastTargetBlock;
        private IntPtr _repeatActionBlock;
        private IntPtr _lockedTargetPtr;
        private IntPtr _repeatActionFlag;
        
        private bool _isHookInstalled;

        private long _lockedTargetOrigin;
        private readonly byte[] _lockedTargetOriginBytes = { 0x48, 0x8D, 0x54, 0x24, 0x38 };
        private long _repeatActionOrigin;
        
        public EnemyService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
            
            _codeCave = CodeCaveOffsets.CodeCave1.Base;
            _lastTargetBlock = _codeCave + CodeCaveOffsets.CodeCave1.LockedTarget;
            _repeatActionBlock = _codeCave + CodeCaveOffsets.CodeCave1.RepeatAction;
        }

        internal void TryInstallTargetHook()
        {
            if (_isHookInstalled) return;
            if (!IsTargetOriginInitialized()) return;
            
            _lockedTargetPtr = _codeCave + CodeCaveOffsets.CodeCave1.LockedTargetPtr;
            
            byte[] lockedTargetBytes = AsmLoader.GetAsmBytes("LastLockedTarget");
          
            byte[] lockedTargetPtrBytes = BitConverter.GetBytes(_lockedTargetPtr.ToInt64());
            Array.Copy(lockedTargetPtrBytes, 0, lockedTargetBytes, 6, lockedTargetPtrBytes.Length);

            int originOffset = (int)(_lockedTargetOrigin + 5 - (_lastTargetBlock.ToInt64() + 25));
            byte[] originAddr = BitConverter.GetBytes(originOffset);
            Array.Copy(originAddr, 0, lockedTargetBytes, 21, originAddr.Length);

            Console.WriteLine(BitConverter.ToString(lockedTargetBytes));
            _memoryIo.WriteBytes(_lastTargetBlock, lockedTargetBytes);
            _hookManager.InstallHook(_lastTargetBlock.ToInt64(), _lockedTargetOrigin, _lockedTargetOriginBytes);
            _isHookInstalled = true;
        }

        internal void ResetHooks()
        {
            _isHookInstalled = false;
            _isRepeatActionInstalled = false;
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

        private bool _isRepeatActionInstalled;

        internal void EnableRepeatAction()
        {
            _repeatActionOrigin = Offsets.Hooks.RepeatAction;
            if (_isRepeatActionInstalled)
            {
                _memoryIo.WriteByte(_repeatActionFlag, 1);
            }
            else
            {
                _repeatActionFlag = _codeCave + CodeCaveOffsets.CodeCave1.RepeatActionFlag;
                _memoryIo.WriteByte(_repeatActionFlag, 1);
                
                byte[] asmBytes = AsmLoader.GetAsmBytes("RepeatAction");

                byte[] bytes = BitConverter.GetBytes(_lockedTargetPtr.ToInt64());
                Array.Copy(bytes, 0, asmBytes, 5, 8);
                bytes = BitConverter.GetBytes(43); //Skip repeat if not locked target
                Array.Copy(bytes, 0, asmBytes, 33, bytes.Length);
                bytes = BitConverter.GetBytes(_repeatActionFlag.ToInt64());
                Array.Copy(bytes, 0, asmBytes, 39, 8);
                bytes = BitConverter.GetBytes(17); //Skip repeat when disabled
                Array.Copy(bytes, 0, asmBytes, 52, bytes.Length);
                bytes = BitConverter.GetBytes(7); 
                Array.Copy(bytes, 0, asmBytes, 69, bytes.Length);
                int originOffset = (int)(_repeatActionOrigin + 7 - (_repeatActionBlock.ToInt64() + 95));
                byte[] originAddr = BitConverter.GetBytes(originOffset);
                Array.Copy(originAddr, 0, asmBytes, asmBytes.Length - 4, originAddr.Length);

                _memoryIo.WriteBytes(_repeatActionBlock, asmBytes);
            
                _hookManager.InstallHook(_repeatActionBlock.ToInt64(), _repeatActionOrigin,
                    new byte[] { 0x0F, 0xBE, 0x80, 0x60, 0x03, 0x00, 0x00 });
                _isRepeatActionInstalled = true;
            }
        }

        internal void DisableRepeatAction()
        {
            _memoryIo.WriteByte(_repeatActionFlag, 0);
        }

        public void ToggleAi(int value)
        {
            var disableAiPtr = Offsets.DebugFlags.Base + Offsets.DebugFlags.DisableAi;
            _memoryIo.WriteInt32(disableAiPtr, value);
        }

        public void ToggleAllNoDamage(int value)
        {
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
