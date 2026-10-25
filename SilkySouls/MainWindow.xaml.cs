using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
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
        private readonly IMemoryService _memoryService;
        private readonly DispatcherTimer _gameLoadedTimer;

        private readonly AoBScanner _aobScanner;
        private readonly IStateService _stateService;
        private readonly IPlayerHitBehaviorService _playerHitBehaviorService;

        public MainWindow()
        {
            _memoryService = new MemoryService();
            _memoryService.StartAutoAttach();
            _stateService = new StateService(_memoryService);

            InitializeComponent();
            
            var savedLeft = SettingsManager.Default.WindowLeft;
            var savedTop = SettingsManager.Default.WindowTop;
            if ((savedLeft != 0 || savedTop != 0) && IsOnVisibleScreen(savedLeft, savedTop))
            {
                Left = savedLeft;
                Top = savedTop;
            }
            else WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var hotkeyManager = new HotkeyManager(_memoryService);
            var hookManager = new HookManager(_memoryService, _stateService);

            IGameTickService gameTickService = new GameTickService(_stateService);

            ITravelService travelService = new TravelService(_memoryService, hookManager);
            IPlayerService playerService = new PlayerService(_memoryService, travelService);

            ITargetService targetService = new TargetService(_memoryService, hookManager);

            _aobScanner = new AoBScanner(_memoryService);

            IEmevdService emevdService = new EmevdService(_memoryService);
            IEzStateService ezStateService = new EzStateService(_memoryService);
            IEnemyService enemyService = new EnemyService(_memoryService, hookManager);
            IEventService eventService = new EventService(_memoryService, playerService, emevdService);
            IUtilityService utilityService = new UtilityService(_memoryService, hookManager);
            IDebugDrawService debugDrawService = new DebugDrawService(_memoryService, hookManager, _stateService);
            IParamService paramService = new ParamService(_memoryService);
            IItemService itemService = new ItemService(_memoryService, _stateService);
            ISettingsService settingsService = new SettingsService(_memoryService);
            _playerHitBehaviorService = new PlayerHitBehaviorService(_memoryService, hookManager,
                gameTickService, _stateService);


            var playerViewModel = new PlayerViewModel(playerService, hotkeyManager, _stateService, gameTickService);
            TargetViewModel targetViewModel =
                new TargetViewModel(targetService, hotkeyManager, gameTickService, _stateService);
            var utilityViewModel = new UtilityViewModel(utilityService, hotkeyManager, playerViewModel, paramService,
                _stateService, ezStateService, debugDrawService);
            var travelViewModel = new TravelViewModel(travelService, hotkeyManager, utilityViewModel, _stateService);
            var eventViewModel = new EventViewModel(eventService,  _stateService);
            var enemyViewModel = new EnemyViewModel(enemyService, hotkeyManager, _stateService, emevdService, _playerHitBehaviorService);
            var itemViewModel = new ItemViewModel(itemService, _stateService);
            var settingsViewModel = new SettingsViewModel(settingsService, hotkeyManager, _stateService);

            var playerTab = new PlayerTab(playerViewModel);
            var travelTab = new TravelTab(travelViewModel);
            var eventTab = new EventTab(eventViewModel);
            var utilityTab = new UtilityTab(utilityViewModel);
            var enemyTab = new EnemyTab(enemyViewModel);
            var targetTab = new TargetTab(targetViewModel);
            var itemTab = new ItemTab(itemViewModel);
            var settingsTab = new SettingsTab(settingsViewModel);

            MainTabControl.Items.Add(new TabItem { Header = "Player", Content = playerTab });
            MainTabControl.Items.Add(new TabItem { Header = "Travel", Content = travelTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            MainTabControl.Items.Add(new TabItem { Header = "Target", Content = targetTab });
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Event", Content = eventTab });
            MainTabControl.Items.Add(new TabItem { Header = "Items", Content = itemTab });
            MainTabControl.Items.Add(new TabItem { Header = "Settings", Content = settingsTab });

            settingsViewModel.ApplyStartUpOptions();
            Closing += MainWindow_Closing;

            _gameLoadedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(25)
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
        private bool _hasAllocatedMemory;
        private bool _hasCheckedPatch;
        private bool _hasPublishedFadedIn;
        private bool _hasPublishedLoaded;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryService.IsAttached)
            {
                IsAttachedText.Text = "Attached to game";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["AttachedBrush"];

                if (!_hasCheckedPatch)
                {
                    if (!PatchManager.Initialize(_memoryService))
                    {
                        _aobScanner.Scan();
                    }

                    _hasCheckedPatch = true;
                }


                if (!_hasAllocatedMemory)
                {
                    _memoryService.AllocCodeCave();
#if DEBUG
                    Console.WriteLine($"Code cave: 0x{(long)CodeCaveOffsets.Base:X}");
#endif
                    _hasAllocatedMemory = true;
                }
                

                if (_stateService.IsLoaded())
                {
                    if (!_hasPublishedFadedIn && _hasPublishedLoaded && !_stateService.IsFading())
                    {
                        _stateService.Publish(State.FadedIn);
                        _hasPublishedFadedIn = true;
                    }
                    if (_loaded) return;
                    _loaded = true;
                    _hasPublishedLoaded = true;
                    _stateService.Publish(State.Loaded);
                    TryPublishNewGameStart();
                }
                else if (_loaded)
                {
                    _stateService.Publish(State.NotLoaded);
                    _loaded = false;
                    _hasPublishedLoaded = false;
                    _hasPublishedFadedIn = false;
                }
            }
            else
            {
                _stateService.Publish(State.Detached);
                _stateService.Publish(State.NotLoaded);
                _hasAllocatedMemory = false;
                _hasCheckedPatch = false;
                _hasPublishedLoaded = false;
                _hasPublishedFadedIn = false;
                _loaded = false;
                IsAttachedText.Text = "Not attached";
                IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["NotAttachedBrush"];
            }
        }

        private void TryPublishNewGameStart()
        {
            var gameDataPtr = _memoryService.Read<nint>(Offsets.GameDataMan.Base);
            IntPtr inGameTimePtr = gameDataPtr + (int)Offsets.GameDataMan.GameDataOffsets.InGameTime;
            long gameTimeMs = _memoryService.Read<long>(inGameTimePtr);
            if (gameTimeMs < 5000)
            {
                _stateService.Publish(State.OnNewGameStart);
            }
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
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void CheckUpdate_Click(object sender, RoutedEventArgs e) => VersionChecker.CheckForUpdates(this, true);
        
        private static bool IsOnVisibleScreen(double left, double top)
        {
            const double minVisibleX = 100;
            const double minVisibleY = 30;
            var vLeft = SystemParameters.VirtualScreenLeft;
            var vTop = SystemParameters.VirtualScreenTop;
            var vRight = vLeft + SystemParameters.VirtualScreenWidth;
            var vBottom = vTop + SystemParameters.VirtualScreenHeight;
            return left + minVisibleX > vLeft
                   && left < vRight - minVisibleX
                   && top + minVisibleY > vTop
                   && top < vBottom - minVisibleY;
        }
    }
}