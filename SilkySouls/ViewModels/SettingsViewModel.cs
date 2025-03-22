using System;
using System.Collections.Generic;
using System.Linq;
using H.Hooks;
using SilkySouls.Services;
using SilkySouls.Utilities;

namespace SilkySouls.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private bool _isFastQuitoutEnabled;
        private bool _isEnableHotkeysEnabled;
        private bool _isGuaranteedBkhEnabled;
        private bool _isLoaded;
        
        private readonly HotkeyManager _hotkeyManager;
        private string _currentSettingHotkeyId;
        private LowLevelKeyboardHook _tempHook;
        private Keys _currentKeys;
        
        private string _savePos1HotkeyText;
        public string SavePos1HotkeyText
        {
            get => _savePos1HotkeyText;
            set => SetProperty(ref _savePos1HotkeyText, value);
        }
        
        private string _savePos2HotkeyText;
        public string SavePos2HotkeyText
        {
            get => _savePos2HotkeyText;
            set => SetProperty(ref _savePos2HotkeyText, value);
        }
        
        private string _restorePos1HotkeyText;
        public string RestorePos1HotkeyText
        {
            get => _restorePos1HotkeyText;
            set => SetProperty(ref _restorePos1HotkeyText, value);
        }
        
        private string _restorePos2HotkeyText;
        public string RestorePos2HotkeyText
        {
            get => _restorePos2HotkeyText;
            set => SetProperty(ref _restorePos2HotkeyText, value);
        }
        
        private string _rtsrHotkeyText;
        public string RtsrHotkeyText
        {
            get => _rtsrHotkeyText;
            set => SetProperty(ref _rtsrHotkeyText, value);
        }
        
        private string _noClipHotkeyText;
        public string NoClipHotkeyText
        {
            get => _noClipHotkeyText;
            set => SetProperty(ref _noClipHotkeyText, value);
        }
        
        private string _noDeathHotkeyText;
        public string NoDeathHotkeyText
        {
            get => _noDeathHotkeyText;
            set => SetProperty(ref _noDeathHotkeyText, value);
        }

        private string _oneShotHotkeyText;
        public string OneShotHotkeyText
        {
            get => _oneShotHotkeyText;
            set => SetProperty(ref _oneShotHotkeyText, value);
        }

        private string _restoreSpellCastsHotkeyText;
        public string RestoreSpellCastsHotkeyText
        {
            get => _restoreSpellCastsHotkeyText;
            set => SetProperty(ref _restoreSpellCastsHotkeyText, value);
        }

        private string _increaseSpeedHotkeyText;
        public string IncreaseSpeedHotkeyText
        {
            get => _increaseSpeedHotkeyText;
            set => SetProperty(ref _increaseSpeedHotkeyText, value);
        }

        private string _decreaseSpeedHotkeyText;
        public string DecreaseSpeedHotkeyText
        {
            get => _decreaseSpeedHotkeyText;
            set => SetProperty(ref _decreaseSpeedHotkeyText, value);
        }
        
        
        private string _repeatActionHotkeyText;
        public string RepeatActionHotkeyText
        {
            get => _repeatActionHotkeyText;
            set => SetProperty(ref _repeatActionHotkeyText, value);
        }

        private string _disableTargetAiHotkeyText;

        public string DisableTargetAiHotkeyText
        {
            get => _disableTargetAiHotkeyText;
            set => SetProperty(ref _disableTargetAiHotkeyText, value);
        }
        
        private string _freezeHpHotkeyText;
        public string FreezeHpHotkeyText
        {
            get => _freezeHpHotkeyText;
            set => SetProperty(ref _freezeHpHotkeyText, value);
        }
        
        private string _increaseTargetSpeedHotkeyText;
        public string IncreaseTargetSpeedHotkeyText
        {
            get => _increaseTargetSpeedHotkeyText;
            set => SetProperty(ref _increaseTargetSpeedHotkeyText, value);
        }

        private string _decreaseTargetSpeedHotkeyText;
        public string DecreaseTargetSpeedHotkeyText
        {
            get => _decreaseTargetSpeedHotkeyText;
            set => SetProperty(ref _decreaseTargetSpeedHotkeyText, value);
        }
        
        private string _enableTargetOptionsHotkeyText;
        public string EnableTargetOptionsHotkeyText
        {
            get => _enableTargetOptionsHotkeyText;
            set => SetProperty(ref _enableTargetOptionsHotkeyText, value);
        }

        private string _showAllResistancesHotkeyText;
        public string ShowAllResistancesHotkeyText
        {
            get => _showAllResistancesHotkeyText;
            set => SetProperty(ref _showAllResistancesHotkeyText, value);
        }

        private string _quitoutHotkeyText;
        public string QuitoutHotkeyText
        {
            get => _quitoutHotkeyText;
            set => SetProperty(ref _quitoutHotkeyText, value);
        }

        private string _disableAiHotkeyText;
        public string DisableAiHotkeyText
        {
            get => _disableAiHotkeyText;
            set => SetProperty(ref _disableAiHotkeyText, value);
        }
        
        private string _allNoDeathHotkeyText;
        public string AllNoDeathHotkeyText
        {
            get => _allNoDeathHotkeyText;
            set => SetProperty(ref _allNoDeathHotkeyText, value);
        }

        private string _allNoDamageHotkeyText;
        public string AllNoDamageHotkeyText
        {
            get => _allNoDamageHotkeyText;
            set => SetProperty(ref _allNoDamageHotkeyText, value);
        }


        private readonly Dictionary<string, Action<string>> _propertySetters;
        private readonly SettingsService _settingsService;
        
        public SettingsViewModel(SettingsService settingsService, HotkeyManager hotkeyManager)
        {
            _settingsService = settingsService;
            _hotkeyManager = hotkeyManager;

            RegisterHotkeys();

            _propertySetters = new Dictionary<string, Action<string>>
            {
                { "SavePos1", text => SavePos1HotkeyText = text },
                { "SavePos2", text => SavePos2HotkeyText = text },
                { "RestorePos1", text => RestorePos1HotkeyText = text },
                { "RestorePos2", text => RestorePos2HotkeyText = text },
                { "RTSR", text => RtsrHotkeyText = text },
                { "NoDeath", text => NoDeathHotkeyText = text },
                { "OneShot", text => OneShotHotkeyText = text },
                { "RestoreSpellCasts", text => RestoreSpellCastsHotkeyText = text },
                { "IncreaseSpeed", text => IncreaseSpeedHotkeyText = text },
                { "DecreaseSpeed", text => DecreaseSpeedHotkeyText = text },
                { "NoClip", text => NoClipHotkeyText = text},
                { "RepeatAction", text => RepeatActionHotkeyText = text },
                {"DisableTargetAi", text => DisableTargetAiHotkeyText = text},
                { "FreezeHp", text => FreezeHpHotkeyText = text },
                { "Quitout", text => QuitoutHotkeyText = text },
                { "DisableAi", text => DisableAiHotkeyText = text },
                { "AllNoDeath", text => AllNoDeathHotkeyText = text },
                { "AllNoDamage", text => AllNoDamageHotkeyText = text },
                { "IncreaseTargetSpeed", text => IncreaseTargetSpeedHotkeyText = text },
                { "DecreaseTargetSpeed", text => DecreaseTargetSpeedHotkeyText = text },
                { "EnableTargetOptions", text => EnableTargetOptionsHotkeyText = text },
                { "ShowAllResistances", text => ShowAllResistancesHotkeyText = text },
            };

            LoadHotkeyDisplays();
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction("Quitout", () => _settingsService.Quitout());
        }

        private void LoadHotkeyDisplays()
        {
            foreach (var entry in _propertySetters)
            {
                string actionId = entry.Key;
                Action<string> setter = entry.Value;
                
                setter(GetHotkeyDisplayText(actionId));
            }
        }
        
        private string GetHotkeyDisplayText(string actionId)
        {
            Keys keys = _hotkeyManager.GetHotkey(actionId);
            return keys != null && keys.Values.ToArray().Length > 0 ? string.Join(" + ", keys) : "None";
        }
        
        
        public void StartSettingHotkey(string actionId)
        {
            if (_currentSettingHotkeyId != null && _propertySetters.TryGetValue(_currentSettingHotkeyId, out var prevSetter))
            {
                prevSetter(GetHotkeyDisplayText(_currentSettingHotkeyId));
            }
            
            _currentSettingHotkeyId = actionId;
            
            if (_propertySetters.TryGetValue(actionId, out var setter))
            {
                setter("Press keys...");
            }
            
            _tempHook = new LowLevelKeyboardHook();
            _tempHook.IsExtendedMode = true;
            _tempHook.Down += TempHook_Down;
            _tempHook.Start();
        }

        private void TempHook_Down(object sender, KeyboardEventArgs e)
        {
            if (_currentSettingHotkeyId == null || e.Keys.IsEmpty) 
                return;
    
            try
            {
                bool containsEnter = e.Keys.Values.Contains(Key.Enter) || e.Keys.Values.Contains(Key.Return);
                bool containsEscape = e.Keys.Values.Contains(Key.Escape);
                
                if (containsEnter && _currentKeys != null)
                {
                    _hotkeyManager.SetHotkey(_currentSettingHotkeyId, _currentKeys);
                    StopSettingHotkey();
                    e.IsHandled = true;
                    return;
                }

                if (containsEscape)
                {
            
                    CancelSettingHotkey();
                    e.IsHandled = true;
                    return;
                }

                if (containsEnter)
                {
                    e.IsHandled = true;
                    return;
                }
                
                if (e.Keys.IsEmpty) 
                    return;
                
                _currentKeys = e.Keys;
        
                if (_propertySetters.TryGetValue(_currentSettingHotkeyId, out var setter))
                {
                    string keyText = e.Keys.ToString();
                    setter(keyText);
                }
            }
            catch (Exception ex)
            {
                if (_propertySetters.TryGetValue(_currentSettingHotkeyId, out var setter))
                {
                    setter("Error: Invalid key combination");
                }
            }
            e.IsHandled = true;
        }

        private void StopSettingHotkey()
        {
            if (_tempHook != null)
            {
                _tempHook.Down -= TempHook_Down;
                _tempHook.Dispose();
                _tempHook = null;
            }
            
            _currentSettingHotkeyId = null;
            _currentKeys = null;
        }

        public void ConfirmHotkey()
        {
            if (_currentSettingHotkeyId != null && (_currentKeys == null || _currentKeys.IsEmpty))
            {
                CancelSettingHotkey();
                return;
            }
            
            if (_currentSettingHotkeyId != null && _currentKeys != null)
            {
                _hotkeyManager.SetHotkey(_currentSettingHotkeyId, _currentKeys);
                if (_propertySetters.TryGetValue(_currentSettingHotkeyId, out var setter))
                {
                    setter(new Keys(_currentKeys.Values.ToArray()).ToString());
                }
            }
            StopSettingHotkey();
        }

        public void CancelSettingHotkey()
        {
            if (_currentSettingHotkeyId != null && _propertySetters.TryGetValue(_currentSettingHotkeyId, out var setter))
            {
                setter("None");
                _hotkeyManager.SetHotkey(_currentSettingHotkeyId, new Keys());
            }
            StopSettingHotkey();
        }
        
        
        public bool IsFastQuitoutEnabled
        {
            get => _isFastQuitoutEnabled;
            set
            {
                if (SetProperty(ref _isFastQuitoutEnabled, value))
                {
                    Properties.Settings.Default.FastQuitout = value;
                    Properties.Settings.Default.Save();
                    if (_isLoaded)
                    {
                        _settingsService.ToggleFastQuitout(_isFastQuitoutEnabled ? 1 : 0);
                    }
                }
            }
        }
        
        public bool IsEnableHotkeysEnabled
        {
            get => _isEnableHotkeysEnabled;
            set
            {
                if (SetProperty(ref _isEnableHotkeysEnabled, value))
                {
                    Properties.Settings.Default.EnableHotkeys = value;
                    Properties.Settings.Default.Save();
                    if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
                    else _hotkeyManager.Stop();
                }
            }
        }
        
        public bool IsGuaranteedBkhEnabled
        {
            get => _isGuaranteedBkhEnabled;
            set
            {
                if (SetProperty(ref _isGuaranteedBkhEnabled, value))
                {
                    Properties.Settings.Default.GuaranteedBkh = value;
                    Properties.Settings.Default.Save();
                    if (_isLoaded)
                    {
                        _settingsService.SetGuaranteedBkhDrop(_isGuaranteedBkhEnabled);
                    }
                }
            }
        }
        
        public void ApplyLoadedOptions()
        {
            _isLoaded = true;
            if (IsFastQuitoutEnabled)  _settingsService.ToggleFastQuitout(1);
            if (IsGuaranteedBkhEnabled) _settingsService.SetGuaranteedBkhDrop(true);
            
        }

        public void ApplyStartUpOptions()
        {
            _isEnableHotkeysEnabled = Properties.Settings.Default.EnableHotkeys;
            if (_isEnableHotkeysEnabled) _hotkeyManager.Start();
            else _hotkeyManager.Stop();
            OnPropertyChanged(nameof(IsEnableHotkeysEnabled));
            _isGuaranteedBkhEnabled = Properties.Settings.Default.GuaranteedBkh;
            OnPropertyChanged(nameof(IsGuaranteedBkhEnabled));
            _isFastQuitoutEnabled = Properties.Settings.Default.FastQuitout;
            OnPropertyChanged(nameof(IsFastQuitoutEnabled));
        }

        public void ResetAttached()
        {
            _isLoaded = false;
        }
    }
}