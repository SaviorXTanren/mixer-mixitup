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
    }

    public static class StreamingPlatforms
    {
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

        public static string GetPlatformImage(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.Twitch) { return "/Assets/Images/Twitch.png"; }
            else if (platform == StreamingPlatformTypeEnum.YouTube) { return "/Assets/Images/Youtube.png"; }
            else if (platform == StreamingPlatformTypeEnum.Trovo) { return "/Assets/Images/Trovo.png"; }
            else if (platform == StreamingPlatformTypeEnum.Glimesh) { return "/Assets/Images/Glimesh.png"; }
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