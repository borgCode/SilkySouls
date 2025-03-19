using System;
using System.Collections.Generic;

namespace SilkySouls.Memory
{
    public class HookManager
    {
        private readonly MemoryIo _memoryIo;
        private readonly Dictionary<long, HookData> _hookRegistry = new Dictionary<long, HookData>();
        
        private class HookData
        {
            public long OriginAddr { get; set; }
            public long CaveAddr { get; set; }
            public byte[] OriginalBytes { get; set; }
        }
        
        public HookManager(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }


        public long InstallHook(long target, long origin, byte[] originalBytes)
        {
            byte[] hookBytes = GetHookBytes(originalBytes.Length, target, origin);
            _memoryIo.WriteBytes((IntPtr) origin, hookBytes);
            _hookRegistry[target] = new HookData
            {
                CaveAddr = target,
                OriginAddr = origin,
                OriginalBytes = originalBytes
            };
            return target;
        }

        private byte[] GetHookBytes(int originalBytesLength, long target, long origin)
        {
            byte[] hookBytes = new byte[originalBytesLength];
            hookBytes[0] = 0xE9;

            int jumpOffset = (int)(target - (origin + 5));
            byte[] offsetBytes = BitConverter.GetBytes(jumpOffset);
            Array.Copy(offsetBytes, 0, hookBytes, 1, 4);

            for (int i = 5; i < hookBytes.Length; i++)
            {
                hookBytes[i] = 0x90;
            }
            return hookBytes;
        }

        public void UninstallHook(long key)
        {
            if (!_hookRegistry.TryGetValue(key, out HookData hookToUninstall))
            {
                return;
            }
            
            IntPtr originAddrPtr = (IntPtr)hookToUninstall.OriginAddr;
            _memoryIo.WriteBytes(originAddrPtr, hookToUninstall.OriginalBytes);
            _hookRegistry.Remove(key);

        }

        public void ClearHooks()
        {
            _hookRegistry.Clear();
        }
    }
}