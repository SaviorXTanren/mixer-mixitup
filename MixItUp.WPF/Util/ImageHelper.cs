using CacheManager.Core;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Util
{
    public static class ImageHelper
    {
        private static ICacheManager<WriteableBitmap> bitmapCache = CacheFactory.Build<WriteableBitmap>(settings => settings
                                                                                                        .WithSystemRuntimeCacheHandle()
                                                                                                        .WithExpiration(ExpirationMode.Sliding, TimeSpan.FromMinutes(30)));

        public static void SetImageSource(Image image, string path, double width, double height, string tooltip = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && path.Length > 0)
                {
                    WriteableBitmap writeableBitmap = ImageHelper.bitmapCache.Get(path);
                    if (writeableBitmap != null)
                    {
                        ImageHelper.SetImageSource(image, width, height, tooltip, writeableBitmap);
                        return;
                    }

                    if (path.StartsWith("http"))
                    {
                        Task.Run(async () =>
                        {
                            byte[] bytes = null;
                            using (AdvancedHttpClient client = new AdvancedHttpClient())
                            {
                                bytes = await client.GetByteArrayAsync(path);
                            }
                            await Application.Current.Dispatcher.InvokeAsync(() => ImageHelper.AddImageToCacheAndSetImageSourceFromBytes(image, path, width, height, tooltip, bytes));
                        });
                    }
                    else if (ServiceManager.Get<IFileService>().FileExists(path))
                    {
                        Task.Run(async () =>
                        {
                            byte[] bytes = await ServiceManager.Get<IFileService>().ReadFileAsBytes(path);
                            await Application.Current.Dispatcher.InvokeAsync(() => ImageHelper.AddImageToCacheAndSetImageSourceFromBytes(image, path, width, height, tooltip, bytes));
                        });
                    }
                    else
                    {
                        ImageHelper.AddImageToCacheAndSetImageSource(image, path, width, height, tooltip, BitmapFactory.FromResource(path));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(path + " - " + ex);
            }
        }

        private static void AddImageToCacheAndSetImageSourceFromBytes(Image image, string id, double width, double height, string tooltip, byte[] bytes)
        {
            try
            {
                if (bytes != null && bytes.Length > 0)
                {
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.DecodePixelWidth = (int)width;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.CreateOptions = BitmapCreateOptions.None;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();

                        if (bitmapImage.CanFreeze)
                        {
                            bitmapImage.Freeze();
                        }

                        ImageHelper.AddImageToCacheAndSetImageSource(image, id, width, height, tooltip, new WriteableBitmap(bitmapImage));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static void AddImageToCacheAndSetImageSource(Image image, string id, double width, double height, string tooltip, WriteableBitmap writeableBitmap)
        {
            ImageHelper.bitmapCache.Put(id, writeableBitmap);
            ImageHelper.SetImageSource(image, width, height, tooltip, writeableBitmap);
        }

        private static void SetImageSource(Image image, double width, double height, string tooltip, WriteableBitmap writeableBitmap)
        {
            if (writeableBitmap != null)
            {
                image.Width = width;
                image.Height = height;
                image.Source = writeableBitmap;
                if (!string.IsNullOrEmpty(tooltip))
                {
                    image.ToolTip = tooltip;
                }
            }
        }
    }
}
