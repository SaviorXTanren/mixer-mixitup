using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;

namespace MixItUp.Base.Model.YouTube
{
    public class YouTubeMembershipsGiftedModel
    {
        public UserV2ViewModel Gifter { get; private set; }

        public int Amount { get; set; }

        public string Tier { get; private set; }

        public List<UserV2ViewModel> Receivers { get; private set; } = new List<UserV2ViewModel>();

        public YouTubeMembershipsGiftedModel(UserV2ViewModel user, LiveChatMembershipGiftingDetails giftingDetails)
        {
            this.Gifter = user;
            this.Amount = giftingDetails.GiftMembershipsCount.GetValueOrDefault();
            this.Tier = giftingDetails.GiftMembershipsLevelName;
        }
    }
}
