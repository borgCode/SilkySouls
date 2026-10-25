using System;
using SilkySouls.Enums;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.Enums.GameVersion;

namespace SilkySouls.memory
{
    public static class Offsets
    {
        private static GameVersion? _version;

        public static GameVersion Version => _version
                                             ?? Version1_0_3_1;

        public static bool Initialize(long fileSize, nint moduleBase)
        {
            _version = fileSize switch
            {
                74186240 => Version1_0_1_0,
                75245056 => Version1_0_1_1,
                56756736 => Version1_0_1_2,
                57067008 => Version1_0_3_0,
                50286344 => Version1_0_3_1,
                _ => null
            };

            if (!_version.HasValue)
            {
                MsgBox.Show(
                    $@"Unknown patch version (file size: {fileSize}), please report it on GitHub",
                    "Unknown patch version");
                return false;
            }


            InitializeBaseAddresses(moduleBase);
            return true;
        }

        public static class WorldChrMan
        {
            public static nint Base;

            public const int PlayerIns = 0x68;

            public const int ChrSet = 0x30;
            public const int ChrSetCount = 0x130;
            public const int ChrSetEntries = 0x138;
            public const int ChrSetEntrySize = 0x38;

            public static readonly int[] CurrentBlockId = [0x370, 0x10, 0x288];

            public enum BaseOffsets
            {
                UpdateCoordsBasePtr = 0x40,

                DeathCam = 0x70
            }

            public const int UpdateCoords = 0x28;

            public enum PlayerInsOffsets
            {
                CoordsPtr1 = 0x18,
                PadMan = 0x70,
            }

            public const int CoordsPtr2 = 0x28;
            public const int CoordsPtr3 = 0x50;
            public const int CoordsPtr4 = 0x20;
        }

        public static class ChrIns
        {
            public const int Handle = 0x8;

            public const int ChrCtrl = 0x68;

            public const int ChrId = 0xC8;

            public const int CurrentPoise = 0x250;
            public const int MaxPoise = 0x254;
            public const int PoiseTimer = 0x25C;
            public const int ReadOnlyCoords = 0x2C0;
            public const int Health = 0x3E8;
            public const int MaxHealth = 0x3EC;
            public const int Stamina = 0x3F8;
            public const int PoisonCurrent = 0x418;
            public const int ToxicCurrent = 0x41C;
            public const int BleedCurrent = 0x420;
            public const int PoisonMax = 0x428;
            public const int ToxicMax = 0x42C;
            public const int BleedMax = 0x430;

            
            public static readonly BitFlag NoGravity = new(0x2A5, 1 << 5);
            public static readonly BitFlag InfinitePoise = new(0x2A6, 1 << 0);
            public static readonly BitFlag NoDamage = new(0x524, 1 << 6);
            public static readonly BitFlag InfiniteStam = new(0x525, 1 << 2);
            public static readonly BitFlag NoUpdate = new(0x525, 1 << 7);
            public static readonly BitFlag NoGoodsConsume = new(0x527, 1 << 0);

            public static readonly int[] AnimSpeed = [ChrCtrl, 0x18, 0xA8];
            public static readonly int[] NpcParam = [0x580, 0x8];

            public static readonly int[] PhysicsModule = [ChrCtrl, 0x28];
            public const int Angle = 0x4;
            public const int Coords = 0x10;
            public static readonly int[] HavokCoords = [0x38, 0x80, 0x30, 0x30];
            

            public enum NpcParamOffsets
            {
                AuxImmunitySpEffect = 0x50,
            }
        }

        public static class DebugEventMan
        {
            public static nint Base;
            public const int DisableEvents = 0xDC;
        }

        public static class DebugFlags
        {
            public static nint Base;

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
        
        public static class GameDataMan
        {
            public static nint Base;

            public enum GameDataOffsets
            {
                PlayerGameData = 0x10,
                PcOptionData = 0x68,
                Ng = 0x78,
                InGameTime = 0xA4,
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
                EquipGameData = 0x430,
            }

            public enum PcOptionData
            {
                AntiAliasingMode = 0xF0,
            }
        }

        public static class ItemGetMenuManImpl
        {
            public static nint Base;
        }
        
        public static class FieldArea
        {
            public static nint Base;
            public const int RenderPtr = 0x28;
            public const int FilterRemoval = 0x34D;
            public const int Brightness = 0x350;
            
            public const int ChrCam = 0x38;
            public const int ChrExFollowCam = 0x60;
        }

        public static class GameMan
        {
            public static nint Base;
            public const int LastBonfire = 0xB34;
        }

        public static class PadMan
        {
            public static nint Base;
        }

        public static class EventMan
        {
            public static nint Base;
        }

        public static class DamageManager
        {
            public static nint Base;
            public const int HitboxFlag = 0x30;
        }

        public static class MenuMan
        {
            public static nint Base;

            public const int IsLevelUpMenuOpen = 0x8C;
            public const int IsFadeActive = 0xB8;
            public const int Warp = 0xC0;
            public const int BottomlessBox = 0x98;
            public const int Feed = 0x130;
            public const int Quitout = 0x24C;
            public const int LoadedFlag = 0x258;


        }
        

        public enum LockedTarget
        {
            ForceActPtr = 0xAD0,
        }

        public const int BattleGoalId = 0x4;

        public static readonly int[] NpcThinkParam = [0xAD0, 0xC0];

        public const int ForceActOffset = 0x360;

        public static class EventFlagMan
        {
            public static nint Base;
            public const int FlagPtr = 0x0;
            public const int WarpFlag = 0x5B;
            public const int WarpFlagBit1 = 1;
            public const int WarpFlagBit2 = 5;

            public const int BonfireFlags = 0x18;
            
            public enum BonfireBitFlag
            {
                OolaSanc = 13,
                Anorlondo1 = 20,
                Gwyndolin = 15,
                Parish = 8,
                SunlightAltar = 18,
                Depths = 9,
                Quelana = 21,
                AshLake = 22,
                OS = 16,
                PaintedWorld = 7,
                DukesArchives = 5,
                Vamos = 3,
                OolaTown = 12,
                OolaDungeon = 10,
                TombOfTheGiants = 6,
                Nito = 17,
                Seath = 4,
                FourKings = 19,
                Firelink = 23,
                SancGarden = 14,
                Manus = 11
            }
        }

        public static class HgDraw
        {
            public static nint Base;
            public const int EzDraw = 0x58;
        }

        public static class SoloParamMan
        {
            public static nint Base;
            public const int ParamResCap = 0x570;
            public const int ItemLot = 0x38;
            public const int BkhItemLotEntry = 0x32C30;

            public enum BkhDropRateSlots
            {
                Nothing = 0x40,
                Bkh = 0x42,
                Bks = 0x44,
            }
        }

        public static class WorldAiMan
        {
            public static nint Base;

            public static readonly int[] LuaGlobalTable = [0x17E8, 0x8, 0x28, 0x78];
        }

        public static class EmkEventIns
        {
            public static nint Base;
        }

        public static class EmkSystem
        {
            public static nint Base;
        }

        public static nint DbgMapWalkPadVtable;
        public static nint BeginTargetSceneVtable;
        public static nint EndTargetSceneVtable;
        public static nint HGDrawPlanEntityVtable;
        
        public static class Hooks
        {
            public static nint LastLockedTarget;
            public static nint AllNoDamage;
            public static nint Hit;
            public static nint Draw;
            public static nint InAirTimer;
            public static nint Keyboard;
            public static nint ControllerR2;
            public static nint ControllerL2;
            public static nint UpdateCoords;
            public static nint WarpCoords;
            public static nint WarpAngle;
            public static nint LuaLowerOrEqual;
            public static nint LuaVmSwitch;
            public static nint BattleActivate;
            public static nint Emevd;
            public static nint FourKingsGenerator;
            public static nint HgDrawCommandExecutor;
        }

        public static class Patches
        {
            public static nint DrawEvent;
            public static nint DrawSoundView;
            public static nint InfiniteDurability;
            public static nint NoRoll;
            public static nint NoBackStep;
            public static nint Quitout;
        }

        public static class Functions
        {
            public static nint SetEvent;
            public static nint GetEvent;
            public static nint ExecuteEmevdCommand;
            public static nint GetInventoryIndexByCatAndId;
            public static nint Warp;
            public static nint ItemDlgFunc;
            public static nint LevelUpFunc;
            public static nint RestoreCastsFunc;
            public static nint ItemGet;
            public static nint EmkEventInsCtor;
            public static nint SetExternalEventTempParam;
            public static nint ExternalEventTempCtor;
            public static nint ExecuteTalkEvent;
            public static nint GetYMovement;
            public static nint GetXMovement;
            public static nint MatrixVectorProduct;
            public static nint AllocateMemory;
            public static nint SetTag;
            public static nint FinalizeEntry;
        }
        
        private static void InitializeBaseAddresses(nint moduleBase)
        {
            WorldChrMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CEE830,
                Version1_0_1_1 => 0x1C7E820,
                Version1_0_1_2 => 0x1D01FC0,
                Version1_0_3_0 => 0x1D151B0,
                Version1_0_3_1 => 0x1C77E50,
                _ => 0
            };
            
            DebugEventMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CF20B8,
                Version1_0_1_1 => 0x1C820A8,
                Version1_0_1_2 => 0x1D05848,
                Version1_0_3_0 => 0x1D18A38,
                Version1_0_3_1 => 0x1C7B6D8,
                _ => 0
            };

            
            DebugFlags.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CEE849,
                Version1_0_1_1 => 0x1C7E839,
                Version1_0_1_2 => 0x1D01FD9,
                Version1_0_3_0 => 0x1D151C9,
                Version1_0_3_1 => 0x1C77E59,
                _ => 0
            };


            GameDataMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1D00F50,
                Version1_0_1_1 => 0x1C90F40,
                Version1_0_1_2 => 0x1D146E0,
                Version1_0_3_0 => 0x1D278F0,
                Version1_0_3_1 => 0x1C8A530,
                _ => 0
            };

            ItemGetMenuManImpl.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CFFBD8,
                Version1_0_1_1 => 0x1C8FBC8,
                Version1_0_1_2 => 0x1D13358,
                Version1_0_3_0 => 0x1D26578,
                Version1_0_3_1 => 0x1C891A8,
                _ => 0
            };

            FieldArea.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CF0A48,
                Version1_0_1_1 => 0x1C80A38,
                Version1_0_1_2 => 0x1D041D8,
                Version1_0_3_0 => 0x1D173C8,
                Version1_0_3_1 => 0x1C7A058,
                _ => 0
            };
            
            GameMan.Base = moduleBase + Version switch
            {
                // WARNING: No match found for: Version1_0_1_0, Version1_0_1_1
                Version1_0_1_2 => 0x1CFDC48,
                Version1_0_3_0 => 0x1D10E18,
                Version1_0_3_1 => 0x1C74E08,
                _ => 0
            };
            
            EventMan.Base = moduleBase + Version switch
            {
                // WARNING: No match found for: Version1_0_1_0, Version1_0_1_1, Version1_0_1_2
                Version1_0_3_0 => 0x1D18530,
                Version1_0_3_1 => 0x1C7B1B0,
                _ => 0
            };

            DamageManager.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CF0A40,
                Version1_0_1_1 => 0x1C80A30,
                Version1_0_1_2 => 0x1D041D0,
                Version1_0_3_0 => 0x1D173C0,
                Version1_0_3_1 => 0x1C7A050,
                _ => 0
            };
            
            
            MenuMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CFF7C8,
                Version1_0_1_1 => 0x1C8F7B8,
                Version1_0_1_2 => 0x1D12F48,
                Version1_0_3_0 => 0x1D26168,
                Version1_0_3_1 => 0x1C88D98,
                _ => 0
            };

            EventFlagMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CF2FD0,
                Version1_0_1_1 => 0x1C82FC0,
                Version1_0_1_2 => 0x1D06760,
                Version1_0_3_0 => 0x1D19950,
                Version1_0_3_1 => 0x1C7C5F0,
                _ => 0
            };

            HgDraw.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1BDE5C0,
                Version1_0_1_1 => 0x1B6E5A0,
                Version1_0_1_2 => 0x1BF1D08,
                Version1_0_3_0 => 0x1C04ED8,
                Version1_0_3_1 => 0x1B68EC8,
                _ => 0
            };
            
            SoloParamMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CF49E0,
                Version1_0_1_1 => 0x1C849D0,
                Version1_0_1_2 => 0x1D08170,
                Version1_0_3_0 => 0x1D1B360,
                Version1_0_3_1 => 0x1C7E000,
                _ => 0
            };

            WorldAiMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CF8EF8,
                Version1_0_1_1 => 0x1C88EE8,
                Version1_0_1_2 => 0x1D0C688,
                Version1_0_3_0 => 0x1D1F878,
                Version1_0_3_1 => 0x1C82508,
                _ => 0
            };
            
            EmkEventIns.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1D01740,
                Version1_0_1_1 => 0x1C91730,
                Version1_0_1_2 => 0x1D14F10,
                Version1_0_3_0 => 0x1D28130,
                Version1_0_3_1 => 0x1C8ADC0,
                _ => 0
            };
            
            EmkSystem.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1D01740,
                Version1_0_1_1 => 0x1C91730,
                Version1_0_1_2 => 0x1D14F10,
                Version1_0_3_0 => 0x1D28130,
                Version1_0_3_1 => 0x1C8ADC0,
                _ => 0
            };
            
            PadMan.Base = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1CE0570,
                Version1_0_1_1 => 0x1C70550,
                Version1_0_1_2 => 0x1CF3CD0,
                Version1_0_3_0 => 0x1D06EB0,
                Version1_0_3_1 => 0x1C6AEA0,
                _ => 0
            };
            
            Hooks.LastLockedTarget = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x316EB5,
                Version1_0_1_1 => 0x316BB5,
                Version1_0_1_2 => 0x31A0D5,
                Version1_0_3_0 => 0x320075,
                Version1_0_3_1 => 0x3222C5,
                _ => 0
            };

            Hooks.AllNoDamage = moduleBase + Version switch
            {
                // WARNING: No match found for: Version1_0_1_0, Version1_0_1_1, Version1_0_1_2
                Version1_0_3_0 => 0x3206C9,
                Version1_0_3_1 => 0x322919,
                _ => 0
            };

            Hooks.Hit = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x28065C5,
                Version1_0_1_1 => 0x210131E,
                Version1_0_1_2 => 0x2B90267,
                Version1_0_3_0 => 0x2305994,
                Version1_0_3_1 => 0x395221,
                _ => 0
            };

            Hooks.Draw = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x2C4C14,
                Version1_0_1_1 => 0x2C4914,
                Version1_0_1_2 => 0x2C7E34,
                Version1_0_3_0 => 0x2CD384,
                Version1_0_3_1 => 0x2CEE84,
                _ => 0
            };

            Hooks.InAirTimer = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x2B1E56,
                Version1_0_1_1 => 0x2B1B56,
                Version1_0_1_2 => 0x2B4FA6,
                Version1_0_3_0 => 0x2B95E6,
                Version1_0_3_1 => 0x2BB0E6,
                _ => 0
            };

            Hooks.Keyboard = moduleBase + Version switch
            {
                Version1_0_1_0 => 0xC8FAC1,
                Version1_0_1_1 => 0xC8F941,
                Version1_0_1_2 => 0xC95561,
                Version1_0_3_0 => 0xC9CB11,
                Version1_0_3_1 => 0xCA06F1,
                _ => 0
            };

            Hooks.ControllerR2 = moduleBase + Version switch
            {
                Version1_0_1_0 => 0xC8F0CA,
                Version1_0_1_1 => 0xC8EF4A,
                Version1_0_1_2 => 0xC94B6A,
                Version1_0_3_0 => 0xC9C11A,
                Version1_0_3_1 => 0xC9FCFA,
                _ => 0
            };

            Hooks.ControllerL2 = moduleBase + Version switch
            {
                Version1_0_1_0 => 0xC8F0A0,
                Version1_0_1_1 => 0xC8EF20,
                Version1_0_1_2 => 0xC94B40,
                Version1_0_3_0 => 0xC9C0F0,
                Version1_0_3_1 => 0xC9FCD0,
                _ => 0
            };

            Hooks.UpdateCoords = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x2B22E6,
                Version1_0_1_1 => 0x2B1FE6,
                Version1_0_1_2 => 0x2B5436,
                Version1_0_3_0 => 0x2B9A76,
                Version1_0_3_1 => 0x2BB576,
                _ => 0
            };

            Hooks.WarpCoords = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x2BF58A,
                Version1_0_1_1 => 0x2BF28A,
                Version1_0_1_2 => 0x2C273A,
                Version1_0_3_0 => 0x2C731A,
                Version1_0_3_1 => 0x2C8E1A,
                _ => 0
            };

            Hooks.WarpAngle = moduleBase + Version switch
            {
                // WARNING: No match found for: Version1_0_1_0, Version1_0_1_1
                Version1_0_1_2 => 0x2C277A,
                Version1_0_3_0 => 0x2C735A,
                Version1_0_3_1 => 0x2C8E5A,
                _ => 0
            };
            
            Hooks.LuaLowerOrEqual = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x106786A,
                Version1_0_1_1 => 0x10676EA,
                Version1_0_1_2 => 0xDCCFDA,
                Version1_0_3_0 => 0xDD44DA,
                Version1_0_3_1 => 0xDD80BA,
                _ => 0
            };

            Hooks.LuaVmSwitch = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x106691F,
                Version1_0_1_1 => 0x106679F,
                Version1_0_1_2 => 0xDCC08F,
                Version1_0_3_0 => 0xDD358F,
                Version1_0_3_1 => 0xDD716F,
                _ => 0
            };

            Hooks.BattleActivate = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x105B750,
                Version1_0_1_1 => 0x105B5D0,
                Version1_0_1_2 => 0xDC0EC0,
                Version1_0_3_0 => 0xDC83C0,
                Version1_0_3_1 => 0xDCBFA0,
                _ => 0
            };

            Hooks.Emevd = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1557BC,
                Version1_0_1_1 => 0x1554AC,
                Version1_0_1_2 => 0x15796C,
                Version1_0_3_0 => 0x15BE3C,
                Version1_0_3_1 => 0x15D66C,
                _ => 0
            };
            
            Hooks.FourKingsGenerator = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x36A0D0,
                Version1_0_1_1 => 0x369DD0,
                Version1_0_1_2 => 0x36D2F0,
                Version1_0_3_0 => 0x3737B0,
                Version1_0_3_1 => 0x372E70,
                _ => 0
            };
            
            Hooks.HgDrawCommandExecutor = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x11AF785,
                Version1_0_1_1 => 0x11AF745,
                Version1_0_1_2 => 0x11B3F45,
                Version1_0_3_0 => 0x11BB2F5,
                Version1_0_3_1 => 0x11BE7B5,
                _ => 0
            };

            
            Patches.DrawEvent = moduleBase + Version switch
            {
                // WARNING: No match found for: Version1_0_1_0, Version1_0_1_1, Version1_0_1_2
                Version1_0_3_0 => 0x49B6B7,
                Version1_0_3_1 => 0x49c1b4,
                _ => 0
            };

            Patches.DrawSoundView = moduleBase + Version switch
            {
                // WARNING: No match found for: Version1_0_1_0, Version1_0_1_1, Version1_0_1_2
                Version1_0_3_0 => 0x622289,
                Version1_0_3_1 => 0x624b89,
                _ => 0
            };
            
            Patches.InfiniteDurability = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x73F1B0,
                Version1_0_1_1 => 0x73EFE0,
                Version1_0_1_2 => 0x744400,
                Version1_0_3_0 => 0x74BA90,
                Version1_0_3_1 => 0x74E770,
                _ => 0
            };

            Patches.NoRoll = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x39011F,
                Version1_0_1_1 => 0x38FE1F,
                Version1_0_1_2 => 0x39334F,
                Version1_0_3_0 => 0x39984F,
                Version1_0_3_1 => 0x398E4F,
                _ => 0
            };
            
            Patches.NoBackStep = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x39021E,
                Version1_0_1_1 => 0x38FF1E,
                Version1_0_1_2 => 0x39344E,
                Version1_0_3_0 => 0x39994E,
                Version1_0_3_1 => 0x398F4E,
                _ => 0
            };
            
            Patches.Quitout = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x7F35B5,
                Version1_0_1_1 => 0x7F3435,
                Version1_0_1_2 => 0x7F9055,
                Version1_0_3_0 => 0x7FEC85,
                Version1_0_3_1 => 0x8035A5,
                _ => 0
            };
            
            Functions.SetEvent = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x4E7490,
                Version1_0_1_1 => 0x4E7240,
                Version1_0_1_2 => 0x4EA770,
                Version1_0_3_0 => 0x4F1470,
                Version1_0_3_1 => 0x4F22B0,
                _ => 0
            };

            Functions.GetEvent = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x4E75B0,
                Version1_0_1_1 => 0x4E7360,
                Version1_0_1_2 => 0x4EA890,
                Version1_0_3_0 => 0x4F1590,
                Version1_0_3_1 => 0x4F23D0,
                _ => 0
            };
            
            Functions.Warp = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x47BD60,
                Version1_0_1_1 => 0x47BB10,
                Version1_0_1_2 => 0x47F040,
                Version1_0_3_0 => 0x485CE0,
                Version1_0_3_1 => 0x4867E0,

                _ => 0
            };
            
            Functions.ExecuteEmevdCommand = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x7BE910,
                Version1_0_1_1 => 0x7BE790,
                Version1_0_1_2 => 0x7C4000,
                Version1_0_3_0 => 0x7C9890,
                Version1_0_3_1 => 0x7CDCE0,
                _ => 0
            };

            Functions.GetInventoryIndexByCatAndId = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x73A390,
                Version1_0_1_1 => 0x73A1C0,
                Version1_0_1_2 => 0x73F5E0,
                Version1_0_3_0 => 0x746C70,
                Version1_0_3_1 => 0x749950,
                _ => 0
            };

            
            Functions.ItemDlgFunc = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x719AF0,
                Version1_0_1_1 => 0x7198B0,
                Version1_0_1_2 => 0x71ECB0,
                Version1_0_3_0 => 0x725FB0,
                Version1_0_3_1 => 0x728C90,
                _ => 0
            };

            Functions.LevelUpFunc = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x689920,
                Version1_0_1_1 => 0x689680,
                Version1_0_1_2 => 0x68E6E0,
                Version1_0_3_0 => 0x695890,
                Version1_0_3_1 => 0x6981A0,
                _ => 0
            };

            Functions.RestoreCastsFunc = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x743B90,
                Version1_0_1_1 => 0x7439C0,
                Version1_0_1_2 => 0x748DE0,
                Version1_0_3_0 => 0x750470,
                Version1_0_3_1 => 0x753190,
                _ => 0
            };

            Functions.ItemGet = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x738420,
                Version1_0_1_1 => 0x738250,
                Version1_0_1_2 => 0x73D670,
                Version1_0_3_0 => 0x744D00,
                Version1_0_3_1 => 0x7479E0,
                _ => 0
            };

            Functions.EmkEventInsCtor = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x7C4BA0,
                Version1_0_1_1 => 0x7C4A20,
                Version1_0_1_2 => 0x7CA290,
                Version1_0_3_0 => 0x7CFB20,
                Version1_0_3_1 => 0x7D3F70,
                _ => 0
            };
            
            Functions.SetExternalEventTempParam = moduleBase + Version switch
            {
                Version1_0_1_0 => 0xCD5750,
                Version1_0_1_1 => 0xCD55D0,
                Version1_0_1_2 => 0xE9B510,
                Version1_0_3_0 => 0xEA29A0,
                Version1_0_3_1 => 0xEA6580,
                _ => 0
            };

            Functions.ExternalEventTempCtor = moduleBase + Version switch
            {
                Version1_0_1_0 => 0xCD5650,
                Version1_0_1_1 => 0xCD54D0,
                Version1_0_1_2 => 0xE9B410,
                Version1_0_3_0 => 0xEA28A0,
                Version1_0_3_1 => 0xEA6480,
                _ => 0
            };

            Functions.ExecuteTalkEvent = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x4D0940,
                Version1_0_1_1 => 0x4D06F0,
                Version1_0_1_2 => 0x4D3C20,
                Version1_0_3_0 => 0x4DA920,
                Version1_0_3_1 => 0x4DB340,
                _ => 0
            };
            
            Functions.GetYMovement = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x19A6A0,
                Version1_0_1_1 => 0x19A3A0,
                Version1_0_1_2 => 0x19D740,
                Version1_0_3_0 => 0x1A1E10,
                Version1_0_3_1 => 0x1A3700,
                _ => 0
            };


            
            Functions.GetXMovement = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x19A6F0,
                Version1_0_1_1 => 0x19A3F0,
                Version1_0_1_2 => 0x19D790,
                Version1_0_3_0 => 0x1A1E60,
                Version1_0_3_1 => 0x1A3750,
                _ => 0
            };
            
            Functions.MatrixVectorProduct = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x93CA0,
                Version1_0_1_1 => 0x93990,
                Version1_0_1_2 => 0x93E80,
                Version1_0_3_0 => 0x93E50,
                Version1_0_3_1 => 0x93A30,
                _ => 0
            };
            
            
            Functions.AllocateMemory = moduleBase + Version switch
            {
                Version1_0_1_0 => 0xF89B60,
                Version1_0_1_1 => 0xF899E0,
                Version1_0_1_2 => 0xCB8BF0,
                Version1_0_3_0 => 0xCC0230,
                Version1_0_3_1 => 0xCC3E10,
                _ => 0
            };
            
            Functions.SetTag = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x11A4820,
                Version1_0_1_1 => 0x11A46A0,
                Version1_0_1_2 => 0x11A8FE0,
                Version1_0_3_0 => 0x11B0240,
                Version1_0_3_1 => 0x11B3710,
                _ => 0
            };

            Functions.FinalizeEntry = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x11A47C0,
                Version1_0_1_1 => 0x11A4640,
                Version1_0_1_2 => 0x11A8F80,
                Version1_0_3_0 => 0x11B01E0,
                Version1_0_3_1 => 0x11B36B0,
                _ => 0
            };


            BeginTargetSceneVtable = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x16878A8,
                Version1_0_1_1 => 0x1617C60,
                Version1_0_1_2 => 0x1699A08,
                Version1_0_3_0 => 0x16AADB8,
                Version1_0_3_1 => 0x160E5F0,
                _ => 0
            };

            EndTargetSceneVtable = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x1687820,
                Version1_0_1_1 => 0x1617098,
                Version1_0_1_2 => 0x1699930,
                Version1_0_3_0 => 0x16AAC80,
                Version1_0_3_1 => 0x160DA40,
                _ => 0
            };


            DbgMapWalkPadVtable = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x12CDF58,
                Version1_0_1_1 => 0x12CDE80,
                Version1_0_1_2 => 0x12D3118,
                Version1_0_3_0 => 0x12DA5B8,
                Version1_0_3_1 => 0x12DE530,
                _ => 0
            };

            HGDrawPlanEntityVtable = moduleBase + Version switch
            {
                Version1_0_1_0 => 0x168A558,
                Version1_0_1_1 => 0x16180F0,
                Version1_0_1_2 => 0x169C098,
                Version1_0_3_0 => 0x16AA6F0,
                Version1_0_3_1 => 0x160EA80,
                _ => 0
            };



#if DEBUG
            _baseAddr = moduleBase;
            Console.WriteLine("--- Bases ---");
            PrintOffset("WorldChrMan", WorldChrMan.Base);
            PrintOffset("DebugEventMan", DebugEventMan.Base);
            PrintOffset("DebugFlags", DebugFlags.Base);
            PrintOffset("GameDataMan", GameDataMan.Base);
            PrintOffset("ItemGetMenuManImpl", ItemGetMenuManImpl.Base);
            PrintOffset("FieldArea", FieldArea.Base);
            PrintOffset("GameMan", GameMan.Base);
            PrintOffset("EventMan", EventMan.Base);
            PrintOffset("DamageManager", DamageManager.Base);
            PrintOffset("MenuMan", MenuMan.Base);
            PrintOffset("EventFlagMan", EventFlagMan.Base);
            PrintOffset("HgDraw", HgDraw.Base);
            PrintOffset("SoloParamMan", SoloParamMan.Base);
            PrintOffset("WorldAiMan", WorldAiMan.Base);
            PrintOffset("EmkEventIns", EmkEventIns.Base);
            PrintOffset("EmkSystem", EmkSystem.Base);
            PrintOffset("PadMan", PadMan.Base);
            PrintOffset("DbgMapWalkPadVtable", DbgMapWalkPadVtable);
            PrintOffset("BeginTargetSceneVtable", BeginTargetSceneVtable);
            PrintOffset("EndTargetSceneVtable", EndTargetSceneVtable);
            PrintOffset("HGDrawPlanEntityVtable", HGDrawPlanEntityVtable);

            Console.WriteLine("\n--- Hooks ---");
            PrintOffset("LastLockedTarget", Hooks.LastLockedTarget);
            PrintOffset("AllNoDamage", Hooks.AllNoDamage);
            PrintOffset("Draw", Hooks.Draw);
            PrintOffset("InAirTimer", Hooks.InAirTimer);
            PrintOffset("Keyboard", Hooks.Keyboard);
            PrintOffset("ControllerR2", Hooks.ControllerR2);
            PrintOffset("ControllerL2", Hooks.ControllerL2);
            PrintOffset("UpdateCoords", Hooks.UpdateCoords);
            PrintOffset("WarpCoords", Hooks.WarpCoords);
            PrintOffset("WarpAngle", Hooks.WarpAngle);
            PrintOffset("LuaLowerOrEqual", Hooks.LuaLowerOrEqual);
            PrintOffset("LuaVmSwitch", Hooks.LuaVmSwitch);
            PrintOffset("BattleActivate", Hooks.BattleActivate);
            PrintOffset("Emevd", Hooks.Emevd);
            PrintOffset("FourKingsGenerator", Hooks.FourKingsGenerator);
            PrintOffset("HgDrawCommandExecutor", Hooks.HgDrawCommandExecutor);

            Console.WriteLine("\n--- Patches ---");
            PrintOffset("DrawEvent", Patches.DrawEvent);
            PrintOffset("DrawSoundView", Patches.DrawSoundView);
            PrintOffset("InfiniteDurability", Patches.InfiniteDurability);
            PrintOffset("NoRoll", Patches.NoRoll);
            PrintOffset("NoBackStep", Patches.NoBackStep);
            PrintOffset("Quitout", Patches.Quitout);

            Console.WriteLine("\n--- Functions ---");
            PrintOffset("SetEvent", Functions.SetEvent);
            PrintOffset("GetEvent", Functions.GetEvent);
            PrintOffset("ExecuteEmevdCommand", Functions.ExecuteEmevdCommand);
            PrintOffset("GetInventoryIndexByCatAndId", Functions.GetInventoryIndexByCatAndId);
            PrintOffset("Warp", Functions.Warp);
            PrintOffset("ItemDlgFunc", Functions.ItemDlgFunc);
            PrintOffset("LevelUpFunc", Functions.LevelUpFunc);
            PrintOffset("RestoreCastsFunc", Functions.RestoreCastsFunc);
            PrintOffset("ItemGet", Functions.ItemGet);
            PrintOffset("EmkEventInsCtor", Functions.EmkEventInsCtor);
            PrintOffset("SetExternalEventTempParam", Functions.SetExternalEventTempParam);
            PrintOffset("ExternalEventTempCtor", Functions.ExternalEventTempCtor);
            PrintOffset("ExecuteTalkEvent", Functions.ExecuteTalkEvent);
            PrintOffset("GetYMovement", Functions.GetYMovement);
            PrintOffset("GetXMovement", Functions.GetXMovement);
            PrintOffset("MatrixVectorProduct", Functions.MatrixVectorProduct);
            PrintOffset("AllocateMemory", Functions.AllocateMemory);
            PrintOffset("SetTag", Functions.SetTag);
            PrintOffset("FinalizeEntry", Functions.FinalizeEntry);
            
            Console.WriteLine("\n====================================\n");
#endif
        }

#if DEBUG
        private static nint _baseAddr;
        private static void PrintOffset(string name, nint value)
        {
            var rel = value - _baseAddr;
            Console.WriteLine(rel <= 0
                ? $"  {name,-40} *** NOT SET ***"
                : $"  {name,-40} 0x{(long)value:X}  (0x{(long)rel:X})");
        }
#endif
    }
}