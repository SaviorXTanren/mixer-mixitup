using MixItUp.Base.Model;

namespace MixItUp.Base.Services.Twitch
{
    public class TwitchStatusService : StatusPageStreamingPlatformStatusService
    {
        public TwitchStatusService() : base(StreamingPlatformTypeEnum.Twitch, "https://status.twitch.tv/api/v2/incidents/unresolved.json") { }
    }
}
