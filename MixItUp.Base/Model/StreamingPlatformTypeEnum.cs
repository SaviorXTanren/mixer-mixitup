using MixItUp.Base.Services;
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
        [Obsolete]
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

        public const string TwitchSmallLogoImageAssetFilePath = "/Assets/Images/Twitch-XS.png";
        public const string YouTubeSmallLogoImageAssetFilePath = "/Assets/Images/YouTube-XS.png";
        public const string TrovoSmallLogoImageAssetFilePath = "/Assets/Images/Trovo-XS.png";

        public static ISet<StreamingPlatformTypeEnum> SupportedPlatforms { get; private set; } = new HashSet<StreamingPlatformTypeEnum>()
        {
            StreamingPlatformTypeEnum.Twitch,
            StreamingPlatformTypeEnum.YouTube,
            StreamingPlatformTypeEnum.Trovo,
        };

        public static ISet<StreamingPlatformTypeEnum> SelectablePlatforms { get; private set; } = new HashSet<StreamingPlatformTypeEnum>()
        {
            StreamingPlatformTypeEnum.All,
            StreamingPlatformTypeEnum.Twitch,
            StreamingPlatformTypeEnum.YouTube,
            StreamingPlatformTypeEnum.Trovo,
        };

        public static bool IsValidPlatform(StreamingPlatformTypeEnum platform) { return StreamingPlatforms.SupportedPlatforms.Contains(platform); }

        public static IStreamingPlatformSessionService GetPlatformSessionService(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.Twitch) { return ServiceManager.Get<TwitchSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.YouTube) { return ServiceManager.Get<YouTubeSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.Trovo) { return ServiceManager.Get<TrovoSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.Mock) { return ServiceManager.Get<MockSessionService>(); }
            else if (platform == StreamingPlatformTypeEnum.All && ChannelSession.Settings != null)
            {
                return StreamingPlatforms.GetPlatformSessionService(ChannelSession.Settings.DefaultStreamingPlatform);
            }
            return null;
        }

        public static string GetPlatformImage(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.Twitch) { return TwitchLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.YouTube) { return YouTubeLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.Trovo) { return TrovoLogoImageAssetFilePath; }
            return string.Empty;
        }

        public static string GetPlatformSmallImage(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.Twitch) { return TwitchSmallLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.YouTube) { return YouTubeSmallLogoImageAssetFilePath; }
            else if (platform == StreamingPlatformTypeEnum.Trovo) { return TrovoSmallLogoImageAssetFilePath; }
            return string.Empty;
        }

        public static IEnumerable<StreamingPlatformTypeEnum> GetConnectedPlatforms()
        {
            List<StreamingPlatformTypeEnum> platforms = new List<StreamingPlatformTypeEnum>();
            foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
            {
                if (StreamingPlatforms.GetPlatformSessionService(platform).IsConnected)
                {
                    platforms.Add(platform);
                }
            }
            return platforms;
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