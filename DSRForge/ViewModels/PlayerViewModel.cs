using System;
using System.Windows.Threading;
using DSRForge.memory;
using DSRForge.Services;
using DSRForge.Utilities;

namespace DSRForge.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {

        private int? _currentHp;
        private int? _currentMaxHp;

        private bool _isPos1Saved;
        private bool _isPos2Saved;
        
        private bool _isNoDeathEnabled;
        private bool _isNoDamageEnabled;
        private bool _isInfiniteStaminaEnabled;
        private bool _isNoGoodsConsumeEnabled;
        private bool _isInfiniteCastsEnabled;
        private bool _isOneShotEnabled;
        private bool _isInvisibleEnabled;
        private bool _isSilentEnabled;
        private bool _isNoAmmoConsumeEnabled;
        
        private bool _areOptionsEnabled;
        
        private int? _vitality;
        private int? _attunement;
        private int? _endurance;
        private int? _strength;
        private int? _dexterity;
        private int? _resistance;
        private int? _intelligence;
        private int? _faith;
        private int? _humanity;
        private int? _souls;
        private int? _newGame;
        private float? _playerSpeed;
        
        private bool _pauseUpdates;
        private readonly PlayerService _playerService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly DispatcherTimer _timer;
        
        public PlayerViewModel(PlayerService playerService, HotkeyManager hotkeyManager)
        {
            _playerService = playerService;
            
            _hotkeyManager = hotkeyManager;
            RegisterHotkeys();

            InitStats();
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (s, e) =>
            {
                if (_pauseUpdates) return;
                CurrentHp = _playerService.GetHp();
                CurrentMaxHp = _playerService.GetMaxHp();
            };
            _timer.Start();
        }


        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("SavePos1", () => SavePos(0));
            _hotkeyManager.RegisterAction("SavePos2", () => SavePos(1));
            _hotkeyManager.RegisterAction("RestorePos1", () => RestorePos(0));
            _hotkeyManager.RegisterAction("RestorePos2", () => RestorePos(1));
            _hotkeyManager.RegisterAction("RTSR", () => SetHp(1));
            _hotkeyManager.RegisterAction("NoDeath", () => { IsNoDeathEnabled = !IsNoDeathEnabled; });
            _hotkeyManager.RegisterAction("OneShot", () => { IsOneShotEnabled = !IsOneShotEnabled; });
            _hotkeyManager.RegisterAction("RestoreSpellCasts", RestoreSpellCasts);
            _hotkeyManager.RegisterAction("IncreaseSpeed", () => SetSpeed(PlayerSpeed.HasValue ? Math.Min(10, PlayerSpeed.Value + 0.25f) : 0.25f));
            _hotkeyManager.RegisterAction("DecreaseSpeed", () =>
            {
                if (PlayerSpeed != null) SetSpeed(Math.Max(0, PlayerSpeed.Value - 0.25f));
            });

        }

        private void InitStats()
        {
            _vitality = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Vitality);
            _attunement = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Attunement);
            _endurance = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Endurance);
            _strength = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Strength);
            _dexterity = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Dexterity);
            _resistance = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Resistance);
            _intelligence = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Intelligence);
            _faith = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Faith);
            _humanity = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Humanity);
            _souls = _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Souls);
            _newGame = _playerService.GetSetNewGame(null);
            _playerSpeed = _playerService.GetSetPlayerSpeed(null);

        }

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }
        
        public int? CurrentHp
        {
            get => _currentHp;
            set => SetProperty(ref _currentHp, value);
        }
        
        public int? CurrentMaxHp
        {
            get => _currentMaxHp;
            set => SetProperty(ref _currentMaxHp, value);
        }
        
        public void SetHp(int? hp)
        {
            if (hp.HasValue)
            {
                _playerService.SetHp(hp.Value);
                CurrentHp = hp;
            }
        }

        public bool IsPos1Saved
        {
            get => _isPos1Saved;
            set => SetProperty(ref _isPos1Saved, value);
        }

        public bool IsPos2Saved
        {
            get => _isPos2Saved;
            set => SetProperty(ref _isPos2Saved, value);
        }
        
        
        public void PauseUpdates()
        {
            _pauseUpdates = true;
        }
        
        public void ResumeUpdates()
        {
            _pauseUpdates = false;
        }

        public void SavePos(int index)
        {
            if (index == 0)
            {
                IsPos1Saved = true;
            }
            else
            {
                IsPos2Saved = true;
            }
            
            _playerService.SavePos(index);
            
        }

        public void RestorePos(int index)
        {
            _playerService.RestorePos(index);
            
        }
        
        public bool IsNoDeathEnabled
        {
            get => _isNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isNoDeathEnabled, value))
                {
                    _playerService.ToggleNoDeath(_isNoDeathEnabled ? 1 : 0);
                }
            }
        }
        
        public bool IsNoDamageEnabled
        {
            get => _isNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isNoDamageEnabled, value))
                {
                    _playerService.ToggleNoDamage(_isNoDamageEnabled);
                }
            }
        }
        
        public bool IsInfiniteStaminaEnabled
        {
            get => _isInfiniteStaminaEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteStaminaEnabled, value))
                {
                    _playerService.ToggleInfiniteStamina(_isInfiniteStaminaEnabled);
                }
            }
        }
        
        public bool IsNoGoodsConsumeEnabled
        {
            get => _isNoGoodsConsumeEnabled;
            set
            {
                if (SetProperty(ref _isNoGoodsConsumeEnabled, value))
                {
                    _playerService.ToggleNoGoodsConsume(_isNoGoodsConsumeEnabled);
                }
            }
        }

        public bool IsInfiniteCastsEnabled
        {
            get => _isInfiniteCastsEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteCastsEnabled, value))
                {
                    _playerService.ToggleInfiniteCasts(_isInfiniteCastsEnabled ? 1 : 0);
                }
            }
        }

        public bool IsOneShotEnabled
        {
            get => _isOneShotEnabled;
            set
            {
                if (SetProperty(ref _isOneShotEnabled, value))
                {
                    _playerService.ToggleOneShot(_isOneShotEnabled ? 1 : 0);
                }
            }
        }

        public bool IsInvisibleEnabled
        {
            get => _isInvisibleEnabled;
            set
            {
                if (SetProperty(ref _isInvisibleEnabled, value))
                {
                    _playerService.ToggleInvisible(_isInvisibleEnabled ? 1 : 0);
                }
            }
        }

        public bool IsSilentEnabled
        {
            get => _isSilentEnabled;
            set
            {
                if (SetProperty(ref _isSilentEnabled, value))
                {
                    _playerService.ToggleSilent(_isSilentEnabled ? 1 : 0);
                }
            }
        }
        public bool IsNoAmmoConsumeEnabled
        {
            get => _isNoAmmoConsumeEnabled;
            set
            {
                if (SetProperty(ref _isNoAmmoConsumeEnabled, value))
                {
                    _playerService.ToggleNoAmmoConsume(_isNoAmmoConsumeEnabled ? 1 : 0);
                }
            }
        }
        
        public void RestoreSpellCasts()
        {
            _playerService.RestoreSpellCasts();
        }
        

        public void DisableButtons()
        {
            AreOptionsEnabled = false;
        }

        public void TryEnableActiveOptions()
        {
            if (IsNoDeathEnabled)
                _playerService.ToggleNoDeath(1);
            if (IsNoDamageEnabled)
                _playerService.ToggleNoDamage(true);
            if (IsInfiniteStaminaEnabled)
                _playerService.ToggleInfiniteStamina(true);
            if (IsNoGoodsConsumeEnabled)
                _playerService.ToggleNoGoodsConsume(true);
            if (IsInfiniteCastsEnabled)
                _playerService.ToggleInfiniteCasts(1);
            if (IsOneShotEnabled)
                _playerService.ToggleOneShot(1);
            if (IsInvisibleEnabled)
                _playerService.ToggleInvisible(1);
            if (IsSilentEnabled)
                _playerService.ToggleSilent(1);
            if (IsNoAmmoConsumeEnabled)
                _playerService.ToggleNoAmmoConsume(1);
    
            AreOptionsEnabled = true;
        }
        
        
        public int? Vitality 
        { 
            get => _vitality; 
            set 
            {
                if (SetProperty(ref _vitality, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Vitality, value);
                }
            }
        }

        public int? Attunement 
        { 
            get => _attunement; 
            set 
            {
                if (SetProperty(ref _attunement, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Attunement, value);
                }
            }
        }

        public int? Endurance 
        { 
            get => _endurance; 
            set 
            {
                if (SetProperty(ref _endurance, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Endurance, value);
                }
            }
        }

        public int? Strength 
        { 
            get => _strength; 
            set 
            {
                if (SetProperty(ref _strength, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Strength, value);
                }
            }
        }

        public int? Dexterity 
        { 
            get => _dexterity; 
            set 
            {
                if (SetProperty(ref _dexterity, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Dexterity, value);
                }
            }
        }

        public int? Resistance 
        { 
            get => _resistance; 
            set 
            {
                if (SetProperty(ref _resistance, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Resistance, value);
                }
            }
        }

        public int? Intelligence 
        { 
            get => _intelligence; 
            set 
            {
                if (SetProperty(ref _intelligence, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Intelligence, value);
                }
            }
        }

        public int? Faith 
        { 
            get => _faith; 
            set 
            {
                if (SetProperty(ref _faith, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Faith, value);
                }
            }
        }

        public int? Humanity 
        { 
            get => _humanity; 
            set 
            {
                if (SetProperty(ref _humanity, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Humanity, value);
                }
            }
        }

        public int? Souls 
        { 
            get => _souls; 
            set 
            {
                if (SetProperty(ref _souls, value))
                {
                    _playerService.GetSetPlayerStat(Offsets.PlayerGameData.Souls, value);
                }
            }
        }

        public void SetVitality(int? value)
        {
            Vitality = value;
        }

        public void SetAttunement(int? value)
        {
            Attunement = value;
        }

        public void SetEndurance(int? value)
        {
            Endurance = value;
        }

        public void SetStrength(int? value)
        {
            Strength = value;
        }

        public void SetDexterity(int? value)
        {
            Dexterity = value;
        }

        public void SetResistance(int? value)
        {
            Resistance = value;
        }

        public void SetIntelligence(int? value)
        {
            Intelligence = value;
        }

        public void SetFaith(int? value)
        {
            Faith = value;
        }
        
        public void SetHumanity(int? value)
        {
            Humanity = value;
        }
        
        public void SetSouls(int? value)
        {
            Souls = value;
        }
        
        public int? NewGame 
        { 
            get => _newGame; 
            set 
            {
                if (SetProperty(ref _newGame, value))
                {
                    _playerService.GetSetNewGame(value);
                }
            }
        }
        
        public void SetNewGame(int? value)
        {
            NewGame = value;
        }
        
        public float? PlayerSpeed 
        { 
            get => _playerSpeed; 
            set 
            {
                if (SetProperty(ref _playerSpeed, value))
                {
                    _playerService.GetSetPlayerSpeed(value);
                }
            }
        }
        
        public void SetSpeed(float? value)
        {
            PlayerSpeed = value;
        }

    }
}
