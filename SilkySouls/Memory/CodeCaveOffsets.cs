﻿using System;

namespace SilkySouls.Memory
{
    public static class CodeCaveOffsets
    {
       
            public static IntPtr Base;
            public const int EnableDraw = 0x0;
            public const int TargetView = 0x200;
            public const int LockedTarget = 0x220;
            public const int LockedTargetPtr = 0x270;
            public const int AllNoDamage = 0x300;
            public const int SavePos1 = 0x320;
            public const int SavePos2 = 0x330;
            
            public enum WarpCoords
            {
                Coords = 0x380,
                Angle = 0x390,
                CoordCode = 0x3A0,
                AngleCode = 0x3D0
            }
            
            public enum NoClip
            {
                ZDirectionVariable = 0x400,
                InAirTimer = 0x420,
                ZDirectionKbCheck = 0x460,
                ZDirectionL2Check = 0x4C0,
                ZDirectionR2Check = 0x4F0,
                UpdateCoords = 0x570,
            }

            public enum ItemSpawn
            {
                ShouldExitFlag = 0x6A0,
                ShouldProcessFlag = 0x6A1,
                Code = 0x6B0,
            }
            
            public enum LevelUp 
            {
                SoulsPtr = 0x7F0,
                StatsArray = 0x800,
                CodeBlock = 0x830,
                NewLevel = 0xA84,
                RequiredSouls = 0xA8C,
                CurrentSouls = 0xA90,
            }

            public enum RepeatAct
            {
                TargetActIndex = 0xAF0,
                LuaIfCounter = 0xAF4,
                IfConditionFlag = 0xB14,
                FinalActIndex = 0xB18,
                LuaIfManipulationCode = 0xB20,
                LuaSwitchHistory = 0xC00,
                LuaSwitchPatternMatchFlag = 0xC10,
                LuaSwitchCheckCode = 0xC20,
                EnemyId = 0xD40,
                EnemyIdLength = 0xD50,
                EnemyRaxIdentifier = 0xD60,
                EnemyIdentifierCode = 0xD70,
            }

            public enum EmevdCommand
            {
                XmmStorage = 0x1000, 
                CommandParams = 0x1090,
                FuncParams = 0x1098,
                Flag = 0x10A0,
                ParamStruct = 0x10B0,
                Code = 0x11B0,
            }

            public const int GetEventResult = 0x13B0;

    }
}