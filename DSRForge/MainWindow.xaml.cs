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

        public MainWindow()
        {
            _memoryIo = new MemoryIo();
            _memoryIo.FindAndAttach();

            InitializeComponent();

            var hookManager = new HookManager(_memoryIo);

            var playerService = new PlayerService(_memoryIo, hookManager);
            var utilityService = new UtilityService(_memoryIo, hookManager);
            var targetService = new EnemyService(_memoryIo, hookManager);
            var itemService = new ItemService(_memoryIo, hookManager);

            var hotkeyManager = new HotkeyManager(_memoryIo.TargetProcess.Id);


            _playerViewModel = new PlayerViewModel(playerService, hotkeyManager);
            _utilityViewModel = new UtilityViewModel(utilityService, playerService, hotkeyManager);
            _enemyViewModel = new EnemyViewModel(targetService, hotkeyManager);
            _itemViewModel = new ItemViewModel(itemService);
            var settingsViewModel = new SettingsViewModel(hotkeyManager);

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

            if (_memoryIo.IsGameLoaded())
            {
                if (_loaded) return;
                _loaded = true;
                _playerViewModel.RestoreOptions();
                _utilityViewModel.RestoreOptions();
                _enemyViewModel.RestoreOptions();
                _itemViewModel.RestoreOptions();
            }
            else if (_loaded)
            {
                _playerViewModel.SaveAndDisableOptions();
                _utilityViewModel.SaveAndDisableOptions();
                _enemyViewModel.SaveAndDisableOptions();
                _itemViewModel.SaveAndDisableOptions();
                _loaded = false;
            }
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
                // Double-click to maximize/restore
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
            {
                // Single click to drag the window
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