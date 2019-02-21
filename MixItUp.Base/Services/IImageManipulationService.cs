using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IImageManipulationService
    {
        Task<byte[]> Resize(byte[] imageData, int width, int height);
    }
}
