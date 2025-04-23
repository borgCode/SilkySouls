namespace SilkySouls.Memory
{
    public static class GameIds
    {
        public static class EventFlags
        {
            public static readonly ulong[] UnlockKalameet = { 0xAB0F60, 0x71D };
            public const ulong GargBell = 0xA8028C;
            public const ulong QuelaagBell = 0xADF408;
            public const ulong Sens = 0xAF7AA8;
            public const ulong PlaceLordVessel = 0xB40E24;
            public const ulong NewLondoWater = 0xB100E5;
            public const ulong LaurentiusToFirelink = 0x4E3;
            public static readonly ulong[] GriggsToFirelink = {0x458, 0xA82996};
            public static readonly ulong[] LoganToFirelink = {0x443, 0xAF7C32};
        }

        public static class ShopParams
        {
            public static readonly ulong[] MaleUdMerchant = { 0x44C, 0x4AF };
            public static readonly ulong[] FemaleUdMerchant = { 0x4B0, 0x513 };
            public static readonly ulong[] Zena = { 0x5DC, 0x63F };
            public static readonly ulong[] Patches = { 0x640, 0x6A3 };
            public static readonly ulong[] Shiva = { 0x6A4, 0x707 };
            public static readonly ulong[] Griggs = { 0x7D0, 0x7E3 };
            public static readonly ulong[] Dusk = { 0x898, 0x8FB };
            public static readonly ulong[] Ingward = { 0x960, 0x9C3 };
            public static readonly ulong[] Laurentius = { 0xBB8, 0xC1B };
            public static readonly ulong[] Eingyi = { 0xC80, 0xC89 };
            public static readonly ulong[] Quelana = { 0xD48, 0xDAB };
            public static readonly ulong[] Reah = { 0x1068, 0x10CB };
            public static readonly ulong[] Petrus = { 0xFA0, 0x1003 };
            public static readonly ulong[] Oswald = { 0x1130, 0x1193 };
            public static readonly ulong[] Logan = { 0x1388, 0x13EB };
            public static readonly ulong[] CrestfallenMerchant = { 0x14B4, 0x15DF };
            public static readonly ulong[] Chester = { 0x1900, 0x1963 };
            public static readonly ulong[] Elizabeth = { 0x1964, 0x19C7 };
            public static readonly ulong[] Gough = { 0x19C8, 0x1A2B };
        }

        public static class EmevdCommands
        {
            public static readonly int[] ReproduceObjectAnimation = { 0x7D5, 0x7 };
            public static readonly int[] DeactiveObject = { 0x7D5, 0x3 };
            public static readonly int[] DeleteMapSfx = { 0x7D6, 0x1 };

        }

        public static class EmevdCommandParams
        {
            public static readonly int[] SensDoor = { 0x16E748, 0x0 };
            public static readonly int[] DukesFogDeactiveObject = { 0x19F74E, 0x0 };
            public static readonly int[] DukesFogDeleteMapSfx = { 0x19F74F, 0x0 };
            public static readonly int[] DemonRuinsFogDeactiveObject = { 0x158A7E, 0x0 };
            public static readonly int[] DemonRuinsFogDeleteMapSfx = { 0x158A7F, 0x0 };
            public static readonly int[] NitoFogDeactiveObject = { 0x1403DE, 0x0 };
            public static readonly int[] NitoFogDeleteMapSfx = { 0x1403DF, 0x0 };
        }
    }
}