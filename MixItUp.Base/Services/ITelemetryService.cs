using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services.External;
using System;

namespace MixItUp.Base.Services
{
    public interface ITelemetryService : IExternalService
    {
        void TrackException(Exception ex);
        void TrackPageView(string pageName);
        void TrackLogin(string userID, bool isStreamer, bool isPartner);
        void TrackCommand(CommandTypeEnum type, bool isBasic);
        void TrackAction(ActionTypeEnum type);

        void TrackRemoteAuthentication(Guid clientID);
        void TrackRemoteSendProfiles(Guid clientID);
        void TrackRemoteSendBoard(Guid clientID, Guid profileID, Guid boardID);

        void SetUserID(string userID);
    }
}