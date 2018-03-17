using Mixer.Base.Model.OAuth;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IGameWispService
    {
        Task<bool> Connect();

        Task Disconnect();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
