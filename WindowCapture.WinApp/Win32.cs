using System;
using System.Text;
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using System.Collections.Generic;
using Windows.Foundation;
using System.Numerics;

namespace WindowCapture.WinApp
{
    public static class Win32
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static readonly uint WM_GETICON = 0x007f;
        public static readonly IntPtr ICON_SMALL2 = new IntPtr(2);
        public static readonly IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
        public static readonly int GCL_HICON = -14;

        /// <summary>
        /// check if windows visible
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// return windows text
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpWindowText"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        /// <summary>
        /// enumarator on all desktop windows
        /// </summary>
        /// <param name="hDesktop"></param>
        /// <param name="lpEnumCallbackFunction"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// filter function
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        public static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 64 bit version maybe loses significant 64-bit specific information
        /// </summary>
        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr((long)GetClassLong32(hWnd, nIndex));
            else
                return GetClassLong64(hWnd, nIndex);
        }
    }

    public static class CaptureHelper
    {
        static readonly Guid GraphicsCaptureItemGuid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");

        [ComImport]
        [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        public interface IGraphicsCaptureItemInterop
        {
            IntPtr CreateForWindow(
                [In] IntPtr window,
                [In] ref Guid iid);

            IntPtr CreateForMonitor(
                [In] IntPtr monitor,
                [In] ref Guid iid);
        }

        public static GraphicsCaptureItem CreateItemForWindow(IntPtr hwnd)
        {
            IGraphicsCaptureItemInterop interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            IntPtr itemPointer = interop.CreateForWindow(hwnd, GraphicsCaptureItemGuid);

            GraphicsCaptureItem item = GraphicsCaptureItem.FromAbi(itemPointer);
            GraphicsCaptureItem item2 = Marshal.GetObjectForIUnknown(itemPointer) as GraphicsCaptureItem;

            var releaseRes = Marshal.Release(itemPointer);

            return item;
        }

        public static GraphicsCaptureItem CreateItemForMonitor(IntPtr hmon)
        {
            IGraphicsCaptureItemInterop interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            IntPtr itemPointer = interop.CreateForMonitor(hmon, GraphicsCaptureItemGuid);

            GraphicsCaptureItem item = GraphicsCaptureItem.FromAbi(itemPointer);
            GraphicsCaptureItem item2 = Marshal.GetObjectForIUnknown(itemPointer) as GraphicsCaptureItem;

            var releaseRes = Marshal.Release(itemPointer);

            return item;
        }
    }

    public static class WindowEnumerationHelper
    {
        enum GetAncestorFlags
        {
            // Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
            GetParent = 1,
            // Retrieves the root window by walking the chain of parent windows.
            GetRoot = 2,
            // Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
            GetRootOwner = 3
        }

        public enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        [Flags]
        private enum WindowStyles : uint
        {
            WS_BORDER = 0x800000,
            WS_CAPTION = 0xc00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000u,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEFRAME = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000
        }

        enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly.
        // http://pinvoke.net/default.aspx/user32/GetWindowLong.html
        static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);

        public static bool IsWindowValidForCapture(IntPtr hwnd)
        {
            if (hwnd.ToInt32() == 0)
            {
                return false;
            }

            if (hwnd == GetShellWindow())
            {
                return false;
            }

            if (!IsWindowVisible(hwnd))
            {
                return false;
            }

            if (GetAncestor(hwnd, GetAncestorFlags.GetRoot) != hwnd)
            {
                return false;
            }

            var style = (WindowStyles)(uint)GetWindowLongPtr(hwnd, (int)GWL.GWL_STYLE).ToInt64();
            if (style.HasFlag(WindowStyles.WS_DISABLED))
            {
                return false;
            }

            var cloaked = false;
            var hrTemp = DwmGetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.Cloaked, out cloaked, Marshal.SizeOf<bool>());
            if (hrTemp == 0 && cloaked)
            {
                return false;
            }

            return true;
        }
    }

    public class MonitorInfo
    {
        public bool IsPrimary { get; set; }
        public Vector2 ScreenSize { get; set; }
        public Rect MonitorArea { get; set; }
        public Rect WorkArea { get; set; }
        public string DeviceName { get; set; }
        public IntPtr Hmon { get; set; }
    }

    public static class MonitorEnumerationHelper
    {
        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int CCHDEVICENAME = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MonitorInfoEx
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        public static IEnumerable<MonitorInfo> GetMonitors()
        {
            var result = new List<MonitorInfo>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
                {
                    MonitorInfoEx mi = new MonitorInfoEx();
                    mi.Size = Marshal.SizeOf(mi);
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        var info = new MonitorInfo
                        {
                            ScreenSize = new Vector2(mi.Monitor.right - mi.Monitor.left, mi.Monitor.bottom - mi.Monitor.top),
                            MonitorArea = new Rect(mi.Monitor.left, mi.Monitor.top, mi.Monitor.right - mi.Monitor.left, mi.Monitor.bottom - mi.Monitor.top),
                            WorkArea = new Rect(mi.WorkArea.left, mi.WorkArea.top, mi.WorkArea.right - mi.WorkArea.left, mi.WorkArea.bottom - mi.WorkArea.top),
                            IsPrimary = mi.Flags > 0,
                            Hmon = hMonitor,
                            DeviceName = mi.DeviceName
                        };
                        result.Add(info);
                    }
                    return true;
                }, IntPtr.Zero);
            return result;
        }
    }
}
