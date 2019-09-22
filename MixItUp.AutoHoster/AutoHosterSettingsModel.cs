using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Model.OAuth;
using System.Collections.Generic;

namespace MixItUp.AutoHoster
{
    public class AutoHosterSettingsModel
    {
        public OAuthTokenModel OAuthToken { get; set; }

        public List<ChannelHostModel> Channels { get; set; } = new List<ChannelHostModel>();

        public HostingOrderEnum HostingOrder { get; set; } = HostingOrderEnum.InOrder;

        public int MaxHostLength { get; set; } = 0;

        public AgeRatingEnum AgeRating { get; set; } = AgeRatingEnum.Adult;

        public string WhisperMessage { get; set; } = "";
    }
}
