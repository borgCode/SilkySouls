﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SilkySouls.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SilkySouls.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 50                      push   rax
        ///8b 81 ec 03 00 00       mov    eax,DWORD PTR [rcx+0x3ec]
        ///89 81 e8 03 00 00       mov    DWORD PTR [rcx+0x3e8],eax
        ///58                      pop    rax
        ///f6 81 54 01 00 00 40    test   BYTE PTR [rcx+0x154],0x40
        ///e9 00 00 00 00          jmp    1a &lt;_main+0x1a&gt;.
        /// </summary>
        internal static string AllNoDamage {
            get {
                return ResourceManager.GetString("AllNoDamage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 2007000,999,0,Dragonslayer Arrow
        ///2002000,999,0,Feather Arrow
        ///2003000,999,0,Fire Arrow
        ///2008000,999,0,Gough&apos;s Great Arrow
        ///2101000,999,0,Heavy Bolt
        ///2001000,999,0,Large Arrow
        ///2104000,999,0,Lightning Bolt
        ///2005000,999,0,Moonlight Arrow
        ///2004000,999,0,Poison Arrow
        ///2102000,999,0,Sniper Bolt
        ///2000000,999,0,Standard Arrow
        ///2100000,999,0,Standard Bolt
        ///2103000,999,0,Wood Bolt
        ///2006000,999,0,Wooden Arrow
        ///.
        /// </summary>
        internal static string Ammo {
            get {
                return ResourceManager.GetString("Ammo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 553000,1,0,Anklet of the Great Lord
        ///331000,1,1,Antiquated Dress
        ///332000,1,1,Antiquated Gloves
        ///333000,1,1,Antiquated Skirt
        ///661000,1,1,Armor of Artorias
        ///111000,1,0,Armor of the Glorious
        ///161000,1,2,Armor of the Sun
        ///201000,1,1,Armor of Thorns
        ///511000,1,2,Balder Armor
        ///512000,1,2,Balder Gauntlets
        ///510000,1,2,Balder Helm
        ///513000,1,2,Balder Leggings
        ///380000,1,1,Big Hat
        ///151000,1,1,Black Cleric Robe
        ///71000,1,1,Black Iron Armor
        ///72000,1,1,Black Iron Gauntlets
        ///70000,1,1,Black Iron Helm
        ///73000,1,1,Black Iron Le [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Armor {
            get {
                return ResourceManager.GetString("Armor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 294,99,0,Alluring Skull
        ///297,99,0,Black Firebomb
        ///270,99,0,Bloodred Moss Clump
        ///272,99,0,Blooming Purple Moss Clump
        ///310,99,0,Charcoal Pine Resin
        ///381,99,0,Copper Coin
        ///703,99,0,Core of an Iron Golem
        ///111,99,0,Cracked Red Eye Orb
        ///240,99,0,Divine Blessing
        ///293,99,0,Dung Pie
        ///275,99,0,Egg Vermifuge
        ///230,99,0,Elizabeth&apos;s Mushroom
        ///109,99,0,Eye of Death
        ///390,1,0,Fire Keeper Soul (Anastacia of Astora)
        ///394,1,0,Fire Keeper Soul (Blighttown)
        ///391,1,0,Fire Keeper Soul (Darkmoon Knightess)
        ///392,1,0,Fire Keeper Soul [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Consumables {
            get {
                return ResourceManager.GetString("Consumables", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///8b 58 08                mov    ebx,DWORD PTR [rax+0x8]
        ///41 89 f0                mov    r8d,esi
        ///ba 16 00 00 00          mov    edx,0x16
        ///48 89 f9                mov    rcx,rdi
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///ff d0                   call   rax
        ///48 8b 0f                mov    rcx,QWORD PTR [rdi]
        ///48 83 c1 03             add    rcx,0x3
        ///48 83 e1 fc             and    rcx,0xfffffffffffffffc
        ///48 8d 41 04             lea    rax,[rcx+0x4]
        ///48 89  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string EnableDraw {
            get {
                return ResourceManager.GetString("EnableDraw", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 371,1,0,Binoculars
        ///115,1,0,Black Eye Orb
        ///103,1,0,Black Separation Crystal
        ///113,1,0,Blue Eye Orb
        ///108,1,0,Book of the Guilty
        ///117,1,0,Darksign
        ///114,1,0,Dragon Eye
        ///377,1,0,Dragon Head Stone
        ///378,1,0,Dragon Torso Stone
        ///385,1,0,Dried Finger
        ///510,1,0,Hello Carving
        ///514,1,0,Help me! Carving
        ///513,1,0,I&apos;m sorry Carving
        ///106,1,0,Orange Guidance Soapstone
        ///118,1,0,Purple Coward&apos;s Crystal
        ///102,1,0,Red Eye Orb
        ///101,1,0,Red Sign Soapstone
        ///112,1,0,Servant Roster
        ///220,1,0,Silver Pendant
        ///511,1,0,Thank you Carving
        ///51 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string InfiniteUseItems {
            get {
                return ResourceManager.GetString("InfiniteUseItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 48 81 ec a0 00 00 00    sub    rsp,0xa0
        ///0f 29 84 24 80 00 00    movaps XMMWORD PTR [rsp+0x80],xmm0
        ///00
        ///0f 29 8c 24 90 00 00    movaps XMMWORD PTR [rsp+0x90],xmm1
        ///00
        ///0f 29 94 24 a0 00 00    movaps XMMWORD PTR [rsp+0xa0],xmm2
        ///00
        ///0f 29 9c 24 b0 00 00    movaps XMMWORD PTR [rsp+0xb0],xmm3
        ///00
        ///0f 29 a4 24 c0 00 00    movaps XMMWORD PTR [rsp+0xc0],xmm4
        ///00
        ///0f 29 ac 24 d0 00 00    movaps XMMWORD PTR [rsp+0xd0],xmm5
        ///00
        ///50                      push   rax
        ///51                      push   rcx
        ///52              [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ItemSpawn {
            get {
                return ResourceManager.GetString("ItemSpawn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 2009,1,0,Annex Key
        ///2020,1,0,Archive Prison Extra Key
        ///2004,1,0,Archive Tower Cell Key
        ///2006,1,0,Archive Tower Giant Cell Key
        ///2005,1,0,Archive Tower Giant Door Key
        ///2601,1,0,Armor Smithbox
        ///2001,1,0,Basement Key
        ///2502,1,0,Bequeathed Lord Soul Shard (Four Kings)
        ///2503,1,0,Bequeathed Lord Soul Shard (Seath)
        ///2011,1,0,Big Pilgrim&apos;s Key
        ///2007,1,0,Blighttown Key
        ///2608,1,0,Bottomless Box
        ///2520,1,0,Broken Pendant
        ///2003,1,0,Cage Key
        ///2022,1,0,Crest Key
        ///2002,1,0,Crest of Artorias
        ///2010,1,0,Dungeon Cell Key
        ///2014,1 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string KeyItems {
            get {
                return ResourceManager.GetString("KeyItems", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 50                      push   rax
        ///51                      push   rcx
        ///48 a1 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///48 8b 48 68             mov    rcx,QWORD PTR [rax+0x68]
        ///48 39 d9                cmp    rcx,rbx
        ///0f 84 00 00 00 00       je     19 &lt;_main+0x19&gt;
        ///48 89 d8                mov    rax,rbx
        ///48 a3 00 00 00 00 00    movabs ds:0x0,rax
        ///00 00 00
        ///59                      pop    rcx
        ///58                      pop    rax
        ///48 8d 4c 24 38          lea    rcx,[rsp+0x38]
        ///e9 00 00 00 00          jmp    32 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LastLockedTarget {
            get {
                return ResourceManager.GetString("LastLockedTarget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///48 89 c2                mov    rdx,rax
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///48 83 ec 28             sub    rsp,0x28
        ///48 89 c1                mov    rcx,rax
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///ff d0                   call   rax
        ///48 83 c4 28             add    rsp,0x28
        ///c3                      ret.
        /// </summary>
        internal static string LevelUp {
            get {
                return ResourceManager.GetString("LevelUp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 50                      push   rax
        ///48 89 c8                mov    rax,rcx
        ///48 89 05 00 00 00 00    mov    QWORD PTR [rip+0x0],rax        # 0xb
        ///58                      pop    rax
        ///48 81 ec e8 00 00 00    sub    rsp,0xe8
        ///e9 00 00 00 00          jmp    0x18.
        /// </summary>
        internal static string LuaHook {
            get {
                return ResourceManager.GetString("LuaHook", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to f3 0f 58 9b b0 01 00    addss  xmm3,DWORD PTR [rbx+0x1b0]
        ///00
        ///50                      push   rax
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///48 39 d8                cmp    rax,rbx
        ///0f 85 00 00 00 00       jne    1c &lt;_main+0x1c&gt;
        ///0f 57 db                xorps  xmm3,xmm3
        ///58                      pop    rax
        ///e9 00 00 00 00          jmp    25 &lt;_main+0x25&gt;.
        /// </summary>
        internal static string NoClip_InAirTimer {
            get {
                return ResourceManager.GetString("NoClip_InAirTimer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 50                      push   rax
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///48 39 c8                cmp    rax,rcx
        ///0f 85 00 00 00 00       jne    0x14 
        ///52                      push   rdx
        ///66 0f 6f b1 20 01 00    movdqa xmm6,XMMWORD PTR [rcx+0x120]
        ///00
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///8b 90 04 03 00 00       mov    edx,DWORD PTR [rax+0x304]
        ///c1 e2 02                shl    edx,0x2
        ///f3 44 0f 10 bc 10 64    movss  xmm15,DWORD PTR [rax+rdx*1+0x264]
        ///02 00 00
        ///45 0f c6 ff 00          [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NoClip_UpdateCoords {
            get {
                return ResourceManager.GetString("NoClip_UpdateCoords", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 48 83 ff 20             cmp    rdi,0x20
        ///0f 84 00 00 00 00       je     a &lt;_main+0xa&gt;
        ///48 81 ff a2 00 00 00    cmp    rdi,0xa2
        ///0f 84 00 00 00 00       je     17 &lt;_main+0x17&gt;
        ///c6 43 f0 01             mov    BYTE PTR [rbx-0x10],0x1
        ///c6 00 01                mov    BYTE PTR [rax],0x1
        ///e9 00 00 00 00          jmp    23 &lt;_main+0x23&gt;
        ///50                      push   rax
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///c6 00 01                mov    BYTE PTR [rax],0x1
        ///c6 43 f0 00             mov    BYTE PTR [rbx [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NoClip_ZDirection_KB {
            get {
                return ResourceManager.GetString("NoClip_ZDirection_KB", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0f b6 44 24 26          movzx  eax,BYTE PTR [rsp+0x26]
        ///3c 1e                   cmp    al,0x1e
        ///0f 86 00 00 00 00       jbe     d &lt;_main+0xd&gt;
        ///51                      push   rcx
        ///48 b9 00 00 00 00 00    movabs rcx,0x0
        ///00 00 00
        ///c6 01 02                mov    BYTE PTR [rcx],0x2
        ///30 c0                   xor    al,al
        ///59                      pop    rcx
        ///e9 00 00 00 00          jmp    23 &lt;_main+0x23&gt;.
        /// </summary>
        internal static string NoClip_ZDirection_L2 {
            get {
                return ResourceManager.GetString("NoClip_ZDirection_L2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0f b6 44 24 27          movzx  eax,BYTE PTR [rsp+0x27]
        ///3c 1e                   cmp    al,0x1e
        ///0f 86 00 00 00 00       jbe     d &lt;_main+0xd&gt;
        ///51                      push   rcx
        ///48 b9 00 00 00 00 00    movabs rcx,0x0
        ///00 00 00
        ///c6 01 01                mov    BYTE PTR [rcx],0x1
        ///30 c0                   xor    al,al
        ///59                      pop    rcx
        ///e9 00 00 00 00          jmp    23 &lt;_main+0x23&gt;.
        /// </summary>
        internal static string NoClip_ZDirection_R2 {
            get {
                return ResourceManager.GetString("NoClip_ZDirection_R2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 48 b9 00 00 00 00 00    movabs rcx,0x0
        ///00 00 00
        ///48 83 ec 28             sub    rsp,0x28
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///ff d0                   call   rax
        ///48 83 c4 28             add    rsp,0x28
        ///c3                      ret.
        /// </summary>
        internal static string OpenEnhanceShop {
            get {
                return ResourceManager.GetString("OpenEnhanceShop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 50                      push   rax
        ///48 83 ec 20             sub    rsp,0x20
        ///f3 0f 7f 04 24          movdqu XMMWORD PTR [rsp],xmm0
        ///f3 0f 7f 4c 24 10       movdqu XMMWORD PTR [rsp+0x10],xmm1
        ///f3 41 0f 7e 86 98 01    movq   xmm0,QWORD PTR [r14+0x198]
        ///00 00
        ///66 48 0f 7e c0          movq   rax,xmm0
        ///48 25 00 fc ff ff       and    rax,0xfffffffffffffc00
        ///66 48 0f 6e c0          movq   xmm0,rax
        ///f3 0f 7e 0d 00 00 00    movq   xmm1,QWORD PTR [rip+0x0]        # 31 &lt;_main+0x31&gt;
        ///00
        ///66 48 0f 7e c8          movq    [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string RepeatAct {
            get {
                return ResourceManager.GetString("RepeatAct", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 48 b9 00 00 00 00 00    movabs rcx,0x0
        ///00 00 00
        ///48 83 ec 28             sub    rsp,0x28
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///ff d0                   call   rax
        ///48 83 c4 28             add    rsp,0x28
        ///c3                      ret.
        /// </summary>
        internal static string RestoreSpellCasts {
            get {
                return ResourceManager.GetString("RestoreSpellCasts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 115,1,0,Bellowing Dragoncrest Ring
        ///109,1,0,Bloodbite Ring
        ///147,1,0,Blue Tearstone Ring
        ///150,1,0,Calamity Ring
        ///103,1,0,Cat Covenant Ring
        ///104,1,0,Cloranthy Ring
        ///138,1,0,Covenant of Artorias
        ///121,1,0,Covetous Gold Serpent Ring
        ///122,1,0,Covetous Silver Serpent Ring
        ///113,1,0,Cursebite Ring
        ///128,1,0,Dark Wood Grain Ring
        ///102,1,0,Darkmoon Blade Covenant Ring
        ///149,1,0,Darkmoon Seance Ring
        ///116,1,0,Dusk Crown Ring
        ///145,1,0,East Wood Grain Ring
        ///105,1,0,Flame Stoneplate Ring
        ///100,1,0,Havel&apos;s Ring
        ///119,1,0,Hawk Ri [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Rings {
            get {
                return ResourceManager.GetString("Rings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 5050,99,0,Miracle: Bountiful Sunlight
        ///5910,99,0,Miracle: Darkmoon Blade
        ///5320,99,0,Miracle: Emit Force
        ///5300,99,0,Miracle: Force
        ///5110,99,0,Miracle: Gravelord Greatsword Dance
        ///5100,99,0,Miracle: Gravelord Sword Dance
        ///5010,99,0,Miracle: Great Heal
        ///5020,99,0,Miracle: Great Heal Excerpt
        ///5510,99,0,Miracle: Great Lightning Spear
        ///5610,99,0,Miracle: Great Magic Barrier
        ///5000,99,0,Miracle: Heal
        ///5210,99,0,Miracle: Homeward
        ///5700,99,0,Miracle: Karmic Justice
        ///5500,99,0,Miracle: Lightning Spear
        ///5600,99,0,Mirac [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Spells {
            get {
                return ResourceManager.GetString("Spells", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 800,1,0,Large Ember
        ///801,1,0,Very Large Ember
        ///802,1,0,Crystal Ember
        ///806,1,0,Large Magic Ember
        ///807,1,0,Enchanted Ember
        ///808,1,0,Divine Ember
        ///809,1,0,Large Divine Ember
        ///810,1,0,Dark Ember
        ///812,1,0,Large Flame Ember
        ///813,1,0,Chaos Flame Ember
        ///1000,99,0,Titanite Shard
        ///1010,99,0,Large Titanite Shard
        ///1030,99,0,Titanite Chunk
        ///1070,99,0,Titanite Slab
        ///1020,99,0,Green Titanite Shard
        ///1040,99,0,Blue Titanite Chunk
        ///1080,99,0,Blue Titanite Slab
        ///1050,99,0,White Titanite Chunk
        ///1090,99,0,White Titanite Slab
        ///1 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string UpgradeMaterials {
            get {
                return ResourceManager.GetString("UpgradeMaterials", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///48 8b 08                mov    rcx,QWORD PTR [rax]
        ///ba 01 00 00 00          mov    edx,0x1
        ///48 83 ec 38             sub    rsp,0x38
        ///48 b8 00 00 00 00 00    movabs rax,0x0
        ///00 00 00
        ///ff d0                   call   rax
        ///48 83 c4 38             add    rsp,0x38
        ///c3                      ret.
        /// </summary>
        internal static string Warp {
            get {
                return ResourceManager.GetString("Warp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 51                      push   rcx
        ///48 b9 00 00 00 00 00    movabs rcx,0x0
        ///00 00 00
        ///44 0f 10 39             movups xmm15,XMMWORD PTR [rcx]
        ///44 0f 11 b8 90 0a 00    movups XMMWORD PTR [rax+0xa90],xmm15
        ///00
        ///45 0f 57 ff             xorps  xmm15,xmm15
        ///59                      pop    rcx
        ///e9 00 00 00 00          jmp    21 &lt;_main+0x21&gt;.
        /// </summary>
        internal static string WarpAngle {
            get {
                return ResourceManager.GetString("WarpAngle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 51                      push   rcx
        ///48 b9 00 00 00 00 00    movabs rcx,0x0
        ///00 00 00
        ///44 0f 10 39             movups xmm15,XMMWORD PTR [rcx]
        ///44 0f 11 b8 80 0a 00    movups XMMWORD PTR [rax+0xa80],xmm15
        ///00
        ///45 0f 57 ff             xorps  xmm15,xmm15
        ///59                      pop    rcx
        ///e9 00 00 00 00          jmp    21 &lt;_main+0x21&gt;.
        /// </summary>
        internal static string WarpCoords {
            get {
                return ResourceManager.GetString("WarpCoords", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1512960,0,Anor Londo (1st Bonfire)
        ///1512961,440.74|132.00|227.13|1.24,Anor Londo (Archers)
        ///1512962,0,Anor Londo (Gwyndolin)
        ///1512961,0,Anor Londo (Interior)
        ///1512961,532.77|142.60|254.83|-1.60,Anor Londo (O&amp;S)
        ///1512960,265.49|129.20|301.03|-2.04,Anor Londo (Rafters)
        ///1322960,0,Ash Lake (Covenant)
        ///1322961,0,Ash Lake (Entrance)
        ///1602951,-90.09|-138.74|17.78|2.11,Blighttown (Back Entrance)
        ///1402962,0,Blighttown (Bridge)
        ///1402963,0,Blighttown (Entrance)
        ///1402961,0,Blighttown (Swamp)
        ///1602961,0,Darkroot Basin        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string WarpLocations {
            get {
                return ResourceManager.GetString("WarpLocations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 9012000,1,1,Abyss Greatsword
        ///209000,1,1,Astora&apos;s Straight Sword
        ///1252000,1,4,Avelyn
        ///1455000,1,4,Balder Shield
        ///204000,1,3,Balder Side Sword
        ///103000,1,3,Bandit&apos;s Knife
        ///207000,1,3,Barbed Straight Sword
        ///300000,1,3,Bastard Sword
        ///701000,1,3,Battle Axe
        ///1301000,1,0,Beatrice&apos;s Catalyst
        ///1202000,1,3,Black Bow of Pharis
        ///9003000,1,4,Black Iron Greatshield
        ///753000,1,1,Black Knight Greataxe
        ///355000,1,1,Black Knight Greatsword
        ///1105000,1,1,Black Knight Halberd
        ///1474000,1,1,Black Knight Shield
        ///310000,1,1,Black Kni [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Weapons {
            get {
                return ResourceManager.GetString("Weapons", resourceCulture);
            }
        }
    }
}
