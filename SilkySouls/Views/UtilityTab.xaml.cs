using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SilkySouls.Memory;
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

        private void UpgradeWeapon_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowUpgradeMenu(isWeapon: true);
        }

        private void UpgradeArmor_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.ShowUpgradeMenu(isWeapon: false);
        }

        private void UnlockBonfires_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.UnlockBonfires();
        }

        private void UnlockKalameet_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.UnlockKalameet();
        }

        private void GargBell_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.RingGargBell();
        }

        private void QuelaggBell_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.RingQuelaggBell();
        }

        private void Sens_Click(object sender, RoutedEventArgs e)
        {
             _utilityViewModel.OpenSens();
        }

        private void MaleUdMerchant_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.MaleUdMerchant);
        }

        private void FemaleUdMerchant_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.FemaleUdMerchant);
        }

        private void Zena_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Zena);
        }

        private void Patches_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Patches);
        }

        private void Shiva_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Shiva);
        }

        private void Griggs_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Griggs);
        }

        private void Dusk_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Dusk);
        }

        private void Ingward_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Ingward);
        }

        private void Laurentius_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Laurentius);
        }

        private void Eingyi_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Eingyi);
        }

        private void Quelana_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Quelana);
        }

        private void Petrus_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Petrus);
        }

        private void Reah_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Reah);
        }

        private void Oswald_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Oswald);
        }

        private void Logan_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Logan);
        }

        private void CrestfallenMerchant_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.CrestfallenMerchant);
        }

        private void Chester_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Chester);
        }

        private void Elizabeth_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Elizabeth);
        }

        private void Gough_Click(object sender, RoutedEventArgs e)
        {
            _utilityViewModel.OpenShop(GameIds.ShopParams.Gough);
        }
    }
}