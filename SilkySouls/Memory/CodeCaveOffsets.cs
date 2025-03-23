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
                Angle = 0x70,
                CoordCode = 0x80,
                AngleCode = 0xB0
            }

            public enum NoClip
            {
                ZDirectionVariable = 0xE0,
                InAirTimer = 0x100,
                ZDirectionKbCheck = 0x140,
                ZDirectionL2Check = 0x1A0,
                ZDirectionR2Check = 0x1D0,
                UpdateCoords = 0x250,
            }

            public const int RestoreCasts = 0x380;

            public enum LevelUp
            {
                SoulsPtr = 0x3B0,
                StatsArray = 0x3C0,
                CodeBlock = 0x3F0,
                NewLevel = 0x644,
                RequiredSouls = 0x64C,
                CurrentSouls = 0x650,
            }
        }

        public static class CodeCave3
        {
            public static IntPtr Base;
            public const int ItemSpawn = 0x10;
        }
    }
}