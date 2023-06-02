using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services.External;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Services
{
    public interface ITelemetryService : IExternalService
    {
        void TrackException(Exception ex);
        void TrackLogin(string userID, IEnumerable<StreamingPlatformTypeEnum> platforms);
        void TrackCommand(CommandTypeEnum type, string details = null);
        void TrackAction(ActionTypeEnum type);
        void TrackService(string type);

        void SetUserID(string userID);
    }
}