using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DSRForge.memory;
using DSRForge.Memory;
using DSRForge.Models;
using DSRForge.Utilities;

namespace DSRForge.Services
{
    public class UtilityService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        private readonly IntPtr _codeCave1;
        private readonly IntPtr _codeCave2;
        private IntPtr _targetView;
        private IntPtr _draw;
        private readonly long _drawOrigin;
        
        private readonly byte[] _drawOriginBytes = { 0x44, 0x8B, 0xC6, 0xBA, 0x16, 0x00, 0x00, 0x00 };
        
        private List<long> _noClipHooks;
        

        public UtilityService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
            _codeCave1 = CodeCaveOffsets.CodeCave1.Base;
            _codeCave2 = CodeCaveOffsets.CodeCave2.Base;
            Console.WriteLine(Offsets.Hooks.Draw);
            _drawOrigin = Offsets.Hooks.Draw;
        }

        internal bool EnableDraw()
        {
            if (!IsDrawOriginInitialized()) return false;
            _draw = _codeCave1 + CodeCaveOffsets.CodeCave1.EnableDraw;

            var ezDraw = _memoryIo.FollowPointers(Offsets.HgDraw.Base, new[] { Offsets.HgDraw.EzDraw }, true);
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
            byte[] jumpBytes = BitConverter.GetBytes((int)(_drawOrigin + 8 -  ( _codeCave1.ToInt64() + 170)));
            Array.Copy(jumpBytes, 0, drawBytes, 166, 4);
            _memoryIo.WriteBytes(_draw, drawBytes);
            
            _hookManager.InstallHook(_draw.ToInt64(), _drawOrigin, _drawOriginBytes);
            return true;
        }

        private bool IsDrawOriginInitialized()
        {
            var originBytes = _memoryIo.ReadBytes((IntPtr) _drawOrigin, 8);
            return originBytes.SequenceEqual(_drawOriginBytes);
        }

        internal void DisableDraw()
        {
            _hookManager.UninstallHook(_draw.ToInt64());
        }

        internal void EnableHitboxView()
        {
            var hitboxAddr =
                _memoryIo.FollowPointers(Offsets.DamageMan.Base, new[] {Offsets.DamageMan.HitboxFlag }, false);
            _memoryIo.WriteInt32(hitboxAddr, 1);
        }

        internal void DisableHitboxView()
        {
            var hitboxAddr =
                _memoryIo.FollowPointers(Offsets.DamageMan.Base, new[] {Offsets.DamageMan.HitboxFlag }, false);

            _memoryIo.WriteInt32(hitboxAddr, 0);
        }
        
        internal void EnableSoundView()
        {
            _memoryIo.WriteByte(Offsets.DrawSoundViewPatch, 1);
        }

        internal void DisableSoundView()
        {
            _memoryIo.WriteByte(Offsets.DrawSoundViewPatch, 0);
        }

        public void EnableDrawEvent()
        {
            _memoryIo.WriteByte(Offsets.DrawEventPatch, 1);
        }

        public void DisableDrawEvent()
        {
            _memoryIo.WriteByte(Offsets.DrawEventPatch, 0);
        }

        private bool _targetViewIsInstalled;

        public void EnableTargetingView()
        {
            if (!_targetViewIsInstalled)
            {
                long targetViewOrigin = Offsets.Hooks.TargetingView;

                _targetView = _codeCave1 + CodeCaveOffsets.CodeCave1.TargetView;
                
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

        public void ResetHook()
        {
            _targetViewIsInstalled = false;
        }

        public void EnableNoClip()
        {
            var zDirectionAddr = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.NoClip.ZDirectionVariable;

            var playerCoordsBase = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                  (int)Offsets.WorldChrMan.BaseOffsets.UpdateCoordsBasePtr, Offsets.WorldChrMan.UpdateCoords
                },
                true);

            var inAirTimerOrigin = Offsets.Hooks.InAirTimer;
            IntPtr inAirTimerBlock = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.NoClip.InAirTimer;
            byte[] inAirTimerCodeBytes = AsmLoader.GetAsmBytes("NoClip_InAirTimer");

            byte[] bytes = BitConverter.GetBytes(playerCoordsBase.ToInt64());
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 11, 8);
            bytes = BitConverter.GetBytes(3);
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 24, 4);
            bytes = BitConverter.GetBytes(inAirTimerOrigin + 5 - (inAirTimerBlock.ToInt64() + 37));
            Array.Copy(bytes, 0, inAirTimerCodeBytes, 33, 4);

            _memoryIo.WriteBytes(inAirTimerBlock, inAirTimerCodeBytes);

            IntPtr zDirectionKbCheck = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.NoClip.ZDirectionKbCheck;
            var keyOrigin = Offsets.Hooks.Keyboard;

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

            IntPtr zDirectionR2Check = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.NoClip.ZDirectionR2Check;
            var r2Origin = Offsets.Hooks.ControllerR2;

            byte[] r2Bytes = AsmLoader.GetAsmBytes("NoClip_ZDirection_R2");

            bytes = BitConverter.GetBytes(17);
            Array.Copy(bytes, 0, r2Bytes, 9, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr.ToInt64());
            Array.Copy(bytes, 0, r2Bytes, 16, 8);
            originOffset = (int)(r2Origin + 5 - (zDirectionR2Check.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, r2Bytes, 31, 4);

            _memoryIo.WriteBytes(zDirectionR2Check, r2Bytes);

            IntPtr zDirectionL2Check = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.NoClip.ZDirectionL2Check;
            var l2Origin = Offsets.Hooks.ControllerL2;

            byte[] l2Bytes = AsmLoader.GetAsmBytes("NoClip_ZDirection_L2");

            bytes = BitConverter.GetBytes(17);
            Array.Copy(bytes, 0, l2Bytes, 9, 4);
            bytes = BitConverter.GetBytes(zDirectionAddr.ToInt64());
            Array.Copy(bytes, 0, l2Bytes, 16, 8);
            originOffset = (int)(l2Origin + 5 - (zDirectionL2Check.ToInt64() + 35));
            bytes = BitConverter.GetBytes(originOffset);
            Array.Copy(bytes, 0, l2Bytes, 31, 4);

            _memoryIo.WriteBytes(zDirectionL2Check, l2Bytes);

            IntPtr updateCoordsBlock = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.NoClip.UpdateCoords;

            var coordsPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,new[]
            {
                (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                (int)Offsets.WorldChrMan.PlayerInsOffsets.CoordsPtr1,
                Offsets.WorldChrMan.CoordsPtr2,
                Offsets.WorldChrMan.CoordsPtr3,
                Offsets.WorldChrMan.CoordsPtr4,
            }, true);

            var updateCoordsOrigin = Offsets.Hooks.UpdateCoords;
            var padManPtr = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    (int)Offsets.WorldChrMan.BaseOffsets.PlayerIns,
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.PadMan
                }, true);

            var camPtr = _memoryIo.FollowPointers(Offsets.Cam.Base, new[] {Offsets.Cam.ChrCam, Offsets.Cam.ChrExFollowCam }, true);

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
            _memoryIo.WriteBytes(_codeCave2 + (int)CodeCaveOffsets.CodeCave2.NoClip.ZDirectionVariable, new byte[641]);
        }


        public void Warp(Location location)
        {
            var lastBonfireAdr =
                _memoryIo.FollowPointers(Offsets.GameMan.Base, new[] {Offsets.GameMan.LastBonfire }, false);

            _memoryIo.WriteInt32(lastBonfireAdr, location.Id);

            byte[] warpBytes = AsmLoader.GetAsmBytes("Warp");
            byte[] bytes = BitConverter.GetBytes(Offsets.WarpEvent.ToInt64());
            Array.Copy(bytes, 0, warpBytes, 2, 8);
            bytes = BitConverter.GetBytes(Offsets.WarpFunc);
            Array.Copy(bytes, 0, warpBytes, 24, 8);
            _memoryIo.WriteBytes(_codeCave2 + CodeCaveOffsets.CodeCave2.Warp, warpBytes);
            _memoryIo.RunThread(_codeCave2 + CodeCaveOffsets.CodeCave2.Warp);

            if (location.HasCoordinates)
            {
                var coordsAddr = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.WarpCoords.Coords;
                var origin = Offsets.Hooks.WarpCoords;
                var codeBlockAddr = _codeCave2 + (int)CodeCaveOffsets.CodeCave2.WarpCoords.CodeBlock;

                byte[] byteArray = new byte[4 * sizeof(float)];
                Buffer.BlockCopy(location.Coords, 0, byteArray, 0, Math.Min(location.Coords.Length, 3) * sizeof(float));
                BitConverter.GetBytes(1.0f).CopyTo(byteArray, 3 * sizeof(float));

                _memoryIo.WriteBytes(coordsAddr, byteArray);

                byte[] coordWarpBytes = AsmLoader.GetAsmBytes("WarpCoords");
                bytes = BitConverter.GetBytes(coordsAddr.ToInt64());
                Array.Copy(bytes, 0, coordWarpBytes, 3, 8);
                int originOffset = (int)(origin + 8 - (codeBlockAddr.ToInt64() + 33));
                bytes = BitConverter.GetBytes(originOffset);
                Array.Copy(bytes, 0, coordWarpBytes, 29, 4);

                _memoryIo.WriteBytes(codeBlockAddr, coordWarpBytes);

                IntPtr loadingFlagAddr = _memoryIo.FollowPointers(Offsets.FileMan.Base,new[] { Offsets.FileMan.LoadedFlag }, false);

                if (!WaitForLoadingFlag(loadingFlagAddr, 1))
                {
                    return;
                }

                _hookManager.InstallHook(codeBlockAddr.ToInt64(), origin,
                    new byte[] { 0x66, 0x0F, 0x7F, 0x80, 0x80, 0x0A, 0x00, 0x00 });

                if (!WaitForLoadingFlag(loadingFlagAddr, 0))
                {
                }

                _hookManager.UninstallHook(codeBlockAddr.ToInt64());
            }
        }

        private bool WaitForLoadingFlag(IntPtr loadingFlagAddr, int expectedValue)
        {
            int startTime = Environment.TickCount;

            while (Environment.TickCount - startTime < 10000)
            {
                int loadingValue = _memoryIo.ReadInt32(loadingFlagAddr);
                if (loadingValue == expectedValue)
                {
                    return true;
                }
                Thread.Sleep(100);
            }
            return false;
        }

        public void ToggleFilter(int value)
        {
            var filterPtr = _memoryIo.FollowPointers(Offsets.FieldArea.Base, new[]
                { Offsets.FieldArea.RenderPtr, Offsets.FieldArea.FilterRemoval}, false );
            _memoryIo.WriteByte(filterPtr, value);
        }

        public void ShowMenu(Offsets.MenuMan.MenuManData menuType)
        {
            var menuPtr = _memoryIo.FollowPointers(Offsets.MenuMan.Base, new[] {(int)menuType }, false);
            _memoryIo.WriteByte(menuPtr, 1);
        }
    }
}