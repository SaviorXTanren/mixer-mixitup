using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IImageService
    {
        string GetImageFormat(byte[] bmpBytes);

        Task<byte[]> Resize(byte[] imageData, int width, int height);
    }
}
