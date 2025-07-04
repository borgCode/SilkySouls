using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class UtilityService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        private IntPtr _targetView;
        private IntPtr _draw;
        private long _drawOrigin;
        private IntPtr _emevdCodeLoc;
        private bool _isEmevdCodeWritten;

        private readonly byte[] _drawOriginBytes = { 0x44, 0x8B, 0xC6, 0xBA, 0x16, 0x00, 0x00, 0x00 };

        private List<long> _noClipHooks;
        
        public UtilityService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        internal bool EnableDraw()
        {
            if (!IsDrawOriginInitialized()) return false;
            _draw = CodeCaveOffsets.Base + CodeCaveOffsets.EnableDraw;

            var ezDraw = _memoryIo.FollowPointers(HgDraw.Base, new[] { HgDraw.EzDraw }, true);
            long drawFunc1 = _drawOrigin + 11 + 5 + _memoryIo.ReadInt32((IntPtr)(_drawOrigin + 11) + 1);
            long drawFunc2 = _drawOrigin + 43 + 5 + _memoryIo.ReadInt32((IntPtr)(_drawOrigin + 43) + 1);

            byte[] drawBytes = AsmLoader.GetAsmBytes("EnableDraw");
            byte[] bytes = BitConverter.GetBytes(ezDraw.ToInt64());
            Array.Copy(bytes, 0, drawBytes, 2, 8);
            bytes = BitConverter.GetBytes(drawFunc1);
            Array.Copy(bytes, 0, drawBytes, 26, 8);
            bytes = BitConverter.GetBytes(drawFunc2);
            Array.Copy(bytes, 0, drawBytes, 85, 8);
            bytes = BitConverter.GetBytes(drawFunc1);
            Array.Copy(bytes, 0, drawBytes, 108, 8);
            bytes = BitConverter.GetBytes(drawFunc2);
            Array.Copy(bytes, 0, drawBytes, 147, 8);
            byte[] jumpBytes = BitConverter.GetBytes((int)(_drawOrigin + 8 - (_draw.ToInt64() + 170)));
            Array.Copy(jumpBytes, 0, drawBytes, 166, 4);
            _memoryIo.WriteBytes(_draw, drawBytes);

            _hookManager.InstallHook(_draw.ToInt64(), _drawOrigin, _drawOriginBytes);
            return true;
        }

        private bool IsDrawOriginInitialized()
        {
            _drawOrigin = Hooks.Draw;
            var originBytes = _memoryIo.ReadBytes((IntPtr)_drawOrigin, 8);
            return originBytes.SequenceEqual(_drawOriginBytes);
        }

        internal void DisableDraw()
        {
            _hookManager.UninstallHook(_draw.ToInt64());
        }

        internal void EnableHitboxView()
        {
            var hitboxAddr =
                _memoryIo.FollowPointers(DamageMan.Base, new[] { DamageMan.HitboxFlag }, false);
            _memoryIo.WriteInt32(hitboxAddr, 1);
        }

        internal void DisableHitboxView()
        {
            var hitboxAddr =
                _memoryIo.FollowPointers(DamageMan.Base, new[] { DamageMan.HitboxFlag }, false);
            _memoryIo.WriteInt32(hitboxAddr, 0);
        }

        internal void EnableSoundView()
        {
            _memoryIo.WriteByte(Patches.DrawSoundViewPatch, 1);
        }

        internal void DisableSoundView()
        {
            _memoryIo.WriteByte(Patches.DrawSoundViewPatch, 0);
        }

        public void EnableDrawEvent()
        {
            _memoryIo.WriteByte(Patches.DrawEventPatch, 1);
        }

        public void DisableDrawEvent()
        {
            _memoryIo.WriteByte(Patches.DrawEventPatch, 0);
        }

        private bool _targetViewIsInstalled;

        public void EnableTargetingView()
        {
            if (!_targetViewIsInstalled)
            {
                long targetViewOrigin = Hooks.TargetingView;
                _targetView = CodeCaveOffsets.Base + CodeCaveOffsets.TargetView;

                byte[] targetViewBytes =
                {
                    0xC6, 0x41, 0x48, 0x02, // mov    BYTE PTR [rcx+0x48],0x2
                    0x40, 0x53, // push   rbx
                    0x48, 0x83, 0xEC, 0x20, // sub    rsp,0x20
                    0xE9,
                };

                int originOffset = (int)(targetViewOrigin + 6 -
                                         (_targetView.ToInt64() + targetViewBytes.Length + 4));
                targetViewBytes = targetViewBytes.Concat(BitConverter.GetBytes(originOffset)).ToArray();

                _memoryIo.WriteBytes(_targetView, targetViewBytes);
                _hookManager.InstallHook(_targetView.ToInt64(), targetViewOrigin,
                    new byte[] { 0x40, 0x53, 0x48, 0x83, 0xEC, 0x20 });

                _targetViewIsInstalled = true;
            }
            else
            {
                IntPtr valueAddr = _targetView + 3;
                _memoryIo.WriteBytes(valueAddr, new byte[] { 0x02 });
            }
        }

        public void DisableTargetingView()
        {
            IntPtr valueAddr = _targetView + 3;
            _memoryIo.WriteBytes(valueAddr, new byte[] { 0x00 });
        }

        public void ResetBools()
        {
            _targetViewIsInstalled = false;
            _isEmevdCodeWritten = false;
        }

        public void EnableNoClip()
        {
            var zDirectionAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionVariable;

            var playerCoordsBase = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.UpdateCoordsBasePtr, WorldChrMan.UpdateCoords
                },
                true);

            var inAirTimerOrigin = Hooks.InAirTimer;
            IntPtr inAirTimerBlock = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.InAirTimer;
            byte[] inAirTimerCodeBytes = AsmLoader.GetAsmBytes("NoClip_InAirTimer");

            byte[] bytes = BitConverter.GetBytes(playerCoordsBase.ToInt64());
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 11, 8);
            bytes = BitConverter.GetBytes(3);
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 24, 4);
            bytes = BitConverter.GetBytes(inAirTimerOrigin + 5 - (inAirTimerBlock.ToInt64() + 37));
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 33, 4);

            _memoryIo.WriteBytes(inAirTimerBlock, inAirTimerCodeBytes);

            IntPtr zDirectionKbCheck = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionKbCheck;
            var keyOrigin = Hooks.Keyboard;

            byte[] zDirectKbBytes = AsmLoader.GetAsmBytes("NoClip_ZDirection_KB");
            bytes = BitConverter.GetBytes(25);
            Array.Copy(bytes, 0, zDirectKbBytes, 6, 4);
            bytes = BitConverter.GetBytes(39);
            Array.Copy(bytes, 0, zDirectKbBytes, 19, 4);
            int originOffset = (int)(keyOrigin + 7 - (zDirectionKbCheck.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, zDirectKbBytes, 31, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr.ToInt64());
            Array.Copy(bytes, 0, zDirectKbBytes, 38, 8);
            originOffset = (int)(keyOrigin + 7 - (zDirectionKbCheck.ToInt64() + 62));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, zDirectKbBytes, 58, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr.ToInt64());
            Array.Copy(bytes, 0, zDirectKbBytes, 65, 8);
            originOffset = (int)(keyOrigin + 7 - (zDirectionKbCheck.ToInt64() + 89));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, zDirectKbBytes, 85, 4);

            _memoryIo.WriteBytes(zDirectionKbCheck, zDirectKbBytes);

            IntPtr zDirectionR2Check = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionR2Check;
            var r2Origin = Hooks.ControllerR2;

            byte[] r2Bytes = AsmLoader.GetAsmBytes("NoClip_ZDirection_R2");

            bytes = BitConverter.GetBytes(17);
            Array.Copy(bytes, 0, r2Bytes, 9, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr.ToInt64());
            Array.Copy(bytes, 0, r2Bytes, 16, 8);
            originOffset = (int)(r2Origin + 5 - (zDirectionR2Check.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, r2Bytes, 31, 4);

            _memoryIo.WriteBytes(zDirectionR2Check, r2Bytes);

            IntPtr zDirectionL2Check = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionL2Check;
            var l2Origin = Hooks.ControllerL2;

            byte[] l2Bytes = AsmLoader.GetAsmBytes("NoClip_ZDirection_L2");

            bytes = BitConverter.GetBytes(17);
            Array.Copy(bytes, 0, l2Bytes, 9, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr.ToInt64());
            Array.Copy(bytes, 0, l2Bytes, 16, 8);
            originOffset = (int)(l2Origin + 5 - (zDirectionL2Check.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, l2Bytes, 31, 4);

            _memoryIo.WriteBytes(zDirectionL2Check, l2Bytes);

            IntPtr updateCoordsBlock = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.UpdateCoords;

            var coordsPtr = _memoryIo.FollowPointers(WorldChrMan.Base, new[]
            {
                (int)WorldChrMan.BaseOffsets.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                WorldChrMan.CoordsPtr2,
                WorldChrMan.CoordsPtr3,
                WorldChrMan.CoordsPtr4,
            }, true);

            var updateCoordsOrigin = Hooks.UpdateCoords;
            var padManPtr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    (int)WorldChrMan.BaseOffsets.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.PadMan
                }, true);

            var camPtr = _memoryIo.FollowPointers(Cam.Base, new[] { Cam.ChrCam, Cam.ChrExFollowCam }, true);

            byte[] updateCoordsCodeBytes = AsmLoader.GetAsmBytes("NoClip_UpdateCoords");

            bytes = BitConverter.GetBytes(coordsPtr.ToInt64());
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 3, 8);
            bytes = BitConverter.GetBytes(247);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 16, 4);
            bytes = BitConverter.GetBytes(padManPtr.ToInt64());
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 31, 8);
            bytes = BitConverter.GetBytes(camPtr.ToInt64());
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 84, 8);
            bytes = BitConverter.GetBytes(padManPtr.ToInt64());
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 106, 8);
            bytes = BitConverter.GetBytes(camPtr.ToInt64());
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 159, 8);
            bytes = BitConverter.GetBytes(zDirectionAddr.ToInt64());
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 181, 8);
            bytes = BitConverter.GetBytes(10);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 196, 4);
            bytes = BitConverter.GetBytes(14);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 206, 4);
            bytes = BitConverter.GetBytes(5);
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 215, 4);
            bytes = BitConverter.GetBytes(updateCoordsOrigin + 7 - (updateCoordsBlock.ToInt64() + 267));
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 263, 4);
            bytes = BitConverter.GetBytes(updateCoordsOrigin + 7 - (updateCoordsBlock.ToInt64() + 273));
            Array.Copy(bytes, 0, updateCoordsCodeBytes, 269, 4);

            _memoryIo.WriteBytes(updateCoordsBlock, updateCoordsCodeBytes);

            _noClipHooks = new List<long>
            {
                _hookManager.InstallHook(inAirTimerBlock.ToInt64(), inAirTimerOrigin,
                    new byte[] { 0xF3, 0x0F, 0x58, 0x9B, 0xB0, 0x01, 0x00, 0x00 }),
                _hookManager.InstallHook(zDirectionKbCheck.ToInt64(), keyOrigin,
                    new byte[] { 0xC6, 0x43, 0xF0, 0x01, 0xC6, 0x00, 0x01 }),
                _hookManager.InstallHook(zDirectionR2Check.ToInt64(), r2Origin,
                    new byte[] { 0x0F, 0xB6, 0x44, 0x24, 0x27 }),
                _hookManager.InstallHook(zDirectionL2Check.ToInt64(), l2Origin,
                    new byte[] { 0x0F, 0xB6, 0x44, 0x24, 0x26 }),
                _hookManager.InstallHook(updateCoordsBlock.ToInt64(), updateCoordsOrigin,
                    new byte[] { 0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00 })
            };
        }

        public void DisableNoClip()
        {
            for (int i = _noClipHooks.Count - 1; i >= 0; i--)
            {
                _hookManager.UninstallHook(_noClipHooks[i]);
            }

            _noClipHooks.Clear();
            _memoryIo.WriteBytes(CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionVariable, new byte[641]);
        }
        
        public void ToggleFilter(bool value)
        {
            if (value)
            {
                var filterPtr = _memoryIo.FollowPointers(FieldArea.Base, new[]
                    { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
                _memoryIo.WriteByte(filterPtr, 1);
                var brightnessPtr = _memoryIo.FollowPointers(FieldArea.Base, new[]
                    { FieldArea.RenderPtr, FieldArea.Brightness }, false);
                var bytes = new byte[12];
                var floatBytes = BitConverter.GetBytes(5.0f);
                Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

                _memoryIo.WriteBytes(brightnessPtr, bytes);
            }
            else
            {
                var filterPtr = _memoryIo.FollowPointers(FieldArea.Base, new[]
                    { FieldArea.RenderPtr, FieldArea.FilterRemoval }, false);
                _memoryIo.WriteByte(filterPtr, 0);
                var brightnessPtr = _memoryIo.FollowPointers(FieldArea.Base, new[]
                    { FieldArea.RenderPtr, FieldArea.Brightness }, false);
                var bytes = new byte[12];
                var floatBytes = BitConverter.GetBytes(1.0f);
                Buffer.BlockCopy(floatBytes, 0, bytes, 0, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 4, 4);
                Buffer.BlockCopy(floatBytes, 0, bytes, 8, 4);

                _memoryIo.WriteBytes(brightnessPtr, bytes);
            }
        }

        public void ShowMenu(MenuMan.MenuManData menuType)
        {
            var menuPtr = _memoryIo.FollowPointers(MenuMan.Base, new[] { (int)menuType }, false);
            _memoryIo.WriteByte(menuPtr, menuType == MenuMan.MenuManData.Warp ? 2 : 1);
        }

        public void ShowUpgradeMenu(bool isWeapon)
        {
            byte[] upgradeBytes = AsmLoader.GetAsmBytes("OpenEnhanceShop");
            var playerGameData = _memoryIo.FollowPointers(GameDataMan.Base,
                new[] { (int)GameDataMan.GameDataOffsets.PlayerGameData }, true);
            byte[] bytes = BitConverter.GetBytes(playerGameData.ToInt64());
            Array.Copy(bytes, 0, upgradeBytes, 2, bytes.Length);
            bytes = BitConverter.GetBytes(isWeapon ? OpenEnhanceShopWeapon : OpenEnhanceShopArmor);
            Array.Copy(bytes, 0, upgradeBytes, 16, bytes.Length);
            _memoryIo.AllocateAndExecute(upgradeBytes);
        }
        

        public void ToggleDeathCam(bool isDeathCamEnabled) =>
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(WorldChrMan.Base) + (int)WorldChrMan.BaseOffsets.DeathCam,
                isDeathCamEnabled ? 1 : 0);


        public void ToggleDisableEvents(bool isDisableEventsEnabled)
        {
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(DebugEventMan.Base) + DebugEventMan.DisableEvents,
                isDisableEventsEnabled ? 1 : 0);
        }

        public void SetEvent(ulong flagId)
        {
            var eventMan = _memoryIo.ReadInt64(EventFlagMan.Base);
            var setEventBytes = AsmLoader.GetAsmBytes("SetEvent");
            var bytes = BitConverter.GetBytes(eventMan);
            Array.Copy(bytes, 0, setEventBytes, 0x2, 8);
            bytes = BitConverter.GetBytes(flagId);
            Array.Copy(bytes, 0, setEventBytes, 0xA + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.SetEvent);
            Array.Copy(bytes, 0, setEventBytes, 0x24 + 2, 8);
            _memoryIo.AllocateAndExecute(setEventBytes);
        }

        public void SetMultipleEvents(params ulong[] flagIds)
        {
            foreach (var flagId in flagIds)
            {
                SetEvent(flagId);
            }
        }

        public bool GetEvent(ulong eventId)
        {
            var getEventBytes = AsmLoader.GetAsmBytes("GetEvent");
            AsmHelper.WriteAbsoluteAddresses64(getEventBytes, new []
            {
                (_memoryIo.ReadInt64(EventFlagMan.Base), 0x0 + 2),
                ((long)eventId, 0xA + 2),
                (Funcs.GetEvent, 0x14 + 2),
                (CodeCaveOffsets.Base.ToInt64() + CodeCaveOffsets.GetEventResult, 0x28 + 2)
            });
            
            _memoryIo.AllocateAndExecute(getEventBytes);
            return _memoryIo.ReadUInt8(CodeCaveOffsets.Base + CodeCaveOffsets.GetEventResult) == 1;
        }

        public void OpenRegularShop(ulong[] shopParams)
        {
            var openRegularShopBytes = AsmLoader.GetAsmBytes("OpenRegularShop");
            var bytes = BitConverter.GetBytes(shopParams[0]);
            Array.Copy(bytes, 0, openRegularShopBytes, 0x0 + 2, 8);
            bytes = BitConverter.GetBytes(shopParams[1]);
            Array.Copy(bytes, 0, openRegularShopBytes, 0xA + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.ShopParamSave);
            Array.Copy(bytes, 0, openRegularShopBytes, 0x14 + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.OpenRegularShop);
            Array.Copy(bytes, 0, openRegularShopBytes, 0x24 + 2, 8);
            _memoryIo.AllocateAndExecute(openRegularShopBytes);
        }


        public void OpenAttunement()
        {
            var codeBytes = AsmLoader.GetAsmBytes("OpenAttunement");
            var bytes = BitConverter.GetBytes(Funcs.AttunementWindowPrep);
            Array.Copy(bytes, 0, codeBytes, 0xE + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.OpenAttunement);
            Array.Copy(bytes, 0, codeBytes, 0x22 + 2, 8);
            _memoryIo.AllocateAndExecute(codeBytes);
        }
    }
}