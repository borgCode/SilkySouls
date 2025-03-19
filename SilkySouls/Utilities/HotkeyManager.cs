using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using H.Hooks;
using SilkySouls.Memory;

namespace SilkySouls.Utilities
{
    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        
        private readonly MemoryIo _memoryIo;
        private readonly LowLevelKeyboardHook _keyboardHook;
        private readonly Dictionary<string, Keys> _hotkeyMappings;
        private readonly Dictionary<string, Action> _actions;

        public HotkeyManager(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
            _hotkeyMappings = new Dictionary<string, Keys>();
            _actions = new Dictionary<string, Action>();

            _keyboardHook = new LowLevelKeyboardHook();
            _keyboardHook.HandleModifierKeys = true;
            
            _keyboardHook.Down += KeyboardHook_Down;
            
            _keyboardHook.Start();
            
            LoadHotkeys();
        }

        public void RegisterAction(string actionId, Action action)
        {
            _actions[actionId] = action;
        }
        
        private void KeyboardHook_Down(object sender, KeyboardEventArgs e)
        {
            if (!IsGameFocused())
                return;
            foreach (var mapping in _hotkeyMappings)
            {
                string actionId = mapping.Key;
                Keys keys = mapping.Value;
                if (!e.Keys.Are(keys.Values.ToArray())) continue;
                if (_actions.TryGetValue(actionId, out var action))
                {
                    action.Invoke();
                }
                break;
            }
        }

        private bool IsGameFocused()
        {
            if (_memoryIo.TargetProcess == null || _memoryIo.TargetProcess.Id == 0) return false;
         
            IntPtr foregroundWindow = GetForegroundWindow();
            GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);
            return foregroundProcessId == (uint)_memoryIo.TargetProcess.Id;
        }
        
        public void SetHotkey(string actionId, Keys keys)
        {
            _hotkeyMappings[actionId] = keys;
            SaveHotkeys();
        }
        
        public void ClearHotkey(string actionId)
        {
            _hotkeyMappings.Remove(actionId);
            SaveHotkeys();
        }
        
        public Keys GetHotkey(string actionId)
        {
            return _hotkeyMappings.TryGetValue(actionId, out var keys) ? keys : null;
        }
        
        
        public void SaveHotkeys()
        {
            try
            {
                Properties.Settings.Default.HotkeyActionIds = "";
                Properties.Settings.Default.HotkeyValues = "";
                
                if (_hotkeyMappings.Count > 0)
                {
                    Properties.Settings.Default.HotkeyActionIds = string.Join(";", _hotkeyMappings.Keys);
                    
                    var hotkeyStrings = _hotkeyMappings.Values.Select(k => k.ToString());
                    Properties.Settings.Default.HotkeyValues = string.Join(";", hotkeyStrings);
                }
                
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving hotkeys: {ex.Message}");
            }
        }
        
        public void LoadHotkeys()
        {
            try
            {
                string actionIdsString = Properties.Settings.Default.HotkeyActionIds;
                string hotkeyValuesString = Properties.Settings.Default.HotkeyValues;
                
                if (!string.IsNullOrEmpty(actionIdsString) && !string.IsNullOrEmpty(hotkeyValuesString))
                {
                    string[] actionIds = actionIdsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] hotkeyStrings = hotkeyValuesString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (actionIds.Length == hotkeyStrings.Length)
                    {
                        
                        for (int i = 0; i < actionIds.Length; i++)
                        {
                            _hotkeyMappings[actionIds[i]] = Keys.Parse(hotkeyStrings[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hotkeys: {ex.Message}");
            }
        }
    }
}