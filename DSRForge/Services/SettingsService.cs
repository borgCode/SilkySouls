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
                _memoryIo.FollowPointers(Offsets.MenuMan.Base, new[]
                {
                   (int)Offsets.MenuMan.MenuManData.Quitout
                }, false);
            _memoryIo.WriteByte(quitoutPtr, 2);
        }

        public void ToggleFastQuitout(int value)
        {
            _memoryIo.WriteByte(Offsets.QuitoutPatch, value);
        }
    }
}