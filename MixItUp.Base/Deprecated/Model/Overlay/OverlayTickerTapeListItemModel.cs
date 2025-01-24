using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public enum OverlayTickerTapeItemTypeEnum
    {
        Followers = 0,
        Hosts = 1,
        Subscribers = 2,
        Donations = 3,

        Bits = 6,
        Raids = 7,
    }

    [Obsolete]
    [DataContract]
    public class OverlayTickerTapeListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate = @"<span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; padding-left: 10px; padding-right: 10px;"">{DETAILS}</span>";

        [DataMember]
        public OverlayTickerTapeItemTypeEnum TickerTapeType { get; set; }

        [DataMember]
        public double MinimumAmountRequiredToShow { get; set; }

        [JsonIgnore]
        private HashSet<Guid> follows = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> hosts = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> raids = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> subs = new HashSet<Guid>();

        public OverlayTickerTapeListItemModel() : base() { }

        public OverlayTickerTapeListItemModel(string html, int totalToShow, OverlayTickerTapeItemTypeEnum tickerTapeType, double minimumAmountRequiredToShow, string textColor, string textFont,
            int width, int height)
            : base(OverlayItemModelTypeEnum.TickerTape, html, totalToShow, 0, textFont, width, height, string.Empty, string.Empty, textColor,
                  OverlayListItemAlignmentTypeEnum.None, OverlayItemEffectEntranceAnimationTypeEnum.None, OverlayItemEffectExitAnimationTypeEnum.None)
        {
            this.TickerTapeType = tickerTapeType;
            this.MinimumAmountRequiredToShow = minimumAmountRequiredToShow;
        }

        public override async Task LoadTestData()
        {
            for (int i = 0; i < 5; i++)
            {
                await this.AddEvent("Joe Smoe");
            }
        }

        public override async Task Enable()
        {
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Followers)
            {
                EventService.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Hosts)
            {

            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Raids)
            {
                EventService.OnRaidOccurred += GlobalEvents_OnRaidOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Subscribers)
            {
                //EventService.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                //EventService.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                //EventService.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Donations)
            {
                EventService.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Bits)
            {
                EventService.OnTwitchBitsCheeredOccurred += GlobalEvents_OnBitsOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            EventService.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            EventService.OnRaidOccurred -= GlobalEvents_OnRaidOccurred;
            //EventService.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            //EventService.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            EventService.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= GlobalEvents_OnBitsOccurred;

            await base.Disable();
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(CommandParametersModel parameters)
        {
            Dictionary<string, string> replacementSets = await base.GetTemplateReplacements(parameters);

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["TEXT_SIZE"] = this.Height.ToString();

            return replacementSets;
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.AddEvent(user.DisplayName);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, UserV2ViewModel host)
        {
            if (!this.hosts.Contains(host.ID))
            {
                this.hosts.Add(host.ID);
                await this.AddEvent(host.DisplayName);
            }
        }

        private async void GlobalEvents_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            if (!this.raids.Contains(raid.Item1.ID))
            {
                this.raids.Add(raid.Item1.ID);
                await this.AddEvent(raid.Item1.DisplayName + " x" + raid.Item2);
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                await this.AddEvent(user.DisplayName);
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                await this.AddEvent(user.Item1.DisplayName + " x" + user.Item2);
            }
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> e)
        {
            await this.AddEvent(e.Item2.DisplayName);
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            if (this.MinimumAmountRequiredToShow == 0.0 || donation.Amount >= this.MinimumAmountRequiredToShow)
            {
                await this.AddEvent(donation.Username + ": " + donation.AmountText);
            }
        }

        private async void GlobalEvents_OnBitsOccurred(object sender, TwitchBitsCheeredEventModel e)
        {
            if (this.MinimumAmountRequiredToShow == 0.0 || e.Amount >= this.MinimumAmountRequiredToShow)
            {
                await this.AddEvent(e.User.DisplayName + ": " + e.Amount);
            }
        }

        private async Task AddEvent(string details)
        {
            OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(Guid.NewGuid().ToString(), null, -1, this.HTML);
            item.TemplateReplacements.Add("DETAILS", details);

            await this.listSemaphore.WaitAsync();

            this.Items.Add(item);
            this.SendUpdateRequired();

            this.listSemaphore.Release();
        }
    }
}
