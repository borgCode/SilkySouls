using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using SilkySouls.Utilities;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class ResistancesWindow
    {
        private double _scaleMultiplier = 1.0;
        public ResistancesWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) => DragMove();
            Background = new SolidColorBrush(
                Color.FromArgb(128, 0, 0, 0));


            Loaded += (s, e) =>
            {
                if (SettingsManager.Default.ResistancesWindowLeft > 0)
                    Left = SettingsManager.Default.ResistancesWindowLeft;
        
                if (SettingsManager.Default.ResistancesWindowTop > 0)
                    Top = SettingsManager.Default.ResistancesWindowTop;
        
                if (SettingsManager.Default.ResistancesWindowScaleX > 0)
                {
                    _scaleMultiplier = SettingsManager.Default.ResistancesWindowScaleX;
                    ContentScale.ScaleX = _scaleMultiplier;
                    ContentScale.ScaleY = _scaleMultiplier;
                }
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                User32.SetTopmost(hwnd);

                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing += (sender, args) => { Close(); };
                }
                
                
            };
            ContentRendered += (s, e) =>
            {
                if (SettingsManager.Default.ResistancesWindowWidth > 0)
                {
                    Width = SettingsManager.Default.ResistancesWindowWidth;
                }
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EnemyViewModel viewModel)
            {
                viewModel.IsResistancesWindowOpen = false;
            }

            Close();
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            SettingsManager.Default.ResistancesWindowScaleX = _scaleMultiplier;
            SettingsManager.Default.ResistancesWindowLeft = Left;
            SettingsManager.Default.ResistancesWindowTop = Top;
            SettingsManager.Default.ResistancesWindowWidth = Width;
            SettingsManager.Default.Save();
        }


        private void DecreaseSize_Click(object sender, RoutedEventArgs e)
        {
            if (_scaleMultiplier > 0.6)
            {
                Width *= _scaleMultiplier / (_scaleMultiplier + 0.2);
                _scaleMultiplier -= 0.2;
                ContentScale.ScaleX = _scaleMultiplier;
                ContentScale.ScaleY = _scaleMultiplier;
            }
        }

        private void IncreaseSize_Click(object sender, RoutedEventArgs e)
        {
            if (_scaleMultiplier < 3.0)
            {
                Width *= (_scaleMultiplier + 0.2) / _scaleMultiplier;
                _scaleMultiplier += 0.2;
                ContentScale.ScaleX = _scaleMultiplier;
                ContentScale.ScaleY = _scaleMultiplier;
            }
        }
    }
}