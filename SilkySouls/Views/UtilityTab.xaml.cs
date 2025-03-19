using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
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

        private void DrawInfoBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "DRAW INFORMATION\n\n" +
                "Enable Draw Instructions:\n" +
                "• You must exit to the main menu before you can view hitboxes, draw events, and other visual elements.\n\n" +
                "Known Display Issues:\n" +
                "• Temporal Anti-Aliasing (default setting) will prevent hitboxes from displaying properly.\n" +
                "• Solution: Select any other anti-aliasing option in quality settings to fix the display.",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void NoClipInfoBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Vertical movement with Ctrl/Space or L2/R2 on controller", "Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LevelUpMenu_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowLevelUpMenu();
        }

        private void AttunementMenu_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowAttunementMenu();
        }
    }
}