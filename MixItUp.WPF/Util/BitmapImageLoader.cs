using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Util
{
    public static class BitmapImageLoader
    {
        public static BitmapImage Load(Uri uri)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            return bitmap;
        }

        public static BitmapImage Load(byte[] bytes)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            return bitmap;
        }
    }
}
