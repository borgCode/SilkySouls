using System;
using System.Threading.Tasks;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services
{
    public class EventService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        
        private IntPtr _emevdCodeLoc;
        private bool _isEmevdCodeWritten;
        
        public EventService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }


        public void ToggleDisableEvents(bool isDisableEventsEnabled)
        {
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(DebugEventMan.Base) + DebugEventMan.DisableEvents,
                isDisableEventsEnabled ? 1 : 0);
        }

        public void SetEvent(ulong flagId, int setVal)
        {
            var eventMan = _memoryIo.ReadInt64(EventFlagMan.Base);
            var setEventBytes = AsmLoader.GetAsmBytes("SetEvent");
            var bytes = BitConverter.GetBytes(eventMan);
            Array.Copy(bytes, 0, setEventBytes, 0x2, 8);
            bytes = BitConverter.GetBytes(flagId);
            Array.Copy(bytes, 0, setEventBytes, 0xA + 2, 8);
            bytes = BitConverter.GetBytes(setVal);
            Array.Copy(bytes, 0, setEventBytes, 0x14 + 2, 4);
            bytes = BitConverter.GetBytes(Funcs.SetEvent);
            Array.Copy(bytes, 0, setEventBytes, 0x24 + 2, 8);
            _memoryIo.AllocateAndExecute(setEventBytes);
        }

        public void SetMultipleEventsOn(params ulong[] flagIds)
        {
            foreach (var flagId in flagIds)
            {
                SetEvent(flagId, 1);
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
        
        public void RingGargBell()
        {
            SetEvent(GameIds.EventFlags.GargBell, 1);
            if (GetEvent(GameIds.EventFlags.QuelaagBell))SetEvent(GameIds.EventFlags.Sens, 1);
        }
        public void RingQuelaagBell()
        {
            SetEvent(GameIds.EventFlags.QuelaagBell, 1);
            if (GetEvent(GameIds.EventFlags.GargBell))SetEvent(GameIds.EventFlags.Sens, 1);
        }
        
        public async Task OpenSensGate(ulong sens)
        {
            SetEvent(sens, 1);
            ExecuteEmevdCommand(GameIds.EmevdCommands.ReproduceObjectAnimation,
                GameIds.EmevdCommandParams.SensDoor);
            await Task.Delay(1000);
            _hookManager.UninstallHook(_emevdCodeLoc.ToInt64());
        }

        private void ExecuteEmevdCommand(int[] commandParams, int[] funcParams)
        {
            var hookLoc = Hooks.Emevd;
            var codeCaveBase = CodeCaveOffsets.Base;
            var commandParamsLoc = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.CommandParams;
            var funcParamsLoc = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.FuncParams;
            var flag = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.Flag;
            
            if (_isEmevdCodeWritten)
            {
                _memoryIo.WriteInt32(commandParamsLoc, commandParams[0]);
                _memoryIo.WriteInt32(commandParamsLoc + 0x4, commandParams[1]);
                _memoryIo.WriteInt32(funcParamsLoc, funcParams[0]);
                _memoryIo.WriteInt32(funcParamsLoc + 0x4, funcParams[1]);
                _memoryIo.WriteByte(flag, 0);
            }
            else
            {
                var xmmStorage = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.XmmStorage;
                var paramStruct = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.ParamStruct;
                _emevdCodeLoc = codeCaveBase + (int)CodeCaveOffsets.EmevdCommand.Code;

                _memoryIo.WriteInt32(commandParamsLoc, commandParams[0]);
                _memoryIo.WriteInt32(commandParamsLoc + 0x4, commandParams[1]);
                _memoryIo.WriteInt32(funcParamsLoc, funcParams[0]);
                _memoryIo.WriteInt32(funcParamsLoc + 0x4, funcParams[1]);

                var codeBytes = AsmLoader.GetAsmBytes("ScriptCommands");
                AsmHelper.WriteRelativeOffsets(codeBytes, new[]
                {
                    (_emevdCodeLoc.ToInt64(), flag.ToInt64(), 7, 0x2),
                    (_emevdCodeLoc.ToInt64() + 0xD, flag.ToInt64(), 7, 0xD + 0x2),
                    (_emevdCodeLoc.ToInt64() + 0x32, xmmStorage.ToInt64(), 8, 0x32 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x3A, xmmStorage.ToInt64() + 0x10, 8, 0x3A + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x42, xmmStorage.ToInt64() + 0x20, 8, 0x42 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x4A, xmmStorage.ToInt64() + 0x30, 8, 0x4A + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x52, xmmStorage.ToInt64() + 0x40, 8, 0x52 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x5A, xmmStorage.ToInt64() + 0x50, 8, 0x5A + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x62, xmmStorage.ToInt64() + 0x60, 8, 0x62 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x6A, xmmStorage.ToInt64() + 0x70, 9, 0x6A + 0x5),
                    (_emevdCodeLoc.ToInt64() + 0x73, xmmStorage.ToInt64() + 0x80, 9, 0x73 + 0x5),
                    (_emevdCodeLoc.ToInt64() + 0x7C, paramStruct.ToInt64(), 7, 0x7C + 0x3),
                    (_emevdCodeLoc.ToInt64() + 0x83, commandParamsLoc.ToInt64(), 7, 0x83 + 0x3),
                    (_emevdCodeLoc.ToInt64() + 0x91, funcParamsLoc.ToInt64(), 7, 0x91 + 0x3),
                    (_emevdCodeLoc.ToInt64() + 0x9F, EmkEventIns.Base.ToInt64(), 7, 0x9F + 0x3),
                    (_emevdCodeLoc.ToInt64() + 0xB5, Funcs.ProcessEmevdCommand, 5, 0xB5 + 0x1),
                    (_emevdCodeLoc.ToInt64() + 0xBE, xmmStorage.ToInt64() + 0x80, 9, 0xBE + 0x5),
                    (_emevdCodeLoc.ToInt64() + 0xC7, xmmStorage.ToInt64() + 0x70, 9, 0xC7 + 0x5),
                    (_emevdCodeLoc.ToInt64() + 0xD0, xmmStorage.ToInt64() + 0x60, 8, 0xD0 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0xD8, xmmStorage.ToInt64() + 0x50, 8, 0xD8 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0xE0, xmmStorage.ToInt64() + 0x40, 8, 0xE0 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0xE8, xmmStorage.ToInt64() + 0x30, 8, 0xE8 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0xF0, xmmStorage.ToInt64() + 0x20, 8, 0xF0 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0xF8, xmmStorage.ToInt64() + 0x10, 8, 0xF8 + 0x4),
                    (_emevdCodeLoc.ToInt64() + 0x100, xmmStorage.ToInt64(), 8, 0x100 + 0x4)
                });

                var jumpBytes = AsmHelper.GetJmpOriginOffsetBytes(hookLoc, 5, _emevdCodeLoc + 0x12D);
                Array.Copy(jumpBytes, 0, codeBytes, 0x128 + 1, 4);

                _memoryIo.WriteBytes(_emevdCodeLoc, codeBytes);

                _isEmevdCodeWritten = true;
            }

            _hookManager.InstallHook(_emevdCodeLoc.ToInt64(), hookLoc, new byte[]
            {
                0xBA, 0x01, 0x00, 0x00, 0x00,
            });
        }

        public async Task PlaceLordVessel()
        {
            SetEvent(GameIds.EventFlags.PlaceLordVessel, 1);
            SetEvent(GameIds.EventFlags.DukesAfterLordVessel, 1);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeactiveObject, GameIds.EmevdCommandParams.DukesFogDeactiveObject);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeleteMapSfx, GameIds.EmevdCommandParams.DukesFogDeleteMapSfx);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeactiveObject, GameIds.EmevdCommandParams.DemonRuinsFogDeactiveObject);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeleteMapSfx, GameIds.EmevdCommandParams.DemonRuinsFogDeleteMapSfx);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeactiveObject, GameIds.EmevdCommandParams.NitoFogDeactiveObject);
            await Task.Delay(5);
            ExecuteEmevdCommand(GameIds.EmevdCommands.DeleteMapSfx, GameIds.EmevdCommandParams.NitoFogDeleteMapSfx);
            await Task.Delay(500);
            _hookManager.UninstallHook(_emevdCodeLoc.ToInt64());
        }
    }
}