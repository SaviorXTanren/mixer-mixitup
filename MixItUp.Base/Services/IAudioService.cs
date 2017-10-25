using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAudioService
    {
        Task Play(string filePath, int volume);
    }
}
