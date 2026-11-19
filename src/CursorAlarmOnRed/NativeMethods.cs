using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CursorAlarmOnRed
{
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT p);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern uint GetPixel(IntPtr hDC, int x, int y);
        [DllImport("kernel32.dll")] private static extern bool Beep(uint frequency, uint durationMs);

        /// <summary>Colour of the screen pixel under the mouse cursor, or null if it cannot be read.</summary>
        public static Color? GetPixelUnderCursor()
        {
            POINT p;
            if (!GetCursorPos(out p)) return null;
            IntPtr dc = GetDC(IntPtr.Zero);
            if (dc == IntPtr.Zero) return null;
            try
            {
                uint c = GetPixel(dc, p.X, p.Y);
                if (c == 0xFFFFFFFF) return null; // CLR_INVALID
                return Color.FromArgb((int)(c & 0xFF), (int)((c >> 8) & 0xFF), (int)((c >> 16) & 0xFF));
            }
            finally { ReleaseDC(IntPtr.Zero, dc); }
        }

        public static void AlarmBeep() { Beep(1000, 250); }
    }
}
