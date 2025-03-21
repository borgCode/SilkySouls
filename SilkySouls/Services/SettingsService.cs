using System;
using SilkySouls.memory;
using SilkySouls.Memory;

namespace SilkySouls.Services
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

        public void SetGuaranteedBkhDrop(bool setValue)
        {
            var bkhPtr = _memoryIo.FollowPointers(Offsets.SoloParamMan.Base, new[]
            {
                Offsets.SoloParamMan.ParamResCap,
                Offsets.SoloParamMan.ItemLot,
                Offsets.SoloParamMan.BkhDropRateBase
            }, false);

            if (setValue)
            {
                _memoryIo.WriteByte(bkhPtr + (int)Offsets.SoloParamMan.BkhDropRateSlots.Nothing, 0);
                _memoryIo.WriteByte(bkhPtr + (int)Offsets.SoloParamMan.BkhDropRateSlots.Bkh, 0x64);
                _memoryIo.WriteByte(bkhPtr + (int)Offsets.SoloParamMan.BkhDropRateSlots.Bks, 0);
            }
            else
            {
                _memoryIo.WriteByte(bkhPtr + (int)Offsets.SoloParamMan.BkhDropRateSlots.Nothing, 0x4B);
                _memoryIo.WriteByte(bkhPtr + (int)Offsets.SoloParamMan.BkhDropRateSlots.Bkh, 0x14);
                _memoryIo.WriteByte(bkhPtr + (int)Offsets.SoloParamMan.BkhDropRateSlots.Bks, 0x5);
            }
        }
    }
}