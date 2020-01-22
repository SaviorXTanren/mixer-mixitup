using System;

namespace MixItUp.Base.Model
{
    [Flags]
    public enum StreamingPlatformTypeEnum
    {
        Mixer = 1,
        Twitch = 2,
        YouTube = 4,
    }
}
