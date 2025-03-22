using System;
using System.Windows.Threading;
using SilkySouls.Services;
using SilkySouls.Utilities;

namespace SilkySouls.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        private bool _areOptionsEnabled;

        private bool _isTargetOptionsEnabled;
        private readonly DispatcherTimer _targetOptionsTimer;

        private int _targetCurrentHealth;
        private int _targetMaxHealth;
        private ulong _currentTargetId;
        private float _targetSpeed;
        private bool _isRepeatActionEnabled;
        private bool _isFreezeHealthEnabled;
        private bool _isDisableTargetAiEnabled;
        private bool _shouldEnableRepeatAction;

        private float _targetCurrentPoise;
        private float _targetMaxPoise;
        private float _targetPoiseTimer;
        private bool _showPoise;

        private int _targetCurrentBleed;
        private int _targetMaxBleed;
        private bool _showBleed;
        private bool _isBleedImmune;

        private int _targetCurrentPoison;
        private int _targetMaxPoison;
        private bool _showPoison;
        private bool _isPoisonImmune;

        private int _targetCurrentToxic;
        private int _targetMaxToxic;
        private bool _showToxic;
        private bool _isToxicImmune;

        private bool _showAllResistances;
        private bool _isDisableAiEnabled;
        private bool _isAllNoDamageEnabled;
        private bool _isAllNoDeathEnabled;

        private int _frozenHealth;

        private readonly EnemyService _enemyService;
        private readonly HotkeyManager _hotkeyManager;

        public EnemyViewModel(EnemyService enemyService, HotkeyManager hotkeyManager)
        {
            _enemyService = enemyService;
            _hotkeyManager = hotkeyManager;

            RegisterHotkeys();

            _targetOptionsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(64)
            };
            _targetOptionsTimer.Tick += TargetOptionsTimerTick;
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("EnableTargetOptions",
                () => { IsTargetOptionsEnabled = !IsTargetOptionsEnabled; });
            _hotkeyManager.RegisterAction("ShowAllResistances", () =>
            {
                _showAllResistances = !_showAllResistances;
                UpdateResistancesDisplay();
            });
            _hotkeyManager.RegisterAction("RepeatAction", () => { IsRepeatActionEnabled = !IsRepeatActionEnabled; });
            _hotkeyManager.RegisterAction("FreezeHp", () => { IsFreezeHealthEnabled = !IsFreezeHealthEnabled; });
            _hotkeyManager.RegisterAction("DisableTargetAi",
                () => { IsDisableTargetAiEnabled = !IsDisableTargetAiEnabled; });
            _hotkeyManager.RegisterAction("IncreaseTargetSpeed", () => SetSpeed(Math.Min(5, TargetSpeed + 0.25f)));
            _hotkeyManager.RegisterAction("DecreaseTargetSpeed", () => SetSpeed(Math.Max(0, TargetSpeed - 0.25f)));
            _hotkeyManager.RegisterAction("DisableAi", () => { IsDisableAiEnabled = !IsDisableAiEnabled; });
            _hotkeyManager.RegisterAction("AllNoDeath", () => { IsAllNoDeathEnabled = !IsAllNoDeathEnabled; });
            _hotkeyManager.RegisterAction("AllNoDamage", () => { IsAllNoDamageEnabled = !IsAllNoDamageEnabled; });
        }

        


        private void TargetOptionsTimerTick(object sender, EventArgs e)
        {
            if (!IsTargetValid()) return;
            if (_shouldEnableRepeatAction && IsRepeatActionEnabled)
            {
                _enemyService.EnableRepeatAction();
                _shouldEnableRepeatAction = false;
            }

            TargetCurrentHealth = _enemyService.GetTargetHp();
            TargetMaxHealth = _enemyService.GetTargetMaxHp();

            ulong targetId = _enemyService.GetTargetId();

            if (targetId != _currentTargetId)
            {
                ResetTargetOptions();
                _currentTargetId = targetId;
                TargetMaxPoise = _enemyService.GetTargetMaxPoise();
                SetResistances();
                TargetMaxBleed = IsBleedImmune ? 0 : _enemyService.GetTargetMaxBleed();
                TargetMaxPoison = IsPoisonImmune ? 0 : _enemyService.GetTargetMaxPoison();
                TargetMaxToxic = IsToxicImmune ? 0 : _enemyService.GetTargetMaxToxic();
            }

            if (IsFreezeHealthEnabled)
            {
                _enemyService.SetTargetHp(_frozenHealth);
            }

            TargetSpeed = _enemyService.GetTargetSpeed();
            TargetCurrentPoise = _enemyService.GetTargetPoise();
            TargetPoiseTimer = _enemyService.GetTargetPoiseTimer();
            TargetCurrentBleed = IsBleedImmune ? 0 : _enemyService.GetTargetBleed();
            TargetCurrentPoison = IsPoisonImmune ? 0 : _enemyService.GetTargetPoison();
            TargetCurrentToxic = IsToxicImmune ? 0 : _enemyService.GetTargetToxic();
        }

        private bool IsTargetValid()
        {
            ulong targetId = _enemyService.GetTargetId();
            if (targetId == 0)
                return false;

            float health = _enemyService.GetTargetHp();
            float maxHealth = _enemyService.GetTargetMaxHp();
            if (health < 0 || maxHealth <= 0 || health > 10000000 || maxHealth > 10000000)
                return false;

            if (health > maxHealth * 1.5)
                return false;


            return true;
        }
        
        private void UpdateResistancesDisplay()
        {
            if (!IsTargetOptionsEnabled) return;
            if (_showAllResistances)
            {
                ShowBleed = true;
                ShowPoise = true;
                ShowPoison = true;
                ShowToxic = true;
            }
            else
            {
                ShowBleed = false;
                ShowPoise = false;
                ShowPoison = false;
                ShowToxic = false;
            }
        }
        
        private void ResetTargetOptions()
        {
            IsFreezeHealthEnabled = false;
            IsRepeatActionEnabled = false;
            IsDisableTargetAiEnabled = _enemyService.IsTargetAiDisabled();
        }

        private void SetResistances()
        {
            IsToxicImmune = false;
            IsPoisonImmune = false;
            IsBleedImmune = false;
            int immunity = _enemyService.GetImmunitySpEffect();
            switch (immunity)
            {
                case 90000:
                case 90001:
                {
                    IsToxicImmune = true;
                    IsPoisonImmune = true;
                    IsBleedImmune = true;
                    break;
                }
                case 90010:
                case 90101:
                {
                    IsPoisonImmune = true;
                    IsBleedImmune = true;
                    break;
                }
                case 90011:
                case 90100:
                {
                    IsToxicImmune = true;
                    IsPoisonImmune = true;
                    break;
                }
                case 91001:
                case 91000:
                {
                    IsBleedImmune = true;
                    IsToxicImmune = true;
                    break;
                }

                case 91011:
                case 91010:
                {
                    IsBleedImmune = true;
                    break;
                }
                case 91100:
                case 91101:
                {
                    IsToxicImmune = true;
                    break;
                }
                case 90111:
                case 90110:
                {
                    IsPoisonImmune = true;
                    break;
                }
            }
        }

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        public bool IsTargetOptionsEnabled
        {
            get => _isTargetOptionsEnabled;
            set
            {
                if (!SetProperty(ref _isTargetOptionsEnabled, value)) return;
                if (value)
                {
                    _targetOptionsTimer.Start();
                }
                else
                {
                    _targetOptionsTimer.Stop();
                    ShowPoise = false;
                    ShowBleed = false;
                    ShowPoison = false;
                    ShowToxic = false;
                }
            }
        }

        public int TargetCurrentHealth
        {
            get => _targetCurrentHealth;
            set => SetProperty(ref _targetCurrentHealth, value);
        }

        public int TargetMaxHealth
        {
            get => _targetMaxHealth;
            set => SetProperty(ref _targetMaxHealth, value);
        }

        public void SetTargetHealth(int value)
        {
            int health = TargetMaxHealth * value / 100;
            if (IsFreezeHealthEnabled)
            {
                _frozenHealth = health;
            }

            _enemyService.SetTargetHp(health);
        }

        public float TargetCurrentPoise
        {
            get => _targetCurrentPoise;
            set => SetProperty(ref _targetCurrentPoise, value);
        }

        public float TargetMaxPoise
        {
            get => _targetMaxPoise;
            set => SetProperty(ref _targetMaxPoise, value);
        }

        public float TargetPoiseTimer
        {
            get => _targetPoiseTimer;
            set => SetProperty(ref _targetPoiseTimer, value);
        }

        public bool ShowPoise
        {
            get => _showPoise;
            set => SetProperty(ref _showPoise, value);
        }


        public int TargetCurrentBleed
        {
            get => _targetCurrentBleed;
            set => SetProperty(ref _targetCurrentBleed, value);
        }

        public int TargetMaxBleed
        {
            get => _targetMaxBleed;
            set => SetProperty(ref _targetMaxBleed, value);
        }

        public bool ShowBleed
        {
            get => _showBleed;
            set => SetProperty(ref _showBleed, value);
        }

        public bool IsBleedImmune
        {
            get => _isBleedImmune;
            set => SetProperty(ref _isBleedImmune, value);
        }

        public int TargetCurrentPoison
        {
            get => _targetCurrentPoison;
            set => SetProperty(ref _targetCurrentPoison, value);
        }

        public int TargetMaxPoison
        {
            get => _targetMaxPoison;
            set => SetProperty(ref _targetMaxPoison, value);
        }

        public bool ShowPoison
        {
            get => _showPoison;
            set => SetProperty(ref _showPoison, value);
        }

        public bool IsPoisonImmune
        {
            get => _isPoisonImmune;
            set => SetProperty(ref _isPoisonImmune, value);
        }

        public int TargetCurrentToxic
        {
            get => _targetCurrentToxic;
            set => SetProperty(ref _targetCurrentToxic, value);
        }

        public int TargetMaxToxic
        {
            get => _targetMaxToxic;
            set => SetProperty(ref _targetMaxToxic, value);
        }

        public bool ShowToxic
        {
            get => _showToxic;
            set => SetProperty(ref _showToxic, value);
        }

        public bool IsToxicImmune
        {
            get => _isToxicImmune;
            set => SetProperty(ref _isToxicImmune, value);
        }

        public float TargetSpeed
        {
            get => _targetSpeed;
            set
            {
                if (SetProperty(ref _targetSpeed, value))
                {
                    _enemyService.SetTargetSpeed(value);
                }
            }
        }

        public void SetSpeed(float value)
        {
            TargetSpeed = value;
        }

        public bool IsFreezeHealthEnabled
        {
            get => _isFreezeHealthEnabled;
            set
            {
                SetProperty(ref _isFreezeHealthEnabled, value);
                _frozenHealth = _targetCurrentHealth;
            }
        }


        public bool IsRepeatActionEnabled
        {
            get => _isRepeatActionEnabled;
            set
            {
                if (!SetProperty(ref _isRepeatActionEnabled, value)) return;
                if (value && IsTargetValid()) _enemyService.EnableRepeatAction();
                else if (value) _shouldEnableRepeatAction = true;
                else
                {
                    _enemyService.DisableRepeatAction();
                }
            }
        }

        public bool IsDisableTargetAiEnabled
        {
            get => _isDisableTargetAiEnabled;
            set
            {
                if (SetProperty(ref _isDisableTargetAiEnabled, value))
                {
                    _enemyService.ToggleTargetAi(_isDisableTargetAiEnabled);
                }
            }
        }

        public bool IsDisableAiEnabled
        {
            get => _isDisableAiEnabled;
            set
            {
                if (SetProperty(ref _isDisableAiEnabled, value))
                {
                    _enemyService.ToggleAi(_isDisableAiEnabled ? 1 : 0);
                }
            }
        }

        public bool IsAllNoDamageEnabled
        {
            get => _isAllNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDamageEnabled, value))
                {
                    _enemyService.ToggleAllNoDamage(_isAllNoDamageEnabled ? 1 : 0);
                }
            }
        }


        public bool IsAllNoDeathEnabled
        {
            get => _isAllNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDeathEnabled, value))
                {
                    _enemyService.ToggleAllNoDeath(_isAllNoDeathEnabled ? 1 : 0);
                }
            }
        }

        public void DisableButtons()
        {
            _targetOptionsTimer.Stop();
            IsFreezeHealthEnabled = false;
            IsRepeatActionEnabled = false;
            AreOptionsEnabled = false;
        }

        public void TryEnableActiveOptions()
        {
            if (IsDisableAiEnabled)
                _enemyService.ToggleAi(1);
            if (IsAllNoDamageEnabled)
                _enemyService.ToggleAllNoDamage(1);
            if (IsAllNoDeathEnabled)
                _enemyService.ToggleAllNoDeath(1);
            if (IsTargetOptionsEnabled)
                _targetOptionsTimer.Start();
            AreOptionsEnabled = true;
        }
    }
}