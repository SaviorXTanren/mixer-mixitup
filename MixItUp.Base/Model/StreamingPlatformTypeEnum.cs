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

        All = 2147483647,
    }

    public static class StreamingPlatforms
    {
        public static IEnumerable<StreamingPlatformTypeEnum> Platforms { get; private set; } = new List<StreamingPlatformTypeEnum>()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            StreamingPlatformTypeEnum.Mixer, StreamingPlatformTypeEnum.Twitch, StreamingPlatformTypeEnum.YouTube
#pragma warning restore CS0612 // Type or member is obsolete
        };
    }
}