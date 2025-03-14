using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DSRForge.Memory;
using H.Hooks;

namespace DSRForge.Utilities
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
            _keyboardHook.IsExtendedMode = true;
            
            _keyboardHook.Down += KeyboardHook_Down;
            
            _keyboardHook.Start();
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
        }
        
        public void ClearHotkey(string actionId)
        {
            _hotkeyMappings.Remove(actionId);
        }
        
        public Keys GetHotkey(string actionId)
        {
            return _hotkeyMappings.TryGetValue(actionId, out var keys) ? keys : null;
        }
    }
}