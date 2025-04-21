using System;
using System.Runtime.InteropServices;

namespace SilkySouls.Utilities
{
    internal static class User32
    {
        public static readonly IntPtr HwndTopmost = new IntPtr(-1);
        public const uint SwpNosize = 0x0001;
        public const uint SwpNomove = 0x0002;
        public const uint SwpNoactivate = 0x0010;

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
            uint uFlags);
        
        public static void SetTopmost(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HwndTopmost, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpNoactivate);
        }
    }
}