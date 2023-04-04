using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowCapture.WinApp.MVVM.Model;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System;

namespace WindowCapture.WinApp.MVVM.ViewModel
{
    public class MediaFolderViewModel : ObservableRecipient
    {
        private StorageFolder _currentFolder;
        public StorageFolder CurrentFolder
        {
            get => _currentFolder;
            set => SetProperty(ref _currentFolder, value);
        }

        private bool _isFolderScaning;
        public bool IsFolderScaning
        {
            get => _isFolderScaning;
            set => SetProperty(ref _isFolderScaning, value);
        }

        public RelayCommand OpenCurrentFolder { get; set; }

        private readonly List<string> _acceptedExtenson = new() { ".mp4", ".mp3" };

        public ObservableCollection<MediaFileDetail> ViewFiles { get; set; }

        public MediaFileDetail SelectedMediaFileDetail { get; set; } = null;

        public StandardUICommand DeleteCommand { get; set; }
        public StandardUICommand OpenCommand { get; set; }

        public MediaFolderViewModel()
        {

            DeleteCommand = new StandardUICommand(StandardUICommandKind.Delete);
            DeleteCommand.ExecuteRequested += async (XamlUICommand sender, ExecuteRequestedEventArgs args) =>
            {
                if (args.Parameter is string displayName)
                {
                    var search = ViewFiles.FirstOrDefault(x => x.DisplayName.Equals(displayName));
                    if (search != null)
                    {
                        await search.File.DeleteAsync(StorageDeleteOption.Default);
                        await LoadFilesInFolder();
                    }
                }
            };
            OpenCommand = new StandardUICommand(StandardUICommandKind.Open);
            OpenCommand.ExecuteRequested += async (XamlUICommand sender, ExecuteRequestedEventArgs args) =>
            {
                if (args.Parameter is string displayName)
                {
                    var search = ViewFiles.FirstOrDefault(x => x.DisplayName.Equals(displayName));
                    if (search != null)
                        System.Diagnostics.Process.Start("CMD.exe", $"/C {search.File.Path}");
                }
            };

            ViewFiles = new();
            CurrentFolder = ApplicationData.Current.LocalCacheFolder;
            OpenCurrentFolder = new RelayCommand(async () =>
            {
                if (CurrentFolder != null)
                    await Launcher.LaunchFolderAsync(CurrentFolder);
            });
            Task t = Task.Run(async () => { await LoadFilesInFolder(); });
            t.Wait();
        }

        private async Task LoadFilesInFolder()
        {
            ViewFiles.Clear();
            IsFolderScaning = true;
            if (CurrentFolder == null)
            {
                IsFolderScaning = false;
                return;
            }
            CommonFileQuery query = CommonFileQuery.DefaultQuery;
            var files = await CurrentFolder.GetFilesAsync(query);

            foreach (var file in files)
                if (_acceptedExtenson.Contains(file.FileType))
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        ImageSource icon = new BitmapImage(new Uri("ms-appx:///Assets/Icons/mp4.png"));
                        ViewFiles.Add(new(file, icon, DeleteCommand, OpenCommand));
                    });
                }

            IsFolderScaning = false;
        }

        public async void ViewFilesClick(MediaFileDetail fileDetail)
        {
            SelectedMediaFileDetail = fileDetail;

            var intptr = Test.GetIconHandleFromFilePath(fileDetail.File.Path, Test.IconSizeEnum.ExtraLargeIcon);
            var bmp = Test.GetBitmapFromIconHandle(intptr);
            bmp.Save("D:\\work\\yesy.png");

            using MemoryStream ras = new MemoryStream();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(fileDetail.File.Path, ras, 1);
            var bmp2 = Bitmap.FromStream(ras);
            bmp2.Save("D:\\work\\yesy2.png");

            //System.Diagnostics.Process.Start("CMD.exe", $"/C {fileDetail.File.Path}");
        }
    }

    public static class Test
    {
        [ComImportAttribute()]
        [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IImageList
        {
            [PreserveSig]
            int Add(
                IntPtr hbmImage,
                IntPtr hbmMask,
                ref int pi);

            [PreserveSig]
            int ReplaceIcon(
                int i,
                IntPtr hicon,
                ref int pi);

            [PreserveSig]
            int SetOverlayImage(
                int iImage,
                int iOverlay);

            [PreserveSig]
            int Replace(
                int i,
                IntPtr hbmImage,
                IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(
                IntPtr hbmImage,
                int crMask,
                ref int pi);

            [PreserveSig]
            int Draw(
                ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(
                int i);

            [PreserveSig]
            int GetIcon(
                int i,
                int flags,
                ref IntPtr picon);
        };

        private struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;
            public int yBitmap;
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 254)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szTypeName;
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetFileInfo(
            string pszPath,
            int dwFileAttributes,
            ref SHFILEINFO psfi,
            int cbFileInfo,
            uint uFlags);

        private const int SHGFI_SMALLICON = 0x1;
        private const int SHGFI_LARGEICON = 0x0;
        private const int SHIL_JUMBO = 0x4;
        private const int SHIL_EXTRALARGE = 0x2;
        private const int WM_CLOSE = 0x0010;

        public enum IconSizeEnum
        {
            SmallIcon16 = SHGFI_SMALLICON,
            MediumIcon32 = SHGFI_LARGEICON,
            LargeIcon48 = SHIL_EXTRALARGE,
            ExtraLargeIcon = SHIL_JUMBO
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetImageList(
            int iImageList,
            ref Guid riid,
            out IImageList ppv);

        [DllImport("user32")]
        private static extern int DestroyIcon(
            IntPtr hIcon);

        public static System.Drawing.Bitmap GetBitmapFromIconHandle(IntPtr hIcon)
        {
            if (hIcon == IntPtr.Zero) return null;
            var myIcon = System.Drawing.Icon.FromHandle(hIcon);
            var bitmap = myIcon.ToBitmap();
            myIcon.Dispose();
            DestroyIcon(hIcon);
            SendMessage(hIcon, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            return bitmap;
        }

        [DllImport("user32")]
        private static extern
            IntPtr SendMessage(
            IntPtr handle,
            int Msg,
            IntPtr wParam,
            IntPtr lParam);

        public static IntPtr GetIconHandleFromFilePath(string filepath, IconSizeEnum iconsize)
        {
            var shinfo = new SHFILEINFO();
            const uint SHGFI_SYSICONINDEX = 0x4000;
            const int FILE_ATTRIBUTE_NORMAL = 0x80;
            uint flags = SHGFI_SYSICONINDEX;
            return GetIconHandleFromFilePathWithFlags(filepath, iconsize, ref shinfo, FILE_ATTRIBUTE_NORMAL, flags);
        }

        public static IntPtr GetIconHandleFromFilePathWithFlags(
            string filepath, IconSizeEnum iconsize,
            ref SHFILEINFO shinfo, int fileAttributeFlag, uint flags)
        {
            const int ILD_TRANSPARENT = 1;
            var retval = SHGetFileInfo(filepath, fileAttributeFlag, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (retval == 0) throw (new System.IO.FileNotFoundException());
            var iconIndex = shinfo.iIcon;
            var iImageListGuid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            IImageList iml;
            var hres = SHGetImageList((int)iconsize, ref iImageListGuid, out iml);
            var hIcon = IntPtr.Zero;
            hres = iml.GetIcon(iconIndex, ILD_TRANSPARENT, ref hIcon);
            return hIcon;
        }
    }
}
