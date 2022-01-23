using MixItUp.Base.Commands;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Bits;
using Twitch.Base.Models.NewAPI.Subscriptions;
using Twitch.Base.Services.NewAPI;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayLeaderboardListItemTypeEnum
    {
        Subscribers,
        Donations,
        CurrencyRank,
        [Obsolete]
        Sparks,
        [Obsolete]
        Embers,
        Bits,
    }

    [Obsolete]
    public enum OverlayLeaderboardListItemDateRangeEnum
    {
        Weekly,
        Monthly,
        Yearly,
        AllTime,
        Daily,
    }

    [DataContract]
    public class OverlayLeaderboardListItemModel : OverlayListItemModelBase
    {
        private class OverlayLeaderboardItem
        {
            public string ID { get; set; }

            public UserViewModel User { get; set; }

            public string Hash { get; set; }

            public OverlayLeaderboardItem(UserViewModel user, string hash)
                : this(user.FullDisplayName, hash)
            {
                this.User = user;
            }

            public OverlayLeaderboardItem(string id, string hash)
            {
                this.ID = id;
                this.Hash = hash;
            }
        }

        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{USERNAME}</p>
          <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-family: '{TEXT_FONT}'; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{DETAILS}</p>
        </div>";

        [DataMember]
        public OverlayLeaderboardListItemTypeEnum LeaderboardType { get; set; }

        [DataMember]
        public BitsLeaderboardPeriodEnum BitsLeaderboardDateRange { get; set; }

        [DataMember]
        public Guid CurrencyID { get; set; }

        [Obsolete]
        [DataMember]
        public CustomCommand NewLeaderCommand { get; set; }

        [DataMember]
        public Guid LeaderChangedCommandID { get; set; }

        [JsonIgnore]
        public CommandModelBase LeaderChangedCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.LeaderChangedCommandID); }
            set
            {
                if (value != null)
                {
                    this.LeaderChangedCommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.LeaderChangedCommandID);
                    this.LeaderChangedCommandID = Guid.Empty;
                }
            }
        }

        [DataMember]
        private List<OverlayListIndividualItemModel> lastItems { get; set; } = new List<OverlayListIndividualItemModel>();

        [JsonIgnore]
        private Dictionary<Guid, DateTimeOffset> userSubDates = new Dictionary<Guid, DateTimeOffset>();
        [JsonIgnore]
        private Dictionary<Guid, UserDonationModel> userDonations = new Dictionary<Guid, UserDonationModel>();

        private DateTimeOffset lastQuery = DateTimeOffset.MinValue;

        public OverlayLeaderboardListItemModel() : base() { }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation,
            OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, CurrencyModel currency, CommandModelBase leaderChangedCommand)
            : this(htmlText, leaderboardType, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation, leaderChangedCommand)
        {
            this.CurrencyID = currency.ID;
        }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation,
            OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, BitsLeaderboardPeriodEnum dateRange, CommandModelBase leaderChangedCommand)
            : this(htmlText, leaderboardType, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation, leaderChangedCommand)
        {
            this.BitsLeaderboardDateRange = dateRange;
        }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, CommandModelBase leaderChangedCommand)
            : base(OverlayItemModelTypeEnum.Leaderboard, htmlText, totalToShow, 0, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation)
        {
            this.LeaderboardType = leaderboardType;
            this.LeaderChangedCommand = leaderChangedCommand;
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating
        {
            get
            {
                return this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank || this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Bits;
            }
        }

        public override Task LoadTestData()
        {
            UserViewModel user = ChannelSession.GetCurrentUser();
            return Task.FromResult(0);
        }

        public override async Task Enable()
        {
            if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Subscribers)
            {
                this.userSubDates.Clear();
                IEnumerable<SubscriptionModel> subscribers = await ChannelSession.TwitchUserConnection.GetSubscriptions(ChannelSession.TwitchUserNewAPI, int.MaxValue);

                foreach (SubscriptionModel subscriber in subscribers)
                {
                    UserViewModel user = await UserViewModel.Create(subscriber);
                }

                await this.UpdateSubscribers();

                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Donations)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            this.lastItems.Clear();

            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;

            await base.Disable();
        }

        public override async Task<JObject> GetProcessedItem(CommandParametersModel parameters)
        {
            List<OverlayLeaderboardItem> items = new List<OverlayLeaderboardItem>();
            if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currency.ContainsKey(this.CurrencyID))
                {
                    CurrencyModel currency = ChannelSession.Settings.Currency[this.CurrencyID];
                    IEnumerable<UserDataModel> userDataList = await SpecialIdentifierStringBuilder.GetUserOrderedCurrencyList(currency);
                    for (int i = 0; i < userDataList.Count() && items.Count < this.TotalToShow; i++)
                    {
                        UserDataModel userData = userDataList.ElementAt(i);
                        if (!userData.IsCurrencyRankExempt)
                        {
                            items.Add(new OverlayLeaderboardItem(new UserViewModel(userData), currency.GetAmount(userData).ToString()));
                        }
                    }
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Bits && this.lastQuery.TotalMinutesFromNow() > 1)
            {
                BitsLeaderboardModel bitsLeaderboard = null;
                switch (this.BitsLeaderboardDateRange)
                {
                    case BitsLeaderboardPeriodEnum.Day:
                        bitsLeaderboard = await ChannelSession.TwitchUserConnection.GetBitsLeaderboard(BitsLeaderboardPeriodEnum.Day, this.TotalToShow);
                        break;
                    case BitsLeaderboardPeriodEnum.Week:
                        bitsLeaderboard = await ChannelSession.TwitchUserConnection.GetBitsLeaderboard(BitsLeaderboardPeriodEnum.Week, this.TotalToShow);
                        break;
                    case BitsLeaderboardPeriodEnum.Month:
                        bitsLeaderboard = await ChannelSession.TwitchUserConnection.GetBitsLeaderboard(BitsLeaderboardPeriodEnum.Month, this.TotalToShow);
                        break;
                    case BitsLeaderboardPeriodEnum.Year:
                        bitsLeaderboard = await ChannelSession.TwitchUserConnection.GetBitsLeaderboard(BitsLeaderboardPeriodEnum.Year, this.TotalToShow);
                        break;
                    case BitsLeaderboardPeriodEnum.All:
                        bitsLeaderboard = await ChannelSession.TwitchUserConnection.GetBitsLeaderboard(BitsLeaderboardPeriodEnum.All, this.TotalToShow);
                        break;
                }
                this.lastQuery = DateTimeOffset.Now;

                if (bitsLeaderboard != null && bitsLeaderboard.users != null)
                {
                    foreach (BitsLeaderboardUserModel bitsUser in bitsLeaderboard.users.OrderBy(u => u.rank).Take(this.TotalToShow))
                    {
                        items.Add(new OverlayLeaderboardItem(bitsUser.user_name, bitsUser.score.ToString()));
                    }
                }
            }

            if (items.Count > 0)
            {
                await this.ProcessLeaderboardItems(items);
            }

            return await base.GetProcessedItem(parameters);
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            userSubDates[user.ID] = DateTimeOffset.Now;
            await this.UpdateSubscribers();
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            await this.UpdateSubscribers();
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            await this.UpdateSubscribers();
        }

        private async Task UpdateSubscribers()
        {
            List<OverlayLeaderboardItem> items = new List<OverlayLeaderboardItem>();

            var orderedUsers = this.userSubDates.OrderByDescending(kvp => kvp.Value.TotalDaysFromNow());
            for (int i = 0; i < orderedUsers.Count() && items.Count() < this.TotalToShow; i++)
            {
                var kvp = orderedUsers.ElementAt(i);
                UserDataModel userData = await ChannelSession.Settings.GetUserDataByID(kvp.Key);
                if (userData != null)
                {
                    items.Add(new OverlayLeaderboardItem(new UserViewModel(userData), kvp.Value.GetAge()));
                }
            }

            await this.ProcessLeaderboardItems(items);
            this.SendUpdateRequired();
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            UserViewModel user = donation.User;
            if (user != null)
            {
                if (!this.userDonations.ContainsKey(user.ID))
                {
                    this.userDonations[user.ID] = donation.Copy();
                    this.userDonations[user.ID].Amount = 0.0;
                }
                this.userDonations[user.ID].Amount += donation.Amount;

                List<OverlayLeaderboardItem> items = new List<OverlayLeaderboardItem>();

                var orderedUsers = this.userDonations.OrderByDescending(kvp => kvp.Value.Amount);
                for (int i = 0; i < orderedUsers.Count() && items.Count() < this.TotalToShow; i++)
                {
                    var kvp = orderedUsers.ElementAt(i);
                    UserDataModel userData = await ChannelSession.Settings.GetUserDataByID(kvp.Key);
                    if (userData != null)
                    {
                        items.Add(new OverlayLeaderboardItem(new UserViewModel(userData), kvp.Value.AmountText));
                    }
                }

                await this.ProcessLeaderboardItems(items);
                this.SendUpdateRequired();
            }
        }

        private async Task ProcessLeaderboardItems(List<OverlayLeaderboardItem> items)
        {
            await this.listSemaphore.WaitAndRelease((Func<Task>)(async () =>
            {
                this.Items.Clear();

                List<OverlayListIndividualItemModel> updatedList = new List<OverlayListIndividualItemModel>();

                for (int i = 0; i < this.lastItems.Count(); i++)
                {
                    if (!items.Any(x => string.Equals(x.ID, this.lastItems[i].ID)))
                    {
                        this.Items.Add(OverlayListIndividualItemModel.CreateRemoveItem(this.lastItems[i].ID));
                    }
                }

                for (int i = 0; i < items.Count() && i < this.TotalToShow; i++)
                {
                    OverlayListIndividualItemModel newItem = OverlayListIndividualItemModel.CreateAddItem(items[i].ID, items[i].User, i + 1, this.HTML);
                    newItem.Hash = items[i].Hash;
                    newItem.TemplateReplacements.Add("USERNAME", newItem.ID);
                    newItem.TemplateReplacements.Add("DETAILS", newItem.Hash);
                    newItem.TemplateReplacements.Add("TOP_TEXT_HEIGHT", ((int)(0.4 * ((double)this.Height))).ToString());
                    newItem.TemplateReplacements.Add("BOTTOM_TEXT_HEIGHT", ((int)(0.2 * ((double)this.Height))).ToString());

                    updatedList.Add(newItem);
                    this.Items.Add(newItem);
                }

                if (this.LeaderChangedCommand != null)
                {
                    // Detect if we had a list before, and we have a list now, and the top user changed, let's trigger the event
                    if (this.lastItems.Count() > 0 && updatedList.Count() > 0)
                    {
                        UserViewModel previous = await this.lastItems.First().GetUser();
                        UserViewModel current = await updatedList .First().GetUser();
                        if (previous != null && current != null && !previous.ID.Equals(current.ID))
                        {
                            await ChannelSession.Services.Command.Queue(this.LeaderChangedCommand, new CommandParametersModel(current, new string[] { previous.Username }) { TargetUser = previous });
                        }
                    }
                }

                this.lastItems = new List<OverlayListIndividualItemModel>(updatedList);
            }));
        }

        private bool ShouldIncludeUser(UserViewModel user)
        {
            if (user == null)
            {
                return false;
            }

            if (user.ID.Equals(ChannelSession.GetCurrentUser()?.ID))
            {
                return false;
            }

            if (ChannelSession.TwitchBotConnection != null && string.Equals(user.TwitchID, ChannelSession.TwitchBotNewAPI?.id))
            {
                return false;
            }

            return true;
        }
    }
}