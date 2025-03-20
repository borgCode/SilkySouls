using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls.ViewModels;
using Xceed.Wpf.Toolkit;

namespace SilkySouls.Views
{
    /// <summary>
    /// Interaction logic for TargetTab.xaml
    /// </summary>
    public partial class EnemyTab
    {
        private readonly EnemyViewModel _enemyViewModel;

        public EnemyTab(EnemyViewModel enemyViewModel)
        {
            InitializeComponent();
            _enemyViewModel = enemyViewModel;
            DataContext = _enemyViewModel;
        }
        
        private void OnHealthButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string parameter = button.CommandParameter.ToString();
            int healthPercentage = int.Parse(parameter);
            _enemyViewModel.SetTargetHealth(healthPercentage);
        }
        
        private void SpeedUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
            var upDown = sender as DoubleUpDown;
            if (upDown?.Value.HasValue == true)
            {
                _enemyViewModel.SetSpeed((float)upDown.Value);
            }
            Focus();
            e.Handled = true;
        }

        private void SpeedUpDown_LostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var upDown = sender as DoubleUpDown;
            if (upDown?.Value.HasValue == true)
            {
                _enemyViewModel.SetSpeed((float)upDown.Value);
            }
        }
    }
}