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
        private float _targetCurrentPoise;
        private float _targetMaxPoise;
        private float _targetPoiseTimer;
        private int _currentTargetId;
        private float _targetSpeed;
        
        private bool _isRepeatActionEnabled;
        private bool _isFreezeHealthEnabled;
        
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
            _hotkeyManager.RegisterAction("RepeatAction", () => { IsRepeatActionEnabled = !IsRepeatActionEnabled; });
            _hotkeyManager.RegisterAction("DisableAi", () => { IsDisableAiEnabled = !IsDisableAiEnabled; });
            _hotkeyManager.RegisterAction("FreezeHp", () => { IsFreezeHealthEnabled = !IsFreezeHealthEnabled; });
        }
        
        private void TargetOptionsTimerTick(object sender, EventArgs e)
        {
            TargetCurrentHealth = _enemyService.GetTargetHp();
            TargetMaxHealth = _enemyService.GetTargetMaxHp();
            
            int targetId = _enemyService.GetTargetId();
            if (targetId != _currentTargetId)
            {
                IsFreezeHealthEnabled = false;
                _currentTargetId = targetId;
            }
            
            if (IsFreezeHealthEnabled)
            {
                _enemyService.SetTargetHp(_frozenHealth);
            }

            TargetSpeed = _enemyService.GetTargetSpeed();
            TargetCurrentPoise = _enemyService.GetTargetPoise();
            TargetMaxPoise = _enemyService.GetTargetMaxPoise();
            TargetPoiseTimer = _enemyService.GetTargetPoiseTimer();
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
                if(value)
                {
                     _targetOptionsTimer.Start();
                }
                else
                {
                    _targetOptionsTimer.Stop();
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
                if (value) _enemyService.EnableRepeatAction();
                else _enemyService.DisableRepeatAction();
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
