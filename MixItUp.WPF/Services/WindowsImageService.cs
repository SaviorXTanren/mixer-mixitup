using MixItUp.Base.Services;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Services
{
    public class WindowsImageService : IImageService
    {
        public static BitmapImage Load(byte[] bytes)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            return bitmap;
        }

        public static BitmapImage LoadLocal(Uri uri)
        {
            return new BitmapImage(uri);
        }

        public string GetImageFormat(byte[] bmpBytes)
        {
            Image image = null;
            using (MemoryStream stream = new MemoryStream(bmpBytes))
            {
                image = Image.FromStream(stream);
                return new ImageFormatConverter().ConvertToString(image.RawFormat).ToLower();
            }
        }

        public async Task<byte[]> Resize(byte[] imageData, int width, int height)
        {
            return await Task.Run(() =>
            {
                using (var image = SixLabors.ImageSharp.Image.Load(imageData))
                {
                    using (var resizedImage = image.Clone(ctx => ctx.Resize(new SixLabors.ImageSharp.Size(width, height))))
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            resizedImage.Save(stream, new PngEncoder());
                            return stream.ToArray();
                        }
                    }
                }
            });
        }
    }
}
