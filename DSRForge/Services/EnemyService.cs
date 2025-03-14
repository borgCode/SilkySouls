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
        
        private const long LockedTargetOrigin = 0x142E76196;
        private readonly byte[] _lockedTargetOriginBytes = { 0x48, 0x8B, 0x03, 0x48, 0x8B, 0xCB };
        private const long RepeatActionOrigin = 0x1405B3FA4;
        
        public EnemyService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;

            _codeCave = memoryIo.BaseAddress + CodeCaveOffsets.CodeCave1.Base;
            _lastTargetBlock = _codeCave + CodeCaveOffsets.CodeCave1.LockedTarget;
            _repeatActionBlock = _codeCave + CodeCaveOffsets.CodeCave1.RepeatAction;
        }

        internal void TryInstallTargetHook()
        {
            if (_isHookInstalled) return;
            if (!IsTargetOriginInitialized()) return;
            
            _lockedTargetPtr = _codeCave + CodeCaveOffsets.CodeCave1.LockedTargetPtr;
            
            byte[] lockedTargetBytes = {
                    0x48, 0x89, 0xD8,                               // mov    rax,rbx
                    0x48, 0xA3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,    //   movabs ds:0x0,rax
                     0x48, 0x8B, 0x03,                               //   mov    rax,QWORD PTR [rbx]
                    0x48, 0x89, 0xD9,                                              //   mov    rcx,rbx
                    0xE9, 0x00, 0x00, 0x00, 0x00                                   //  jmp    18 <_main+0x18>
                };
          
            byte[] lockedTargetPtrBytes = BitConverter.GetBytes(_lockedTargetPtr.ToInt64());
            Array.Copy(lockedTargetPtrBytes, 0, lockedTargetBytes, 5, lockedTargetPtrBytes.Length);

            int originOffset = (int)(LockedTargetOrigin + 6 - (_lastTargetBlock.ToInt64() + 24));
            byte[] originAddr = BitConverter.GetBytes(originOffset);
            Array.Copy(originAddr, 0, lockedTargetBytes, 20, originAddr.Length);

            _memoryIo.WriteBytes(_lastTargetBlock, lockedTargetBytes);
            _hookManager.InstallHook(_lastTargetBlock.ToInt64(), LockedTargetOrigin, _lockedTargetOriginBytes);
        }

        internal void ResetHook()
        {
            _isHookInstalled = false;
        }
        
        private bool IsTargetOriginInitialized()
        {
            var originBytes = _memoryIo.ReadBytes((IntPtr)LockedTargetOrigin, 6);
            return originBytes.SequenceEqual(_lockedTargetOriginBytes);
        }

        public int GetTargetHp()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(_memoryIo.BaseAddress + CodeCaveOffsets.CodeCave1.Base +
                                                   CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetHp);
        }

        public int GetTargetMaxHp()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(_memoryIo.BaseAddress + CodeCaveOffsets.CodeCave1.Base +
                                                      CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetMaxHp);
        }

        public void SetTargetHp(int value)
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(_memoryIo.BaseAddress + CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            _memoryIo.WriteInt32((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.TargetHp, value);
        }

        public float GetTargetPoise()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(_memoryIo.BaseAddress + CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.CurrentPoise);
        }

        public float GetTargetMaxPoise()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(_memoryIo.BaseAddress + CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.MaxPoise);
        }

        public float GetTargetPoiseTimer()
        {
            var lockedTargetPtr = _memoryIo.ReadUInt64(_memoryIo.BaseAddress + CodeCaveOffsets.CodeCave1.Base +
                                                       CodeCaveOffsets.CodeCave1.LockedTargetPtr);
            return _memoryIo.ReadFloat((IntPtr)lockedTargetPtr + (int)Offsets.LockedTarget.PoiseTimer);
        }

        private bool _isRepeatActionInstalled;

        internal void EnableRepeatAction()
        {
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
                int originOffset = (int)(RepeatActionOrigin + 7 - (_repeatActionBlock.ToInt64() + 95));
                byte[] originAddr = BitConverter.GetBytes(originOffset);
                Array.Copy(originAddr, 0, asmBytes, asmBytes.Length - 4, originAddr.Length);

                _memoryIo.WriteBytes(_repeatActionBlock, asmBytes);
            
                _hookManager.InstallHook(_repeatActionBlock.ToInt64(), RepeatActionOrigin,
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
            var disableAiPtr = _memoryIo.BaseAddress + Offsets.DisableAi;
            _memoryIo.WriteInt32(disableAiPtr, value);
        }

        public void ToggleAllNoDamage(int value)
        {
            var codeBlock = _codeCave + CodeCaveOffsets.CodeCave1.AllNoDamage;
            if (value == 1)
            {
                long origin = 0x1403206c9;
                
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
            var allNoDeathPtr = _memoryIo.BaseAddress + Offsets.AllNoDeath;
            _memoryIo.WriteInt32(allNoDeathPtr, value);
        }
        
    }
}
