using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model
{
    [Flags]
    public enum StreamingPlatformTypeEnum
    {
        None = 0,

        [Obsolete]
        Mixer = 1,
        Twitch = 2,
        YouTube = 4,
        Trovo = 8,
        Glimesh = 16,

        All = 2147483647,
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
    }
}