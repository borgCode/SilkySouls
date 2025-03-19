using System;

namespace DSRForge.memory
{
    public static class Offsets
    {
        public static class WorldChrMan
        {
            public static IntPtr Base;
            
            public enum BaseOffsets
            {
                PlayerIns = 0x68,
                UpdateCoordsBasePtr = 0x40,
            }
            
            public const int UpdateCoords = 0x28;
            
            public enum PlayerInsOffsets
            {
                CoordsPtr1 = 0x18,
                PlayerCtrl = 0x68,
                PadMan = 0x70,
                Health = 0x3E8,
                MaxHealth = 0x3EC,
                NoDamage = 0x524,
                InfiniteStam = 0x525,
                NoGoodsConsume = 0x527,
            }
            
            public const byte NoDamage = 1 << 6;
            public const byte InfiniteStam = 1 << 2;
            public const byte NoGoodsConsume = 1 << 0;
            
            public const int PlayerAnim = 0x18;
            public const int PlayerAnimSpeed = 0xA8;
            
            public const int CoordsPtr2 = 0x28;
            public const int CoordsPtr3 = 0x50;
            public const int CoordsPtr4 = 0x20;

            public enum Coords
            {
                X = 0x120,
                Z = 0x124,
                Y = 0x128,
            }
        }

        public static class DebugFlags
        {
            public static IntPtr Base;
            
            public const int NoDeath = 0x0;
            public const int OneShot = 0x1;
            public const int NoAmmoConsume = 0x4;
            public const int InfiniteCasts = 0x5;
            public const int Invisible = 0x6;
            public const int Silent = 0x7;
            public const int AllNoDeath = 0x8;
            public const int AllNoDamage = 0x9;
            public const int DisableAi = 0xD;
        }

        public static class Cam
        {
            public static IntPtr Base;
            
            public const int ChrCam = 0x60;
            public const int ChrExFollowCam = 0x60;
        }
        
        public static class GameDataMan
        {
            public static IntPtr Base;
            
            public enum GameData
            {
                PlayerGameData = 0x10,
                Ng = 0x78,
            }
            
            public enum PlayerGameData
            {
                Vitality = 0x40,
                Attunement = 0x48,
                Endurance = 0x50,
                Strength = 0x58,
                Dexterity = 0x60,
                Intelligence = 0x68,
                Faith = 0x70,
                Humanity = 0x84,
                Resistance = 0x88,
                SoulLevel = 0x90,
                Souls = 0x94,
                TotalSouls = 0x98,

                EquipMagicData = 0x418,
            }
        }
        
        public static long ItemGet;
        public static IntPtr ItemGetMenuMan;
        public static long ItemDlgFunc;
        public static long LevelUpFunc;
        public static long RestoreCastsFunc;

        public static class FieldArea
        {
            public static IntPtr Base;
            public const int RenderPtr = 0x28;
            public const int FilterRemoval = 0x34D;
        }

        public static class GameMan
        {
            public static IntPtr Base;
            public const int BonfireCoords = 0xA80;
            public const int LastBonfire = 0xB34;
        }

        public static IntPtr WarpEvent;
        public static long WarpFunc;

        public static class DamageMan
        {
            public static IntPtr Base;
            public const int HitboxFlag = 0x30;
        }

        public static IntPtr DrawEventPatch;
        public static IntPtr DrawSoundViewPatch;

        public static class MenuMan
        {
            public static IntPtr Base;
            
            public enum MenuManData
            {
                LevelUpMenu = 0x8C,
                AttunementMenu = 0x94,
                Quitout = 0x24C,
                LoadedFlag = 0x258
            }
        }

        public static IntPtr QuitoutPatch;

        public enum LockedTarget
        {
            TargetHp = 0x3E8,
            TargetMaxHp = 0x3EC,
            CurrentPoise = 0x250,
            MaxPoise = 0x254,
            PoiseTimer = 0x25C,
        }

        public static class ProgressionFlagMan
        {
            public static IntPtr Base;
            public static int Offset = 0x0;
            public static int BonfireFlags = 0x18;

            public enum BonfireBitFlag
            {
                OolaSanc = 1 << 13,
                Anorlondo1 = 1 << 29,
                Gwyndolin = 1 << 15,
                Parish = 1 << 8,
                sunlight = 1 << 18,
                depths = 1 << 9,
                Quelana = 1 << 19,
                ashlake = 1 << 22,
                OS = 1 << 16,
                paintedWorld = 1 << 7,
                Dukearchives = 1 << 5,
                vamos = 1 << 3,
                oolatown = 1 << 12,
                ooladung = 1 << 10,
                tomb = 1 << 6,
                
            }
        }

        public static class HgDraw
        {
            public static IntPtr Base;
            public const int EzDraw = 0x58;
        }
        
        
        
        public static class Hooks
        {
            public static long LastLockedTarget;
            public static long RepeatAction;
            public static long AllNoDamage;
            public static long ItemSpawn;
            public static long Draw;
            public static long TargetingView;
            public static long InAirTimer;
            public static long Keyboard;
            public static long ControllerR2;
            public static long ControllerL2;
            public static long UpdateCoords;
            public static long WarpCoords;
        }
    }
}