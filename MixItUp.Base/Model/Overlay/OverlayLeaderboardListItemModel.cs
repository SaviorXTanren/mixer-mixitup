using Mixer.Base.Model.Leaderboards;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
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
        [DataContract]
        public class OverlayLeaderboardItemModel
        {
            [DataMember]
            public string Username { get; set; }

            [DataMember]
            public string Details { get; set; }

            public OverlayLeaderboardItemModel() { }

            public OverlayLeaderboardItemModel(string username, string details)
            {
                this.Username = username;
                this.Details = details;
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
        public List<OverlayLeaderboardItemModel> LeaderboardItems { get; set; } = new List<OverlayLeaderboardItemModel>();

        private Dictionary<UserViewModel, DateTimeOffset> userSubDates = new Dictionary<UserViewModel, DateTimeOffset>();
        private Dictionary<string, UserDonationModel> userDonations = new Dictionary<string, UserDonationModel>();

        public OverlayLeaderboardListItemModel() : base() { }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string borderColor, string backgroundColor, string textColor,
            string textFont, int width, int height, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, UserCurrencyViewModel currency)
            : this(htmlText, leaderboardType, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        {
            this.CurrencyID = currency.ID;
        }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string borderColor, string backgroundColor, string textColor,
            string textFont, int width, int height, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation, OverlayLeaderboardListItemDateRangeEnum dateRange)
            : this(htmlText, leaderboardType, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        {
            this.LeaderboardDateRange = dateRange;
        }

        public OverlayLeaderboardListItemModel(string htmlText, OverlayLeaderboardListItemTypeEnum leaderboardType, int totalToShow, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.Leaderboard, htmlText, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        {
            this.LeaderboardType = leaderboardType;
        }

        public override async Task LoadTestData()
        {
            UserViewModel user = await ChannelSession.GetCurrentUser();

            this.LeaderboardItems.Clear();
            for (int i = 0; i < this.TotalToShow; i++)
            {
                this.LeaderboardItems.Add(new OverlayLeaderboardItemModel(user.UserName, (i + 1).ToString()));
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;

            if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Subscribers)
            {
                foreach (UserWithGroupsModel userWithGroups in await ChannelSession.Connection.GetUsersWithRoles(ChannelSession.Channel, MixerRoleEnum.Subscriber))
                {
                    DateTimeOffset? subDate = userWithGroups.GetSubscriberDate();
                    if (subDate.HasValue)
                    {
                        userSubDates.Add(new UserViewModel(userWithGroups), subDate.GetValueOrDefault());
                    }
                }

                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Donations)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }

            await base.Initialize();

            await this.UpdateList();
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            userSubDates[user] = DateTimeOffset.Now;
            await this.UpdateList();
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { await this.UpdateList(); }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            if (!this.userDonations.ContainsKey(donation.UserName))
            {
                this.userDonations[donation.UserName] = donation.Copy();
                this.userDonations[donation.UserName].Amount = 0.0;
            }
            this.userDonations[donation.UserName].Amount += donation.Amount;

            await this.UpdateList();
        }

        private async Task UpdateList()
        {
            this.LeaderboardItems.Clear();

            if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Subscribers)
            {
                var orderedUsers = userSubDates.OrderByDescending(kvp => kvp.Value.TotalDaysFromNow());
                for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                {
                    this.LeaderboardItems.Add(new OverlayLeaderboardItemModel(orderedUsers.ElementAt(i).Key.UserName, orderedUsers.ElementAt(i).Value.GetAge()));
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Donations)
            {
                var orderedUsers = this.userDonations.OrderByDescending(kvp => kvp.Value.Amount);
                for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                {
                    this.LeaderboardItems.Add(new OverlayLeaderboardItemModel(orderedUsers.ElementAt(i).Key, orderedUsers.ElementAt(i).Value.AmountText));
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
                {
                    UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[this.CurrencyID];

                    Dictionary<UserDataViewModel, int> currencyAmounts = new Dictionary<UserDataViewModel, int>();
                    foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        currencyAmounts[userData] = userData.GetCurrencyAmount(currency);
                    }

                    var orderedUsers = currencyAmounts.OrderByDescending(kvp => kvp.Value);
                    for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                    {
                        try
                        {
                            this.LeaderboardItems.Add(new OverlayLeaderboardItemModel(orderedUsers.ElementAt(i).Key.UserName, orderedUsers.ElementAt(i).Value.ToString()));
                        }
                        catch (Exception ex) { Util.Logger.Log(ex); }
                    }
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Sparks)
            {
                IEnumerable<SparksLeaderboardModel> leaderboard = null;
                switch (this.LeaderboardDateRange)
                {
                    case OverlayLeaderboardListItemDateRangeEnum.Weekly:
                        leaderboard = await ChannelSession.Connection.GetWeeklySparksLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Monthly:
                        leaderboard = await ChannelSession.Connection.GetMonthlySparksLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Yearly:
                        leaderboard = await ChannelSession.Connection.GetYearlySparksLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.AllTime:
                        leaderboard = await ChannelSession.Connection.GetAllTimeSparksLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                }

                if (leaderboard != null)
                {
                    var orderedUsers = leaderboard.OrderByDescending(sl => sl.statValue);
                    for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                    {
                        this.LeaderboardItems.Add(new OverlayLeaderboardItemModel(orderedUsers.ElementAt(i).username, orderedUsers.ElementAt(i).statValue.ToString()));
                    }
                }
            }
            else if (this.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Embers)
            {
                IEnumerable<EmbersLeaderboardModel> leaderboard = null;
                switch (this.LeaderboardDateRange)
                {
                    case OverlayLeaderboardListItemDateRangeEnum.Weekly:
                        leaderboard = await ChannelSession.Connection.GetWeeklyEmbersLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Monthly:
                        leaderboard = await ChannelSession.Connection.GetMonthlyEmbersLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.Yearly:
                        leaderboard = await ChannelSession.Connection.GetYearlyEmbersLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                    case OverlayLeaderboardListItemDateRangeEnum.AllTime:
                        leaderboard = await ChannelSession.Connection.GetAllTimeEmbersLeaderboard(ChannelSession.Channel, this.TotalToShow);
                        break;
                }

                if (leaderboard != null)
                {
                    var orderedUsers = leaderboard.OrderByDescending(sl => sl.statValue);
                    for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                    {
                        this.LeaderboardItems.Add(new OverlayLeaderboardItemModel(orderedUsers.ElementAt(i).username, orderedUsers.ElementAt(i).statValue.ToString()));
                    }
                }
            }

            this.SendUpdateRequired();
        }
    }
}
