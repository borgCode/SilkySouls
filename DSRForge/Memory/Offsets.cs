namespace DSRForge.memory
{
    public static class Offsets
    {
        public const int WorldChrMan = 0x1D151B0;

        
        public enum WorldChrOffsets
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

        public enum PadMan
        {
            PlayerYMovement = 0x264,
            PlayerXMovement = 0x2b4,
            CircularBuffer = 0x304,
        }
  
        public const int CoordsPtr2 = 0x28;
        public const int CoordsPtr3 = 0x50;
        public const int CoordsPtr4 = 0x20;

        public enum Coords
        {
            X = 0x120,
            Z = 0x124,
            Y = 0x128,
            
        }
        
        public const int NoDeath = 0x1D151C9;
        public const int OneShot = 0x1D151CA;
        public const int Invisible = 0x1D151CF;
        public const int Silent = 0x1D151D0;
        public const int DisableAi = 0x1D151D6;
        public const int AllNoDamage = 0x1D151D2;
        public const int AllNoDeath = 0x1D151D1;
        public const int InfiniteCasts = 0x1D151CE;
        public const int NoAmmoConsume = 0x1D151CD;

        public const int ResistGaugeMenuMan = 0x1D26608;
        public const int CamBase = 0x1D0;

        public const int FileMan = 0x1D1E4F8;
        public const int LoadingScreenFlag = 0x168;
        
        public const int GameDataMan = 0x1D278F0;
        
        
        public enum GameData
        {
            PlayerGameData = 0x10,
            Ng = 0x78,
            Inventory = 0xF90,
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
        
        public const int ItemGet = 0x744D00;
        public const int ItemGetMenuMan = 0x01D26578;
        public const int ItemDlgFunc = 0x725FB0;

        public const int FieldArea = 0x1D173C8;
        public const int RenderPtr = 0x28;
        public const int FilterRemoval = 0x34D;
        
        public const int GameMan = 0x1D10E18;

        public enum GameManData
        {
            BonfireCoords = 0xA80,
            LastBonfire = 0xB34,
        }
        
        public const int DamageMan = 0x1D173C0;

        public enum DamManOffsets
        {
            HitboxFlag = 0x30,
        }
        
        public const int DrawEventPatch = 0x49B6B7;
        public const int DrawSoundViewPatch = 0x622289;
        
        

        public const int Warp = 0x744D00; // use with chr man
        
        
        // Likely menu man
        public const int MenuMan = 0x1d26168;

        public enum MenuManData
        {
            //TODO Set byte to 1
            LevelUpMenu = 0x8C,
            AttunementMenu = 0x94,
        }
        
        public const int DbgFlags = 0x3336B1;


        public const int LastLockOnTarget = 0x2E76196;

        public enum LockedTarget
        {
            TargetHp = 0x3E8,
            TargetMaxHp = 0x3EC,
            CurrentPoise = 0x250, 
            MaxPoise = 0x254,
            PoiseTimer = 0x25C,
        }
      

        //Not sure if needed
        public const int EnemyId = 0xC8;
        public const int EnemyLookDirection = 0x110;
        // +4 for Y, +8 for Z

        // has item dbg menu test 1413faa58
        // worldmapmanimpl 0x141313D50

    }
}