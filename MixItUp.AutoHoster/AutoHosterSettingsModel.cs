using Mixer.Base.Model.OAuth;
using System.Collections.Generic;

namespace MixItUp.AutoHoster
{
    public class AutoHosterSettingsModel
    {
        public OAuthTokenModel OAuthToken { get; set; }

        public List<ChannelHostModel> Channels { get; set; } = new List<ChannelHostModel>();
    }
}
