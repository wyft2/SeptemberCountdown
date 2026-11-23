using System;
using System.Runtime.InteropServices;

namespace SeptemberCountdown
{
    internal static class NativeMethods
    {
        internal const int GWL_STYLE = -16;
        internal const int GWL_EXSTYLE = -20;
        internal const int WS_EX_TOOLWINDOW = 0x00000080;
        internal const int WS_EX_APPWINDOW = 0x00040000;

        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        internal const uint SWP_NOSIZE = 0x0001;
        internal const uint SWP_NOMOVE = 0x0002;
        internal const uint SWP_NOACTIVATE = 0x0010;
        internal const uint SWP_SHOWWINDOW = 0x0040;

        private const int WCA_ACCENT_POLICY = 19;

        private enum AccentState
        {
            AccentDisabled = 0,
            AccentEnableGradient = 1,
            AccentEnableTransparentGradient = 2,
            AccentEnableBlurBehind = 3,
            AccentEnableAcrylicBlurBehind = 4,
            AccentEnableHostBackdrop = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd,
            ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute,
            ref int attributeValue, int attributeSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        internal static TimeSpan GetSystemIdleTime()
        {
            try
            {
                LASTINPUTINFO info = new LASTINPUTINFO();
                info.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
                if (!GetLastInputInfo(ref info))
                {
                    return TimeSpan.Zero;
                }
                uint idle = unchecked((uint)GetTickCount() - info.dwTime);
                return TimeSpan.FromMilliseconds(idle);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        internal static void ApplyWindowEffects(IntPtr hwnd)
        {
            try
            {
                int dark = 1;
                DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int));

                int round = 2;
                DwmSetWindowAttribute(hwnd, 33, ref round, sizeof(int));

                int border = unchecked((int)0xFFFFFFFE);
                DwmSetWindowAttribute(hwnd, 34, ref border, sizeof(int));

                EnableAcrylic(hwnd);
            }
            catch
            {
            }
        }

        private static bool EnableAcrylic(IntPtr hwnd)
        {
            // AABBGGRR - a smoky ink tint; the WPF layer supplies the colour.
            int gradientColor = unchecked((int)0xA6180E10);

            if (!SetAccent(hwnd, AccentState.AccentEnableAcrylicBlurBehind, gradientColor))
            {
                return SetAccent(hwnd, AccentState.AccentEnableBlurBehind, gradientColor);
            }
            return true;
        }

        private static bool SetAccent(IntPtr hwnd, AccentState state, int gradientColor)
        {
            AccentPolicy policy = new AccentPolicy();
            policy.AccentState = (int)state;
            policy.AccentFlags = 2;
            policy.GradientColor = gradientColor;
            policy.AnimationId = 0;

            GCHandle handle = GCHandle.Alloc(policy, GCHandleType.Pinned);
            try
            {
                WindowCompositionAttributeData data = new WindowCompositionAttributeData();
                data.Attribute = WCA_ACCENT_POLICY;
                data.Data = handle.AddrOfPinnedObject();
                data.SizeOfData = Marshal.SizeOf(typeof(AccentPolicy));
                return SetWindowCompositionAttribute(hwnd, ref data) != 0;
            }
            finally
            {
                handle.Free();
            }
        }

        internal static void SetTopmost(IntPtr hwnd, bool topmost)
        {
            try
            {
                SetWindowPos(hwnd, topmost ? HWND_TOPMOST : HWND_NOTOPMOST,
                    0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
            catch
            {
            }
        }

        internal static bool ActivateExistingWindow()
        {
            try
            {
                IntPtr hwnd = FindWindow(null, "九月倒计时");
                if (hwnd == IntPtr.Zero)
                {
                    return false;
                }
                ShowWindow(hwnd, 9);
                SetForegroundWindow(hwnd);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
