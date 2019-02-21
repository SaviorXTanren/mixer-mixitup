using MixItUp.Base.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopImageManipulationService : IImageManipulationService
    {
        public async Task<byte[]> Resize(byte[] imageData, int width, int height)
        {
            return await Task.Run(() =>
            {
                using (var image = Image.Load(imageData))
                {
                    using (var resizedImage = image.Clone(ctx => ctx.Resize(new Size(width, height))))
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
