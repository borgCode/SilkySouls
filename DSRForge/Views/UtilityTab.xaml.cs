using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DSRForge.ViewModels;

namespace DSRForge.Views
{
    /// <summary>
    /// Interaction logic for UtilityTab.xaml
    /// </summary>
    public partial class UtilityTab
    {
        private readonly UtilityViewModel _utilityViewModel;
        public UtilityTab(UtilityViewModel utilityViewModel)
        {
            InitializeComponent();
            _utilityViewModel = utilityViewModel;
            DataContext = utilityViewModel;
        }
        
        
        private void Warp_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.Warp();
        }

        private void InfoBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Enabling draw requires you to exit to main menu before being able to use the different draws", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
