using MixItUp.Base.Services;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Mock;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Model
{
    public enum StreamingPlatformTypeEnum
    {
        None = 0,

        [Obsolete]
        Mixer = 1,
        Twitch = 2,
        YouTube = 3,
        Trovo = 4,
        Glimesh = 5,
        [Obsolete]
        Facebook = 6,

        All = 99999,

        Mock = 100000,
    }

    public static class StreamingPlatforms
    {
        public const string TwitchLogoImageAssetFilePath = "/Assets/Images/Twitch.png";
        public const string YouTubeLogoImageAssetFilePath = "/Assets/Images/YouTube.png";
        public const string TrovoLogoImageAssetFilePath = "/Assets/Images/Trovo.png";
        public const string GlimeshLogoImageAssetFilePath = "/Assets/Images/Glimesh.png";

        public const string TwitchSmallLogoImageAssetFilePath = "/Assets/Images/Twitch-XS.png";
        public const string YouTubeSmallLogoImageAssetFilePath = "/Assets/Images/YouTube-XS.png";
        public const string TrovoSmallLogoImageAssetFilePath = "/Assets/Images/Trovo-XS.png";
        public const string GlimeshSmallLogoImageAssetFilePath = "/Assets/Images/Glimesh-XS.png";

        public static IEnumerable<StreamingPlatformTypeEnum> SupportedPlatforms { get; private set; } = new List<StreamingPlatformTypeEnum>()
        {
            StreamingPlatformTypeEnum.Twitch,
            StreamingPlatformTypeEnum.YouTube,
            StreamingPlatformTypeEnum.Trovo,
            StreamingPlatformTypeEnum.Glimesh
        };

        public static IEnumerable<StreamingPlatformTypeEnum> SelectablePlatforms { get; private set; } = new List<StreamingPlatformTypeEnum>()
        {
            StreamingPlatformTypeEnum.All,
            StreamingPlatformTypeEnum.Twitch,
            StreamingPlatformTypeEnum.YouTube,
            StreamingPlatformTypeEnum.Trovo,
            StreamingPlatformTypeEnum.Glimesh
        };

        public static IStreamingPlatformSessionService GetPlatformSessionService(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.Twitch) { return ServiceManager.Get<TwitchSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.YouTube) { return ServiceManager.Get<YouTubeSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.Glimesh) { return ServiceManager.Get<GlimeshSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.Trovo) { return ServiceManager.Get<TrovoSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.Mock) { return ServiceManager.Get<MockSessionService>(); }
            return null;
        }

        public static string GetPlatformImage(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.Twitch) { return TwitchLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.YouTube) { return YouTubeLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.Trovo) { return TrovoLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.Glimesh) { return GlimeshLogoImageAssetFilePath; }
            return string.Empty;
        }

        public static string GetPlatformSmallImage(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.Twitch) { return TwitchSmallLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.YouTube) { return YouTubeSmallLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.Trovo) { return TrovoSmallLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.Glimesh) { return GlimeshSmallLogoImageAssetFilePath; }
            return string.Empty;
        }

        public static void ForEachPlatform(Action<StreamingPlatformTypeEnum> action)
        {
            foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
            {
                action(platform);
            }
        }

        public static async Task ForEachPlatform(Func<StreamingPlatformTypeEnum, Task> function)
        {
            foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
            {
                await function(platform);
            }
        }
    }
}