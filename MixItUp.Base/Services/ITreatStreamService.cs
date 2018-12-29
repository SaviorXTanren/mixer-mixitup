using Mixer.Base.Model.OAuth;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class TreatStreamEvent
    {

    }

    public interface ITreatStreamService
    {
        bool WebSocketConnectedAndAuthenticated { get; }

        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler OnWebSocketDisconnectedOccurred;

        event EventHandler<TreatStreamEvent> OnDonationOccurred;

        Task<bool> Connect();
        Task Disconnect();

        Task GetTreats();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
