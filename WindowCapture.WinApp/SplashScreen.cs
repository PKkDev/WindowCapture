using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using static WindowCapture.WinApp.MFPlayTools;

namespace WindowCapture.WinApp
{
    public enum HRESULT : int
    {
        S_OK = 0,
        S_FALSE = 1,
        E_NOTIMPL = unchecked((int)0x80004001),
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_POINTER = unchecked((int)0x80004003),
        E_FAIL = unchecked((int)0x80004005),
        E_UNEXPECTED = unchecked((int)0x8000FFFF),
        E_OUTOFMEMORY = unchecked((int)0x8007000E),
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public RECT(int Left, int Top, int Right, int Bottom)
        {
            left = Left;
            top = Top;
            right = Right;
            bottom = Bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
        public SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LARGE_INTEGER
    {
        [FieldOffset(0)]
        public int LowPart;
        [FieldOffset(4)]
        public int HighPart;
        [FieldOffset(0)]
        public long QuadPart;
    }

    public enum VARENUM
    {
        VT_EMPTY = 0,
        VT_NULL = 1,
        VT_I2 = 2,
        VT_I4 = 3,
        VT_R4 = 4,
        VT_R8 = 5,
        VT_CY = 6,
        VT_DATE = 7,
        VT_BSTR = 8,
        VT_DISPATCH = 9,
        VT_ERROR = 10,
        VT_BOOL = 11,
        VT_VARIANT = 12,
        VT_UNKNOWN = 13,
        VT_DECIMAL = 14,
        VT_I1 = 16,
        VT_UI1 = 17,
        VT_UI2 = 18,
        VT_UI4 = 19,
        VT_I8 = 20,
        VT_UI8 = 21,
        VT_INT = 22,
        VT_UINT = 23,
        VT_VOID = 24,
        VT_HRESULT = 25,
        VT_PTR = 26,
        VT_SAFEARRAY = 27,
        VT_CARRAY = 28,
        VT_USERDEFINED = 29,
        VT_LPSTR = 30,
        VT_LPWSTR = 31,
        VT_RECORD = 36,
        VT_INT_PTR = 37,
        VT_UINT_PTR = 38,
        VT_FILETIME = 64,
        VT_BLOB = 65,
        VT_STREAM = 66,
        VT_STORAGE = 67,
        VT_STREAMED_OBJECT = 68,
        VT_STORED_OBJECT = 69,
        VT_BLOB_OBJECT = 70,
        VT_CF = 71,
        VT_CLSID = 72,
        VT_VERSIONED_STREAM = 73,
        VT_BSTR_BLOB = 0xfff,
        VT_VECTOR = 0x1000,
        VT_ARRAY = 0x2000,
        VT_BYREF = 0x4000,
        VT_RESERVED = 0x8000,
        VT_ILLEGAL = 0xffff,
        VT_ILLEGALMASKED = 0xfff,
        VT_TYPEMASK = 0xfff
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPARRAY
    {
        public uint cElems;
        public IntPtr pElems;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PROPVARIANT
    {
        [FieldOffset(0)]
        public ushort varType;
        [FieldOffset(2)]
        public ushort wReserved1;
        [FieldOffset(4)]
        public ushort wReserved2;
        [FieldOffset(6)]
        public ushort wReserved3;

        [FieldOffset(8)]
        public byte bVal;
        [FieldOffset(8)]
        public sbyte cVal;
        [FieldOffset(8)]
        public ushort uiVal;
        [FieldOffset(8)]
        public short iVal;
        [FieldOffset(8)]
        public UInt32 uintVal;
        [FieldOffset(8)]
        public Int32 intVal;
        [FieldOffset(8)]
        public UInt64 ulVal;
        [FieldOffset(8)]
        public Int64 lVal;
        [FieldOffset(8)]
        public float fltVal;
        [FieldOffset(8)]
        public double dblVal;
        [FieldOffset(8)]
        public short boolVal;
        [FieldOffset(8)]
        public IntPtr pclsidVal; // GUID ID pointer
        [FieldOffset(8)]
        public IntPtr pszVal; // Ansi string pointer
        [FieldOffset(8)]
        public IntPtr pwszVal; // Unicode string pointer
        [FieldOffset(8)]
        public IntPtr punkVal; // punkVal (interface pointer)
        [FieldOffset(8)]
        public PROPARRAY ca;
        [FieldOffset(8)]
        public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PROPERTYKEY
    {
        private readonly Guid _fmtid;
        private readonly uint _pid;

        public PROPERTYKEY(Guid fmtid, uint pid)
        {
            _fmtid = fmtid;
            _pid = pid;
        }

        public static readonly PROPERTYKEY PKEY_ItemNameDisplay = new PROPERTYKEY(new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 10);
        public static readonly PROPERTYKEY PKEY_FileVersion = new PROPERTYKEY(new Guid("0CEF7D53-FA64-11D1-A203-0000F81FEDEE"), 4);
    }

    public class GlobalTools
    {
        public static void SafeRelease<T>(ref T comObject) where T : class
        {
            T t = comObject;
            comObject = default(T);
            if (null != t)
            {
                if (Marshal.IsComObject(t))
                    Marshal.ReleaseComObject(t);
            }
        }
    }



    internal class MFPlayTools
    {
        [DllImport("Mfplay.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT MFPCreateMediaPlayer(string pwszURL, bool fStartPlayback, MFP_CREATION_OPTIONS creationOptions,
            IMFPMediaPlayerCallback pCallback, IntPtr hWnd, out IMFPMediaPlayer ppMediaPlayer);

        public enum MFP_CREATION_OPTIONS
        {
            MFP_OPTION_NONE = 0,
            MFP_OPTION_FREE_THREADED_CALLBACK = 0x1,
            MFP_OPTION_NO_MMCSS = 0x2,
            MFP_OPTION_NO_REMOTE_DESKTOP_OPTIMIZATION = 0x4
        };
    }

    [ComImport]
    [Guid("A714590A-58AF-430a-85BF-44F5EC838D85")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPMediaPlayer
    {
        HRESULT Play();
        HRESULT Pause();
        HRESULT Stop();
        HRESULT FrameStep();
        HRESULT SetPosition(ref Guid guidPositionType, PROPVARIANT pvPositionValue);
        HRESULT GetPosition(ref Guid guidPositionType, out PROPVARIANT pvPositionValue);
        HRESULT GetDuration(ref Guid guidPositionType, out PROPVARIANT pvDurationValue);
        HRESULT SetRate(float flRate);
        HRESULT GetRate(out float pflRate);
        HRESULT GetSupportedRates(bool fForwardDirection, out float pflSlowestRate, out float pflFastestRate);
        HRESULT GetState(out MFP_MEDIAPLAYER_STATE peState);
        HRESULT CreateMediaItemFromURL(string pwszURL, bool fSync, IntPtr dwUserData, out IMFPMediaItem ppMediaItem);
        HRESULT CreateMediaItemFromObject(IntPtr pIntPtrObj, bool fSync, IntPtr dwUserData, out IMFPMediaItem ppMediaItem);
        HRESULT SetMediaItem(IMFPMediaItem pIMFPMediaItem);
        HRESULT ClearMediaItem();
        HRESULT GetMediaItem(out IMFPMediaItem ppIMFPMediaItem);
        HRESULT GetVolume(out float pflVolume);
        HRESULT SetVolume(float flVolume);
        HRESULT GetBalance(out float pflBalance);
        HRESULT SetBalance(float flBalance);
        HRESULT GetMute(out bool pfMute);
        HRESULT SetMute(bool fMute);
        [PreserveSig]
        HRESULT GetNativeVideoSize(out SIZE pszVideo, out SIZE pszARVideo);
        [PreserveSig]
        HRESULT GetIdealVideoSize(out SIZE pszMin, out SIZE pszMax);
        HRESULT SetVideoSourceRect(MFVideoNormalizedRect pnrcSource);
        HRESULT GetVideoSourceRect(out MFVideoNormalizedRect pnrcSource);
        HRESULT SetAspectRatioMode(uint dwAspectRatioMode);
        HRESULT GetAspectRatioMode(out uint pdwAspectRatioMode);
        HRESULT GetVideoWindow(out IntPtr pIntPtrVideo);
        HRESULT UpdateVideo();
        HRESULT SetBorderColor(uint Clr);
        HRESULT GetBorderColor(out uint pClr);
        HRESULT InsertEffect(IntPtr pEffect, bool fOptional);
        HRESULT RemoveEffect(IntPtr pEffect);
        HRESULT RemoveAllEffects();
        HRESULT Shutdown();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MFVideoNormalizedRect
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
        public MFVideoNormalizedRect(float Left, float Top, float Right, float Bottom)
        {
            left = Left;
            top = Top;
            right = Right;
            bottom = Bottom;
        }
    }

    public enum MFP_MEDIAPLAYER_STATE
    {
        MFP_MEDIAPLAYER_STATE_EMPTY = 0,
        MFP_MEDIAPLAYER_STATE_STOPPED = 0x1,
        MFP_MEDIAPLAYER_STATE_PLAYING = 0x2,
        MFP_MEDIAPLAYER_STATE_PAUSED = 0x3,
        MFP_MEDIAPLAYER_STATE_SHUTDOWN = 0x4
    }

    [ComImport]
    [Guid("90EB3E6B-ECBF-45cc-B1DA-C6FE3EA70D57")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPMediaItem
    {
        HRESULT GetMediaPlayer(out IMFPMediaPlayer ppMediaPlayer);
        HRESULT GetURL(out string ppwszURL);
        HRESULT GetObject(out IntPtr ppIUnknown);
        HRESULT GetUserData(out IntPtr pdwUserData);
        HRESULT SetUserData(IntPtr dwUserData);
        HRESULT GetStartStopPosition(out Guid pGuidStartPositionType, out PROPVARIANT pvStartValue, out Guid pGuidStopPositionType, out PROPVARIANT pvStopValue);
        HRESULT SetStartStopPosition(ref Guid pGuidStartPositionType, PROPVARIANT pvStartValue, ref Guid pGuidStopPositionType, PROPVARIANT pvStopValue);
        HRESULT HasVideo(out bool pfHasVideo, out bool pfSelected);
        HRESULT HasAudio(out bool fHasAudio, out bool pfSelected);
        HRESULT IsProtected(out bool pfProtected);
        HRESULT GetDuration(ref Guid GuidPositionType, out PROPVARIANT pvDurationValue);
        HRESULT GetNumberOfStreams(out uint pdwStreamCount);
        HRESULT GetStreamSelection(uint dwStreamIndex, out bool pfEnabled);
        HRESULT SetStreamSelection(uint dwStreamIndex, bool fEnabled);
        HRESULT GetStreamAttribute(uint dwStreamIndex, ref Guid GuidMFAttribute, out PROPVARIANT pvValue);
        HRESULT GetPresentationAttribute(ref Guid GuidMFAttribute, out PROPVARIANT pvValue);
        HRESULT GetCharacteristics(out MFP_MEDIAITEM_CHARACTERISTICS pCharacteristics);
        HRESULT SetStreamSink(uint dwStreamIndex, IntPtr pMediaSink);
        HRESULT GetMetadata(out IPropertyStore ppMetadataStore);
    }

    public enum MFP_MEDIAITEM_CHARACTERISTICS
    {
        MFP_MEDIAITEM_IS_LIVE = 0x1,
        MFP_MEDIAITEM_CAN_SEEK = 0x2,
        MFP_MEDIAITEM_CAN_PAUSE = 0x4,
        MFP_MEDIAITEM_HAS_SLOW_SEEK = 0x8
    }

    [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStore
    {
        HRESULT GetCount([Out] out uint propertyCount);
        HRESULT GetAt([In] uint propertyIndex, [Out, MarshalAs(UnmanagedType.Struct)] out PROPERTYKEY key);
        HRESULT GetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [Out, MarshalAs(UnmanagedType.Struct)] out PROPVARIANT pv);
        HRESULT SetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [In, MarshalAs(UnmanagedType.Struct)] ref PROPVARIANT pv);
        HRESULT Commit();
    }

    [ComImport, Guid("766C8FFB-5FDB-4fea-A28D-B912996F51BD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPMediaPlayerCallback
    {
        [PreserveSig]
        void OnMediaPlayerEvent(MFP_EVENT_HEADER pEventHeader);
    };

    [StructLayout(LayoutKind.Sequential)]
    public class MFP_EVENT_HEADER
    {
        public MFP_EVENT_TYPE eEventType;
        public HRESULT hrEvent;
        public IMFPMediaPlayer pMediaPlayer;
        public MFP_MEDIAPLAYER_STATE eState;
        public IPropertyStore pPropertyStore;
    }

    public enum MFP_EVENT_TYPE
    {
        MFP_EVENT_TYPE_PLAY = 0,
        MFP_EVENT_TYPE_PAUSE = 1,
        MFP_EVENT_TYPE_STOP = 2,
        MFP_EVENT_TYPE_POSITION_SET = 3,
        MFP_EVENT_TYPE_RATE_SET = 4,
        MFP_EVENT_TYPE_MEDIAITEM_CREATED = 5,
        MFP_EVENT_TYPE_MEDIAITEM_SET = 6,
        MFP_EVENT_TYPE_FRAME_STEP = 7,
        MFP_EVENT_TYPE_MEDIAITEM_CLEARED = 8,
        MFP_EVENT_TYPE_MF = 9,
        MFP_EVENT_TYPE_ERROR = 10,
        MFP_EVENT_TYPE_PLAYBACK_ENDED = 11,
        MFP_EVENT_TYPE_ACQUIRE_USER_CREDENTIAL = 12
    }

    public class SplashScreen
    {
        //[DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        //public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref System.Drawing.Point pptDst, ref System.Drawing.Size psize, IntPtr hdcSrc, ref System.Drawing.Point pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, IntPtr pptDst, IntPtr psize, IntPtr hdcSrc, IntPtr pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        public const byte AC_SRC_OVER = 0x00;
        public const byte AC_SRC_ALPHA = 0x01;

        public const int ULW_COLORKEY = 0x00000001;
        public const int ULW_ALPHA = 0x00000002;
        public const int ULW_OPAQUE = 0x00000004;

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetObject(IntPtr hFont, int nSize, out BITMAP bm);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public short bmPlanes;
            public short bmBitsPixel;
            public IntPtr bmBits;
        }

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                left = Left;
                top = Top;
                right = Right;
                bottom = Bottom;
            }
        }

        public const long STGM_READ = 0x00000000L;
        public const long GENERIC_READ = (0x80000000L);
        public const long GENERIC_WRITE = (0x40000000L);

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern HRESULT SHCreateStreamOnFile(string pszFile, int grfMode, out System.Runtime.InteropServices.ComTypes.IStream ppstm);

        [DllImport("GdiPlus.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern GpStatus GdiplusStartup(out IntPtr token, ref StartupInput input, out StartupOutput output);

        [DllImport("GdiPlus.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GdiplusShutdown(IntPtr token);

        [StructLayout(LayoutKind.Sequential)]
        public struct StartupInput
        {
            public int GdiplusVersion;             // Must be 1

            // public DebugEventProc DebugEventCallback; // Ignored on free builds
            public IntPtr DebugEventCallback;

            public bool SuppressBackgroundThread;     // FALSE unless you're prepared to call 
                                                      // the hook/unhook functions properly

            public bool SuppressExternalCodecs;       // FALSE unless you want GDI+ only to use
                                                      // its internal image codecs.

            public static StartupInput GetDefault()
            {
                StartupInput result = new StartupInput();
                result.GdiplusVersion = 1;
                // result.DebugEventCallback = null;
                result.SuppressBackgroundThread = false;
                result.SuppressExternalCodecs = false;
                return result;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StartupOutput
        {
            // The following 2 fields won't be used.  They were originally intended 
            // for getting GDI+ to run on our thread - however there are marshalling
            // dealing with function *'s and what not - so we make explicit calls
            // to gdi+ after the fact, via the GdiplusNotificationHook and 
            // GdiplusNotificationUnhook methods.
            public IntPtr hook;//not used
            public IntPtr unhook;//not used.
        }

        [DllImport("GdiPlus.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern GpStatus GdipCreateBitmapFromFile(string filename, out IntPtr bitmap);

        [DllImport("GdiPlus.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern GpStatus GdipCreateBitmapFromStream(System.Runtime.InteropServices.ComTypes.IStream Stream, out IntPtr bitmap);

        [DllImport("GdiPlus.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern GpStatus GdipCreateHBITMAPFromBitmap(HandleRef nativeBitmap, out IntPtr hbitmap, int argbBackground);

        public enum GpStatus : int
        {
            Ok = 0,
            GenericError = 1,
            InvalidParameter = 2,
            OutOfMemory = 3,
            ObjectBusy = 4,
            InsufficientBuffer = 5,
            NotImplemented = 6,
            Win32Error = 7,
            WrongState = 8,
            Aborted = 9,
            FileNotFound = 10,
            ValueOverflow = 11,
            AccessDenied = 12,
            UnknownImageFormat = 13,
            FontFamilyNotFound = 14,
            FontStyleNotFound = 15,
            NotTrueTypeFont = 16,
            UnsupportedGdiplusVersion = 17,
            GdiplusNotInitialized = 18,
            PropertyNotFound = 19,
            PropertyNotSupported = 20,
            ProfileNotFound = 21,
        }

        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        public const int WS_OVERLAPPED = 0x00000000,
            WS_POPUP = unchecked((int)0x80000000),
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_CAPTION = 0x00C00000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_TABSTOP = 0x00010000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_OVERLAPPEDWINDOW = 0xcf0000;

        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_NOPARENTNOTIFY = 0x00000004;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_ACCEPTFILES = 0x00000010;
        public const int WS_EX_TRANSPARENT = 0x00000020;

        public const int WS_EX_MDICHILD = 0x00000040;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_WINDOWEDGE = 0x00000100;
        public const int WS_EX_CLIENTEDGE = 0x00000200;
        public const int WS_EX_CONTEXTHELP = 0x00000400;

        public const int WS_EX_RIGHT = 0x00001000;
        public const int WS_EX_LEFT = 0x00000000;
        public const int WS_EX_RTLREADING = 0x00002000;
        public const int WS_EX_LTRREADING = 0x00000000;
        public const int WS_EX_LEFTSCROLLBAR = 0x00004000;
        public const int WS_EX_RIGHTSCROLLBAR = 0x00000000;

        public const int WS_EX_CONTROLPARENT = 0x00010000;
        public const int WS_EX_STATICEDGE = 0x00020000;
        public const int WS_EX_APPWINDOW = 0x00040000;

        public const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public const int WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);

        public const int WS_EX_LAYERED = 0x00080000;

        public const int WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children

        public const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

        public const int WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring

        public const int WS_EX_COMPOSITED = 0x02000000;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        public const int CS_USEDEFAULT = unchecked((int)0x80000000);
        public const int CS_DBLCLKS = 8;
        public const int CS_VREDRAW = 1;
        public const int CS_HREDRAW = 2;
        public const int COLOR_BACKGROUND = 1;
        public const int COLOR_WINDOW = 5;
        public const int IDC_ARROW = 32512;
        public const int IDC_IBEAM = 32513;
        public const int IDC_WAIT = 32514;
        public const int IDC_CROSS = 32515;
        public const int IDC_UPARROW = 32516;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int DefWindowProc(IntPtr hWnd, uint uMsg, int wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern short RegisterClass(ref WNDCLASS wc);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern short RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        public delegate int WNDPROC(IntPtr hwnd, uint uMsg, int wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASS
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint style;
            public WNDPROC lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public WNDPROC lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        const int GWL_STYLE = (-16);
        const int GWL_EXSTYLE = (-20);
        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // public static IntPtr GetWindowLong(HandleRef hWnd, int nIndex)
        public static long GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("User32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern long GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern long GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        public const int SPI_GETWORKAREA = 0x30;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, [In, Out] ref RECT pvParam, uint fWinIni);

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
        public const int SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */
        public const int SWP_DRAWFRAME = SWP_FRAMECHANGED;
        public const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        private DispatcherTimer dTimer;
        private TimeSpan tsFadeoutDuration;
        private DateTime tsFadeoutEnd;
        IntPtr hWndSplash = IntPtr.Zero;
        private WNDPROC delegateWndProc;
        private IntPtr hBitmap = IntPtr.Zero;
        private IntPtr initToken = IntPtr.Zero;

        public void Initialize()
        {
            StartupInput input = StartupInput.GetDefault();
            StartupOutput output;
            GpStatus nStatus = GdiplusStartup(out initToken, ref input, out output);
        }

        public void DisplaySplash(IntPtr hWnd, IntPtr hBitmap, string sVideo)
        {
            this.hBitmap = hBitmap;
            delegateWndProc = Win32WndProc;
            WNDCLASSEX wcex = new WNDCLASSEX();
            wcex.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            wcex.style = (CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS);
            wcex.hbrBackground = (IntPtr)COLOR_BACKGROUND + 1;
            wcex.cbClsExtra = 0;
            wcex.cbWndExtra = 0;
            wcex.hInstance = Marshal.GetHINSTANCE(this.GetType().Module); // Process.GetCurrentProcess().Handle;
            wcex.hIcon = IntPtr.Zero;
            wcex.hCursor = LoadCursor(IntPtr.Zero, (int)IDC_ARROW);
            wcex.lpszMenuName = null;
            wcex.lpszClassName = "Win32Class";
            //wind_class.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(delegWndProc);
            wcex.lpfnWndProc = delegateWndProc;
            wcex.hIconSm = IntPtr.Zero;
            short nRet = RegisterClassEx(ref wcex);
            if (nRet == 0)
            {
                int nError = Marshal.GetLastWin32Error();
                if (nError != 1410) //0x582 ERROR_CLASS_ALREADY_EXISTS
                    return;
            }
            string sClassName = wcex.lpszClassName;
            int nWidth = 0, nHeight = 0;
            if (hBitmap != IntPtr.Zero)
            {
                BITMAP bm;
                GetObject(hBitmap, Marshal.SizeOf(typeof(BITMAP)), out bm);
                nWidth = bm.bmWidth;
                nHeight = bm.bmHeight;
            }
            //hWndSplash = CreateWindowEx(WS_EX_TOOLWINDOW | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_NOACTIVATE, sClassName, "Win32 window", WS_POPUP | WS_VISIBLE, 400, 400, nWidth, nHeight, hWnd, IntPtr.Zero, wcex.hInstance, IntPtr.Zero);
            hWndSplash = CreateWindowEx(WS_EX_TOOLWINDOW | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST, sClassName, "Win32 window", WS_POPUP | WS_VISIBLE, 400, 400, nWidth, nHeight, hWnd, IntPtr.Zero, wcex.hInstance, IntPtr.Zero);
            if (hBitmap != IntPtr.Zero)
            {
                SetPictureToLayeredWindow(hWndSplash, hBitmap);
                CenterToScreen(hWndSplash);
            }
            if (sVideo != null)
            {
                MFPlayer pPlayer = new MFPlayer(this, hWndSplash, sVideo);
            }
        }

        public class MFPlayer : IMFPMediaPlayerCallback
        {
            public MFPlayer(SplashScreen ss, IntPtr hWnd, string sVideo)
            {
                HRESULT hr = MFPCreateMediaPlayer(sVideo, false, MFP_CREATION_OPTIONS.MFP_OPTION_NONE, this, hWnd, out m_pMediaPlayer);
                m_hWndParent = hWnd;
                m_ss = ss;
            }

            SplashScreen m_ss = null;
            IntPtr m_hWndParent = IntPtr.Zero;
            IMFPMediaPlayer m_pMediaPlayer;

            public void OnMediaPlayerEvent(MFP_EVENT_HEADER pEventHeader)
            {
                switch (pEventHeader.eEventType)
                {
                    case MFP_EVENT_TYPE.MFP_EVENT_TYPE_MEDIAITEM_CREATED:
                        break;

                    case MFP_EVENT_TYPE.MFP_EVENT_TYPE_MEDIAITEM_SET:
                        {
                            SIZE szVideo, szARVideo;
                            HRESULT hr = m_pMediaPlayer.GetNativeVideoSize(out szVideo, out szARVideo);
                            if (hr == HRESULT.S_OK)
                            {
                                SetWindowPos(m_hWndParent, IntPtr.Zero, 0, 0, szVideo.cx, szVideo.cy, SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);
                                m_ss.CenterToScreen(m_hWndParent);
                                hr = m_pMediaPlayer.Play();
                            }
                        }
                        break;
                }
                return;
            }
        }

        public void HideSplash(int nSeconds)
        {
            dTimer = new DispatcherTimer();
            dTimer.Interval = TimeSpan.FromMilliseconds(60);
            tsFadeoutDuration = TimeSpan.FromSeconds(nSeconds);
            tsFadeoutEnd = DateTime.UtcNow + tsFadeoutDuration;
            dTimer.Tick += Dt_Tick;
            dTimer.Start();
        }

        public async System.Threading.Tasks.Task<IntPtr> GetBitmap(string sBitmapFile)
        {
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr pBitmap = IntPtr.Zero;
            // Uri uri = new System.Uri("ms-appx:///Assets/Butterfly.png");
            //            Some APIs require package identity and are not supported in unpackaged apps, such as:
            //            ApplicationData
            //            StorageFile.GetFileFromApplicationUriAsync
            // var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            string sDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sPath = sDirectory + sBitmapFile;

            //var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(sPath);
            //Stream stream = await storageFile.OpenStreamForReadAsync();
            // new GPStream(stream)

            System.Runtime.InteropServices.ComTypes.IStream pstm;
            HRESULT hr = SHCreateStreamOnFile(sPath, (int)STGM_READ, out pstm);
            if (hr == HRESULT.S_OK)
            {
                GpStatus nStatus = GdipCreateBitmapFromStream(pstm, out pBitmap);
                if (nStatus == GpStatus.Ok)
                {
                    GdipCreateHBITMAPFromBitmap(new HandleRef(this, pBitmap), out hBitmap, System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(0)));
                }
            }
            return (hBitmap);
        }

        // Adapted from WPF source code
        public void Dt_Tick(object sender, object e)
        {
            DateTime dtNow = DateTime.UtcNow;
            if (dtNow >= tsFadeoutEnd)
            {
                if (dTimer != null)
                {
                    dTimer.Stop();
                    dTimer = null;
                }
                if (hWndSplash != IntPtr.Zero)
                {
                    DestroyWindow(hWndSplash);
                }
                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                    hBitmap = IntPtr.Zero;
                }
                GdiplusShutdown(initToken);
            }
            else
            {
                double nProgress = (tsFadeoutEnd - dtNow).TotalMilliseconds / tsFadeoutDuration.TotalMilliseconds;
                BLENDFUNCTION bf = new BLENDFUNCTION();
                bf.BlendOp = AC_SRC_OVER;
                bf.AlphaFormat = AC_SRC_ALPHA;
                bf.SourceConstantAlpha = (byte)(255 * nProgress);
                UpdateLayeredWindow(hWndSplash, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref bf, ULW_ALPHA);
            }
        }

        private void SetPictureToLayeredWindow(IntPtr hWnd, IntPtr hBitmap)
        {
            BITMAP bm;
            GetObject(hBitmap, Marshal.SizeOf(typeof(BITMAP)), out bm);
            System.Drawing.Size sizeBitmap = new System.Drawing.Size(bm.bmWidth, bm.bmHeight);

            IntPtr hDCScreen = GetDC(IntPtr.Zero);
            IntPtr hDCMem = CreateCompatibleDC(hDCScreen);
            IntPtr hBitmapOld = SelectObject(hDCMem, hBitmap);

            BLENDFUNCTION bf = new BLENDFUNCTION();
            bf.BlendOp = AC_SRC_OVER;
            bf.SourceConstantAlpha = 255;
            bf.AlphaFormat = AC_SRC_ALPHA;

            RECT rectWnd;
            GetWindowRect(hWnd, out rectWnd);

            System.Drawing.Point ptSrc = new System.Drawing.Point();
            System.Drawing.Point ptDest = new System.Drawing.Point(rectWnd.left, rectWnd.top);

            IntPtr pptSrc = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Drawing.Point)));
            Marshal.StructureToPtr(ptSrc, pptSrc, false);

            IntPtr pptDest = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Drawing.Point)));
            Marshal.StructureToPtr(ptDest, pptDest, false);

            IntPtr psizeBitmap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Drawing.Size)));
            Marshal.StructureToPtr(sizeBitmap, psizeBitmap, false);

            bool bRet = UpdateLayeredWindow(hWnd, hDCScreen, pptDest, psizeBitmap, hDCMem, pptSrc, 0, ref bf, ULW_ALPHA);
            //bool bRet = UpdateLayeredWindow(hWnd, hDCScreen, ref ptDest, ref sizeBitmap, hDCMem, ref ptSrc, ColorTranslator.ToWin32(System.Drawing.Color.White), ref bf, ULW_ALPHA);

            Marshal.FreeHGlobal(pptSrc);
            Marshal.FreeHGlobal(pptDest);
            Marshal.FreeHGlobal(psizeBitmap);

            SelectObject(hDCMem, hBitmapOld);
            DeleteDC(hDCMem);
            ReleaseDC(IntPtr.Zero, hDCScreen);
        }

        public void CenterToScreen(IntPtr hWnd)
        {
            RECT rcWorkArea = new RECT();
            SystemParametersInfo(SPI_GETWORKAREA, 0, ref rcWorkArea, 0);
            RECT rc;
            GetWindowRect(hWnd, out rc);
            int nX = System.Convert.ToInt32((rcWorkArea.left + rcWorkArea.right) / (double)2 - (rc.right - rc.left) / (double)2);
            int nY = System.Convert.ToInt32((rcWorkArea.top + rcWorkArea.bottom) / (double)2 - (rc.bottom - rc.top) / (double)2);
            SetWindowPos(hWnd, IntPtr.Zero, nX, nY, -1, -1, SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOACTIVATE);
        }

        private int Win32WndProc(IntPtr hwnd, uint msg, int wParam, IntPtr lParam)
        {
            //int wmId, wmEvent;
            switch (msg)
            {
                default:
                    break;
            }
            return DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }
}
