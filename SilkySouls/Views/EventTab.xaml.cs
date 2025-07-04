using System.Windows;
using System.Windows.Controls;
using SilkySouls.ViewModels;

namespace SilkySouls.Views
{
    public partial class EventTab : UserControl
    {
        private readonly EventViewModel _eventViewModel;
        public EventTab(EventViewModel eventViewModel)
        {
            InitializeComponent();
            _eventViewModel = eventViewModel;
            DataContext = eventViewModel;
        }

        private void SetFlag_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.SetFlag();
        }

        private void GetEvent_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.GetEvent();
        }
        
        
        private void LordVessel_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.PlaceLordVessel();
        }

        private void NewLondo_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.NewLondoNoWater();
        }
        
        private void UnlockKalameet_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.UnlockKalameet();
        }

        private void GargBell_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.RingGargBell();
        }

        private void QuelaggBell_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.RingQuelaggBell();
        }

        private void Sens_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.OpenSens();
        }
        
        private void LaurentiusToFirelink_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.LaurentiusToFirelink();
        }

        private void LoganToFirelink_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.LoganToFirelink();
        }

        private void GriggsToFirelink_Click(object sender, RoutedEventArgs e)
        {
            _eventViewModel.GriggsToFirelink();
        }
    }
}