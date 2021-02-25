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
        public static IEnumerable<StreamingPlatformTypeEnum> Platforms { get; private set; } = new List<StreamingPlatformTypeEnum>()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            StreamingPlatformTypeEnum.Mixer, StreamingPlatformTypeEnum.Twitch, StreamingPlatformTypeEnum.YouTube, StreamingPlatformTypeEnum.Trovo, StreamingPlatformTypeEnum.Glimesh
#pragma warning restore CS0612 // Type or member is obsolete
        };
    }
}