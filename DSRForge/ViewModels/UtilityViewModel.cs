using System.Collections.Generic;
using System.Linq;
using DSRForge.Models;
using DSRForge.Services;
using DSRForge.Utilities;

namespace DSRForge.ViewModels
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
        
        private bool _savedHitboxEnabled;
        private bool _savedSoundViewEnabled;
        private bool _savedDrawEventEnabled;
        private bool _savedTargetingViewEnabled;
        private bool _savedFilterRemoval;
        
        private bool _areOptionsEnabled;

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


        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
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
                _utilityService.Warp(_warpLocations[SelectedLocation.Key]);
            }
            else
            {
                _utilityService.Warp(_warpLocations[SelectedLocation.Key]);
            }
        }
        
        public void SaveAndDisableOptions()
        {
            _savedHitboxEnabled = IsHitboxEnabled;
            _savedSoundViewEnabled = IsSoundViewEnabled;
            _savedDrawEventEnabled = IsDrawEventEnabled;
            _savedTargetingViewEnabled = IsTargetingViewEnabled;
            _savedFilterRemoval = IsFilterRemoveEnabled;
            
            AreOptionsEnabled = false;

            IsNoClipEnabled = false;
            IsHitboxEnabled = false;
            IsSoundViewEnabled = false;
            IsDrawEventEnabled = false;
            IsTargetingViewEnabled = false;
            IsFilterRemoveEnabled = false;


        }

        public void RestoreOptions()
        {
            IsHitboxEnabled = _savedHitboxEnabled;
            IsSoundViewEnabled = _savedSoundViewEnabled;
            IsDrawEventEnabled = _savedDrawEventEnabled;
            IsTargetingViewEnabled = _savedTargetingViewEnabled;
            IsFilterRemoveEnabled = _savedFilterRemoval;
            
            AreOptionsEnabled = true;
        }
    }
}
