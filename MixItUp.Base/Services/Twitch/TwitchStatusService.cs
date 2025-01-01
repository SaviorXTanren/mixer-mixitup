using MixItUp.Base.Model;
using System;

namespace MixItUp.Base.Services.Twitch
{
    [Obsolete]
    public class TwitchStatusService : StatusPageStreamingPlatformStatusService
    {
        public TwitchStatusService() : base(StreamingPlatformTypeEnum.Twitch, "https://status.twitch.tv/api/v2/incidents/unresolved.json") { }
    }
}
