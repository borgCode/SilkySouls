using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DSRForge.Memory;
using DSRForge.Services;
using DSRForge.Utilities;
using DSRForge.ViewModels;
using DSRForge.Views;

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
        private readonly AoBScanner _aobScanner;

        public MainWindow()
        {
            _memoryIo = new MemoryIo();
            var patch = VersionChecker.GetPatch();
            _memoryIo.StartAutoAttach();

            InitializeComponent();

            _hookManager = new HookManager(_memoryIo);
            var hotkeyManager = new HotkeyManager(_memoryIo);
            _aobScanner = new AoBScanner(_memoryIo);
            
            switch (patch)
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
            PatchText.Text = "Patch: " + patch;
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
        private bool _hasScanned;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryIo.IsAttached)
            {
                IsAttachedText.Text = "Attached to game";
                if (!_hasScanned)
                {
                    _aobScanner.Scan();
                    _hasScanned = true;
                }
                
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
                IsAttachedText.Text = "Not attached";
            }
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