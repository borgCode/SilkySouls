using System.Threading.Tasks;
using System.Windows.Input;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Utilities;
using static SilkySouls.GameIds.EzState;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        private bool _wasNoDeathEnabled;
        private bool _wasNoDmgEnabled;

        private bool _areAttachedOptionsRestored;
        
        private readonly IUtilityService _utilityService;
        private readonly PlayerViewModel _playerViewModel;
        private readonly IParamService _paramService;
        private readonly IEzStateService _ezStateService;
        private readonly IDebugDrawService _debugDrawService;
        private readonly HotkeyManager _hotkeyManager;

        public const int EquipParamGoodsTableIdx = 3;
        public const int EstusParamRowIdx = 32;
        public const int LordVesselIconId = 2085;
        public const int IconIdOffset = 0x2C;

        private const float DefaultNoclipSpeedScale = 1f;

        public UtilityViewModel(IUtilityService utilityService,
            HotkeyManager hotkeyManager,
            PlayerViewModel playerViewModel, IParamService paramService, IStateService stateService,
            IEzStateService ezStateService, IDebugDrawService debugDrawService)
        {
            _utilityService = utilityService;
            _playerViewModel = playerViewModel;
            _paramService = paramService;
            _ezStateService = ezStateService;
            _debugDrawService = debugDrawService;
            _hotkeyManager = hotkeyManager;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);

            ShowLevelUpMenuCommand = new DelegateCommand(ShowLevelUpMenu);
            ShowAttunementMenuCommand = new DelegateCommand(ShowAttunementMenu);
            UpgradeWeaponCommand = new DelegateCommand(UpgradeWeapon);
            UpgradeArmorCommand = new DelegateCommand(UpgradeArmor);
            OpenFeedMenuCommand = new DelegateCommand(OpenFeedMenu);
            OpenWarpMenuCommand = new DelegateCommand(OpenWarpMenu);
            OpenBottomlessBoxCommand = new DelegateCommand(OpenBottomlessBox);
            OpenShopCommand = new DelegateCommand<int[]>(OpenShop);

            RegisterHotkeys();
        }

        #region Commands

        public ICommand ShowLevelUpMenuCommand { get; }
        public ICommand ShowAttunementMenuCommand { get; }
        public ICommand UpgradeWeaponCommand { get; }
        public ICommand UpgradeArmorCommand { get; }
        public ICommand OpenFeedMenuCommand { get; }
        public ICommand OpenWarpMenuCommand { get; }
        public ICommand OpenBottomlessBoxCommand { get; }
        public ICommand OpenShopCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }
        
        private bool _isHitboxEnabled;

        public bool IsHitboxEnabled
        {
            get => _isHitboxEnabled;
            set
            {
                if (!SetProperty(ref _isHitboxEnabled, value)) return;
                DoTemporalAliasingCheck();
                _debugDrawService.ToggleDrawHitbox(_isHitboxEnabled);
            }
        }

        private bool _isSoundViewEnabled;

        public bool IsSoundViewEnabled
        {
            get => _isSoundViewEnabled;
            set
            {
                if (!SetProperty(ref _isSoundViewEnabled, value)) return;
                _debugDrawService.ToggleDrawSoundView(_isSoundViewEnabled);
            }
        }

        private bool _isDrawEventEnabled;

        public bool IsDrawEventEnabled
        {
            get => _isDrawEventEnabled;
            set
            {
                if (!SetProperty(ref _isDrawEventEnabled, value)) return;
                _debugDrawService.ToggleDrawEvents(_isDrawEventEnabled);
            }
        }

        private bool _isNoClipEnabled;

        public bool IsNoClipEnabled
        {
            get => _isNoClipEnabled;
            set
            {
                if (!SetProperty(ref _isNoClipEnabled, value)) return;
                if (_isNoClipEnabled)
                {
                    _utilityService.WriteNoClipSpeed(NoClipSpeedScale);

                    _wasNoDeathEnabled = _playerViewModel.IsNoDeathEnabled;
                    _wasNoDmgEnabled = _playerViewModel.IsNoDamageEnabled;
                    _playerViewModel.IsNoDeathEnabled = true;
                    _playerViewModel.IsNoDamageEnabled = true;
                }
                else
                {
                    _playerViewModel.IsNoDeathEnabled = _wasNoDeathEnabled;
                    _playerViewModel.IsNoDamageEnabled = _wasNoDmgEnabled;
                }

                _utilityService.ToggleNoClip(_isNoClipEnabled);
            }
        }

        private bool _isDeathCamEnabled;

        public bool IsDeathCamEnabled
        {
            get => _isDeathCamEnabled;
            set
            {
                if (!SetProperty(ref _isDeathCamEnabled, value)) return;
                _utilityService.ToggleDeathCamera(_isDeathCamEnabled);
            }
        }

        private bool _isWarpHomePoseEnabled;

        public bool IsWarpHomePoseEnabled
        {
            get => _isWarpHomePoseEnabled;
            set
            {
                if (!SetProperty(ref _isWarpHomePoseEnabled, value)) return;
                _utilityService.ToggleWarpHomePose(_isWarpHomePoseEnabled);
            }
        }

        private bool _isFilterRemoveEnabled;

        public bool IsFilterRemoveEnabled
        {
            get => _isFilterRemoveEnabled;
            set
            {
                if (!SetProperty(ref _isFilterRemoveEnabled, value)) return;
                _utilityService.ToggleFilter(_isFilterRemoveEnabled);
            }
        }

        private bool _isGuaranteedBkhEnabled;

        public bool IsGuaranteedBkhEnabled
        {
            get => _isGuaranteedBkhEnabled;
            set
            {
                if (SetProperty(ref _isGuaranteedBkhEnabled, value))
                {
                    if (AreOptionsEnabled)
                    {
                        var estusRow = _paramService.GetParamRow(EquipParamGoodsTableIdx, 0, EstusParamRowIdx);
                        _paramService.WriteInt32(estusRow, IconIdOffset, LordVesselIconId);
                        _utilityService.SetGuaranteedBkhDrop(_isGuaranteedBkhEnabled);
                    }
                }
            }
        }

        private float _noClipSpeedScale = DefaultNoclipSpeedScale;

        public float NoClipSpeedScale
        {
            get => _noClipSpeedScale;
            set
            {
                if (SetProperty(ref _noClipSpeedScale, value))
                {
                    if (!IsNoClipEnabled) return;
                    _utilityService.WriteNoClipSpeed(_noClipSpeedScale);
                }
            }
        }

        #endregion

        #region Public Methods

        
        public void DisableNoClip()
        {
            _utilityService.ToggleNoClip(false);
            IsNoClipEnabled = false;
        }

        #endregion

        #region Private Methods

        private void ShowLevelUpMenu() => _utilityService.ShowMenu(MenuMan.IsLevelUpMenuOpen, 1);
        private void ShowAttunementMenu() => _ezStateService.ExecuteTalkCommand(TalkCommands.OpenAttunement);
        private void UpgradeWeapon() => _ezStateService.ExecuteTalkCommand(TalkCommands.OpenEnhanceWeapon);
        private void UpgradeArmor() => _ezStateService.ExecuteTalkCommand(TalkCommands.OpenEnhanceArmor);
        private void OpenFeedMenu() => _utilityService.ShowMenu(MenuMan.Feed, 1);
        private void OpenWarpMenu() => _utilityService.ShowMenu(MenuMan.Warp, 2);
        private void OpenBottomlessBox() => _utilityService.ShowMenu(MenuMan.BottomlessBox, 1);

        private void OpenShop(int[] shopParams) =>
            _ezStateService.ExecuteTalkCommand(TalkCommands.OpenRegularShop(shopParams[0], shopParams[1]));

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.NoClip, () => { IsNoClipEnabled = !IsNoClipEnabled; });
        }

        private void OnNotLoaded()
        {
            IsNoClipEnabled = false;
            AreOptionsEnabled = false;
        }

        private void OnLoaded()
        {
            if (IsHitboxEnabled)
                _debugDrawService.ToggleDrawHitbox(true);
            if (IsSoundViewEnabled)
                _debugDrawService.ToggleDrawSoundView(true);
            if (IsDrawEventEnabled)
                _debugDrawService.ToggleDrawEvents(true);
            if (IsFilterRemoveEnabled)
                _utilityService.ToggleFilter(IsFilterRemoveEnabled);
            if (IsDeathCamEnabled)
                _utilityService.ToggleDeathCamera(IsDeathCamEnabled);
            if (IsWarpHomePoseEnabled)
                _utilityService.ToggleWarpHomePose(IsWarpHomePoseEnabled);
            if (IsGuaranteedBkhEnabled)
            {
                _utilityService.SetGuaranteedBkhDrop(true);
                _ = Task.Run(() =>
                {
                    Task.Delay(500).Wait();
                    var estusRow = _paramService.GetParamRow(EquipParamGoodsTableIdx, 0, EstusParamRowIdx);
                    _paramService.WriteInt32(estusRow, IconIdOffset, LordVesselIconId);
                });
            }
            else
            {
                _utilityService.SetGuaranteedBkhDrop(false);
            }

            AreOptionsEnabled = true;
        }

        private void DoTemporalAliasingCheck()
        {
            if (_utilityService.HasTemporalAntiAliasing())
            {
                MsgBox.Show(
                    "Temporal Anti-Aliasing (default setting) will prevent hitboxes from displaying properly.\n" +
                    "Go to PC Settings --> Quality --> Anti-Aliasing and set it to anything else to properly view them",
                    "Temporal Anti-Aliasing");
            }
        }

        #endregion
    }
}