using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.Twitch.Clips;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Util
{
    public class SpecialIdentifierStringBuilder
    {
        public const string SpecialIdentifierHeader = "$";

        public const string SpecialIdentifierNumberRegexPattern = "\\d+";
        public const string SpecialIdentifierNumberRangeRegexPattern = "\\d+:\\d+";

        public const string StreamingPlatformSpecialIdentifier = "streamingplatform";

        public const string TopSpecialIdentifierHeader = "top";
        public const string TopTimeSpecialIdentifier = TopSpecialIdentifierHeader + "time";
        public const string TopTimeRegexSpecialIdentifier = TopSpecialIdentifierHeader + "\\d+time";

        public const string TopBitsCheeredSpecialIdentifier = TopSpecialIdentifierHeader + "bitscheered";
        public const string TopBitsCheeredRegexSpecialIdentifier = TopSpecialIdentifierHeader + "\\d+bitscheered";

        public const string UserSpecialIdentifierHeader = "user";
        public const string ArgSpecialIdentifierHeader = "arg";
        public const string ArgDelimitedSpecialIdentifierHeader = ArgSpecialIdentifierHeader + "delimited";
        public const string StreamerSpecialIdentifierHeader = "streamer";
        public const string BotSpecialIdentifierHeader = "bot";
        public const string TargetSpecialIdentifierHeader = "target";

        public const string RandomSpecialIdentifierHeader = "random";
        public const string RandomNumberRegexSpecialIdentifier = RandomSpecialIdentifierHeader + "number";
        public const string RandomFollowerSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "follower";
        public const string RandomSubscriberSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "subscriber";
        public const string RandomRegularSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "regular";

        public const string StreamSpecialIdentifierHeader = "stream";
        public const string StreamUptimeSpecialIdentifierHeader = StreamSpecialIdentifierHeader + "uptime";
        public const string StreamStartSpecialIdentifierHeader = StreamSpecialIdentifierHeader + "start";

        public const string MusicPlayerSpecialIdentifierHeader = "musicplayer";

        public const string TwitchSpecialIdentifierHeader = "twitch";
        public const string YouTubeSpecialIdentifierHeader = "youtube";

        public const string QuoteSpecialIdentifierHeader = "quote";

        public const string GameQueueSpecialIdentifierHeader = "gamequeue";
        public const string GameQueueUsersRegexSpecialIdentifier = GameQueueSpecialIdentifierHeader + "users\\d+";

        public const string DonationSourceSpecialIdentifier = "donationsource";
        public const string DonationTypeSpecialIdentifier = "donationtype";
        public const string DonationAmountNumberSpecialIdentifier = "donationamountnumber";
        public const string DonationAmountNumberDigitsSpecialIdentifier = "donationamountnumberdigits";
        public const string DonationAmountSpecialIdentifier = "donationamount";
        public const string DonationMessageSpecialIdentifier = "donationmessage";
        public const string DonationImageSpecialIdentifier = "donationimage";

        public const string PatreonTierNameSpecialIdentifier = "patreontiername";
        public const string PatreonTierAmountSpecialIdentifier = "patreontieramount";
        public const string PatreonTierImageSpecialIdentifier = "patreontierimage";

        public const string DonorDriveSpecialIdentifierHeader = "donordrive";
        public const string TiltifySpecialIdentifierHeader = "tiltify";
        public const string PulsoidSpecialIdentifierHeader = "pulsoid";

        public const string UnicodeRegexSpecialIdentifier = "unicode";

        public const string LatestFollowerUserData = "latestfollower";
        public const string LatestHostUserData = "latesthost";
        public const string LatestRaidUserData = "latestraid";
        public const string LatestRaidViewerCountData = "latestraidviewercount";
        public const string LatestSubscriberUserData = "latestsubscriber";
        public const string LatestSubscriberSubMonthsData = "latestsubscribersubmonths";
        public const string LatestBitsCheeredUserData = "latestbitscheered";
        public const string LatestBitsCheeredAmountData = "latestbitscheeredamount";
        public const string LatestDonationUserData = "latestdonation";
        public const string LatestDonationAmountData = "latestdonationamount";
        public const string LatestSuperChatUserData = "latestsuperchat";
        public const string LatestSuperChatAmountData = "latestsuperchatamount";

        public const string InteractiveTextBoxTextEntrySpecialIdentifierHelpText = "User Text Entered = " + SpecialIdentifierStringBuilder.SpecialIdentifierHeader +
            SpecialIdentifierStringBuilder.ArgSpecialIdentifierHeader + "1text";

        private static Dictionary<string, string> GlobalSpecialIdentifiers = new Dictionary<string, string>();

        public static async Task<string> ProcessSpecialIdentifiers(string str, CommandParametersModel parameters, bool encode = false)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, encode);
            await siString.ReplaceCommonSpecialModifiers(parameters);
            return siString.ToString();
        }

        public static void AddGlobalSpecialIdentifier(string specialIdentifier, string replacement)
        {
            SpecialIdentifierStringBuilder.GlobalSpecialIdentifiers[specialIdentifier] = replacement;
        }

        public static void RemoveGlobalSpecialIdentifier(string specialIdentifier)
        {
            SpecialIdentifierStringBuilder.GlobalSpecialIdentifiers.Remove(specialIdentifier);
        }

        public static bool IsValidSpecialIdentifier(string text)
        {
            return !string.IsNullOrEmpty(text) && text.All(c => Char.IsLetterOrDigit(c));
        }

        public static string ConvertToSpecialIdentifier(string text, int maxLength = 0)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Trim();

                StringBuilder specialIdentifier = new StringBuilder();
                for (int i = 0; i < text.Length; i++)
                {
                    if (char.IsLetterOrDigit(text[i]))
                    {
                        specialIdentifier.Append(text[i]);
                    }
                }
                string result = specialIdentifier.ToString().ToLower();

                if (maxLength > 0 && result.Length > maxLength)
                {
                    result = result.Substring(0, maxLength);
                }

                return result;
            }
            return null;
        }

        public static string ReplaceParameterVariablesEntries(string text, string pattern, string preReplacement, string postReplacement = null)
        {
            int startIndex = 0;
            do
            {
                startIndex = text.IndexOf(pattern);
                if (startIndex >= 0)
                {
                    int endIndex = text.IndexOf(")", startIndex);
                    if (endIndex >= 0)
                    {
                        string fullEntry = text.Substring(startIndex, endIndex - startIndex + 1);
                        string leftOver = fullEntry.Replace(pattern, "").Replace(")", "");
                        text = text.Replace(fullEntry, preReplacement + leftOver + (!string.IsNullOrEmpty(postReplacement) ? postReplacement : string.Empty));
                    }
                }
            } while (startIndex >= 0);
            return text;
        }

        public static async Task<IEnumerable<UserV2Model>> GetUserOrderedCurrencyList(CurrencyModel currency)
        {
            IEnumerable<UserV2Model> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
            return applicableUsers.OrderByDescending(u => currency.GetAmount(u));
        }

        public static async Task<IEnumerable<UserV2Model>> GetAllNonExemptUsers()
        {
            await ServiceManager.Get<UserService>().LoadAllUserData();

            List<UserV2Model> exemptUsers = new List<UserV2Model>(ChannelSession.Settings.Users.Values.Where(u => !u.IsSpecialtyExcluded));
            exemptUsers.Remove(ChannelSession.User.Model);
            return exemptUsers;
        }

        public static bool ContainsSpecialIdentifiers(string text) { return !string.IsNullOrEmpty(text) && text.Contains(SpecialIdentifierHeader); }

        private string text;
        private bool encode;

        public SpecialIdentifierStringBuilder(string text, bool encode = false)
        {
            this.text = !string.IsNullOrEmpty(text) ? text : string.Empty;
            this.encode = encode;
        }

        public async Task ReplaceCommonSpecialModifiers(CommandParametersModel parameters)
        {
            StreamingPlatformTypeEnum platform = parameters.Platform;
            if (platform == StreamingPlatformTypeEnum.All)
            {
                platform = ChannelSession.Settings.DefaultStreamingPlatform;
            }

            if (!SpecialIdentifierStringBuilder.ContainsSpecialIdentifiers(this.text))
            {
                return;
            }

            foreach (var kvp in parameters.SpecialIdentifiers.OrderByDescending(kvp => kvp.Key))
            {
                this.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
            }

            foreach (var kvp in SpecialIdentifierStringBuilder.GlobalSpecialIdentifiers.OrderByDescending(kvp => kvp.Key))
            {
                this.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
            }

            foreach (CounterModel counter in ChannelSession.Settings.Counters.Values.OrderByDescending(c => c.Name))
            {
                this.ReplaceSpecialIdentifier(counter.Name, counter.Amount.ToString());
                this.ReplaceSpecialIdentifier(counter.Name + "display", counter.Amount.ToNumberDisplayString());
            }

            this.ReplaceSpecialIdentifier("dayoftheweek", DateTimeOffset.Now.DayOfWeek.ToString());
            this.ReplaceSpecialIdentifier("datetime", DateTimeOffset.Now.ToString("g"));
            this.ReplaceSpecialIdentifier("dateyear", DateTimeOffset.Now.ToString("yyyy"));
            this.ReplaceSpecialIdentifier("datemonthname", DateTimeOffset.Now.ToString("MMMM"));
            this.ReplaceSpecialIdentifier("datemonth", DateTimeOffset.Now.ToString("MM"));
            this.ReplaceSpecialIdentifier("dateday", DateTimeOffset.Now.ToString("dd"));
            this.ReplaceSpecialIdentifier("date", DateTimeOffset.Now.ToString("d"));
            this.ReplaceSpecialIdentifier("timedigits", DateTimeOffset.Now.ToString("HHmm"));
            this.ReplaceSpecialIdentifier("timehour", DateTimeOffset.Now.ToString("HH"));
            this.ReplaceSpecialIdentifier("timeminute", DateTimeOffset.Now.ToString("mm"));
            this.ReplaceSpecialIdentifier("timesecond", DateTimeOffset.Now.ToString("ss"));
            this.ReplaceSpecialIdentifier("time", DateTimeOffset.Now.ToString("t"));
            this.ReplaceSpecialIdentifier("linebreak", Environment.NewLine);

            if (this.ContainsSpecialIdentifier("streamcurrentscene"))
            {
                IStreamingSoftwareService ssService = null;
                switch (ChannelSession.Settings.DefaultStreamingSoftware)
                {
                    case Model.Actions.StreamingSoftwareTypeEnum.OBSStudio:
                        ssService = ServiceManager.Get<IOBSStudioService>();
                        break;
                    case Model.Actions.StreamingSoftwareTypeEnum.XSplit:
                        ssService = ServiceManager.Get<XSplitService>();
                        break;
                    case Model.Actions.StreamingSoftwareTypeEnum.StreamlabsDesktop:
                        ssService = ServiceManager.Get<StreamlabsDesktopService>();
                        break;
                }

                string currentScene = "Unknown";
                if (ssService != null && ssService.IsEnabled)
                {
                    if (!ssService.IsConnected)
                    {
                        Result result = await ssService.Connect();
                        if (!result.Success)
                        {
                            Logger.Log(LogLevel.Error, result.Message);
                        }
                    }

                    if (ssService.IsConnected)
                    {
                        currentScene = await ssService.GetCurrentScene();
                    }
                }

                this.ReplaceSpecialIdentifier("streamcurrentscene", currentScene);
            }

            if (ServiceManager.Get<GameQueueService>().IsEnabled && this.ContainsSpecialIdentifier(GameQueueSpecialIdentifierHeader))
            {
                this.ReplaceSpecialIdentifier(GameQueueSpecialIdentifierHeader + "total", ServiceManager.Get<GameQueueService>().Queue.Count().ToString());
                if (this.ContainsRegexSpecialIdentifier(GameQueueUsersRegexSpecialIdentifier))
                {
                    await this.ReplaceNumberBasedRegexSpecialIdentifier(GameQueueUsersRegexSpecialIdentifier, (total) =>
                    {
                        IEnumerable<CommandParametersModel> applicableUsers = ServiceManager.Get<GameQueueService>().Queue.Take(total).ToList();
                        string result = MixItUp.Base.Resources.NoUsersFound;
                        if (applicableUsers.Count() > 0)
                        {
                            result = string.Join(" ", applicableUsers.Select(p => $"@{p.User.Username}"));
                        }
                        return Task.FromResult(result);
                    });
                }
            }

            if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader))
            {
                if (this.ContainsRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredRegexSpecialIdentifier))
                {
                    await this.HandleTopBitsCheeredRegex(BitsLeaderboardPeriodEnum.Day);
                    await this.HandleTopBitsCheeredRegex(BitsLeaderboardPeriodEnum.Week);
                    await this.HandleTopBitsCheeredRegex(BitsLeaderboardPeriodEnum.Month);
                    await this.HandleTopBitsCheeredRegex(BitsLeaderboardPeriodEnum.Year);
                    await this.HandleTopBitsCheeredRegex(BitsLeaderboardPeriodEnum.All);
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier))
                {
                    await this.HandleTopBitsCheered(BitsLeaderboardPeriodEnum.Day);
                    await this.HandleTopBitsCheered(BitsLeaderboardPeriodEnum.Week);
                    await this.HandleTopBitsCheered(BitsLeaderboardPeriodEnum.Month);
                    await this.HandleTopBitsCheered(BitsLeaderboardPeriodEnum.Year);
                    await this.HandleTopBitsCheered(BitsLeaderboardPeriodEnum.All);
                }

                if (this.ContainsRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopTimeRegexSpecialIdentifier))
                {
                    await this.ReplaceNumberBasedRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopTimeRegexSpecialIdentifier, async (total) =>
                    {
                        IEnumerable<UserV2Model> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
                        List<string> timeUserList = new List<string>();
                        int userPosition = 1;
                        foreach (UserV2Model timeUser in applicableUsers.OrderByDescending(u => u.OnlineViewingMinutes).Take(total))
                        {
                            UserV2ViewModel timeUserViewModel = new UserV2ViewModel(timeUser);
                            timeUserList.Add($"#{userPosition}) {timeUserViewModel.Username} - {timeUserViewModel.OnlineViewingTimeString}");
                            userPosition++;
                        }

                        string result = MixItUp.Base.Resources.NoUsersFound;
                        if (timeUserList.Count > 0)
                        {
                            result = string.Join(", ", timeUserList);
                        }
                        return result;
                    });
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TopTimeSpecialIdentifier))
                {
                    IEnumerable<UserV2Model> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
                    UserV2Model topUserData = applicableUsers.Top(u => u.OnlineViewingMinutes);
                    UserV2ViewModel topUser = ServiceManager.Get<UserService>().GetActiveUserByID(parameters.Platform, topUserData.ID);
                    if (topUser == null)
                    {
                        topUser = new UserV2ViewModel(topUserData);
                    }
                    await this.HandleUserSpecialIdentifiers(topUser, SpecialIdentifierStringBuilder.TopTimeSpecialIdentifier);
                }

                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                {
                    if (this.ContainsRegexSpecialIdentifier(currency.TopRegexSpecialIdentifier))
                    {
                        await this.ReplaceNumberBasedRegexSpecialIdentifier(currency.TopRegexSpecialIdentifier, async (total) =>
                        {
                            List<string> currencyUserList = new List<string>();
                            int userPosition = 1;
                            foreach (UserV2Model userData in (await SpecialIdentifierStringBuilder.GetUserOrderedCurrencyList(currency)).Take(total))
                            {
                                UserV2ViewModel userViewModel = new UserV2ViewModel(userData);
                                currencyUserList.Add($"#{userPosition}) {userViewModel.Username} - {currency.GetAmount(userData).ToNumberDisplayString()}");
                                userPosition++;
                            }

                            string result = MixItUp.Base.Resources.NoUsersFound;
                            if (currencyUserList.Count > 0)
                            {
                                result = string.Join(", ", currencyUserList);
                            }
                            return result;
                        });
                    }

                    if (this.ContainsSpecialIdentifier(currency.TopUserSpecialIdentifier))
                    {
                        IEnumerable<UserV2Model> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
                        UserV2Model topUserData = applicableUsers.Top(u => currency.GetAmount(u));
                        UserV2ViewModel topUser = ServiceManager.Get<UserService>().GetActiveUserByID(parameters.Platform, topUserData.ID);
                        if (topUser == null)
                        {
                            topUser = new UserV2ViewModel(topUserData);
                        }
                        await this.HandleUserSpecialIdentifiers(topUser, currency.TopSpecialIdentifier);
                    }
                }
            }

            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                if (this.ContainsRegexSpecialIdentifier(currency.AllTotalAmountSpecialIdentifier) || this.ContainsRegexSpecialIdentifier(currency.AllTotalAmountDisplaySpecialIdentifier))
                {
                    await ServiceManager.Get<UserService>().LoadAllUserData();

                    IEnumerable<UserV2Model> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
                    int total = applicableUsers.Sum(u => currency.GetAmount(u));

                    this.ReplaceSpecialIdentifier(currency.AllTotalAmountDisplaySpecialIdentifier, total.ToNumberDisplayString());
                    this.ReplaceSpecialIdentifier(currency.AllTotalAmountSpecialIdentifier, total.ToString());
                }
            }

            if (ChannelSession.Settings.QuotesEnabled && ChannelSession.Settings.Quotes.Count > 0)
            {
                if (this.ContainsSpecialIdentifier(QuoteSpecialIdentifierHeader))
                {
                    this.ReplaceSpecialIdentifier(QuoteSpecialIdentifierHeader + "random", ChannelSession.Settings.Quotes.PickRandom().ToString());
                    this.ReplaceSpecialIdentifier(QuoteSpecialIdentifierHeader + "latest", ChannelSession.Settings.Quotes.Last().ToString());
                    this.ReplaceSpecialIdentifier(QuoteSpecialIdentifierHeader + "total", ChannelSession.Settings.Quotes.Count.ToString());

                    if (this.ContainsRegexSpecialIdentifier(QuoteSpecialIdentifierHeader + SpecialIdentifierNumberRegexPattern))
                    {
                        await this.ReplaceNumberBasedRegexSpecialIdentifier(QuoteSpecialIdentifierHeader + SpecialIdentifierNumberRegexPattern, (index) =>
                        {
                            UserQuoteModel quote = ChannelSession.Settings.Quotes.SingleOrDefault(q => q.ID == index);
                            if (quote != null)
                            {
                                return Task.FromResult(quote.ToString());
                            }
                            return Task.FromResult<string>(null);
                        });
                    }   
                }
            }

            if (ServiceManager.Get<GiveawayService>().IsRunning)
            {
                foreach (var kvp in ServiceManager.Get<GiveawayService>().GetSpecialIdentifiers())
                {
                    this.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
                }
            }

            if (ServiceManager.Get<DonorDriveService>().IsConnected)
            {
                if (this.ContainsSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "user") ||
                    this.ContainsSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "event") ||
                    this.ContainsSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "team"))
                {
                    await ServiceManager.Get<DonorDriveService>().RefreshData();

                    DonorDriveParticipant participant = ServiceManager.Get<DonorDriveService>().Participant;
                    this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "userdonationgoal", participant.fundraisingGoal.ToString());
                    this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "userdonationcount", participant.numDonations.ToString());
                    this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "userdonationamount", participant.sumDonations.ToString());

                    DonorDriveEvent ddEvent = ServiceManager.Get<DonorDriveService>().Event;
                    this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "eventdonationgoal", ddEvent.fundraisingGoal.ToString());
                    this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "eventdonationcount", ddEvent.numDonations.ToString());
                    this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "eventdonationamount", ddEvent.sumDonations.ToString());

                    DonorDriveTeam team = ServiceManager.Get<DonorDriveService>().Team;
                    if (team != null)
                    {
                        this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "teamdonationgoal", team.fundraisingGoal.ToString());
                        this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "teamdonationcount", team.numDonations.ToString());
                        this.ReplaceSpecialIdentifier(DonorDriveSpecialIdentifierHeader + "teamdonationamount", team.sumDonations.ToString());
                    }
                }
            }

            if (ServiceManager.Get<TiltifyService>().IsConnected)
            {
                if (this.ContainsSpecialIdentifier(TiltifySpecialIdentifierHeader))
                {
                    await ServiceManager.Get<TiltifyService>().RefreshCampaign();

                    TiltifyCampaign campaign = ServiceManager.Get<TiltifyService>().Campaign;
                    if (campaign != null)
                    {
                        this.ReplaceSpecialIdentifier(TiltifySpecialIdentifierHeader + "campaignurl", campaign.url);
                        this.ReplaceSpecialIdentifier(TiltifySpecialIdentifierHeader + "donationurl", campaign.donate_url);
                        this.ReplaceSpecialIdentifier(TiltifySpecialIdentifierHeader + "donationgoal", campaign.Goal.ToString());
                        this.ReplaceSpecialIdentifier(TiltifySpecialIdentifierHeader + "donationamount", campaign.AmountRaised.ToString());
                    }
                }
            }

            if (ServiceManager.Get<PulsoidService>().IsConnected)
            {
                if (this.ContainsSpecialIdentifier(PulsoidSpecialIdentifierHeader))
                {
                    PulsoidHeartRate heartRate = ServiceManager.Get<PulsoidService>().LastHeartRate;
                    if (heartRate != null)
                    {
                        this.ReplaceSpecialIdentifier(PulsoidSpecialIdentifierHeader + "heartrate", heartRate.HeartRate.ToString());
                    }
                }
            }

            if (this.ContainsSpecialIdentifier(MusicPlayerSpecialIdentifierHeader))
            {
                this.ReplaceSpecialIdentifier(MusicPlayerSpecialIdentifierHeader + "state", ServiceManager.Get<IMusicPlayerService>().State.ToString());

                MusicPlayerSong song = ServiceManager.Get<IMusicPlayerService>().CurrentSong;
                if (song != null)
                {
                    this.ReplaceSpecialIdentifier(MusicPlayerSpecialIdentifierHeader + "title", song.Title);
                    this.ReplaceSpecialIdentifier(MusicPlayerSpecialIdentifierHeader + "artist", song.Artist);
                    this.ReplaceSpecialIdentifier(MusicPlayerSpecialIdentifierHeader + "display", song.ToString());
                }
            }

            if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader))
            {
                if (this.ContainsSpecialIdentifier(StreamUptimeSpecialIdentifierHeader) || this.ContainsSpecialIdentifier(StreamStartSpecialIdentifierHeader))
                {
                    DateTimeOffset startTime = await UptimePreMadeChatCommandModel.GetStartTime(parameters.Platform);
                    Logger.Log(LogLevel.Debug, $"Channel stream info: {JSONSerializerHelper.SerializeToString(ServiceManager.Get<TwitchSession>().Stream)} - {JSONSerializerHelper.SerializeToString(ServiceManager.Get<YouTubeSession>().LiveBroadcasts.Values.FirstOrDefault())} - {ServiceManager.Get<TrovoSession>().ChannelModel}");
                    if (startTime > DateTimeOffset.MinValue)
                    {
                        TimeSpan duration = DateTimeOffset.Now.Subtract(startTime);

                        this.ReplaceSpecialIdentifier(StreamStartSpecialIdentifierHeader + "datetime", startTime.ToString("g"));
                        this.ReplaceSpecialIdentifier(StreamStartSpecialIdentifierHeader + "date", startTime.ToString("d"));
                        this.ReplaceSpecialIdentifier(StreamStartSpecialIdentifierHeader + "time", startTime.ToString("t"));

                        this.ReplaceSpecialIdentifier(StreamUptimeSpecialIdentifierHeader + "total", (int)duration.TotalHours + duration.ToString("\\:mm"));
                        this.ReplaceSpecialIdentifier(StreamUptimeSpecialIdentifierHeader + "hours", ((int)duration.TotalHours).ToString());
                        this.ReplaceSpecialIdentifier(StreamUptimeSpecialIdentifierHeader + "minutes", duration.ToString("mm"));
                        this.ReplaceSpecialIdentifier(StreamUptimeSpecialIdentifierHeader + "seconds", duration.ToString("ss"));
                    }
                }

                StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(platform);
                if (session.IsConnected)
                {
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "islive", session.IsLive.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "viewercount", session.StreamViewerCount.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "title", session.StreamTitle);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "gameid", session.StreamCategoryID);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "gamename", session.StreamCategoryName);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "gameimage", session.StreamCategoryImageURL);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "game", session.StreamCategoryName);
                }

                if (platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSession>().IsConnected)
                {
                    string thumbnail = ServiceManager.Get<TwitchSession>().Stream?.thumbnail_url;
                    if (!string.IsNullOrEmpty(thumbnail))
                    {
                        thumbnail = thumbnail.Replace("{width}", "1920");
                        thumbnail = thumbnail.Replace("{height}", "1080");
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "thumbnail", thumbnail);
                    }

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount"))
                    {
                        long followCount = await ServiceManager.Get<TwitchSession>().StreamerService.GetFollowerCount(ServiceManager.Get<TwitchSession>().StreamerModel);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount", followCount.ToString());
                    }

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount"))
                    {
                        long subCount = await ServiceManager.Get<TwitchSession>().StreamerService.GetSubscriberCount(ServiceManager.Get<TwitchSession>().StreamerModel);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount", subCount.ToString());
                    }

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "subscriberpoints"))
                    {
                        long subCount = await ServiceManager.Get<TwitchSession>().StreamerService.GetSubscriberPoints(ServiceManager.Get<TwitchSession>().StreamerModel);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subscriberpoints", subCount.ToString());
                    }

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "twitchtags"))
                    {
                        var tags = ServiceManager.Get<TwitchSession>().Channel?.tags;
                        if (tags != null && tags.Count() > 0)
                        {
                            this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "twitchtags", string.Join(", ", tags));
                        }
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeSession>().IsConnected)
                {
                    LiveBroadcast broadcast = ServiceManager.Get<YouTubeSession>().LiveBroadcasts.Values.FirstOrDefault();

                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "description", broadcast?.Snippet?.Description);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount", ServiceManager.Get<YouTubeSession>().StreamerModel.Statistics.SubscriberCount.GetValueOrDefault().ToString());
                    if (ServiceManager.Get<YouTubeSession>().IsLive)
                    {
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "youtubeid", broadcast?.Id);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "youtubeurl", ServiceManager.Get<YouTubeSession>().StreamLink);
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSession>().IsConnected)
                {
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount", ServiceManager.Get<TrovoSession>().ChannelModel?.followers.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount", ServiceManager.Get<TrovoSession>().ChannelModel?.subscriber_num.ToString());
                }

                this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "chattercount", ServiceManager.Get<UserService>().ActiveUserCount.ToString());
            }

            if (this.ContainsSpecialIdentifier(UserSpecialIdentifierHeader))
            {
                await this.HandleUserSpecialIdentifiers(parameters.User, string.Empty);
            }

            if (parameters.Arguments != null)
            {
                for (int i = 0; i < parameters.Arguments.Count(); i++)
                {
                    string currentArgumentSpecialIdentifierHeader = ArgSpecialIdentifierHeader + (i + 1);
                    if (this.ContainsSpecialIdentifier(currentArgumentSpecialIdentifierHeader))
                    {
                        if (this.ContainsSpecialIdentifier(currentArgumentSpecialIdentifierHeader + UserSpecialIdentifierHeader))
                        {
                            UserV2ViewModel argUser = await ServiceManager.Get<UserService>().GetUserByPlatform(platform, platformUsername: parameters.Arguments.ElementAt(i), performPlatformSearch: true);
                            if (argUser != null)
                            {
                                await this.HandleUserSpecialIdentifiers(argUser, currentArgumentSpecialIdentifierHeader);
                            }
                        }

                        this.ReplaceSpecialIdentifier(currentArgumentSpecialIdentifierHeader + "text", parameters.Arguments.ElementAt(i));
                        if (double.TryParse(parameters.Arguments.ElementAt(i), out double result))
                        {
                            this.ReplaceSpecialIdentifier(currentArgumentSpecialIdentifierHeader + "numberdisplay", result.ToNumberDisplayString());
                        }
                    }
                }

                string allArgs = string.Join(" ", parameters.Arguments);
                this.ReplaceSpecialIdentifier("allargs", allArgs);
                this.ReplaceSpecialIdentifier("argcount", parameters.Arguments.Count().ToString());

                if (!string.IsNullOrEmpty(allArgs))
                {
                    if (this.ContainsSpecialIdentifier(ArgDelimitedSpecialIdentifierHeader) || this.ContainsSpecialIdentifier(ArgDelimitedSpecialIdentifierHeader + "count"))
                    {
                        List<string> delimitedArgs = new List<string>(allArgs.Split(new string[] { ChannelSession.Settings.DelimitedArgumentsSeparator }, StringSplitOptions.RemoveEmptyEntries));

                        for (int i = 0; i < delimitedArgs.Count(); i++)
                        {
                            string currentArgumentSpecialIdentifierHeader = ArgDelimitedSpecialIdentifierHeader + (i + 1);
                            this.ReplaceSpecialIdentifier(currentArgumentSpecialIdentifierHeader + "text", delimitedArgs[i].Trim());
                        }

                        this.ReplaceSpecialIdentifier(ArgDelimitedSpecialIdentifierHeader + "count", delimitedArgs.Count.ToString());
                    }
                }

                await this.ReplaceNumberRangeBasedRegexSpecialIdentifier(ArgSpecialIdentifierHeader + SpecialIdentifierNumberRangeRegexPattern + "text", (min, max) =>
                {
                    string result = "";

                    min = min - 1;
                    max = Math.Min(max, parameters.Arguments.Count());
                    int total = max - min;

                    if (total > 0 && min <= parameters.Arguments.Count())
                    {
                        result = string.Join(" ", parameters.Arguments.Skip(min).Take(total));
                    }

                    return Task.FromResult(result);
                });
            }

            if (this.ContainsSpecialIdentifier(TargetSpecialIdentifierHeader))
            {
                await parameters.SetTargetUser();
                await this.HandleUserSpecialIdentifiers(parameters.TargetUser, TargetSpecialIdentifierHeader);
            }

            if (this.ContainsSpecialIdentifier(StreamerSpecialIdentifierHeader))
            {
                await this.HandleUserSpecialIdentifiers(ChannelSession.User, StreamerSpecialIdentifierHeader);
            }

            if (this.ContainsSpecialIdentifier(BotSpecialIdentifierHeader))
            {
                await this.HandleUserSpecialIdentifiers(ChannelSession.Bot, BotSpecialIdentifierHeader);
            }

            if (this.ContainsSpecialIdentifier(OverlayStreamBossV3Model.StreamBossSpecialIdentifierPrefix))
            {
                OverlayWidgetV3Model streamBossWidget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.Type == OverlayItemV3Type.StreamBoss);
                if (streamBossWidget != null)
                {
                    OverlayStreamBossV3Model streamBossOverlay = (OverlayStreamBossV3Model)streamBossWidget.Item;
                    if (streamBossOverlay != null)
                    {
                        this.ReplaceSpecialIdentifier(OverlayStreamBossV3Model.StreamBossHealthSpecialIdentifier, streamBossOverlay.CurrentHealth.ToString());

                        UserV2ViewModel streamBossUser = await streamBossOverlay.GetCurrentBoss();
                        await this.HandleUserSpecialIdentifiers(streamBossUser, OverlayStreamBossV3Model.StreamBossSpecialIdentifierPrefix);
                    }
                }
            }

            if (this.ContainsSpecialIdentifier(RandomSpecialIdentifierHeader))
            {
                if (this.ContainsSpecialIdentifier(RandomNumberRegexSpecialIdentifier))
                {
                    await this.ReplaceNumberRangeBasedRegexSpecialIdentifier(RandomNumberRegexSpecialIdentifier + SpecialIdentifierNumberRangeRegexPattern, (min, max) =>
                    {
                        int number = RandomHelper.GenerateRandomNumber(min, max + 1);
                        return Task.FromResult(number.ToString());
                    });

                    await this.ReplaceNumberBasedRegexSpecialIdentifier(RandomNumberRegexSpecialIdentifier + SpecialIdentifierNumberRegexPattern, (maxNumber) =>
                    {
                        int number = RandomHelper.GenerateRandomNumber(maxNumber) + 1;
                        return Task.FromResult(number.ToString());
                    });
                }

                IEnumerable<UserV2ViewModel> workableUsers = ServiceManager.Get<UserService>().GetActiveUsers(excludeSpecialtyExcluded: true);
                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomSpecialIdentifierHeader + "user"))
                {
                    if (workableUsers != null && workableUsers.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(workableUsers.Random(), RandomSpecialIdentifierHeader);
                    }
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomFollowerSpecialIdentifierHeader))
                {
                    IEnumerable<UserV2ViewModel> users = workableUsers.Where(u => u.IsFollower);
                    if (users != null && users.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(users.Random(), RandomFollowerSpecialIdentifierHeader);
                    }
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomSubscriberSpecialIdentifierHeader))
                {
                    IEnumerable<UserV2ViewModel> users = workableUsers.Where(u => u.IsSubscriber);
                    if (users != null && users.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(users.Random(), RandomSubscriberSpecialIdentifierHeader);
                    }
                }

                if (this.ContainsRegexSpecialIdentifier(SpecialIdentifierStringBuilder.RandomRegularSpecialIdentifierHeader))
                {
                    IEnumerable<UserV2ViewModel> users = workableUsers.Where(u => u.IsRegular);
                    if (users != null && users.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(users.Random(), RandomRegularSpecialIdentifierHeader);
                    }
                }
            }

            if (ServiceManager.Get<TwitchSession>().IsConnected && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader))
            {
                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subcount"))
                {
                    long subCount = await ServiceManager.Get<TwitchSession>().StreamerService.GetSubscriberCount(ServiceManager.Get<TwitchSession>().StreamerModel);
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subcount", subCount.ToString());
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subpoints"))
                {
                    long subPoints = await ServiceManager.Get<TwitchSession>().StreamerService.GetSubscriberPoints(ServiceManager.Get<TwitchSession>().StreamerModel);
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subpoints", subPoints.ToString());
                }

                if (ServiceManager.Get<TwitchSession>().AdSchedule != null && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "ad"))
                {
                    AdScheduleModel adSchedule = ServiceManager.Get<TwitchSession>().AdSchedule;
                    DateTimeOffset nextAd = adSchedule.NextAdTimestamp();
                    int nextAdMinutes = adSchedule.NextAdMinutesFromNow();
                    
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "adsnoozecount", adSchedule.snooze_count.ToString());
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "adnextduration", adSchedule.duration.ToString());
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "adnextminutes", nextAdMinutes.ToString());
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "adnexttime", nextAd.ToFriendlyTimeString());
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "cliprandom"))
                {
                    IEnumerable<ClipModel> clips = await ServiceManager.Get<TwitchSession>().StreamerService.GetClips(ServiceManager.Get<TwitchSession>().StreamerModel, maxResults: 100);
                    if (clips != null && clips.Count() > 0)
                    {
                        ClipModel randomClip = clips.Random();
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "cliprandom", randomClip.url);
                    }
                }
            }

            if (ServiceManager.Get<YouTubeSession>().IsConnected && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader))
            {
                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestvideo"))
                {
                    SearchResult searchResult = await ServiceManager.Get<YouTubeSession>().GetLatestNonStreamVideo();
                    if (searchResult != null)
                    {
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestvideoid", searchResult.Id.VideoId);
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestvideotitle", searchResult.Snippet.Title);
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestvideourl", $"https://www.youtube.com/watch?v={searchResult.Id.VideoId}");
                    }
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestshort"))
                {
                    Video video = await ServiceManager.Get<YouTubeSession>().GetLatestShort();
                    if (video != null)
                    {
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestshortid", video.Id);
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestshorttitle", video.Snippet.Title);
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.YouTubeSpecialIdentifierHeader + "latestshorturl", $"https://www.youtube.com/shorts/{video.Id}");
                    }
                }
            }

            await this.HandleLatestSpecialIdentifier(SpecialIdentifierStringBuilder.LatestFollowerUserData);
            await this.HandleLatestSpecialIdentifier(SpecialIdentifierStringBuilder.LatestHostUserData);
            await this.HandleLatestSpecialIdentifier(SpecialIdentifierStringBuilder.LatestRaidUserData, SpecialIdentifierStringBuilder.LatestRaidViewerCountData);
            await this.HandleLatestSpecialIdentifier(SpecialIdentifierStringBuilder.LatestSubscriberUserData, SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData);
            await this.HandleLatestSpecialIdentifier(SpecialIdentifierStringBuilder.LatestBitsCheeredUserData, SpecialIdentifierStringBuilder.LatestBitsCheeredAmountData);
            await this.HandleLatestSpecialIdentifier(SpecialIdentifierStringBuilder.LatestDonationUserData, SpecialIdentifierStringBuilder.LatestDonationAmountData);

            foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values.OrderByDescending(c => c.SpecialIdentifier))
            {
                if (this.ContainsSpecialIdentifier(inventory.RandomItemSpecialIdentifier))
                {
                    this.ReplaceSpecialIdentifier(inventory.RandomItemSpecialIdentifier, inventory.Items.Values.Random().Name);
                }

                if (this.ContainsSpecialIdentifier(inventory.UniqueItemsTotalSpecialIdentifier))
                {
                    this.ReplaceSpecialIdentifier(inventory.UniqueItemsTotalSpecialIdentifier, inventory.Items.Count().ToString());
                }
            }

            if (this.ContainsSpecialIdentifier(UnicodeRegexSpecialIdentifier))
            {
                await this.ReplaceNumberBasedRegexSpecialIdentifier(UnicodeRegexSpecialIdentifier + SpecialIdentifierNumberRegexPattern, (number) =>
                {
                    char uChar = (char)number;
                    return Task.FromResult(uChar.ToString());
                });
            }
        }

        public void ReplaceSpecialIdentifier(string identifier, string replacement, bool includeSpecialIdentifierHeader = true)
        {
            replacement = (replacement == null) ? string.Empty : replacement;
            if (this.encode)
            {
                replacement = HttpUtility.UrlEncode(replacement);
            }
            if (replacement.StartsWith("$"))
            {
                replacement = "$" + replacement;
            }
            this.text = Regex.Replace(this.text, "\\" + (includeSpecialIdentifierHeader ? SpecialIdentifierHeader : string.Empty) + identifier, replacement, RegexOptions.IgnoreCase);
        }

        public bool ContainsSpecialIdentifier(string identifier)
        {
            return this.text.Contains(SpecialIdentifierHeader + identifier, StringComparison.OrdinalIgnoreCase);
        }

        public int GetFirstInstanceOfSpecialIdentifier(string identifier, int startIndex)
        {
            return this.text.IndexOf(SpecialIdentifierHeader + identifier, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        public bool ContainsRegexSpecialIdentifier(string identifier)
        {
            return Regex.IsMatch(this.text, "\\" + SpecialIdentifierHeader + identifier, RegexOptions.IgnoreCase);
        }

        public async Task ReplaceNumberBasedRegexSpecialIdentifier(string regex, Func<int, Task<string>> replacer)
        {
            foreach (Match match in Regex.Matches(this.text, "\\" + SpecialIdentifierHeader + regex))
            {
                string text = new String(match.Value.Where(c => char.IsDigit(c)).ToArray());
                if (int.TryParse(text, out int number))
                {
                    string replacement = await replacer(number);
                    if (replacement != null)
                    {
                        this.ReplaceSpecialIdentifier(match.Value, replacement, includeSpecialIdentifierHeader: false);
                    }
                }
            }
        }

        public async Task ReplaceNumberRangeBasedRegexSpecialIdentifier(string regex, Func<int, int, Task<string>> replacer)
        {
            foreach (Match match in Regex.Matches(this.text, "\\" + SpecialIdentifierHeader + regex))
            {
                string text = new String(match.Value.Where(c => char.IsDigit(c) || c == ':').ToArray());
                string[] splits = text.Split(':');
                if (splits.Length == 2 && int.TryParse(splits[0], out int min) && int.TryParse(splits[1], out int max) && max >= min)
                {
                    string replacement = await replacer(min, max);
                    if (replacement != null)
                    {
                        this.ReplaceSpecialIdentifier(match.Value, replacement, includeSpecialIdentifierHeader: false);
                    }
                }
            }
        }

        public override string ToString() { return this.text; }

        private async Task HandleUserSpecialIdentifiers(UserV2ViewModel user, string identifierHeader)
        {
            if (user != null && this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader))
            {
                await user.Refresh();

                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values.OrderByDescending(c => c.UserAmountSpecialIdentifier))
                {
                    if (this.ContainsSpecialIdentifier(identifierHeader + currency.UserAmountSpecialIdentifier))
                    {
                        if (this.ContainsSpecialIdentifier(identifierHeader + currency.UserPositionSpecialIdentifier))
                        {
                            List<UserV2Model> sortedUsers = (await SpecialIdentifierStringBuilder.GetUserOrderedCurrencyList(currency)).ToList();
                            int index = sortedUsers.IndexOf(user.Model);
                            this.ReplaceSpecialIdentifier(identifierHeader + currency.UserPositionSpecialIdentifier, (index + 1).ToString());
                        }

                        int amount = currency.GetAmount(user);
                        RankModel rank = currency.GetRank(user);
                        RankModel nextRank = currency.GetNextRank(user);

                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserRankNextNameSpecialIdentifier, nextRank.Name);
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountNextDisplaySpecialIdentifier, nextRank.Amount.ToNumberDisplayString());
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountNextSpecialIdentifier, nextRank.Amount.ToString());

                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserRankNameSpecialIdentifier, rank.Name);
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountDisplaySpecialIdentifier, amount.ToNumberDisplayString());
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountSpecialIdentifier, amount.ToString());
                    }
                }

                foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values.OrderByDescending(c => c.UserAmountSpecialIdentifierHeader))
                {
                    if (this.ContainsSpecialIdentifier(identifierHeader + inventory.UserAmountSpecialIdentifierHeader))
                    {
                        Dictionary<string, int> userItems = new Dictionary<string, int>();

                        foreach (InventoryItemModel item in inventory.Items.Values.OrderByDescending(i => i.Name))
                        {
                            var quantity = inventory.GetAmount(user, item);
                            if (quantity > 0)
                            {
                                userItems[item.Name] = quantity;
                            }

                            string itemHeaderSpecialIdentifier = identifierHeader + inventory.UserAmountSpecialIdentifierHeader;

                            this.ReplaceSpecialIdentifier(itemHeaderSpecialIdentifier + item.MaxAmountSpecialIdentifier, item.MaxAmount.ToString());
                            this.ReplaceSpecialIdentifier(itemHeaderSpecialIdentifier + item.ShopBuyPriceSpecialIdentifier, item.BuyAmount.ToString());
                            this.ReplaceSpecialIdentifier(itemHeaderSpecialIdentifier + item.ShopSellPriceSpecialIdentifier, item.SellAmount.ToString());
                            this.ReplaceSpecialIdentifier(itemHeaderSpecialIdentifier + item.SpecialIdentifier, quantity.ToString());
                        }

                        if (userItems.Count > 0)
                        {
                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserUniqueItemsTotalSpecialIdentifier, userItems.Count.ToString());

                            List<string> userAllItems = new List<string>();
                            foreach (var kvp in userItems.OrderBy(i => i.Key))
                            {
                                if (kvp.Value > 0)
                                {
                                    userAllItems.Add(kvp.Key + " x" + kvp.Value);
                                }
                            }
                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserAllAmountSpecialIdentifier, string.Join(", ", userAllItems));
                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserRandomItemSpecialIdentifier, userItems.Keys.Random());
                        }
                        else
                        {
                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserUniqueItemsTotalSpecialIdentifier, userItems.Count.ToString());

                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserAllAmountSpecialIdentifier, Resources.Nothing);
                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserRandomItemSpecialIdentifier, Resources.Nothing);
                        }
                    }
                }

                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values.OrderByDescending(c => c.UserAmountSpecialIdentifier))
                {
                    if (this.ContainsSpecialIdentifier(identifierHeader + streamPass.UserAmountSpecialIdentifier))
                    {
                        this.ReplaceSpecialIdentifier(identifierHeader + streamPass.UserLevelSpecialIdentifier, streamPass.GetLevel(user).ToString());
                        this.ReplaceSpecialIdentifier(identifierHeader + streamPass.UserPointsDisplaySpecialIdentifier, streamPass.GetAmount(user).ToNumberDisplayString());
                        this.ReplaceSpecialIdentifier(identifierHeader + streamPass.UserAmountSpecialIdentifier, streamPass.GetAmount(user).ToString());
                    }
                }

                if (this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "gamequeueposition"))
                {
                    string gameQueuePosition = MixItUp.Base.Resources.QueueNotIn;
                    if (ServiceManager.Get<GameQueueService>().IsEnabled)
                    {
                        int position = ServiceManager.Get<GameQueueService>().GetUserPosition(user);
                        if (position > 0)
                        {
                            gameQueuePosition = position.ToString();
                        }
                    }
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "gamequeueposition", gameQueuePosition);
                }

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "id", user.ID.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "name", user.Username);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "displayname", user.DisplayName);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "fulldisplayname", user.FullDisplayName);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "url", user.ChannelLink);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "avatar", user.AvatarLink);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "roles", user.RolesString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "displayroles", user.DisplayRolesString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "primaryrole", user.PrimaryRoleString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "title", user.Title);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "alejopronouns", user.AlejoPronoun);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "moderationstrikes", user.ModerationStrikes.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "color", string.IsNullOrEmpty(user.Color) ? "#000000" : user.Color);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "inchat", ServiceManager.Get<UserService>().IsUserActive(user.ID).ToString());

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "time", user.OnlineViewingTimeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "hours", user.OnlineViewingHoursOnly.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "mins", user.OnlineViewingMinutesOnly.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "accountdays", user.AccountDays.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "accountage", user.AccountAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "accountdate", user.AccountDateString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followdays", user.FollowDays.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followage", user.FollowAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followdate", user.FollowDateString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "lastseendays", user.LastActivityDays.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "lastseenage", user.LastActivityAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "lastseendate", user.LastActivityDateString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subdays", user.SubscribeDays.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subage", user.SubscribeAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subdate", user.SubscribeDateString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subtier", user.SubscriberTierString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "submonths", user.SubscribeMonths.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subbadge", user.PlatformSubscriberBadgeLink);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isfollower", user.IsFollower.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isregular", user.IsRegular.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "issubscriber", user.IsPlatformSubscriber.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isvip", user.HasRole(UserRoleEnum.TwitchVIP).ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "ismod", user.MeetsRole(UserRoleEnum.Moderator).ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isspecialtyexcluded", user.IsSpecialtyExcluded.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "notes", user.Notes);

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalstreamswatched", user.TotalStreamsWatched.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalamountdonated", CurrencyHelper.ToCurrencyString(user.TotalAmountDonated));
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalsubsgifted", user.TotalSubsGifted.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalsubsreceived", user.TotalSubsReceived.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalchatmessagessent", user.TotalChatMessageSent.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totaltimestagged", user.TotalTimesTagged.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalcommandsrun", user.TotalCommandsRun.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalmonthssubbed", user.TotalMonthsSubbed.ToString());

                if (user.HasPlatformData(StreamingPlatformTypeEnum.Twitch))
                {
                    TwitchUserPlatformV2Model pUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "twitchid", pUser?.ID);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "twitchcolor", pUser?.Color);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalbitscheered", pUser?.TotalBitsCheered.ToString());

                    if (this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "bitslifetimeamount") &&
                        user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSession>().IsConnected)
                    {
                        long amount = await ServiceManager.Get<TwitchSession>().StreamerService.GetUserLifetimeBits(user.PlatformID);
                        this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "bitslifetimeamount", amount.ToString());
                    }
                }

                if (user.HasPlatformData(StreamingPlatformTypeEnum.YouTube))
                {
                    YouTubeUserPlatformV2Model pUser = user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "youtubeid", pUser?.ID);
                }

                if (user.HasPlatformData(StreamingPlatformTypeEnum.Trovo))
                {
                    TrovoUserPlatformV2Model pUser = user.GetPlatformData<TrovoUserPlatformV2Model>(StreamingPlatformTypeEnum.Trovo);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "trovoid", pUser?.ID);
                    if (pUser != null)
                    {
                        this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "trovocustomroles", string.Join(", ", pUser?.CustomRoles));
                    }
                }

                string userStreamHeader = identifierHeader + UserSpecialIdentifierHeader + "stream";
                if (this.ContainsSpecialIdentifier(userStreamHeader))
                {
                    if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSession>().IsConnected)
                    {
                        TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);
                        if (twitchUser != null)
                        {
                            MixItUp.Base.Model.Twitch.User.UserModel tUser = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIUserByID(twitchUser.ID);
                            if (tUser != null)
                            {
                                Task<MixItUp.Base.Model.Twitch.Channels.ChannelInformationModel> tChannel = ServiceManager.Get<TwitchSession>().StreamerService.GetChannelInformation(tUser);
                                Task<MixItUp.Base.Model.Twitch.Streams.StreamModel> stream = ServiceManager.Get<TwitchSession>().StreamerService.GetLatestStream(tUser);

                                await Task.WhenAll(tChannel, stream);

                                if (this.ContainsSpecialIdentifier(userStreamHeader + "gameimage"))
                                {
                                    Task<GameModel> game = ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIGameByID(tChannel.Result?.game_id);
                                    this.ReplaceSpecialIdentifier(userStreamHeader + "gameimage", game.Result?.box_art_url);
                                }

                                this.ReplaceSpecialIdentifier(userStreamHeader + "title", tChannel.Result?.title);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "gamename", tChannel.Result?.game_name);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "game", tChannel.Result?.game_name);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "islive", (stream.Result != null).ToString());
                            }
                        }
                    }
                    else if (user.Platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeSession>().IsConnected)
                    {
                        YouTubeUserPlatformV2Model youtubeUser = user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube);
                        if (youtubeUser != null)
                        {
                            Google.Apis.YouTube.v3.Data.Channel yChannel = await ServiceManager.Get<YouTubeSession>().StreamerService.GetChannelByID(youtubeUser.ID);
                            if (yChannel != null)
                            {

                            }
                        }
                    }
                    else if (user.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSession>().IsConnected)
                    {
                        TrovoUserPlatformV2Model trovoUser = user.GetPlatformData<TrovoUserPlatformV2Model>(StreamingPlatformTypeEnum.Trovo);
                        if (trovoUser != null)
                        {
                            MixItUp.Base.Model.Trovo.Users.UserModel tUser = await ServiceManager.Get<TrovoSession>().StreamerService.GetUserByName(trovoUser.Username);
                            if (tUser != null)
                            {
                                MixItUp.Base.Model.Trovo.Channels.ChannelModel tChannel = await ServiceManager.Get<TrovoSession>().StreamerService.GetChannelByID(tUser.channel_id);

                                this.ReplaceSpecialIdentifier(userStreamHeader + "title", tChannel?.live_title);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "gamename", tChannel?.category_name);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "game", tChannel?.category_name);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "islive", tChannel?.is_live.ToString());
                            }
                        }
                    }
                }

                if (ServiceManager.Get<PatreonService>().IsConnected)
                {
                    string tierName = MixItUp.Base.Resources.NotSubscribed;
                    PatreonTier tier = user.PatreonTier;
                    if (tier != null)
                    {
                        tierName = tier.Title;
                    }
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "patreontier", tierName);
                }
            }
        }

        private async Task HandleTopBitsCheeredRegex(BitsLeaderboardPeriodEnum period)
        {
            if (ServiceManager.Get<TwitchSession>().IsConnected && this.ContainsRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredRegexSpecialIdentifier + period.ToString().ToLower()))
            {
                await this.ReplaceNumberBasedRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredRegexSpecialIdentifier + period.ToString().ToLower(), async (total) =>
                {
                    string result = MixItUp.Base.Resources.NoUsersFound;
                    BitsLeaderboardModel leaderboard = await ServiceManager.Get<TwitchSession>().StreamerService.GetBitsLeaderboard(period, total);
                    if (leaderboard != null && leaderboard.users != null && leaderboard.users.Count > 0)
                    {
                        IEnumerable<BitsLeaderboardUserModel> users = leaderboard.users.OrderBy(l => l.rank);

                        List<string> leaderboardsList = new List<string>();
                        int position = 1;
                        for (int i = 0; i < total && i < users.Count(); i++)
                        {
                            BitsLeaderboardUserModel user = users.ElementAt(i);
                            leaderboardsList.Add($"#{i + 1}) {user.user_name} - {user.score}");
                            position++;
                        }

                        if (leaderboardsList.Count > 0)
                        {
                            result = string.Join(", ", leaderboardsList);
                        }
                    }
                    return result;
                });
            }
        }

        private async Task HandleTopBitsCheered(BitsLeaderboardPeriodEnum period)
        {
            if (ServiceManager.Get<TwitchSession>().IsConnected && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier + period.ToString().ToLower()))
            {
                BitsLeaderboardModel leaderboard = await ServiceManager.Get<TwitchSession>().StreamerService.GetBitsLeaderboard(period, 1);
                if (leaderboard != null && leaderboard.users != null && leaderboard.users.Count > 0)
                {
                    BitsLeaderboardUserModel bitsUser = leaderboard.users.OrderBy(u => u.rank).First();

                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier + period.ToString().ToLower() + "amount", bitsUser.score.ToString());

                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: bitsUser.user_id, platformUsername: bitsUser.user_name);
                    if (user == null)
                    {
                        user = UserV2ViewModel.CreateUnassociated(bitsUser.user_name);
                    }
                    await this.HandleUserSpecialIdentifiers(user, SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier + period.ToString().ToLower());
                }
            }
        }

        private async Task HandleLatestSpecialIdentifier(string userkey, string extrakey = null)
        {
            if (this.ContainsSpecialIdentifier(extrakey) && !string.IsNullOrEmpty(extrakey) && ChannelSession.Settings.LatestSpecialIdentifiersData.ContainsKey(extrakey) && ChannelSession.Settings.LatestSpecialIdentifiersData[extrakey] != null)
            {
                this.ReplaceSpecialIdentifier(extrakey, ChannelSession.Settings.LatestSpecialIdentifiersData[extrakey].ToString());
            }

            if (this.ContainsSpecialIdentifier(userkey) && ChannelSession.Settings.LatestSpecialIdentifiersData.ContainsKey(userkey) && ChannelSession.Settings.LatestSpecialIdentifiersData[userkey] != null)
            {
                if (Guid.TryParse(ChannelSession.Settings.LatestSpecialIdentifiersData[userkey].ToString(), out Guid userID))
                {
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByID(StreamingPlatformTypeEnum.All, userID);
                    if (user != null)
                    {
                        await this.HandleUserSpecialIdentifiers(user, userkey);
                    }
                }
            }
        }
    }
}
