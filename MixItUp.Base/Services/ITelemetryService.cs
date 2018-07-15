using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MixItUp.Base.Services
{
    public interface ITelemetryService
    {
        void TrackException(Exception ex);
        void TrackPageView(string pageName);
        void TrackLogin();

        void Start();
        void SetUserId(string userId);
        void End();
    }
}
