using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.Util
{
    public static class GlobalEvents
    {
        public static event EventHandler OnRestartRequested;
        public static void RestartRequested()
        {
            if (GlobalEvents.OnRestartRequested != null)
            {
                GlobalEvents.OnRestartRequested(null, new EventArgs());
            }
        }

        public static event EventHandler<bool> OnMainMenuStateChanged;
        public static void MainMenuStateChained(bool state)
        {
            if (GlobalEvents.OnMainMenuStateChanged != null)
            {
                GlobalEvents.OnMainMenuStateChanged(null, state);
            }
        }

        public static event EventHandler<string> OnServiceDisconnect;
        public static void ServiceDisconnect(string serviceName)
        {
            if (GlobalEvents.OnServiceDisconnect != null)
            {
                GlobalEvents.OnServiceDisconnect(null, serviceName);
            }
        }

        public static event EventHandler<string> OnServiceReconnect;
        public static void ServiceReconnect(string serviceName)
        {
            if (GlobalEvents.OnServiceReconnect != null)
            {
                GlobalEvents.OnServiceReconnect(null, serviceName);
            }
        }
    }
}
