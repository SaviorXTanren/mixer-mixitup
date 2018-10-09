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
        void TrackLogin();
        void TrackCommand(CommandTypeEnum type);
        void TrackAction(ActionTypeEnum type);
        void TrackInteractiveGame(InteractiveGameModel game);
        void TrackSongRequest(SongRequestServiceTypeEnum songService);

        void Start();
        void SetUserId(string userId);
        void End();
    }
}
