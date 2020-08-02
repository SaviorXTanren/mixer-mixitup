using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services.External;
using System;

namespace MixItUp.Base.Services
{
    public interface ITelemetryService : IExternalService
    {
        void TrackException(Exception ex);
        void TrackPageView(string pageName);
        void TrackLogin(string userID, string userType);
        void TrackCommand(CommandTypeEnum type);
        void TrackAction(ActionTypeEnum type);

        void TrackRemoteAuthentication(Guid clientID);
        void TrackRemoteSendProfiles(Guid clientID);
        void TrackRemoteSendBoard(Guid clientID, Guid profileID, Guid boardID);

        void SetUserID(string userID);
    }
}