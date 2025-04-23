using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SilkySouls.Memory;
using SilkySouls.Models;
using SilkySouls.Services;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        private bool _isDrawEnabled;
        private bool _isHitboxEnabled;
        private bool _isSoundViewEnabled;
        private bool _isDrawEventEnabled;
        private bool _isTargetingViewEnabled;
        private bool _isNoClipEnabled;
        private bool _isDeathCamEnabled;
        private bool _isDisableEventsEnabled;
        private bool _isFilterRemoveEnabled;

        private bool _areButtonsEnabled;
        private bool _areAttachedOptionsEnabled;
        private bool _areAttachedOptionsRestored;

        private bool _wasNoDeathEnabled;
        private bool _wasNoDmgEnabled;

        private readonly UtilityService _utilityService;
        private readonly PlayerViewModel _playerViewModel;
        private readonly HotkeyManager _hotkeyManager;

        private readonly Dictionary<string, Location> _warpLocations;
        private KeyValuePair<string, string> _selectedLocation;

        public UtilityViewModel(UtilityService utilityService, HotkeyManager hotkeyManager,
            PlayerViewModel playerViewModel)
        {
            _utilityService = utilityService;
            _playerViewModel = playerViewModel;
            _warpLocations = DataLoader.GetLocationDict();
            _hotkeyManager = hotkeyManager;

            if (_warpLocations.Any())
            {
                var firstLocation = _warpLocations.First();
                _selectedLocation = new KeyValuePair<string, string>(firstLocation.Key, firstLocation.Value.Name);
            }
            
            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("NoClip", () => { IsNoClipEnabled = !IsNoClipEnabled; });
            _hotkeyManager.RegisterAction("Warp", Warp);
        }

        public IEnumerable<KeyValuePair<string, string>> WarpLocations =>
            _warpLocations.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Name));

        public KeyValuePair<string, string> SelectedLocation
        {
            get => _selectedLocation;
            set => SetProperty(ref _selectedLocation, value);
        }


        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set => SetProperty(ref _areButtonsEnabled, value);
        }

        public bool AreAttachedOptionsEnabled
        {
            get => _areAttachedOptionsEnabled;
            set => SetProperty(ref _areAttachedOptionsEnabled, value);
        }


        public bool IsDrawEnabled
        {
            get => _isDrawEnabled;
            set
            {
                if (!SetProperty(ref _isDrawEnabled, value)) return;
                if (value)
                {
                    _utilityService.EnableDraw();
                }
                else
                {
                    _utilityService.DisableDraw();
                    IsHitboxEnabled = false;
                    IsDrawEventEnabled = false;
                    IsSoundViewEnabled = false;
                    IsTargetingViewEnabled = false;
                }
            }
        }

        public bool IsHitboxEnabled
        {
            get => _isHitboxEnabled;
            set
            {
                if (!SetProperty(ref _isHitboxEnabled, value)) return;
                if (_isHitboxEnabled)
                {
                    _utilityService.EnableHitboxView();
                }
                else
                {
                    _utilityService.DisableHitboxView();
                }
            }
        }

        public bool IsSoundViewEnabled
        {
            get => _isSoundViewEnabled;
            set
            {
                if (!SetProperty(ref _isSoundViewEnabled, value)) return;
                if (_isSoundViewEnabled)
                {
                    _utilityService.EnableSoundView();
                }
                else
                {
                    _utilityService.DisableSoundView();
                }
            }
        }

        public bool IsDrawEventEnabled
        {
            get => _isDrawEventEnabled;
            set
            {
                if (!SetProperty(ref _isDrawEventEnabled, value)) return;
                if (_isDrawEventEnabled)
                {
                    _utilityService.EnableDrawEvent();
                }
                else
                {
                    _utilityService.DisableDrawEvent();
                }
            }
        }

        public bool IsTargetingViewEnabled
        {
            get => _isTargetingViewEnabled;
            set
            {
                if (!SetProperty(ref _isTargetingViewEnabled, value)) return;
                if (_isTargetingViewEnabled)
                {
                    _utilityService.EnableTargetingView();
                }
                else
                {
                    _utilityService.DisableTargetingView();
                }
            }
        }

        public bool IsNoClipEnabled
        {
            get => _isNoClipEnabled;
            set
            {
                if (!SetProperty(ref _isNoClipEnabled, value)) return;
                if (_isNoClipEnabled)
                {
                    _utilityService.EnableNoClip();
                    _wasNoDeathEnabled = _playerViewModel.IsNoDeathEnabled;
                    _wasNoDmgEnabled = _playerViewModel.IsNoDamageEnabled;
                    _playerViewModel.IsNoDeathEnabled = true;
                    _playerViewModel.IsNoDamageEnabled = true;
                    _playerViewModel.IsSilentEnabled = true;
                    _playerViewModel.IsInvisibleEnabled = true;
                }
                else
                {
                    _utilityService.DisableNoClip();
                    _playerViewModel.IsNoDeathEnabled = _wasNoDeathEnabled;
                    _playerViewModel.IsNoDamageEnabled = _wasNoDmgEnabled;
                    _playerViewModel.IsSilentEnabled = false;
                    _playerViewModel.IsInvisibleEnabled = false;
                }
            }
        }

        public bool IsDeathCamEnabled
        {
            get => _isDeathCamEnabled;
            set
            {
                if (!SetProperty(ref _isDeathCamEnabled, value)) return;
                _utilityService.ToggleDeathCam(_isDeathCamEnabled);
            }
        }
        
        public bool IsDisableEventsEnabled
        {
            get => _isDisableEventsEnabled;
            set
            {
                if (!SetProperty(ref _isDisableEventsEnabled, value)) return;
                _utilityService.ToggleDisableEvents(_isDisableEventsEnabled);
            }
        }

        public bool IsFilterRemoveEnabled
        {
            get => _isFilterRemoveEnabled;
            set
            {
                if (!SetProperty(ref _isFilterRemoveEnabled, value)) return;
                _utilityService.ToggleFilter(_isFilterRemoveEnabled);
            }
        }

        public void Warp()
        {
            if (_isNoClipEnabled)
            {
                _utilityService.DisableNoClip();
                IsNoClipEnabled = false;
            }

            _ = Task.Run(() => _utilityService.Warp(_warpLocations[SelectedLocation.Key]));
        }

        public void DisableButtons()
        {
            IsNoClipEnabled = false;
            IsDisableEventsEnabled = false;
            AreButtonsEnabled = false;
        }

        public void TryEnableActiveOptions()
        {
            if (IsHitboxEnabled)
                _utilityService.EnableHitboxView();
            if (IsSoundViewEnabled)
                _utilityService.EnableSoundView();
            if (IsDrawEventEnabled)
                _utilityService.EnableDrawEvent();
            if (IsTargetingViewEnabled)
                _utilityService.EnableTargetingView();
            if (IsFilterRemoveEnabled)
                _utilityService.ToggleFilter(IsFilterRemoveEnabled);
            if (IsDeathCamEnabled)
                _utilityService.ToggleDeathCam(IsDeathCamEnabled);
            AreButtonsEnabled = true;
        }

        public void ResetAttached()
        {
            IsNoClipEnabled = false;
            _areAttachedOptionsRestored = false;
            _utilityService.ResetBools();
        }

        public void TryRestoreAttachedFeatures()
        {
            if (_areAttachedOptionsRestored) return;
            if (IsDrawEnabled)
            {
                if (!_utilityService.EnableDraw()) return;
            }

            _areAttachedOptionsRestored = true;
        }

        public void ShowLevelUpMenu() => _utilityService.ShowMenu(MenuMan.MenuManData.LevelUpMenu);
        
        public void ShowAttunementMenu() => _utilityService.ShowMenu(MenuMan.MenuManData.AttunementMenu);
        
        public void UnlockBonfires() => _utilityService.UnlockBonfireWarps();

        public void UnlockKalameet() => _utilityService.SetMultipleEvents(GameIds.EventFlags.UnlockKalameet);

        public void ShowUpgradeMenu(bool isWeapon) => _utilityService.ShowUpgradeMenu(isWeapon);

        public void RingGargBell() => _utilityService.RingGargBell();

        public void RingQuelaggBell() => _utilityService.RingQuelaagBell();
        public void OpenSens() => _ = _utilityService.OpenSensGate(GameIds.EventFlags.Sens);
        public void PlaceLordVessel() => _ = _utilityService.PlaceLordVessel();
        public void OpenShop(ulong[] shopParams) => _utilityService.OpenRegularShop(shopParams);

        public void NewLondoNoWater() => _utilityService.SetEvent(GameIds.EventFlags.NewLondoWater);

        public void LaurentiusToFirelink() => _utilityService.SetEvent(GameIds.EventFlags.LaurentiusToFirelink);

        public void LoganToFirelink() => _utilityService.SetMultipleEvents(GameIds.EventFlags.LoganToFirelink);

        public void GriggsToFirelink() => _utilityService.SetMultipleEvents(GameIds.EventFlags.GriggsToFirelink);

        public void OpenFeedMenu()=> _utilityService.ShowMenu(MenuMan.MenuManData.Feed);
        
        public void OpenWarpMenu()=> _utilityService.ShowMenu(MenuMan.MenuManData.Warp);

        public void OpenBottomlessBox() => _utilityService.ShowMenu(MenuMan.MenuManData.BottomlessBox);
    }
}