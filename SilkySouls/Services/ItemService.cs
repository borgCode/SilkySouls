using SilkySouls.memory;
using SilkySouls.Memory;
using SilkySouls.Utilities;

namespace SilkySouls.Services
{
    public class ItemService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        private bool _codeIsWritten;
        
        public ItemService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        public void ItemSpawn(int itemId, int category, int quantity)
        {
            var shouldProcessFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.ShouldProcessFlag;
            var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.Code;
            if (!_codeIsWritten)
            {
                
                var shouldExitFlag = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.ShouldExitFlag;
               
                var gameDataMan = _memoryIo.ReadInt64(Offsets.GameDataMan.Base);
                var sleepAddr = _memoryIo.GetProcAddress("kernel32.dll", "Sleep");
            
                var itemGetMenuMan = _memoryIo.ReadInt64(Offsets.ItemGetMenuMan);

                byte[] spawnBytes = AsmLoader.GetAsmBytes("ItemSpawn");
                AsmHelper.WriteRelativeOffsets(spawnBytes, new []
                {
                    (code.ToInt64(), shouldProcessFlag.ToInt64(), 7, 0x0 + 2),
                    (code.ToInt64() + 0xD, shouldProcessFlag.ToInt64(), 7, 0xD + 2),
                    (code.ToInt64() + 0xA9, shouldExitFlag.ToInt64(), 7, 0xA9 + 2)
                });
                
                AsmHelper.WriteAbsoluteAddresses64(spawnBytes, new []
                {
                    (gameDataMan, 0x2B + 2),
                    (Offsets.ItemGet, 0x54 + 2),
                    (itemGetMenuMan, 0x64 + 2),
                    (Offsets.ItemDlgFunc, 0x86 + 2),
                    (sleepAddr.ToInt64(), 0x96 + 2)
                });
                
                _memoryIo.WriteBytes(code, spawnBytes);
                _codeIsWritten = true;
                _memoryIo.RunPersistentThread(code);
            }

            _memoryIo.WriteInt32(code + 0x14 + 1, category);
            _memoryIo.WriteInt32(code + 0x19 + 2, quantity);
            _memoryIo.WriteInt32(code + 0x1F + 2, itemId);

            _memoryIo.WriteInt32(code + 0x71 + 1, category);
            _memoryIo.WriteInt32(code + 0x76 + 2, quantity);
            _memoryIo.WriteInt32(code + 0x7C + 2, itemId);
            
            _memoryIo.WriteByte(shouldProcessFlag, 1);
            
        }
        
        
        public void Reset()
        {
            _codeIsWritten = false;
        }

        public void SignalClose()
        {
            _memoryIo.WriteByte(CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.ShouldExitFlag, 1);
        }
    }
}