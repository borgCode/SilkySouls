using System;
using System.Windows.Threading;
using SilkySouls.Models;
using SilkySouls.Services;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        private bool _isStateIncluded;
        private (float x, float y, float z) _coords;
        private float _posX;
        private float _posZ;
        private float _posY;
        private CharacterState _saveState1 = new CharacterState();
        private CharacterState _saveState2 = new CharacterState();

        private int _currentHp;
        private int _currentMaxHp;

        private bool _isPos1Saved;
        private bool _isPos2Saved;

        private bool _isNoDeathEnabled;
        private bool _isNoDamageEnabled;
        private bool _isInfiniteStaminaEnabled;
        private bool _isNoGoodsConsumeEnabled;
        private bool _isInfiniteCastsEnabled;
        private bool _isInfiniteDurabilityEnabled;
        private bool _isOneShotEnabled;
        private bool _isInvisibleEnabled;
        private bool _isSilentEnabled;
        private bool _isNoAmmoConsumeEnabled;
        private bool _isInfinitePoiseEnabled;
        private bool _isAutoSetNewGameSixEnabled;
        private bool _isNoRollEnabled;

        private bool _areOptionsEnabled;

        private int _soulLevel;
        private int _vitality;
        private int _attunement;
        private int _endurance;
        private int _strength;
        private int _dexterity;
        private int _resistance;
        private int _intelligence;
        private int _faith;
        private int _humanity;
        private int _souls;
        private int _newGame;
        private float _playerSpeed;
        private int _currentSoulLevel;

        private float _playerDesiredSpeed = -1f;
        private const float DefaultSpeed = 1f;
        private const float Epsilon = 0.0001f;

        private bool _pauseUpdates;
        private readonly PlayerService _playerService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly DispatcherTimer _timer;
        private bool _wasNoDamageEnabled;
        private bool _wasNoDeathEnabled;

        public PlayerViewModel(PlayerService playerService, HotkeyManager hotkeyManager)
        {
            _playerService = playerService;

            _hotkeyManager = hotkeyManager;
            RegisterHotkeys();

            LoadStats();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (s, e) =>
            {
                if (_pauseUpdates) return;

                CurrentHp = _playerService.GetHp();
                CurrentMaxHp = _playerService.GetMaxHp();
                Souls = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Souls);
                PlayerSpeed = _playerService.GetPlayerSpeed();
                _coords = _playerService.GetReadOnlyCoords();
                PosX = _coords.x;
                PosY = _coords.y;
                PosZ = _coords.z;
                int newSoulLevel = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.SoulLevel);
                if (_currentSoulLevel != newSoulLevel)
                {
                    SoulLevel = newSoulLevel;
                    _currentSoulLevel = newSoulLevel;
                    LoadStats();
                }
            };
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
            _hotkeyManager.RegisterAction("ToggleSpeed", ToggleSpeed);
            _hotkeyManager.RegisterAction("IncreaseSpeed",() => SetSpeed(Math.Min(10, PlayerSpeed + 0.25f)));
            _hotkeyManager.RegisterAction("DecreaseSpeed", () => SetSpeed(Math.Max(0, PlayerSpeed - 0.25f)));
        }

        private void LoadStats()
        {
            Vitality = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Vitality);
            Attunement = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Attunement);
            Endurance = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Endurance);
            Strength = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Strength);
            Dexterity = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Dexterity);
            Resistance = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Resistance);
            Intelligence = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Intelligence);
            Faith = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Faith);
            Humanity = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Humanity);
            Souls = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.Souls);
            NewGame = _playerService.GetNewGame();
            PlayerSpeed = _playerService.GetPlayerSpeed();
            SoulLevel = _playerService.GetPlayerStat(GameDataMan.PlayerGameData.SoulLevel);
        }

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        public int CurrentHp
        {
            get => _currentHp;
            set => SetProperty(ref _currentHp, value);
        }

        public int CurrentMaxHp
        {
            get => _currentMaxHp;
            set => SetProperty(ref _currentMaxHp, value);
        }

        public void SetHp(int hp)
        {
            _playerService.SetHp(hp);
            CurrentHp = hp;
        }

        public void SetMaxHp()
        {
            _playerService.SetHp(CurrentMaxHp);
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
            var state = index == 0 ? _saveState1 : _saveState2;
            if (index == 0) IsPos1Saved = true;
            else IsPos2Saved = true;

            if (IsStateIncluded)
            {
                state.Hp = CurrentHp;
                state.Sp = _playerService.GetSp();
            }

            _playerService.SavePos(index);
        }

        public void RestorePos(int index)
        {
            _wasNoDamageEnabled = IsNoDamageEnabled;
            _wasNoDeathEnabled = IsNoDeathEnabled;
            IsNoDamageEnabled = true;
            _isNoDeathEnabled = true;
            _playerService.RestorePos(index);
            IsNoDamageEnabled = _wasNoDamageEnabled;
            IsNoDeathEnabled = _wasNoDeathEnabled;
            if (!IsStateIncluded) return;

            var state = index == 0 ? _saveState1 : _saveState2;
            _playerService.SetHp(state.Hp);
            _playerService.SetSp(state.Sp);
        }

        public bool IsStateIncluded
        {
            get => _isStateIncluded;
            set => SetProperty(ref _isStateIncluded, value);
        }

        public float PosX
        {
            get => _posX;
            set
            {
                if (SetProperty(ref _posX, value))
                {
                    _playerService.SetAxis(WorldChrMan.Coords.X, value);
                }
            }
        }
        
        public float PosZ
        {
            get => _posZ;
            set
            {
                if (SetProperty(ref _posZ, value))
                {
                    _playerService.SetAxis(WorldChrMan.Coords.Z, value);
                }
            }
        }
        
        public float PosY
        {
            get => _posY;
            set
            {
                if (SetProperty(ref _posY, value))
                {
                    _playerService.SetAxis(WorldChrMan.Coords.Y, value);
                }
            }
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

        public bool IsInfiniteDurabilityEnabled
        {
            get => _isInfiniteDurabilityEnabled;
            set
            {
                if (SetProperty(ref _isInfiniteDurabilityEnabled, value))
                {
                    _playerService.ToggleInfiniteDurability(_isInfiniteDurabilityEnabled);
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

        public bool IsInfinitePoiseEnabled
        {
            get => _isInfinitePoiseEnabled;
            set
            {
                if (SetProperty(ref _isInfinitePoiseEnabled, value))
                {
                    _playerService.ToggleInfinitePoise(_isInfinitePoiseEnabled);
                }
            }
        }

        public bool IsAutoSetNewGameSixEnabled
        {
            get => _isAutoSetNewGameSixEnabled;
            set => SetProperty(ref _isAutoSetNewGameSixEnabled, value);
        }

        public void RestoreSpellCasts()
        {
            _playerService.RestoreSpellCasts();
        }


        public void DisableButtons()
        {
            AreOptionsEnabled = false;
            _timer.Stop();
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
            if (IsInfinitePoiseEnabled)
                _playerService.ToggleInfinitePoise(true);
            if (IsInfiniteDurabilityEnabled)
                _playerService.ToggleInfiniteDurability(true);
            if (IsNoRollEnabled)
                _playerService.ToggleNoRoll(true);
            AreOptionsEnabled = true;
            LoadStats();
            _timer.Start();
        }

        public int SoulLevel
        {
            get => _soulLevel;
            private set => SetProperty(ref _soulLevel, value);
        }

        public int Vitality
        {
            get => _vitality;
            set => SetProperty(ref _vitality, value);
        }

        public int Attunement
        {
            get => _attunement;
            set => SetProperty(ref _attunement, value);
        }

        public int Endurance
        {
            get => _endurance;
            set => SetProperty(ref _endurance, value);
        }

        public int Strength
        {
            get => _strength;
            set => SetProperty(ref _strength, value);
        }

        public int Dexterity
        {
            get => _dexterity;
            set => SetProperty(ref _dexterity, value);
        }

        public int Resistance
        {
            get => _resistance;
            set => SetProperty(ref _resistance, value);
        }

        public int Intelligence
        {
            get => _intelligence;
            set => SetProperty(ref _intelligence, value);
        }

        public int Faith
        {
            get => _faith;
            set => SetProperty(ref _faith, value);
        }

        public int Humanity
        {
            get => _humanity;
            set => SetProperty(ref _humanity, value);
        }

        public int Souls
        {
            get => _souls;
            set => SetProperty(ref _souls, value);
        }
        
        public int NewGame
        {
            get => _newGame;
            set
            {
                if (SetProperty(ref _newGame, value))
                {
                    _playerService.SetNewGame(value);
                }
            }
        }
        
        private void ToggleSpeed()
        {
            if (!AreOptionsEnabled) return;

            if (!IsApproximately(PlayerSpeed, DefaultSpeed))
            {
                _playerDesiredSpeed = PlayerSpeed;
                SetSpeed(DefaultSpeed);
            }
            else if (_playerDesiredSpeed >= 0)
            {
                SetSpeed(_playerDesiredSpeed);
            }
        }

        private bool IsApproximately(float a, float b)
        {
            return Math.Abs(a - b) < Epsilon;
        }


        public float PlayerSpeed
        {
            get => _playerSpeed;
            set
            {
                if (SetProperty(ref _playerSpeed, value))
                {
                    _playerService.SetPlayerSpeed(value);
                }
            }
        }

        public void SetSpeed(float value)
        {
            PlayerSpeed = value;
        }

        public void TrySetNgPref()
        {
            if (IsAutoSetNewGameSixEnabled)
                _playerService.SetNewGame(7);
            NewGame = _playerService.GetNewGame();
        }
        
        public bool IsNoRollEnabled
        {
            get => _isNoRollEnabled;
            set
            {
                if (!SetProperty(ref _isNoRollEnabled, value)) return;
                _playerService.ToggleNoRoll(_isNoRollEnabled);
            }
        }

        public void SetStat(string statName, int val)
        {
            GameDataMan.PlayerGameData stat =
                (GameDataMan.PlayerGameData)Enum.Parse(typeof(GameDataMan.PlayerGameData), statName);
            _playerService.SetPlayerStat(stat, val);
        }

        public void GiveSouls()
        {
            _playerService.GiveSouls();
        }
    }
}