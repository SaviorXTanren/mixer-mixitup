using Mixer.Base.Model.Interactive;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using System;

namespace MixItUp.Base.Services
{
    public interface ITelemetryService
    {
        void TrackException(Exception ex);
        void TrackPageView(string pageName);
        void TrackLogin(bool isStreamer, bool isPartner);
        void TrackCommand(CommandTypeEnum type, bool isBasic);
        void TrackAction(ActionTypeEnum type);
        void TrackInteractiveGame(InteractiveGameModel game);
        void TrackSongRequest(SongRequestServiceTypeEnum songService);

        void TrackRemoteAuthentication(Guid clientID);
        void TrackRemoteSendProfiles(Guid clientID);
        void TrackRemoteSendBoard(Guid clientID, Guid profileID, Guid boardID);

        void Start();
        void SetUserId(string userId);
        void End();
    }
}
