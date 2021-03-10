using YouTube.Base;

namespace MixItUp.Base.Services.YouTube
{
    public interface IYouTubePlatformService
    {

    }

    public class YouTubePlatformService : StreamingPlatformServiceBase, IYouTubePlatformService
    {
        public const string ClientID = "284178717531-kago2rk85ip02qb0vmlo8898m17s6oo8.apps.googleusercontent.com";

        public override string Name { get { return "YouTube Connection"; } }
    }
}
