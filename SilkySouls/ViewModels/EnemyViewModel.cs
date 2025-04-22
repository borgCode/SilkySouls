using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using SilkySouls.Services;
using SilkySouls.Utilities;
using SilkySouls.Views;

namespace SilkySouls.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        private bool _areOptionsEnabled;

        private bool _isTargetOptionsEnabled;
        private bool _isValidTarget;
        private readonly DispatcherTimer _targetOptionsTimer;
        
        
        private ResistancesWindow _resistancesWindowWindow;
        private bool _isResistancesWindowOpen;

        private int _targetCurrentHealth;
        private int _targetMaxHealth;
        private ulong _currentTargetId;
        private float _targetSpeed;
        private bool _isFreezeHealthEnabled;
        private bool _isDisableTargetAiEnabled;
        private bool _isRepeatActEnabled;
        private ObservableCollection<string> _repeatActOptions;
        private string _selectedRepeatActOption;
   
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
        private bool _is4KingsTimerStopped;

        private string _currentlyRepeatingAct = "None";
        
        private readonly EnemyService _enemyService;
        private readonly HotkeyManager _hotkeyManager;

        public EnemyViewModel(EnemyService enemyService, HotkeyManager hotkeyManager)
        {
            _enemyService = enemyService;
            _hotkeyManager = hotkeyManager;

            RegisterHotkeys();

            _repeatActOptions = new ObservableCollection<string> { "None" };
            SelectedRepeatActOption = "None";
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
            if (!IsTargetValid())
            {
                IsValidTarget = false;
                return;
            }

            IsValidTarget = true;
            TargetCurrentHealth = _enemyService.GetTargetHp();
            TargetMaxHealth = _enemyService.GetTargetMaxHp();

            ulong targetId = _enemyService.GetTargetId();

            if (targetId != _currentTargetId)
            {
                IsDisableTargetAiEnabled = _enemyService.IsTargetAiDisabled();
                IsRepeatActEnabled = IsCurrentTargetRepeating();
                IsFreezeHealthEnabled = _enemyService.IsTargetNoDamageEnabled();
                _currentTargetId = targetId;
                TargetMaxPoise = _enemyService.GetTargetMaxPoise();
                SetResistances();
                TargetMaxBleed = IsBleedImmune ? 0 : _enemyService.GetTargetMaxBleed();
                TargetMaxPoison = IsPoisonImmune ? 0 : _enemyService.GetTargetMaxPoison();
                TargetMaxToxic = IsToxicImmune ? 0 : _enemyService.GetTargetMaxToxic();
                if (!IsResistancesWindowOpen || _resistancesWindowWindow == null) return;
                _resistancesWindowWindow.DataContext = null;
                _resistancesWindowWindow.DataContext = this;
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

            if (health > maxHealth * 1.5) return false;

            var position = _enemyService.GetTargetPos();
            
            if (float.IsNaN(position[0]) || float.IsNaN(position[1]) || float.IsNaN(position[2]))
                return false;
            
            if (Math.Abs(position[0]) > 10000 || Math.Abs(position[1]) > 10000 || Math.Abs(position[2]) > 10000)
                return false;
            
            return true;
        }
        
        public bool IsResistancesWindowOpen
        {
            get => _isResistancesWindowOpen;
            set
            {
                if (!SetProperty(ref _isResistancesWindowOpen, value)) return;
                if (value)
                    OpenResistancesWindow();
                else
                    CloseResistancesWindow();
            }
        }
        private void OpenResistancesWindow()
        {
            if (_resistancesWindowWindow != null && _resistancesWindowWindow.IsVisible) return;
            _resistancesWindowWindow = new ResistancesWindow
            {
                DataContext = this
            };
            _resistancesWindowWindow.Closed += (s, e) => _isResistancesWindowOpen = false;
            _resistancesWindowWindow.Show();
        }
        
        private void CloseResistancesWindow()
        {
            if (_resistancesWindowWindow == null || !_resistancesWindowWindow.IsVisible) return;
            _resistancesWindowWindow.Close();
            _resistancesWindowWindow = null;
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
            if (!IsResistancesWindowOpen || _resistancesWindowWindow == null) return;
            _resistancesWindowWindow.DataContext = null;
            _resistancesWindowWindow.DataContext = this;
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
        
        public bool IsValidTarget
        {
            get => _isValidTarget;
            set => SetProperty(ref _isValidTarget, value);
        }

        public bool IsTargetOptionsEnabled
        {
            get => _isTargetOptionsEnabled;
            set
            {
                if (!SetProperty(ref _isTargetOptionsEnabled, value)) return;
                if (value)
                {
                    _enemyService.InstallTargetHook();
                    _targetOptionsTimer.Start();
                    ShowAllResistances = true;
                }
                else
                {
                    _targetOptionsTimer.Stop();
                    IsRepeatActEnabled = false;
                    ShowAllResistances = false;
                    IsResistancesWindowOpen = false;
                    IsFreezeHealthEnabled = false;
                    _enemyService.UninstallTargetHook();
                    ShowPoise = false;
                    ShowBleed = false;
                    ShowPoison = false;
                    ShowToxic = false;
                }
            }
        }
        
        public bool ShowAllResistances
        {
            get => _showAllResistances;
            set
            {
                if (SetProperty(ref _showAllResistances, value))
                {
                    UpdateResistancesDisplay();
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
                _enemyService.ToggleTargetNoDamage(_isFreezeHealthEnabled);
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
        
        public ObservableCollection<string> RepeatActOptions
        {
            get => _repeatActOptions;
            set => SetProperty(ref _repeatActOptions, value);
        }
        
        
        public string SelectedRepeatActOption
        {
            get => _selectedRepeatActOption;
            set
            {
                SetProperty(ref _selectedRepeatActOption, value);
                int index = RepeatActOptions.IndexOf(value);
                int maxAct = RepeatActOptions.Count - 1;
                _enemyService.RepeatAct(index, maxAct);
                if (value == null) return;
                _currentlyRepeatingAct = value;
            } 
        }
        
        public bool IsRepeatActEnabled
        {
            get => _isRepeatActEnabled;
            set
            {
                if (!SetProperty(ref _isRepeatActEnabled, value)) return;
                if (_isRepeatActEnabled)
                {
             
                    for (int i = RepeatActOptions.Count - 1; i >= 0; i--)
                    {
                        if (RepeatActOptions[i] != "None")
                            RepeatActOptions.RemoveAt(i);
                    }

                    int[] acts = _enemyService.GetActs();
                    foreach (int act in acts)
                    {
                        string actLabel = $"Act {act}";
                        if (!RepeatActOptions.Contains(actLabel))
                            RepeatActOptions.Add(actLabel);
                    }
                    
                    if (!IsCurrentTargetRepeating())
                    {
                        _enemyService.DisableRepeatAct();
                        _currentlyRepeatingAct = "None";
                    }
                        
                    if (!RepeatActOptions.Contains(_currentlyRepeatingAct))
                    {
                        _currentlyRepeatingAct = "None";
                    }
                    SelectedRepeatActOption = _currentlyRepeatingAct;
                }
                else
                {
                    if (!IsCurrentTargetRepeating()) return;
                    _enemyService.DisableRepeatAct();
                    _currentlyRepeatingAct = "None";
                }
            }
        }

        private bool IsCurrentTargetRepeating()
        {
            var currentRepeatEnemyId = _enemyService.GetCurrentRepeatEnemyId();
            var lockedTargetId = _enemyService.GetEnemyBattleId();
            return currentRepeatEnemyId == lockedTargetId;
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
        
        public bool Is4KingsTimerStopped
        {
            get => _is4KingsTimerStopped;
            set
            {
                if (SetProperty(ref _is4KingsTimerStopped, value))
                {
                    _enemyService.Toggle4KingsTimer(_is4KingsTimerStopped);
                }
            }
        }

        public void DisableButtons()
        {
            _targetOptionsTimer.Stop();
            IsFreezeHealthEnabled = false;
            IsRepeatActEnabled = false;
            _enemyService.DisableRepeatAct();
            _currentlyRepeatingAct = "None";
            AreOptionsEnabled = false;
        }

        public void TryEnableActiveOptions()
        {
            if (IsTargetOptionsEnabled)
            {
                _enemyService.InstallTargetHook();
                _targetOptionsTimer.Start();
            }
            if (IsDisableAiEnabled)
                _enemyService.ToggleAi(1);
            if (IsAllNoDamageEnabled)
                _enemyService.ToggleAllNoDamage(1);
            if (IsAllNoDeathEnabled)
                _enemyService.ToggleAllNoDeath(1);
            if (IsTargetOptionsEnabled)
                _targetOptionsTimer.Start();
            if (Is4KingsTimerStopped)
                _enemyService.Toggle4KingsTimer(true);
            AreOptionsEnabled = true;
        }
    }
}