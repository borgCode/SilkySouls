using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class ItemTab
    {
        private readonly ItemViewModel _itemViewModel;
        private string _lastValidText;
        
        public ItemTab(ItemViewModel itemViewModel)
        {
            InitializeComponent();
            _itemViewModel = itemViewModel;
            DataContext = _itemViewModel;

        }
        private void SpawnButton_Click(object sender, RoutedEventArgs e)
        {
            _itemViewModel.SpawnItem();
        }

        private void AutoSpawn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            _lastValidText = combo.Text;
            
            combo.PreviewMouseDown -= AutoSpawn_PreviewMouseDown;
            combo.DropDownClosed += AutoSpawn_DropDownClosed;
                
            combo.Dispatcher.BeginInvoke(new Action(() =>
            {
                combo.IsEditable = true;
                combo.Focus();
                combo.IsDropDownOpen = true;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
        
        private void AutoSpawn_DropDownClosed(object sender, EventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            
            if (string.IsNullOrWhiteSpace(combo.Text))
            {
                combo.Text = _lastValidText;
            }
            combo.IsEditable = false;
            combo.DropDownClosed -= AutoSpawn_DropDownClosed;
            combo.PreviewMouseDown += AutoSpawn_PreviewMouseDown;
        }
    }
}