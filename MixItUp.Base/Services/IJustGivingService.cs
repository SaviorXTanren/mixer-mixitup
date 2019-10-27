using StreamingClient.Base.Model.OAuth;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IJustGivingService
    {
        Task<bool> Connect();

        Task Disconnect();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
