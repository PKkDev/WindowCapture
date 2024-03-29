﻿using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using WindowCapture.WinApp.HelpersWin32;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.Storage;
using CaptureHelper.Extensions;
using static WindowCapture.WinApp.HelpersWin32.FileIconHelper;

namespace WindowCapture.WinApp.Extensios
{
    public static class StorageFileExtensions
    {
        private static readonly string _baseImgUri = "ms-appx:///Assets/Icons/dat.png";

        public static async Task<ImageSource> GetFileIcon(this StorageFile file, IconSizeEnum iconSize = IconSizeEnum.MediumIcon32)
        {
            try
            {
                var intptr = FileIconHelper.GetIconHandleFromFilePath(file.Path, iconSize);
                var icon = FileIconHelper.GetBitmapFromIconHandle(intptr);

                if (icon == null)
                    return new BitmapImage(new Uri(_baseImgUri));

                var previewVector = icon.GetRGBAVector();
                IBuffer buffer = CryptographicBuffer.CreateFromByteArray(previewVector);
                SoftwareBitmap previewSB = new(BitmapPixelFormat.Rgba8, icon.Width, icon.Height);
                previewSB.CopyFromBuffer(buffer);

                SoftwareBitmapSource previewSBS = new();
                if (previewSB.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || previewSB.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                    previewSB = SoftwareBitmap.Convert(previewSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                await previewSBS.SetBitmapAsync(previewSB);

                icon.Dispose();

                return previewSBS;
            }
            catch (Exception)
            {
                return new BitmapImage(new Uri(_baseImgUri));
            }
        }
    }
}
