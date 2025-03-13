using System;
using DSRForge.memory;
using DSRForge.Memory;
using DSRForge.Utilities;

namespace DSRForge.Services
{
    public class ItemService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        private readonly IntPtr _itemSpawnBlock;
        private readonly IntPtr _flagLoc;
        private readonly IntPtr _codeCaveBase;
        private bool _hasSetupCave;
        private bool _isHookInstalled;

        
        public ItemService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;

            _codeCaveBase = memoryIo.BaseAddress + CodeCaveOffsets.CodeCave3.Base;
            _flagLoc = _codeCaveBase;

            _itemSpawnBlock = _codeCaveBase + CodeCaveOffsets.CodeCave3.ItemSpawn;
        }

        public void ItemSpawn(int itemId, int category, int quantity)
        {
            if (!_hasSetupCave)
            {
                _memoryIo.WriteBytes(_itemSpawnBlock, GetItemSpawnBytes());
                _hasSetupCave = true;
            }

            PopulateItemDetails(itemId, category, quantity);

            byte[] spawnFlag = { 1 };
            _memoryIo.WriteBytes(_flagLoc, spawnFlag);

            if (_isHookInstalled) return;

            _hookManager.InstallHook(_itemSpawnBlock.ToInt64(),0x140497483,
                new byte[] { 0xC6, 0x05, 0xEE, 0x12, 0x88, 0x01, 0x00 });
            _isHookInstalled = true;
        }

        private byte[] GetItemSpawnBytes()
        {
            ulong chrItemSpawnAddr =
                _memoryIo.ReadUInt64(_memoryIo.BaseAddress + Offsets.GameDataMan);
            long itemSpawnFunc = _memoryIo.BaseAddress.ToInt64() + Offsets.ItemGet;

            ulong itemGetMenuMan = _memoryIo.ReadUInt64(_memoryIo.BaseAddress + Offsets.ItemGetMenuMan);

            long itemGetDlgFunc = _memoryIo.BaseAddress.ToInt64() + Offsets.ItemDlgFunc;

            long originLocation = _memoryIo.BaseAddress.ToInt64() + 0x497483;

            byte[] spawnBytes = AsmLoader.GetAsmBytes("ItemSpawn");

            byte[] bytes = BitConverter.GetBytes(_flagLoc.ToInt64());
            Array.Copy(bytes, 0, spawnBytes, 81, 8);
            bytes = BitConverter.GetBytes(130);
            Array.Copy(bytes, 0, spawnBytes, 94, 4);
            bytes = BitConverter.GetBytes(chrItemSpawnAddr);
            Array.Copy(bytes, 0, spawnBytes, 123, 8);
            bytes = BitConverter.GetBytes(itemSpawnFunc);
            Array.Copy(bytes, 0, spawnBytes, 164, 8);
            bytes = BitConverter.GetBytes(itemGetMenuMan);
            Array.Copy(bytes, 0, spawnBytes, 180, 8);
            bytes = BitConverter.GetBytes(itemGetDlgFunc);
            Array.Copy(bytes, 0, spawnBytes, 214, 8);
            bytes = BitConverter.GetBytes(_flagLoc.ToInt64());
            Array.Copy(bytes, 0, spawnBytes, 230, 8);
            bytes = BitConverter.GetBytes(originLocation + 7 - (_itemSpawnBlock.ToInt64() + 312));
            Array.Copy(bytes, 0, spawnBytes, 308, 4);
            return spawnBytes;
        }

        private void PopulateItemDetails(int itemId, int category, int quantity)
        {
            _memoryIo.WriteInt32(_itemSpawnBlock + 99, category);
            _memoryIo.WriteInt32(_itemSpawnBlock + 105, quantity);
            _memoryIo.WriteInt32(_itemSpawnBlock + 111, itemId);

            _memoryIo.WriteInt32(_itemSpawnBlock + 192, category);
            _memoryIo.WriteInt32(_itemSpawnBlock + 198, quantity);
            _memoryIo.WriteInt32(_itemSpawnBlock + 204, itemId);
        }
    }
}