using DSRForge.memory;
using DSRForge.Memory;

namespace DSRForge.Services
{
    public class SettingsService
    {
        private readonly MemoryIo _memoryIo;
        public SettingsService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }

        public void Quitout()
        {
            var quitoutPtr =
                _memoryIo.FollowPointers(new[]
                {
                    Offsets.MenuMan, (int)Offsets.MenuManData.Quitout
                }, false);
            _memoryIo.WriteByte(quitoutPtr, 2);
        }
    }
}