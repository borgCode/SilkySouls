using System.Windows.Media;
using SilkySouls.Memory;
using SilkySouls.Services;

namespace SilkySouls.ViewModels
{
    public class EventViewModel : BaseViewModel
    {
        private readonly EventService _eventService;
        private bool _isDisableEventsEnabled;
        private string _setFlagId;
        private int _flagStateIndex;
        private string _getFlagId;
        
        private string _eventStatusText;
        private Brush _eventStatusColor;

        private bool _areButtonsEnabled;

        public EventViewModel(EventService eventService)
        {
            _eventService = eventService;
        }
        
                
        public bool IsDisableEventsEnabled
        {
            get => _isDisableEventsEnabled;
            set
            {
                if (!SetProperty(ref _isDisableEventsEnabled, value)) return;
                _eventService.ToggleDisableEvents(_isDisableEventsEnabled);
            }
        }
        
        public string SetFlagId
        {
            get => _setFlagId;
            set => SetProperty(ref _setFlagId, value);
        }

        public int FlagStateIndex
        {
            get => _flagStateIndex;
            set => SetProperty(ref _flagStateIndex, value);
        }

        public void SetFlag()
        {
            if (string.IsNullOrWhiteSpace(SetFlagId))
                return;
            
            string trimmedFlagId = SetFlagId.Trim();
        
            if (!ulong.TryParse(trimmedFlagId, out ulong flagIdValue) || flagIdValue <= 0)
                return;
            _eventService.SetEvent(flagIdValue, FlagStateIndex == 0 ? 1 : 0);
        }
        
        public string GetFlagId
        {
            get => _getFlagId;
            set => SetProperty(ref _getFlagId, value);
        }
        
        public string EventStatusText
        {
            get => _eventStatusText;
            set => SetProperty(ref _eventStatusText, value);
        }

        public Brush EventStatusColor
        {
            get => _eventStatusColor;
            set => SetProperty(ref _eventStatusColor, value);
        }

        public void GetEvent()
        {
            if (string.IsNullOrWhiteSpace(GetFlagId))
                return;
            
            string trimmedFlagId = GetFlagId.Trim();
            
            if (!ulong.TryParse(trimmedFlagId, out ulong flagIdValue) || flagIdValue <= 0)
                return;

            if (_eventService.GetEvent(flagIdValue))
            {
                EventStatusText = "True";
                EventStatusColor = Brushes.Chartreuse;
            }
            else
            {
                EventStatusText = "False";
                EventStatusColor = Brushes.Red;
            }
        }
        
        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set => SetProperty(ref _areButtonsEnabled, value);
        }

        public void TryEnableActiveOptions()
        {
            AreButtonsEnabled = true;
        }

        public void DisableFeatures()
        {
            AreButtonsEnabled = false;
            IsDisableEventsEnabled = false;
        }
        
        public void UnlockKalameet() => _eventService.SetMultipleEventsOn(GameIds.EventFlags.UnlockKalameet);
        public void NewLondoNoWater() => _eventService.SetEvent(GameIds.EventFlags.NewLondoWater, 1);
        public void RingGargBell() => _eventService.RingGargBell();
        public void RingQuelaggBell() => _eventService.RingQuelaagBell();
        public void OpenSens() => _ = _eventService.OpenSensGate(GameIds.EventFlags.Sens);
        public void PlaceLordVessel() => _ = _eventService.PlaceLordVessel();
        public void LaurentiusToFirelink() => _eventService.SetEvent(GameIds.EventFlags.LaurentiusToFirelink, 1);
        public void LoganToFirelink() => _eventService.SetMultipleEventsOn(GameIds.EventFlags.LoganToFirelink);
        public void GriggsToFirelink() => _eventService.SetMultipleEventsOn(GameIds.EventFlags.GriggsToFirelink);
    }
}