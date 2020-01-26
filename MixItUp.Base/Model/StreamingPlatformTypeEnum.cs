using System;

namespace MixItUp.Base.Model
{
    [Flags]
    public enum StreamingPlatformTypeEnum
    {
        None = 0,

        Mixer = 1,
        Twitch = 2,
        YouTube = 4,

        All = 1073741824,
    }
}