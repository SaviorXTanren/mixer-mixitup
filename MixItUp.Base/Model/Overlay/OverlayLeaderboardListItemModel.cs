using Mixer.Base.Model.Leaderboards;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
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

        public OverlayLeaderboardListItemModel() : base() { }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation,
            OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, UserCurrencyModel currency, CustomCommand newLeaderCommand)
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

        public override async Task LoadTestData()
        {
            UserViewModel user = await ChannelSession.GetCurrentUser();
        }

        public override async Task Enable()
        {
            if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Subscribers)
            {
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
            List<OverlayListIndividualItemModel> items = new List<OverlayListIndividualItemModel>();
            if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
                {
                    UserCurrencyModel currency = ChannelSession.Settings.Currencies[this.CurrencyID];
                    Dictionary<Guid, int> currencyAmounts = currency.UserAmounts.ToDictionary();

                    var orderedUsers = currencyAmounts.OrderByDescending(kvp => kvp.Value);
                    for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                    {
                        var kvp = orderedUsers.ElementAt(i);
                        UserDataModel userData = ChannelSession.Settings.GetUserData(kvp.Key);

                        OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(userData.Username, new UserViewModel(userData), i + 1, this.HTML);
                        item.Hash = kvp.Value.ToString();
                        items.Add(item);
                    }
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks)
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

                if (sparkLeaderboard != null)
                {
                    for (int i = 0; i < this.TotalToShow && i < sparkLeaderboard.Count(); i++)
                    {
                        var sl = sparkLeaderboard.ElementAt(i);

                        OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(sl.username, null, i + 1, this.HTML);
                        item.Hash = sl.statValue.ToString();
                        items.Add(item);
                    }
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Embers)
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

                if (emberLeaderboard != null)
                {
                    for (int i = 0; i < this.TotalToShow && i < emberLeaderboard.Count(); i++)
                    {
                        var sl = emberLeaderboard.ElementAt(i);

                        OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(sl.username, null, i + 1, this.HTML);
                        item.Hash = sl.statValue.ToString();
                        items.Add(item);
                    }
                }
            }

            if (items.Count > 0)
            {
                await this.AddLeaderboardItems(items);
            }

            return await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            userSubDates[user] = DateTimeOffset.Now;
            this.SendUpdateRequired();
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            await this.UpdateSubscribers();
            this.SendUpdateRequired();
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            await this.UpdateSubscribers();
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

            List<OverlayListIndividualItemModel> items = new List<OverlayListIndividualItemModel>();

            var orderedUsers = this.userDonations.OrderByDescending(kvp => kvp.Value.Amount);
            for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
            {
                var kvp = orderedUsers.ElementAt(i);

                OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(kvp.Key.MixerUsername, kvp.Key, i + 1, this.HTML);
                item.Hash = kvp.Value.AmountText;
            }

            await this.AddLeaderboardItems(items);
            this.SendUpdateRequired();
        }

        private async Task UpdateSubscribers()
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

            List<OverlayListIndividualItemModel> items = new List<OverlayListIndividualItemModel>();

            var orderedUsers = this.userSubDates.OrderByDescending(kvp => kvp.Value.TotalDaysFromNow());
            for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
            {
                var kvp = orderedUsers.ElementAt(i);

                OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(kvp.Key.MixerUsername, kvp.Key, i + 1, this.HTML);
                item.Hash = kvp.Value.GetAge();
            }

            await this.AddLeaderboardItems(items);
            this.SendUpdateRequired();
        }

        private async Task AddLeaderboardItems(IEnumerable<OverlayListIndividualItemModel> items)
        {
            await this.listSemaphore.WaitAndRelease(async () =>
            {
                foreach (OverlayListIndividualItemModel item in this.lastItems)
                {
                    if (!items.Any(i => i.ID.Equals(item.ID)))
                    {
                        this.Items.Add(OverlayListIndividualItemModel.CreateRemoveItem(item.ID));
                    }
                }

                for (int i = 0; i < items.Count() && i < this.TotalToShow; i++)
                {
                    OverlayListIndividualItemModel item = items.ElementAt(i);

                    OverlayListIndividualItemModel foundItem = this.lastItems.FirstOrDefault(oi => oi.ID.Equals(item.ID));
                    if (foundItem == null || foundItem.Position != item.Position || !foundItem.Hash.Equals(item.Hash))
                    {
                        this.Items.Add(item);
                        item.TemplateReplacements.Add("USERNAME", item.ID);
                        item.TemplateReplacements.Add("DETAILS", item.Hash);
                        item.TemplateReplacements.Add("TOP_TEXT_HEIGHT", ((int)(0.4 * ((double)this.Height))).ToString());
                        item.TemplateReplacements.Add("BOTTOM_TEXT_HEIGHT", ((int)(0.2 * ((double)this.Height))).ToString());
                    }
                }

                if (this.NewLeaderCommand != null)
                {
                    // Detect if we had a list before, and we have a list now, and the top user changed, let's trigger the event
                    if (this.lastItems.Count() > 0 && items.Count() > 0 && !this.lastItems.First().User.MixerID.Equals(items.First().User.MixerID))
                    {
                        await this.NewLeaderCommand.Perform(items.First().User, new string[] { this.lastItems.First().User.MixerUsername });
                    }
                }

                this.lastItems = new List<OverlayListIndividualItemModel>(items);
            });
        }
    }
}
