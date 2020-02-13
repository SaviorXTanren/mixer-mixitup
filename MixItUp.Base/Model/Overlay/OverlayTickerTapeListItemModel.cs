using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayTickerTapeItemTypeEnum
    {
        Followers,
        Hosts,
        Subscribers,
        Donations,
        Sparks,
        Embers,
    }

    [DataContract]
    public class OverlayTickerTapeListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate = @"<span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; padding-left: 10px; padding-right: 10px;"">{DETAILS}</span>";

        [DataMember]
        public OverlayTickerTapeItemTypeEnum TickerTapeType { get; set; }

        [DataMember]
        public double MinimumAmountRequiredToShow { get; set; }

        private HashSet<Guid> follows = new HashSet<Guid>();
        private HashSet<Guid> hosts = new HashSet<Guid>();
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
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Hosts)
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Subscribers)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Donations)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Sparks)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.TickerTapeType == OverlayTickerTapeItemTypeEnum.Embers)
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;

            await base.Disable();
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = await base.GetTemplateReplacements(user, arguments, extraSpecialIdentifiers);

            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["TEXT_SIZE"] = this.Height.ToString();

            return replacementSets;
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.AddEvent(user.Username);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.ID))
            {
                this.hosts.Add(host.Item1.ID);
                await this.AddEvent(host.Item1.Username + " x" + host.Item2);
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                await this.AddEvent(user.Username);
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                await this.AddEvent(user.Item1.Username + " x" + user.Item2);
            }
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            if (!this.subs.Contains(e.Item2.ID))
            {
                this.subs.Add(e.Item2.ID);
                await this.AddEvent(e.Item2.Username);
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            if (this.MinimumAmountRequiredToShow == 0.0 || donation.Amount >= this.MinimumAmountRequiredToShow)
            {
                await this.AddEvent(donation.Username + ": " + donation.AmountText);
            }
        }

        private async void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> sparkUsage)
        {
            if (this.MinimumAmountRequiredToShow == 0.0 || sparkUsage.Item2 >= this.MinimumAmountRequiredToShow)
            {
                await this.AddEvent(sparkUsage.Item1.Username + ": " + sparkUsage.Item2);
            }
        }

        private async void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage)
        {
            if (this.MinimumAmountRequiredToShow == 0.0 || emberUsage.Amount >= this.MinimumAmountRequiredToShow)
            {
                await this.AddEvent(emberUsage.User.Username + ": " + emberUsage.Amount);
            }
        }

        private async Task AddEvent(string details)
        {
            OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(Guid.NewGuid().ToString(), null, -1, this.HTML);
            item.TemplateReplacements.Add("DETAILS", details);

            await this.listSemaphore.WaitAndRelease(() =>
            {
                this.Items.Add(item);
                this.SendUpdateRequired();
                return Task.FromResult(0);
            });
        }
    }
}
