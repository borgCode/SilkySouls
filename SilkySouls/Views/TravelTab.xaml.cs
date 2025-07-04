using System.Windows;
using System.Windows.Controls;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class TravelTab
    {
        private readonly TravelViewModel _travelViewModel;
        public TravelTab(TravelViewModel travelViewModel)
        {
            InitializeComponent();
            _travelViewModel = travelViewModel;
            DataContext = _travelViewModel;
        }

        private void WarpButton_Click(object sender, RoutedEventArgs e)
        {
            _travelViewModel.Warp();
        }

        private void UnlockAllWarps_Click(object sender, RoutedEventArgs e) => _travelViewModel.UnlockAllBonfires();
    }
}