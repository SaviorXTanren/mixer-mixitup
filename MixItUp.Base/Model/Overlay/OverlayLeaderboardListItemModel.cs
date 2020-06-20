using Mixer.Base.Model.Leaderboards;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
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

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayLeaderboardListItemTypeEnum
    {
        Subscribers,
        Donations,
        [Name("Currency/Rank")]
        CurrencyRank,
        Sparks,
        Embers,
    }

    public enum OverlayLeaderboardListItemDateRangeEnum
    {
        Weekly,
        Monthly,
        Yearly,
        [Name("All Time")]
        AllTime,
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
                : this(user.Username, hash)
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
        public OverlayLeaderboardListItemDateRangeEnum LeaderboardDateRange { get; set; }

        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public CustomCommand NewLeaderCommand { get; set; }

        [DataMember]
        private List<OverlayListIndividualItemModel> lastItems { get; set; } = new List<OverlayListIndividualItemModel>();

        private Dictionary<UserViewModel, DateTimeOffset> userSubDates = new Dictionary<UserViewModel, DateTimeOffset>();
        private Dictionary<UserViewModel, UserDonationModel> userDonations = new Dictionary<UserViewModel, UserDonationModel>();

        private DateTimeOffset lastQuery = DateTimeOffset.MinValue;

        public OverlayLeaderboardListItemModel() : base() { }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation,
            OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, CurrencyModel currency, CustomCommand newLeaderCommand)
            : this(htmlText, leaderboardType, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation, newLeaderCommand)
        {
            this.CurrencyID = currency.ID;
        }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation,
            OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, OverlayLeaderboardListItemDateRangeEnum dateRange, CustomCommand newLeaderCommand)
            : this(htmlText, leaderboardType, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation, newLeaderCommand)
        {
            this.LeaderboardDateRange = dateRange;
        }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, CustomCommand newLeaderCommand)
            : base(OverlayItemModelTypeEnum.Leaderboard, htmlText, totalToShow, 0, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation)
        {
            this.LeaderboardType = leaderboardType;
            this.NewLeaderCommand = newLeaderCommand;
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating
        {
            get
            {
                return this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank ||
                    this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks ||
                    this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Embers;
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
                await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.Subscriber, (collection) =>
                {
                    foreach (UserWithGroupsModel userWithGroups in collection)
                    {
                        DateTimeOffset? subDate = userWithGroups.GetSubscriberDate();
                        if (subDate.HasValue)
                        {
                            this.userSubDates[new UserViewModel(userWithGroups)] = subDate.GetValueOrDefault();
                        }
                    }
                    return Task.FromResult(0);
                });

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

        public override async Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            List<OverlayLeaderboardItem> items = new List<OverlayLeaderboardItem>();
            if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currency.ContainsKey(this.CurrencyID))
                {
                    CurrencyModel currency = ChannelSession.Settings.Currency[this.CurrencyID];
                    IEnumerable<UserDataModel> userDataList = SpecialIdentifierStringBuilder.GetUserOrderedCurrencyList(currency);
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
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks && this.lastQuery.TotalMinutesFromNow() > 1)
            {
                IEnumerable<SparksLeaderboardModel> sparkLeaderboard = null;
                switch (this.LeaderboardDateRange)
                {
                    case OverlayLeaderboardListItemDateRangeEnum.Weekly:
                        sparkLeaderboard = await ChannelSession.MixerUserConnection.GetWeeklySparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Monthly:
                        sparkLeaderboard = await ChannelSession.MixerUserConnection.GetMonthlySparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Yearly:
                        sparkLeaderboard = await ChannelSession.MixerUserConnection.GetYearlySparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.AllTime:
                        sparkLeaderboard = await ChannelSession.MixerUserConnection.GetAllTimeSparksLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                }
                this.lastQuery = DateTimeOffset.Now;

                if (sparkLeaderboard != null)
                {
                    for (int i = 0; i < sparkLeaderboard.Count() && items.Count() < this.TotalToShow; i++)
                    {
                        var lData = sparkLeaderboard.ElementAt(i);
                        items.Add(new OverlayLeaderboardItem(lData.username, lData.statValue.ToString()));
                    }
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Embers && this.lastQuery.TotalMinutesFromNow() > 1)
            {
                IEnumerable<EmbersLeaderboardModel> emberLeaderboard = null;
                switch (this.LeaderboardDateRange)
                {
                    case OverlayLeaderboardListItemDateRangeEnum.Weekly:
                        emberLeaderboard = await ChannelSession.MixerUserConnection.GetWeeklyEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Monthly:
                        emberLeaderboard = await ChannelSession.MixerUserConnection.GetMonthlyEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Yearly:
                        emberLeaderboard = await ChannelSession.MixerUserConnection.GetYearlyEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.AllTime:
                        emberLeaderboard = await ChannelSession.MixerUserConnection.GetAllTimeEmbersLeaderboard(ChannelSession.MixerChannel, this.TotalToShow);
                        break;
                }
                this.lastQuery = DateTimeOffset.Now;

                if (emberLeaderboard != null)
                {
                    for (int i = 0; i < emberLeaderboard.Count() && items.Count() < this.TotalToShow; i++)
                    {
                        var lData = emberLeaderboard.ElementAt(i);
                        items.Add(new OverlayLeaderboardItem(lData.username, lData.statValue.ToString()));
                    }
                }
            }

            if (items.Count > 0)
            {
                await this.ProcessLeaderboardItems(items);
            }

            return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            userSubDates[user] = DateTimeOffset.Now;
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
                items.Add(new OverlayLeaderboardItem(kvp.Key, kvp.Value.GetAge()));
            }

            await this.ProcessLeaderboardItems(items);
            this.SendUpdateRequired();
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            UserViewModel user = donation.User;

            if (!this.userDonations.ContainsKey(user))
            {
                this.userDonations[user] = donation.Copy();
                this.userDonations[user].Amount = 0.0;
            }
            this.userDonations[user].Amount += donation.Amount;

            List<OverlayLeaderboardItem> items = new List<OverlayLeaderboardItem>();

            var orderedUsers = this.userDonations.OrderByDescending(kvp => kvp.Value.Amount);
            for (int i = 0; i < orderedUsers.Count() && items.Count() < this.TotalToShow; i++)
            {
                var kvp = orderedUsers.ElementAt(i);
                items.Add(new OverlayLeaderboardItem(kvp.Key, kvp.Value.AmountText));
            }

            await this.ProcessLeaderboardItems(items);
            this.SendUpdateRequired();
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

                if (this.NewLeaderCommand != null)
                {
                    // Detect if we had a list before, and we have a list now, and the top user changed, let's trigger the event
                    if (this.lastItems.Count() > 0 && updatedList.Count() > 0 && !this.lastItems.First().User.ID.Equals(updatedList.First().User.ID))
                    {
                        await this.NewLeaderCommand.Perform(updatedList.First().User, arguments: new string[] { this.lastItems.First().User.Username });
                    }
                }

                this.lastItems = new List<OverlayListIndividualItemModel>(updatedList);
            }));
        }
    }
}