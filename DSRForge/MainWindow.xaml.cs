using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DSRForge.memory;
using DSRForge.Memory;
using DSRForge.Services;
using DSRForge.Utilities;
using DSRForge.ViewModels;
using DSRForge.Views;
using Microsoft.Win32;

namespace DSRForge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MemoryIo _memoryIo;
        private readonly DispatcherTimer _gameLoadedTimer;

        private readonly PlayerViewModel _playerViewModel;
        private readonly UtilityViewModel _utilityViewModel;
        private readonly EnemyViewModel _enemyViewModel;
        private readonly ItemViewModel _itemViewModel;
        private readonly EnemyService _enemyService;
        private readonly HookManager _hookManager;
        private readonly AoBScanner aobScanner;

        private string _patch;

        public MainWindow()
        {
            _memoryIo = new MemoryIo();
            _patch = VersionChecker.GetPatch();
            _memoryIo.StartAutoAttach();

            InitializeComponent();

            _hookManager = new HookManager(_memoryIo);
            var hotkeyManager = new HotkeyManager(_memoryIo);
            aobScanner = new AoBScanner(_memoryIo);


            switch (_patch)
            {
                case "1.3.0.0":
                    CodeCaveOffsets.CodeCave1.Base = _memoryIo.BaseAddress + 0x1298A60;
                    CodeCaveOffsets.CodeCave2.Base = _memoryIo.BaseAddress + 0x1D0DF10;
                    CodeCaveOffsets.CodeCave3.Base = _memoryIo.BaseAddress + 0x1C70350;
                    break;
                case "1.3.0.1":
                    CodeCaveOffsets.CodeCave1.Base = _memoryIo.BaseAddress + 0x1B90B58;
                    CodeCaveOffsets.CodeCave2.Base = _memoryIo.BaseAddress + 0x1BD4320;
                    CodeCaveOffsets.CodeCave3.Base = _memoryIo.BaseAddress + 0x1BE1A30;
                    break;
            }

            var playerService = new PlayerService(_memoryIo);
            var utilityService = new UtilityService(_memoryIo, _hookManager);
            _enemyService = new EnemyService(_memoryIo, _hookManager);
            var itemService = new ItemService(_memoryIo, _hookManager);
            var settingsService = new SettingsService(_memoryIo);

            _playerViewModel = new PlayerViewModel(playerService, hotkeyManager);
            _utilityViewModel = new UtilityViewModel(utilityService, playerService, hotkeyManager);
            _enemyViewModel = new EnemyViewModel(_enemyService, hotkeyManager);
            _itemViewModel = new ItemViewModel(itemService);
            var settingsViewModel = new SettingsViewModel(settingsService, hotkeyManager);

            var playerTab = new PlayerTab(_playerViewModel);
            var utilityTab = new UtilityTab(_utilityViewModel);
            var enemyTab = new EnemyTab(_enemyViewModel);
            var itemTab = new ItemTab(_itemViewModel);
            var settingsTab = new SettingsTab(settingsViewModel);

            MainTabControl.Items.Add(new TabItem { Header = "Player", Content = playerTab });
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            MainTabControl.Items.Add(new TabItem { Header = "Items", Content = itemTab });
            MainTabControl.Items.Add(new TabItem { Header = "Settings", Content = settingsTab });


            _gameLoadedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _gameLoadedTimer.Tick += Timer_Tick;
            _gameLoadedTimer.Start();
        }

        private bool _loaded;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryIo.IsAttached)
            {
                Scan();
                ((TextBlock)MainTabControl.Template.FindName("IsAttachedText", MainTabControl)).Text =
                    "Attached to game";

                _utilityViewModel.TryRestoreAttachedFeatures();
                _enemyService.TryInstallTargetHook();

                if (_memoryIo.IsGameLoaded())
                {
                    if (_loaded) return;
                    _loaded = true;
                    TryEnableActiveOptions();
                }
                else if (_loaded)
                {
                    DisableButtons();
                    _loaded = false;
                }
            }
            else
            {
                _hookManager.ClearHooks();
                _enemyService.ResetHooks();
                DisableButtons();
                _utilityViewModel.ResetAttached();
                _loaded = false;
                ((TextBlock)MainTabControl.Template.FindName("IsAttachedText", MainTabControl)).Text = "Not attached";
            }
        }

        private void Scan()
        {
            var sw = Stopwatch.StartNew();
            Offsets.WorldChrMan.Base = aobScanner.FindAddressByPattern(Patterns.WorldChrMan);
            Offsets.DebugFlags.Base = aobScanner.FindAddressByPattern(Patterns.DebugFlags);
            Offsets.Cam.Base = aobScanner.FindAddressByPattern(Patterns.CamBase);
            Offsets.FileMan.Base = aobScanner.FindAddressByPattern(Patterns.FileMan);
            Offsets.GameDataMan.Base = aobScanner.FindAddressByPattern(Patterns.GameDataMan);
            Offsets.ItemGet = aobScanner.FindAddressByPattern(Patterns.ItemGetFunc).ToInt64();
            Offsets.ItemGetMenuMan = aobScanner.FindAddressByPattern(Patterns.ItemGetMenuMan);
            Offsets.ItemDlgFunc = aobScanner.FindAddressByPattern(Patterns.ItemGetDlgFunc).ToInt64();
            Offsets.FieldArea.Base = aobScanner.FindAddressByPattern(Patterns.FieldArea);
            Offsets.GameMan.Base = aobScanner.FindAddressByPattern(Patterns.GameMan);
            Offsets.DamageMan.Base = aobScanner.FindAddressByPattern(Patterns.DamMan);
            Offsets.DrawEventPatch = aobScanner.FindAddressByPattern(Patterns.DrawEventPatch);
            Offsets.DrawSoundViewPatch = aobScanner.FindAddressByPattern(Patterns.DrawSoundViewPatch);
            Offsets.MenuMan.Base = aobScanner.FindAddressByPattern(Patterns.MenuMan);
            Offsets.ProgressionFlagMan.Base = aobScanner.FindAddressByPattern(Patterns.ProgressionFlagMan);
            Offsets.LevelUpFunc = aobScanner.FindAddressByPattern(Patterns.LevelUpFunc).ToInt64();
            Offsets.RestoreCastsFunc = aobScanner.FindAddressByPattern(Patterns.RestoreCastsFunc).ToInt64();
            Offsets.HgDraw.Base = aobScanner.FindAddressByPattern(Patterns.HgDraw);
            Offsets.WarpEvent = aobScanner.FindAddressByPattern(Patterns.WarpEvent);
            Offsets.WarpFunc = aobScanner.FindAddressByPattern(Patterns.WarpFunc).ToInt64();

            Offsets.Hooks.LastLockedTarget = aobScanner.FindAddressByPattern(Patterns.LastLockedTarget).ToInt64();
            Offsets.Hooks.RepeatAction = aobScanner.FindAddressByPattern(Patterns.RepeatAction).ToInt64();
            Offsets.Hooks.AllNoDamage = aobScanner.FindAddressByPattern(Patterns.AllNoDamage).ToInt64();
            Offsets.Hooks.ItemSpawn = aobScanner.FindAddressByPattern(Patterns.ItemSpawnHook).ToInt64();
            Offsets.Hooks.Draw = aobScanner.FindAddressByPattern(Patterns.DrawHook).ToInt64();
            Offsets.Hooks.TargetingView = aobScanner.FindAddressByPattern(Patterns.TargetingView).ToInt64();
            Offsets.Hooks.InAirTimer = aobScanner.FindAddressByPattern(Patterns.InAirTimer).ToInt64();
            Offsets.Hooks.Keyboard = aobScanner.FindAddressByPattern(Patterns.Keyboard).ToInt64();
            Offsets.Hooks.ControllerR2 = aobScanner.FindAddressByPattern(Patterns.ControllerR2).ToInt64();
            Offsets.Hooks.ControllerL2 = aobScanner.FindAddressByPattern(Patterns.ControllerL2).ToInt64();
            Offsets.Hooks.UpdateCoords = aobScanner.FindAddressByPattern(Patterns.UpdateCoords).ToInt64();
            Offsets.Hooks.WarpCoords = aobScanner.FindAddressByPattern(Patterns.WarpCoords).ToInt64();
            
            sw.Stop();
            Console.WriteLine($"Pattern scanning took: {sw.ElapsedMilliseconds}ms");

            Console.WriteLine($"WorldChrMan: 0x{Offsets.WorldChrMan.Base.ToInt64():X16}");
            Console.WriteLine($"DebugFlags: 0x{Offsets.DebugFlags.Base.ToInt64():X16}");
            Console.WriteLine($"CamBase: 0x{Offsets.Cam.Base.ToInt64():X16}");
            Console.WriteLine($"FileMan: 0x{Offsets.FileMan.Base.ToInt64():X16}");
            Console.WriteLine($"GameDataMan: 0x{Offsets.GameDataMan.Base.ToInt64():X16}");
            Console.WriteLine($"ItemGet: 0x{Offsets.ItemGet:X16}");
            Console.WriteLine($"ItemGetMenuMan: 0x{Offsets.ItemGetMenuMan.ToInt64():X16}");
            Console.WriteLine($"ItemDlgFunc: 0x{Offsets.ItemDlgFunc:X16}");
            Console.WriteLine($"FieldArea: 0x{Offsets.FieldArea.Base.ToInt64():X16}");
            Console.WriteLine($"GameMan: 0x{Offsets.GameMan.Base.ToInt64():X16}");
            Console.WriteLine($"DamageMan: 0x{Offsets.DamageMan.Base.ToInt64():X16}");
            Console.WriteLine($"DrawEventPatch: 0x{Offsets.DrawEventPatch.ToInt64():X16}");
            Console.WriteLine($"DrawSoundViewPatch: 0x{Offsets.DrawSoundViewPatch.ToInt64():X16}");
            Console.WriteLine($"MenuMan: 0x{Offsets.MenuMan.Base.ToInt64():X16}");
            Console.WriteLine($"ProgressionFlagMan: 0x{Offsets.ProgressionFlagMan.Base.ToInt64():X16}");
            Console.WriteLine($"LevelUpFunc: 0x{Offsets.LevelUpFunc:X16}");
            Console.WriteLine($"RestoreCastsFunc: 0x{Offsets.RestoreCastsFunc:X16}");
            Console.WriteLine($"HgDraw: 0x{Offsets.HgDraw.Base.ToInt64():X16}");
            Console.WriteLine($"WarpEvent: 0x{Offsets.WarpEvent.ToInt64():X16}");
            Console.WriteLine($"WarpFunc: 0x{Offsets.WarpFunc:X16}");

            Console.WriteLine($"LastLockedTarget: 0x{Offsets.Hooks.LastLockedTarget:X16}");
            Console.WriteLine($"RepeatAction: 0x{Offsets.Hooks.RepeatAction:X16}");
            Console.WriteLine($"AllNoDamage: 0x{Offsets.Hooks.AllNoDamage:X16}");
            Console.WriteLine($"ItemSpawn: 0x{Offsets.Hooks.ItemSpawn:X16}");
            Console.WriteLine($"Draw: 0x{Offsets.Hooks.Draw:X16}");
            Console.WriteLine($"TargetingView: 0x{Offsets.Hooks.TargetingView:X16}");
            Console.WriteLine($"InAirTimer: 0x{Offsets.Hooks.InAirTimer:X16}");
            Console.WriteLine($"Keyboard: 0x{Offsets.Hooks.Keyboard:X16}");
            Console.WriteLine($"ControllerR2: 0x{Offsets.Hooks.ControllerR2:X16}");
            Console.WriteLine($"ControllerL2: 0x{Offsets.Hooks.ControllerL2:X16}");
            Console.WriteLine($"UpdateCoords: 0x{Offsets.Hooks.UpdateCoords:X16}");
            Console.WriteLine($"WarpCoords: 0x{Offsets.Hooks.WarpCoords:X16}");
        }

        private void TryEnableActiveOptions()
        {
            _playerViewModel.TryEnableActiveOptions();
            _utilityViewModel.TryEnableActiveOptions();
            _enemyViewModel.TryEnableActiveOptions();
            _itemViewModel.TryEnableActiveOptions();
        }

        private void DisableButtons()
        {
            _playerViewModel.DisableButtons();
            _utilityViewModel.DisableButtons();
            _enemyViewModel.DisableButtons();
            _itemViewModel.DisableButtons();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _memoryIo?.Dispose();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}