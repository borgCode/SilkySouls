using System;

namespace SilkySouls.Memory
{
    public static class CodeCaveOffsets
    {
        public static class CodeCave1
        {
            public static IntPtr Base;

            public const int EnableDraw = 0x0;
            public const int TargetView = 0x200;
            public const int LockedTarget = 0x220;
            public const int LockedTargetPtr = 0x270;
            public const int RepeatActionFlag = 0x280;
            public const int RepeatAction = 0x290;
            public const int AllNoDamage = 0x300;
        }

        public static class CodeCave2
        {
            public static IntPtr Base;

            public const int SavePos1 = 0x0;
            public const int SavePos2 = 0x10;

            public const int Warp = 0x20;

            public enum WarpCoords
            {
                Coords = 0x60,
                CodeBlock = 0x70
            }

            public enum NoClip
            {
                ZDirectionVariable = 0x90,
                InAirTimer = 0xB0,
                ZDirectionKbCheck = 0xF0,
                ZDirectionL2Check = 0x150,
                ZDirectionR2Check = 0x180,
                UpdateCoords = 0x200,
            }

            public const int RestoreCasts = 0x330;

            public enum LevelUp
            {
                SoulsPtr = 0x360,
                StatsArray = 0x370,
                CodeBlock = 0x3A0,
                NewLevel = 0x5F4,
                RequiredSouls = 0x5FC,
                CurrentSouls = 0x600,
            }
        }

        public static class CodeCave3
        {
            public static IntPtr Base;
            public const int ItemSpawn = 0x10;
        }
    }
}