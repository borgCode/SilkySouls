using System;

namespace SilkySouls.Memory
{
    public static class CodeCaveOffsets
    {
       
            public static nint Base;
            public const int EnableDraw = 0x0;
            public const int TargetView = 0x200;
            public const int LockedTarget = 0x220;
            public const int LockedTargetPtr = 0x270;
            public const int AllNoDamage = 0x300;
            
            public enum WarpCoords
            {
                Coords = 0x380,
                Angle = 0x390,
                CoordCode = 0x3A0,
                AngleCode = 0x3D0
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

            public const int DisableFourKingsGenerator = 0x1000;
      

            public const int GetEventResult = 0x13B0;

            public const int EmevdResult = 0x13F9;
            public const int EmevdArgs = 0x1400;

            public const int EzStateTalkCode = 0x1500;
            public const int EzStateTalkParams = 0x1600;

            public const int ZDirection = 0x1A00;
            public const int SpeedScale = 0x1A04;
            public const int InAirTimer = 0x1A20;
            public const int Kb = 0x1A60;
            public const int TriggerL2 = 0x1AC0;
            public const int TriggerR2 = 0x1BC0;
            public const int UpdateCoords = 0x1C70;

            public const int EnableFlag = 0x2400;
            public const int LoadedEnableDraw = 0x2410;

            // LoadedEnableDraw ends at 0x2638
            public const int NegateEnemyHitsFlag = 0x2700;
            public const int LastAttackerPtr = 0x2710;
            public const int HitCode = 0x2720;

    }
}