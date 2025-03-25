using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SilkySouls.memory;
using SilkySouls.Models;
using SilkySouls.Services;
using SilkySouls.Utilities;

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
        private bool _isFilterRemoveEnabled;
        
        private bool _areButtonsEnabled;
        private bool _areAttachedOptionsEnabled;
        private bool _areAttachedOptionsRestored;

        private bool _wasNoDeathEnabled;
        
        private readonly UtilityService _utilityService;
        private readonly PlayerService _playerService;
        private readonly HotkeyManager _hotkeyManager;
        
        private readonly Dictionary<string, Location> _warpLocations;
        private KeyValuePair<string, string> _selectedLocation;
        
        public UtilityViewModel(UtilityService utilityService, PlayerService playerService, HotkeyManager hotkeyManager)
        {
            _utilityService = utilityService;
            _playerService = playerService;
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
                    if (_playerService.IsNoDeathOn()) _wasNoDeathEnabled = true;
                    else _playerService.ToggleNoDeath(1);
                }
                else
                {
                    _utilityService.DisableNoClip();
                    if (_wasNoDeathEnabled) _wasNoDeathEnabled = false;
                    else _playerService.ToggleNoDeath(0);
                }
            }
        }
        
        public bool IsFilterRemoveEnabled
        {
            get => _isFilterRemoveEnabled;
            set
            {
                if (!SetProperty(ref _isFilterRemoveEnabled, value)) return;
                _utilityService.ToggleFilter(_isFilterRemoveEnabled ? 1 : 0);
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
                _utilityService.ToggleFilter(1);
            
            AreButtonsEnabled = true;
        }
        
        public void ResetAttached()
        {
            IsNoClipEnabled = false;
            _areAttachedOptionsRestored = false;
            _utilityService.ResetHook();
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

        public void ShowLevelUpMenu()
        {
            _utilityService.ShowMenu(Offsets.MenuMan.MenuManData.LevelUpMenu);
        }

        public void ShowAttunementMenu()
        {
            _utilityService.ShowMenu(Offsets.MenuMan.MenuManData.AttunementMenu);
        }

        

        public void UnlockBonfires()
        {
            _utilityService.UnlockBonfireWarps();
        }

        public void UnlockKalameet()
        {
            _utilityService.UnlockKalameet();
        }

        public void ShowUpgradeMenu(bool isWeapon)
        {
            _utilityService.ShowUpgradeMenu(isWeapon);
        }
    }
}
