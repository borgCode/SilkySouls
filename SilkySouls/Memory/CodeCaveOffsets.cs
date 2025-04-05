using System;

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
            
            public const int ItemSpawnFlagLoc = 0x6A0;
            public const int ItemSpawn = 0x6B0;
            
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
                DesiredAct = 0xAF0,
                Count = 0xAF4,
                Flag = 0xB14,
                LastAct = 0xB18,
                ActManipCode = 0xB20,
                OpcodeHistory = 0xC00,
                RepeatActFlagLoc = 0xC10,
                OpcodeCheckCode = 0xC20,
                EnemyIdV2 = 0xD10,
                EnemyIdLen = 0xD20,
                EnemySavedPtr = 0xD30,
                EnemyIdCheckCode = 0xD40,
                
            }
    }
}