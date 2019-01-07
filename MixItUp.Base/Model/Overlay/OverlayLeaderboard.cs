using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum LeaderboardTypeEnum
    {
        Subscribers,
        Donations,
        [Name("Currency/Rank")]
        CurrencyRank,
    }

    [DataContract]
    public class OverlayLeaderboard : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 35%; left: 5%; width: 50%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TOP_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{USERNAME}</p>
          <p style=""position: absolute; top: 80%; right: 5%; width: 50%; text-align: right; font-family: '{TEXT_FONT}'; font-size: {BOTTOM_TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">{DETAILS}</p>
        </div>";

        public const string LeaderboardItemType = "leaderboard";

        [DataMember]
        public LeaderboardTypeEnum LeaderboardType { get; set; }
        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        public Guid CurrencyID { get; set; }

        [DataMember]
        public List<string> LeaderboardEntries = new List<string>();

        private Dictionary<UserViewModel, DateTimeOffset> userSubDates = new Dictionary<UserViewModel, DateTimeOffset>();
        private bool refreshSubscribers = true;

        private Dictionary<string, UserDonationModel> userDonations = new Dictionary<string, UserDonationModel>();
        private bool refreshDonations = true;

        private DateTimeOffset lastCurrencyRefresh = DateTimeOffset.MinValue;
        private List<UserDataViewModel> currencyUsersToShow = new List<UserDataViewModel>();

        public OverlayLeaderboard() : base(LeaderboardItemType, HTMLTemplate) { }

        public OverlayLeaderboard(string htmlText, LeaderboardTypeEnum leaderboardType, int totalToShow, string borderColor, string backgroundColor, string textColor,
            string textFont, int width, int height, OverlayEffectEntranceAnimationTypeEnum addEvent, OverlayEffectExitAnimationTypeEnum removeEvent, UserCurrencyViewModel currency)
            : this(htmlText, leaderboardType, totalToShow, borderColor, backgroundColor, textColor, textFont, width, height, addEvent, removeEvent)
        {
            this.CurrencyID = currency.ID;
        }

        public OverlayLeaderboard(string htmlText, LeaderboardTypeEnum leaderboardType, int totalToShow, string borderColor, string backgroundColor, string textColor,
            string textFont, int width, int height, OverlayEffectEntranceAnimationTypeEnum addEvent, OverlayEffectExitAnimationTypeEnum removeEvent)
            : base(LeaderboardItemType, htmlText)
        {
            this.LeaderboardType = leaderboardType;
            this.TotalToShow = totalToShow;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.AddEventAnimation = addEvent;
            this.RemoveEventAnimation = removeEvent;
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;

            if (this.LeaderboardType == LeaderboardTypeEnum.Subscribers)
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
            else if (this.LeaderboardType == LeaderboardTypeEnum.Donations)
            {
                if (ChannelSession.Services.Streamlabs != null)
                {
                    foreach (StreamlabsDonation donation in await ChannelSession.Services.Streamlabs.GetDonations(int.MaxValue))
                    {
                        if (!this.userDonations.ContainsKey(donation.UserName))
                        {
                            this.userDonations[donation.UserName] = donation.ToGenericDonation();
                            this.userDonations[donation.UserName].Amount = 0.0;
                        }
                        this.userDonations[donation.UserName].Amount += donation.Amount;
                    }
                }

                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }

            await base.Initialize();
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayLeaderboard>(); }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayLeaderboard copy = (OverlayLeaderboard)this.GetCopy();
            if (this.LeaderboardType == LeaderboardTypeEnum.Subscribers && this.refreshSubscribers)
            {
                this.refreshSubscribers = false;

                List<KeyValuePair<UserViewModel, DateTimeOffset>> usersToShow = new List<KeyValuePair<UserViewModel, DateTimeOffset>>();

                var orderedUsers = userSubDates.OrderByDescending(kvp => kvp.Value.TotalDaysFromNow());
                for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                {
                    usersToShow.Add(orderedUsers.ElementAt(i));
                }

                foreach (KeyValuePair<UserViewModel, DateTimeOffset> userToShow in usersToShow)
                {
                    extraSpecialIdentifiers["DETAILS"] = userToShow.Value.GetAge();
                    OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(userToShow.Key, arguments, extraSpecialIdentifiers);
                    copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                }
                return copy;
            }
            else if (this.LeaderboardType == LeaderboardTypeEnum.Donations && this.refreshDonations)
            {
                this.refreshDonations = false;

                List<UserDonationModel> topDonators = new List<UserDonationModel>();

                var orderedUsers = this.userDonations.OrderByDescending(kvp => kvp.Value.Amount);
                for (int i = 0; i < this.TotalToShow && i < orderedUsers.Count(); i++)
                {
                    topDonators.Add(orderedUsers.ElementAt(i).Value);
                }

                foreach (UserDonationModel topDonator in topDonators)
                {
                    extraSpecialIdentifiers["DETAILS"] = topDonator.AmountText;
                    OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(topDonator.User, arguments, extraSpecialIdentifiers);
                    copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                }
                return copy;
            }
            else if (this.LeaderboardType == LeaderboardTypeEnum.CurrencyRank)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(this.CurrencyID))
                {
                    UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[this.CurrencyID];
                    if (this.lastCurrencyRefresh < DateTimeOffset.Now)
                    {
                        this.lastCurrencyRefresh = DateTimeOffset.Now.AddMinutes(1);

                        Dictionary<uint, int> currencyAmounts = new Dictionary<uint, int>();
                        foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values)
                        {
                            currencyAmounts[userData.ID] = userData.GetCurrencyAmount(currency);
                        }

                        this.currencyUsersToShow.Clear();
                        for (int i = 0; i < this.TotalToShow && i < currencyAmounts.Count; i++)
                        {
                            try
                            {
                                KeyValuePair<uint, int> top = currencyAmounts.Aggregate((current, highest) => (current.Key <= 0 || current.Value < highest.Value) ? highest : current);
                                if (!top.Equals(default(KeyValuePair<uint, int>)))
                                {
                                    this.currencyUsersToShow.Add(ChannelSession.Settings.UserData[top.Key]);
                                    currencyAmounts.Remove(top.Key);
                                }
                            }
                            catch (Exception ex) { Util.Logger.Log(ex); }
                        }
                    }

                    foreach (UserDataViewModel userToShow in this.currencyUsersToShow)
                    {
                        extraSpecialIdentifiers["DETAILS"] = userToShow.GetCurrencyAmount(currency).ToString();
                        OverlayCustomHTMLItem htmlItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(new UserViewModel(userToShow), arguments, extraSpecialIdentifiers);
                        copy.LeaderboardEntries.Add(htmlItem.HTMLText);
                    }
                    return copy;
                }
            }
            return null;
        }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TOP_TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();
            replacementSets["BOTTOM_TEXT_HEIGHT"] = ((int)(0.2 * ((double)this.Height))).ToString();

            replacementSets["USERNAME"] = user.UserName;
            if (extraSpecialIdentifiers.ContainsKey("DETAILS"))
            {
                replacementSets["DETAILS"] = extraSpecialIdentifiers["DETAILS"];
            }

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            userSubDates[user] = DateTimeOffset.Now;
            this.refreshSubscribers = true;
        }

        private void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user) { this.refreshSubscribers = true; }

        private void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            if (!this.userDonations.ContainsKey(donation.UserName))
            {
                this.userDonations[donation.UserName] = donation.Copy();
                this.userDonations[donation.UserName].Amount = 0.0;
            }
            this.userDonations[donation.UserName].Amount += donation.Amount;
            this.refreshDonations = true;
        }
    }
}
