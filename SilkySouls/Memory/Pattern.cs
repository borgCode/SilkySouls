namespace SilkySouls.Memory
{
    public class Pattern
    {
        public byte[] Bytes { get; }
        public string Mask { get; }
        public int InstructionOffset { get; }
        public RipType RipType { get; }

        public Pattern(byte[] bytes, string mask, int instructionOffset, RipType ripType)
        {
            Bytes = bytes;
            Mask = mask;
            InstructionOffset = instructionOffset;
            RipType = ripType;
        }
    }

    public static class Patterns
    {
        public static readonly Pattern WorldChrMan = new Pattern(
            new byte[] { 0x90, 0x48, 0x8B, 0x03, 0x8B, 0x48 },
            "xxxxxx",
            15,
            RipType.Standard
        );

        public static readonly Pattern DebugFlags = new Pattern(
            new byte[] { 0x80, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x8F },
            "xx????xxxx",
            0,
            RipType.Comparison
        );

        public static readonly Pattern CamBase = new Pattern(
            new byte[] { 0x83, 0xBF, 0x88, 0x00, 0x00, 0x00, 0x03, 0x75 },
            "xxxxxxxx",
            15,
            RipType.Standard
        );


        public static readonly Pattern GameDataMan = new Pattern(
            new byte[] { 0x8B, 0x70, 0x4C, 0x89 },
            "xxxx",
            9,
            RipType.Standard
        );

        public static readonly Pattern ItemGetFunc = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5C, 0x24, 0x18, 0x89, 0x54, 0x24, 0x10, 0x55, 0x56, 0x57, 0x41, 0x54, 0x41, 0x55, 0x41,
                0x56, 0x41, 0x57, 0x48, 0x8D
            },
            "xxxxxxxxxxxxxxxxxxxxxx",
            0,
            RipType.None
        );

        public static readonly Pattern ItemGetMenuMan = new Pattern(
            new byte[] { 0x90, 0x41, 0x8B, 0xCC, 0xE8 },
            "xxxxx",
            17,
            RipType.Standard
        );

        public static readonly Pattern ItemGetDlgFunc = new Pattern(
            new byte[] { 0x40, 0x53, 0x4C, 0x8B, 0xD9 },
            "xxxxx",
            0,
            RipType.None
        );

        public static readonly Pattern FieldArea = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x85, 0xC0, 0x74, 0x1A, 0xC6 },
            "xxx????xxxxxx",
            0,
            RipType.Standard
        );

        public static readonly Pattern GameMan = new Pattern(
            new byte[] { 0xEB, 0x19, 0x48, 0x8D, 0x8E, 0x00, 0x00, 0x00, 0x00, 0xE8 },
            "xxxxx????x",
            14,
            RipType.Standard
        );

        public static readonly Pattern DamMan = new Pattern(
            new byte[] { 0x66, 0x0F, 0x7F, 0x83, 0x80, 0x01, 0x00, 0x00, 0x8B },
            "xxxxxxxxx",
            19,
            RipType.Standard
        );

        public static readonly Pattern SoloParamMan = new Pattern(
            new byte[] { 0x4C, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x63, 0xC9 },
            "xxx????xxx",
            0,
            RipType.Standard);

        public static readonly Pattern DrawEventPatch = new Pattern(
            new byte[] { 0x00, 0x80, 0x79, 0x19, 0x00, 0x75, 0x48 },
            "xxxxxxx",
            0,
            RipType.None
        );

        public static readonly Pattern DrawSoundViewPatch = new Pattern(
            new byte[] { 0x00, 0x48, 0x8B, 0xF2, 0x74 },
            "xxxxx",
            0,
            RipType.None
        );

        public static readonly Pattern MenuMan = new Pattern(
            new byte[] { 0x81, 0xF9, 0xF3, 0x01 },
            "xxxx",
            8,
            RipType.Standard
        );

        public static readonly Pattern QuitoutPatch = new Pattern(
            new byte[] { 0x74, 0x35, 0x83, 0xBB },
            "xxxx",
            8,
            RipType.None);

        public static readonly Pattern ProgressionFlagMan = new Pattern(
            new byte[] { 0x8B, 0x13, 0x85, 0xD2, 0x7E, 0x15, 0x48 },
            "xxxxxxx",
            6,
            RipType.Standard
        );

        public static readonly Pattern LevelUpFunc = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x74, 0x24, 0x20, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00,
                0x4C
            },
            "xxxxxxxxxxxxx????x",
            0,
            RipType.None);

        public static readonly Pattern RestoreCastsFunc = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5C, 0x24, 0x08, 0x48, 0x89, 0x74, 0x24, 0x10, 0x57, 0x48, 0x83, 0xEC, 0x30, 0x48, 0x8D,
                0x59
            },
            "xxxxxxxxxxxxxxxxxx",
            0,
            RipType.None);

        public static readonly Pattern HgDraw = new Pattern(
            new byte[] { 0x48, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x40, 0x58, 0xC3 },
            "xxx????xxxxx",
            0,
            RipType.Standard);

        public static readonly Pattern WarpEvent = new Pattern(
            new byte[] { 0x48, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8D, 0x55, 0xC0 },
            "xxx????xxxx",
            0,
            RipType.Standard);

        public static readonly Pattern WarpFunc = new Pattern(
            new byte[]
            {
                0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xD9, 0x8B, 0xFA, 0x48, 0x8B,
                0x49, 0x08, 0x48, 0x85, 0xC9, 0x0F
            },
            "xxxxxxxxxxxxxxxxxxxxxxx",
            0,
            RipType.None);

        public static readonly Pattern LastLockedTarget = new Pattern(
            new byte[] { 0x48, 0x8D, 0x54, 0x24, 0x38, 0x89, 0x44, 0x24, 0x3C },
            "xxxxxxxxx",
            0,
            RipType.None);

        public static readonly Pattern RepeatAction = new Pattern(
            new byte[] { 0x48, 0x8B, 0x41, 0x08, 0x0F, 0xBE },
            "xxxxxx",
            0,
            RipType.None);

        public static readonly Pattern AllNoDamage = new Pattern(
            new byte[] { 0xF6, 0x81, 0x24, 0x05, 0x00, 0x00, 0x40, 0x48 },
            "xxxxxxxx",
            0,
            RipType.None);

        public static readonly Pattern ItemSpawnHook = new Pattern(
            new byte[] { 0xEB, 0x03, 0x48, 0x8B, 0x3F, 0x48, 0x3B, 0x7E, 0x28, 0x75, 0x00, 0xC6 },
            "xxxxxxxxxx?x",
            11,
            RipType.None);

        public static readonly Pattern DrawHook = new Pattern(
            new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x44, 0x8B, 0x85, 0xAC, 0x00, 0x00, 0x00, 0x8B, 0xD6 },
            "x????xxxxxxxxx",
            29,
            RipType.None);

        public static readonly Pattern TargetingView = new Pattern(
            new byte[]
            {
                0x40, 0x53, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xD9, 0x48, 0x8B, 0x49, 0x08, 0x48, 0x85, 0xC9, 0x74,
                0x64
            },
            "xxxxxxxxxxxxxxxxxx",
            0,
            RipType.None);

        public static readonly Pattern InAirTimer = new Pattern(
            new byte[] { 0xF3, 0x0F, 0x58, 0x9B },
            "xxxx",
            0,
            RipType.None);

        public static readonly Pattern Keyboard = new Pattern(
            new byte[] { 0xC6, 0x43, 0xF0, 0x01 },
            "xxxx",
            0,
            RipType.None);

        public static readonly Pattern ControllerR2 = new Pattern(
            new byte[] { 0x0F, 0xB6, 0x44, 0x24, 0x27 },
            "xxxxx",
            0,
            RipType.None);

        public static readonly Pattern ControllerL2 = new Pattern(
            new byte[] { 0x0F, 0xB6, 0x44, 0x24, 0x26, 0x3C },
            "xxxxxx",
            0,
            RipType.None);

        public static readonly Pattern UpdateCoords = new Pattern(
            new byte[] { 0x0F, 0x29, 0x81, 0x20, 0x01, 0x00, 0x00, 0x48 },
            "xxxxxxxx",
            0,
            RipType.None);

        public static readonly Pattern WarpCoords = new Pattern(
            new byte[] { 0x66, 0x0F, 0x7F, 0x80, 0x80, 0x0A },
            "xxxxxx",
            0,
            RipType.None);
    }
}