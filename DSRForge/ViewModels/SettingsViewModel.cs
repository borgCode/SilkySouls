using System;
using System.Collections.Generic;
using System.Linq;
using DSRForge.Utilities;
using H.Hooks;

namespace DSRForge.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
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
        
        
        private readonly Dictionary<string, Action<string>> _propertySetters;
        private string _originalHotkeyText;
        
        public SettingsViewModel(HotkeyManager hotkeyManager)
        {
            _hotkeyManager = hotkeyManager;

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
                {"NoClip", text => NoClipHotkeyText = text},
            };

            LoadHotkeyDisplays();
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
            StopSettingHotkey();
            
            _currentSettingHotkeyId = actionId;
            
            
            if (_propertySetters.TryGetValue(actionId, out var setter))
            {
                _originalHotkeyText = GetHotkeyDisplayText(actionId);
                setter("Press keys...");
            }
            
            
            _tempHook = new LowLevelKeyboardHook();
            _tempHook.IsExtendedMode = true;
            _tempHook.Down += TempHook_Down;
            _tempHook.Start();
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
                
        
                Console.WriteLine(e.Keys);
                
                
                _hotkeyManager.SetHotkey(_currentSettingHotkeyId, e.Keys);
        
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


        public void ConfirmHotkey()
        {
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
                setter(GetHotkeyDisplayText(_currentSettingHotkeyId));
            }
    
            StopSettingHotkey();
        }


        public void ResetAllHotkeys()
        {
            foreach (var entry in _propertySetters)
            {
                _hotkeyManager.ClearHotkey(entry.Key);
            }
            
            LoadHotkeyDisplays();
        }
    }
}