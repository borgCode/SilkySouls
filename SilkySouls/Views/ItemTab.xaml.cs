using System.Windows;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class ItemTab
    {
        private readonly ItemViewModel _itemViewModel;
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
    }
}