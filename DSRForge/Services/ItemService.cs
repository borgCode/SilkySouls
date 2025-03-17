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
        private readonly long _origin;
        private bool _hasSetupCave;
        private bool _isHookInstalled;
        
        public ItemService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;

            var codeCaveBase = CodeCaveOffsets.CodeCave3.Base;
            _flagLoc = codeCaveBase;

            _itemSpawnBlock = codeCaveBase + CodeCaveOffsets.CodeCave3.ItemSpawn;
            _origin = Offsets.Hooks.ItemSpawn;
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

            _hookManager.InstallHook(_itemSpawnBlock.ToInt64(),_origin,
                new byte[] { 0xC6, 0x05, 0xEE, 0x12, 0x88, 0x01, 0x00 });
            _isHookInstalled = true;
        }

        private byte[] GetItemSpawnBytes()
        {
            ulong gameDataMan = _memoryIo.ReadUInt64(Offsets.GameDataMan.Base);
            
            ulong itemGetMenuMan = _memoryIo.ReadUInt64(Offsets.ItemGetMenuMan);

            byte[] spawnBytes = AsmLoader.GetAsmBytes("ItemSpawn");

            long originalInstruction = _origin + 7 + _memoryIo.ReadInt32((IntPtr)_origin + 2);

            byte[] bytes = BitConverter.GetBytes(originalInstruction);
            Array.Copy(bytes, 0, spawnBytes, 68, 8);
                bytes = BitConverter.GetBytes(_flagLoc.ToInt64());
            Array.Copy(bytes, 0, spawnBytes, 81, 8);
            bytes = BitConverter.GetBytes(130);
            Array.Copy(bytes, 0, spawnBytes, 94, 4);
            bytes = BitConverter.GetBytes(gameDataMan);
            Array.Copy(bytes, 0, spawnBytes, 123, 8);
            bytes = BitConverter.GetBytes(Offsets.ItemGet);
            Array.Copy(bytes, 0, spawnBytes, 164, 8);
            bytes = BitConverter.GetBytes(itemGetMenuMan);
            Array.Copy(bytes, 0, spawnBytes, 180, 8);
            bytes = BitConverter.GetBytes(Offsets.ItemDlgFunc);
            Array.Copy(bytes, 0, spawnBytes, 214, 8);
            bytes = BitConverter.GetBytes(_flagLoc.ToInt64());
            Array.Copy(bytes, 0, spawnBytes, 230, 8);
            bytes = BitConverter.GetBytes(_origin + 7 - (_itemSpawnBlock.ToInt64() + 312));
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

        public void UninstallHook()
        {
            _hookManager.UninstallHook(_itemSpawnBlock.ToInt64());
            _isHookInstalled = false;
            _hasSetupCave = false;
        }
    }
}