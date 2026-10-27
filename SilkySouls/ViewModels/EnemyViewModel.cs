using System.Windows.Input;
using SilkySouls.Core;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.memory;
using SilkySouls.Utilities;
using static SilkySouls.GameIds.Emevd;

namespace SilkySouls.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        private readonly IEnemyService _enemyService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IEmevdService _emevdService;
        private readonly IPlayerHitBehaviorService _playerHitBehaviorService;
        private readonly IGameTickService _gameTickService;

        public const int FourKingsHitEventEntityId = 1603300;
        public const int FourKingsGeneratorId = 1603000;

        public EnemyViewModel(IEnemyService enemyService, HotkeyManager hotkeyManager,
            IStateService stateService,
            IEmevdService emevdService,
            IPlayerHitBehaviorService playerHitBehaviorService,
            IGameTickService gameTickService)
        {
            _enemyService = enemyService;
            _hotkeyManager = hotkeyManager;
            _emevdService = emevdService;
            _playerHitBehaviorService = playerHitBehaviorService;
            _gameTickService = gameTickService;

            stateService.Subscribe(State.Loaded, OnLoaded);
            stateService.Subscribe(State.FadedIn, OnFadedIn);
            stateService.Subscribe(State.NotLoaded, OnNotLoaded);
            
            RegisterHotkeys();
            
            TestCommand = new DelegateCommand(Test);
        }

        
        #region Commands
        
        private void Test()
        {
         
        }

        public ICommand  TestCommand { get; set; }
        
        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private bool _isDisableAiEnabled;

        public bool IsDisableAiEnabled
        {
            get => _isDisableAiEnabled;
            set
            {
                if (SetProperty(ref _isDisableAiEnabled, value))
                {
                    _enemyService.ToggleEnemiesDebugFlag(Offsets.DebugFlags.DisableAi, _isDisableAiEnabled);
                }
            }
        }

        private bool _isAllNoDamageEnabled;

        public bool IsAllNoDamageEnabled
        {
            get => _isAllNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDamageEnabled, value))
                {
                    if (!AreOptionsEnabled) return;
                    _enemyService.ToggleEnemiesDebugFlag(Offsets.DebugFlags.AllNoDamage, _isAllNoDamageEnabled);
                }
            }
        }

        private bool _isAllNoDeathEnabled;

        public bool IsAllNoDeathEnabled
        {
            get => _isAllNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDeathEnabled, value))
                {
                    _enemyService.ToggleEnemiesDebugFlag(Offsets.DebugFlags.AllNoDeath, _isAllNoDeathEnabled);
                }
            }
        }

        private bool _is4KingsTimerStopped;

        public bool Is4KingsTimerStopped
        {
            get => _is4KingsTimerStopped;
            set
            {
                if (!SetProperty(ref _is4KingsTimerStopped, value)) return;
                _enemyService.DisableFourKingsGenerator(_is4KingsTimerStopped);
                SetGeneratorStateIfInArena(_is4KingsTimerStopped);
            }
        }

        private bool _isNegateEnemyHitsEnabled;

        public bool IsNegateEnemyHitsEnabled
        {
            get => _isNegateEnemyHitsEnabled;
            set
            {
                if (!SetProperty(ref _isNegateEnemyHitsEnabled, value)) return;
                _playerHitBehaviorService.ToggleNegateEnemyHits(_isNegateEnemyHitsEnabled);
            }
        }

        private bool _isHealEnemiesOnPlayerHitEnabled;

        public bool IsHealEnemiesOnPlayerHitEnabled
        {
            get => _isHealEnemiesOnPlayerHitEnabled;
            set
            {
                if (!SetProperty(ref _isHealEnemiesOnPlayerHitEnabled, value)) return;
                _playerHitBehaviorService.ToggleHealEnemiesOnHit(_isHealEnemiesOnPlayerHitEnabled);
                if (_isHealEnemiesOnPlayerHitEnabled)
                    _gameTickService.Subscribe(HealEnemiesOnHitTick);
                else
                    _gameTickService.Unsubscribe(HealEnemiesOnHitTick);
            }
        }
        
        #endregion

        #region Private Methods
        

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.DisableAi, () => { IsDisableAiEnabled = !IsDisableAiEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoDeath, () => { IsAllNoDeathEnabled = !IsAllNoDeathEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoDamage, () => { IsAllNoDamageEnabled = !IsAllNoDamageEnabled; });
        }

        private void OnNotLoaded()
        {
            AreOptionsEnabled = false;
            _enemyService.ToggleEnemiesDebugFlag(Offsets.DebugFlags.AllNoDamage, false);
        }
        
        private void OnFadedIn()
        {
            if (IsAllNoDamageEnabled) _enemyService.ToggleEnemiesDebugFlag(Offsets.DebugFlags.AllNoDamage, true);
        }

        private void OnLoaded()
        {
            AreOptionsEnabled = true;
            if (IsNegateEnemyHitsEnabled) _playerHitBehaviorService.ToggleNegateEnemyHits(true);
            if (IsHealEnemiesOnPlayerHitEnabled)
            {
                _playerHitBehaviorService.ToggleHealEnemiesOnHit(true);
                _gameTickService.Subscribe(HealEnemiesOnHitTick);
            }
            if (IsDisableAiEnabled) _enemyService.ToggleEnemiesDebugFlag(Offsets.DebugFlags.DisableAi, true);
            if (IsAllNoDeathEnabled) _enemyService.ToggleEnemiesDebugFlag(Offsets.DebugFlags.AllNoDeath, true);
            if (Is4KingsTimerStopped) _enemyService.DisableFourKingsGenerator(true);
        }
        
        private void HealEnemiesOnHitTick()
        {
            if (_playerHitBehaviorService.ConsumeEnemyHit())
                _playerHitBehaviorService.HealEnemies();
        }

        private void SetGeneratorStateIfInArena(bool is4KingsTimerStopped)
        {
            if (!AreOptionsEnabled) return;
            var isPlayerOnHit = _emevdService.ExecuteEmevdCommand(EmevdCommands.IsPlayerStandingOnHit(FourKingsHitEventEntityId));
            if (!isPlayerOnHit.HasValue || !isPlayerOnHit.Value) return;
            _emevdService.ExecuteEmevdCommand(EmevdCommands.SetGeneratorState(FourKingsGeneratorId, !is4KingsTimerStopped));
        }

        #endregion
    }
}
