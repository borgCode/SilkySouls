using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SilkySouls.memory;
using SilkySouls.Memory;
using SilkySouls.Services;
using SilkySouls.Utilities;
using SilkySouls.ViewModels;
using SilkySouls.Views;

namespace SilkySouls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MemoryIo _memoryIo;
        private readonly DispatcherTimer _gameLoadedTimer;

        private readonly PlayerViewModel _playerViewModel;
        private readonly TravelViewModel _travelViewModel;
        private readonly UtilityViewModel _utilityViewModel;
        private readonly EnemyViewModel _enemyViewModel;
        private readonly ItemViewModel _itemViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly HookManager _hookManager;
        private readonly AoBScanner _aobScanner;
        private readonly ItemService _itemService;

        public MainWindow()
        {
            _memoryIo = new MemoryIo();
            _memoryIo.StartAutoAttach();

            InitializeComponent();
            if (SettingsManager.Default.WindowLeft != 0 || SettingsManager.Default.WindowTop != 0)
            {
                Left = SettingsManager.Default.WindowLeft;
                Top = SettingsManager.Default.WindowTop;
            }
            else WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _hookManager = new HookManager(_memoryIo);
            var hotkeyManager = new HotkeyManager(_memoryIo);
            _aobScanner = new AoBScanner(_memoryIo);
            var playerService = new PlayerService(_memoryIo);
            var travelService = new TravelService(_memoryIo, _hookManager);
            var utilityService = new UtilityService(_memoryIo, _hookManager);
            var enemyService = new EnemyService(_memoryIo, _hookManager, _aobScanner);
            _itemService = new ItemService(_memoryIo, _hookManager);
            var settingsService = new SettingsService(_memoryIo);

            _playerViewModel = new PlayerViewModel(playerService, hotkeyManager);
            _utilityViewModel = new UtilityViewModel(utilityService, hotkeyManager, _playerViewModel);
            _travelViewModel = new TravelViewModel(travelService, hotkeyManager, _utilityViewModel);
            _enemyViewModel = new EnemyViewModel(enemyService, hotkeyManager);
            _itemViewModel = new ItemViewModel(_itemService);
            _settingsViewModel = new SettingsViewModel(settingsService, hotkeyManager);

            var playerTab = new PlayerTab(_playerViewModel);
            var travelTab = new TravelTab(_travelViewModel);
            var utilityTab = new UtilityTab(_utilityViewModel);
            var enemyTab = new EnemyTab(_enemyViewModel);
            var itemTab = new ItemTab(_itemViewModel);
            var settingsTab = new SettingsTab(_settingsViewModel);

            MainTabControl.Items.Add(new TabItem { Header = "Player", Content = playerTab });
            MainTabControl.Items.Add(new TabItem { Header = "Travel", Content = travelTab });
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            MainTabControl.Items.Add(new TabItem { Header = "Items", Content = itemTab });
            MainTabControl.Items.Add(new TabItem { Header = "Settings", Content = settingsTab });

            _settingsViewModel.ApplyStartUpOptions();
            Closing += MainWindow_Closing;
            
            _gameLoadedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            _gameLoadedTimer.Tick += Timer_Tick;
            _gameLoadedTimer.Start();
            
            VersionChecker.UpdateVersionText(AppVersion);
            
            if (SettingsManager.Default.EnableUpdateChecks)
            {
                VersionChecker.CheckForUpdates(this);
            }
        }

        private bool _loaded;
        private bool _hasScanned;
        private bool _hasAllocatedMemory;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryIo.IsAttached)
            {
                IsAttachedText.Text = "Attached to game";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["AttachedBrush"];
                
                if (!_hasScanned)
                {
                    _aobScanner.Scan();
                    _hasScanned = true;
                }

                if (!_hasAllocatedMemory)
                {
                    _memoryIo.AllocCodeCave();
                    Console.WriteLine($"Code cave: 0x{CodeCaveOffsets.Base.ToInt64():X}");
                    _hasAllocatedMemory = true;
                }
                
                _utilityViewModel.TryRestoreAttachedFeatures();
                
                if (_memoryIo.IsGameLoaded())
                {
                    if (_loaded) return;
                    _loaded = true;
                    TryEnableFeatures();
                    TrySetGameStartPrefs();
                    _settingsViewModel.ApplyLoadedOptions();
                }
                else if (_loaded)
                {
                    DisableFeatures();
                    _loaded = false;
                }
            }
            else
            {
                _hookManager.ClearHooks();
                DisableFeatures();
                _utilityViewModel.ResetAttached();
                _settingsViewModel.ResetAttached();
                _hasAllocatedMemory = false;
                _loaded = false;
                _itemService.Reset();
                IsAttachedText.Text = "Not attached";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["NotAttachedBrush"];
            }
        }

        private void TryEnableFeatures()
        {
            _playerViewModel.TryEnableActiveOptions();
            _utilityViewModel.TryEnableActiveOptions();
            _enemyViewModel.TryEnableActiveOptions();
            _itemViewModel.TryEnableActiveOptions();
            _travelViewModel.TryEnableFeatures();
        }

        private void TrySetGameStartPrefs()
        {
            ulong gameDataPtr = _memoryIo.ReadUInt64(Offsets.GameDataMan.Base);
            IntPtr inGameTimePtr = (IntPtr)(gameDataPtr + (int)Offsets.GameDataMan.GameDataOffsets.InGameTime);
            long gameTimeMs = _memoryIo.ReadInt64(inGameTimePtr);
            if (gameTimeMs < 5000)
            {
                _playerViewModel.TrySetNgPref();
                _itemViewModel.TrySpawnWeaponPref();
            }
        }

        private void DisableFeatures()
        {
            _travelViewModel.DisableFeatures();
            _playerViewModel.DisableButtons();
            _utilityViewModel.DisableButtons();
            _enemyViewModel.DisableButtons();
            _itemViewModel.DisableButtons();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _hookManager?.UninstallAllHooks();
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
        
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
      
            SettingsManager.Default.WindowLeft = Left;
            SettingsManager.Default.WindowTop = Top;
            SettingsManager.Default.Save();
            _itemService.SignalClose();
            _hookManager.UninstallAllHooks();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void CheckUpdate_Click(object sender, RoutedEventArgs e) => VersionChecker.CheckForUpdates(this, true);
    }
}