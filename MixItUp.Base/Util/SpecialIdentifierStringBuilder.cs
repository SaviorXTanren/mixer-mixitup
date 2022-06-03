using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Twitch.Base.Models.NewAPI.Bits;
using Twitch.Base.Services.NewAPI;

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
        public const string TargetSpecialIdentifierHeader = "target";

        public const string RandomSpecialIdentifierHeader = "random";
        public const string RandomNumberRegexSpecialIdentifier = RandomSpecialIdentifierHeader + "number";
        public const string RandomFollowerSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "follower";
        public const string RandomSubscriberSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "subscriber";
        public const string RandomRegularSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "regular";

        public const string StreamBossSpecialIdentifierHeader = "streamboss";

        public const string StreamSpecialIdentifierHeader = "stream";
        public const string StreamUptimeSpecialIdentifierHeader = StreamSpecialIdentifierHeader + "uptime";
        public const string StreamStartSpecialIdentifierHeader = StreamSpecialIdentifierHeader + "start";

        public const string TwitchSpecialIdentifierHeader = "twitch";

        public const string QuoteSpecialIdentifierHeader = "quote";

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

        public const string ExtraLifeSpecialIdentifierHeader = "extralife";

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

            if (!this.text.Contains(SpecialIdentifierHeader))
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
            }

            this.ReplaceSpecialIdentifier("dayoftheweek", DateTimeOffset.Now.DayOfWeek.ToString());
            this.ReplaceSpecialIdentifier("datetime", DateTimeOffset.Now.ToString("g"));
            this.ReplaceSpecialIdentifier("dateyear", DateTimeOffset.Now.ToString("yyyy"));
            this.ReplaceSpecialIdentifier("datemonth", DateTimeOffset.Now.ToString("MM"));
            this.ReplaceSpecialIdentifier("dateday", DateTimeOffset.Now.ToString("dd"));
            this.ReplaceSpecialIdentifier("date", DateTimeOffset.Now.ToString("d"));
            this.ReplaceSpecialIdentifier("timedigits", DateTimeOffset.Now.ToString("HHmm"));
            this.ReplaceSpecialIdentifier("timehour", DateTimeOffset.Now.ToString("HH"));
            this.ReplaceSpecialIdentifier("timeminute", DateTimeOffset.Now.ToString("mm"));
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
                    case Model.Actions.StreamingSoftwareTypeEnum.StreamlabsOBS:
                        ssService = ServiceManager.Get<StreamlabsOBSService>();
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

            int gameQueueCount = 0;
            if (ServiceManager.Get<GameQueueService>().IsEnabled)
            {
                gameQueueCount = ServiceManager.Get<GameQueueService>().Queue.Count();
            }
            this.ReplaceSpecialIdentifier("gamequeuetotal", gameQueueCount.ToString());

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
                    UserV2ViewModel topUser = ServiceManager.Get<UserService>().GetActiveUserByID(topUserData.ID);
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
                        UserV2ViewModel topUser = ServiceManager.Get<UserService>().GetActiveUserByID(topUserData.ID);
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

                    if (this.ContainsRegexSpecialIdentifier(QuoteSpecialIdentifierHeader + SpecialIdentifierNumberRegexPattern))
                    {
                        await this.ReplaceNumberBasedRegexSpecialIdentifier(QuoteSpecialIdentifierHeader + SpecialIdentifierNumberRegexPattern, (index) =>
                        {
                            if (index > 0 && index <= ChannelSession.Settings.Quotes.Count)
                            {
                                index--;
                                return Task.FromResult(ChannelSession.Settings.Quotes[index].ToString());
                            }
                            return Task.FromResult<string>(null);
                        });
                    }   
                }
            }

            if (ServiceManager.Get<TwitterService>().IsConnected && this.ContainsSpecialIdentifier("tweet"))
            {
                IEnumerable<Tweet> tweets = await ServiceManager.Get<TwitterService>().GetLatestTweets();
                if (tweets != null && tweets.Count() > 0)
                {
                    Tweet latestTweet = tweets.FirstOrDefault();
                    DateTimeOffset latestTweetLocalTime = latestTweet.DateTime.ToCorrectLocalTime();

                    this.ReplaceSpecialIdentifier("tweetlatesturl", latestTweet.TweetLink);
                    this.ReplaceSpecialIdentifier("tweetlatesttext", latestTweet.Text);
                    this.ReplaceSpecialIdentifier("tweetlatestdatetime", latestTweetLocalTime.ToString("g"));
                    this.ReplaceSpecialIdentifier("tweetlatestdate", latestTweetLocalTime.ToString("d"));
                    this.ReplaceSpecialIdentifier("tweetlatesttime", latestTweetLocalTime.ToString("t"));

                    Tweet streamTweet = tweets.FirstOrDefault(t => t.IsStreamTweet);
                    if (streamTweet != null)
                    {
                        DateTimeOffset streamTweetLocalTime = streamTweet.DateTime.ToCorrectLocalTime();
                        this.ReplaceSpecialIdentifier("tweetstreamurl", streamTweet.TweetLink);
                        this.ReplaceSpecialIdentifier("tweetstreamtext", streamTweet.Text);
                        this.ReplaceSpecialIdentifier("tweetstreamdatetime", streamTweetLocalTime.ToString("g"));
                        this.ReplaceSpecialIdentifier("tweetstreamdate", streamTweetLocalTime.ToString("d"));
                        this.ReplaceSpecialIdentifier("tweetstreamtime", streamTweetLocalTime.ToString("t"));
                    }
                }
            }

            if (ServiceManager.Get<ExtraLifeService>().IsConnected && this.ContainsSpecialIdentifier(ExtraLifeSpecialIdentifierHeader))
            {
                ExtraLifeTeam team = await ServiceManager.Get<ExtraLifeService>().GetTeam();

                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "teamdonationgoal", team.fundraisingGoal.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "teamdonationcount", team.numDonations.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "teamdonationamount", team.sumDonations.ToString());

                ExtraLifeTeamParticipant participant = await ServiceManager.Get<ExtraLifeService>().GetParticipant();

                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "userdonationgoal", participant.fundraisingGoal.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "userdonationcount", participant.numDonations.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "userdonationamount", participant.sumDonations.ToString());
            }

            if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader))
            {
                if (this.ContainsSpecialIdentifier(StreamUptimeSpecialIdentifierHeader) || this.ContainsSpecialIdentifier(StreamStartSpecialIdentifierHeader))
                {
                    DateTimeOffset startTime = await UptimePreMadeChatCommandModel.GetStartTime();
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

                if (platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "islive", ServiceManager.Get<TwitchSessionService>().IsLive.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "viewercount", ServiceManager.Get<TwitchSessionService>().Stream?.viewer_count.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "title", ServiceManager.Get<TwitchSessionService>().Channel?.title);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "gameimage", ServiceManager.Get<TwitchSessionService>().Stream?.thumbnail_url);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "game", ServiceManager.Get<TwitchSessionService>().Channel?.game_name);

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount"))
                    {
                        long followCount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetFollowerCount(ServiceManager.Get<TwitchSessionService>().User);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount", followCount.ToString());
                    }

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount"))
                    {
                        long subCount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberCount(ServiceManager.Get<TwitchSessionService>().User);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount", subCount.ToString());
                    }

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "subscriberpoints"))
                    {
                        long subCount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberPoints(ServiceManager.Get<TwitchSessionService>().User);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subscriberpoints", subCount.ToString());
                    }

                    if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "twitchtags"))
                    {
                        var tags = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetStreamTagsForChannel(ServiceManager.Get<TwitchSessionService>().User);
                        if (tags != null && tags.Count() > 0)
                        {
                            string locale = Languages.GetLanguageLocale().ToLower();

                            List<string> tagNames = new List<string>();
                            foreach (var tag in tags)
                            {
                                if (tag.localization_names.TryGetValue(locale, out JToken tagName))
                                {
                                    tagNames.Add(tagName.ToString());
                                }
                                else if (tag.localization_names.TryGetValue("en-us", out tagName))
                                {
                                    tagNames.Add(tagName.ToString());
                                }
                            }

                            this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "twitchtags", string.Join(", ", tagNames));
                        }
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeSessionService>().IsConnected)
                {
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "title", ServiceManager.Get<YouTubeSessionService>().Broadcast?.Snippet?.Title);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "islive", ServiceManager.Get<YouTubeSessionService>().IsLive.ToString());
                }
                else if (platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                {
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "islive", ServiceManager.Get<TrovoSessionService>().IsLive.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "viewercount", ServiceManager.Get<TrovoSessionService>().Channel?.current_viewers.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "title", ServiceManager.Get<TrovoSessionService>().Channel?.live_title);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "game", ServiceManager.Get<TrovoSessionService>().Channel?.category_name);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount", ServiceManager.Get<TrovoSessionService>().Channel?.followers.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount", ServiceManager.Get<TrovoSessionService>().Channel?.subscriber_num.ToString());
                }
                else if (platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().IsConnected)
                {
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "islive", ServiceManager.Get<GlimeshSessionService>().IsLive.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "viewercount", ServiceManager.Get<GlimeshSessionService>().User?.channel?.stream?.countViewers.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "title", ServiceManager.Get<GlimeshSessionService>().User?.channel?.title);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "game", ServiceManager.Get<GlimeshSessionService>().User?.channel?.stream?.category?.name);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount", ServiceManager.Get<GlimeshSessionService>().User?.countFollowers);
                }

                this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "chattercount", ServiceManager.Get<MixItUp.Base.Services.UserService>().ActiveUserCount.ToString());
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
                            UserV2ViewModel argUser = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(platform, parameters.Arguments.ElementAt(i), performPlatformSearch: true);
                            if (argUser != null)
                            {
                                await this.HandleUserSpecialIdentifiers(argUser, currentArgumentSpecialIdentifierHeader);
                            }
                        }

                        this.ReplaceSpecialIdentifier(currentArgumentSpecialIdentifierHeader + "text", parameters.Arguments.ElementAt(i));
                    }
                }

                string allArgs = string.Join(" ", parameters.Arguments);
                this.ReplaceSpecialIdentifier("allargs", allArgs);
                this.ReplaceSpecialIdentifier("argcount", parameters.Arguments.Count().ToString());

                if (!string.IsNullOrEmpty(allArgs))
                {
                    if (this.ContainsSpecialIdentifier(ArgDelimitedSpecialIdentifierHeader) || this.ContainsSpecialIdentifier(ArgDelimitedSpecialIdentifierHeader + "count"))
                    {
                        List<string> delimitedArgs = new List<string>(allArgs.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

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

            if (this.ContainsSpecialIdentifier(StreamBossSpecialIdentifierHeader))
            {
                OverlayWidgetModel streamBossWidget = ChannelSession.Settings.OverlayWidgets.FirstOrDefault(w => w.Item is OverlayStreamBossItemModel);
                if (streamBossWidget != null)
                {
                    OverlayStreamBossItemModel streamBossOverlay = (OverlayStreamBossItemModel)streamBossWidget.Item;
                    if (streamBossOverlay != null && streamBossOverlay.CurrentBoss != null)
                    {
                        await this.HandleUserSpecialIdentifiers(streamBossOverlay.CurrentBoss, StreamBossSpecialIdentifierHeader);
                        this.ReplaceSpecialIdentifier("streambosshealth", streamBossOverlay.CurrentHealth.ToString());
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

            if (ServiceManager.Get<TwitchSessionService>().IsConnected && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader))
            {
                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subcount"))
                {
                    long subCount = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberCount(ServiceManager.Get<TwitchSessionService>().User);
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subcount", subCount.ToString());
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subpoints"))
                {
                    long subPoints = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetSubscriberPoints(ServiceManager.Get<TwitchSessionService>().User);
                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TwitchSpecialIdentifierHeader + "subpoints", subPoints.ToString());
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

                            string itemSpecialIdentifier = identifierHeader + inventory.UserAmountSpecialIdentifierHeader + item.SpecialIdentifier;
                            this.ReplaceSpecialIdentifier(itemSpecialIdentifier, quantity.ToString());
                        }

                        if (userItems.Count > 0)
                        {
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
                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserAllAmountSpecialIdentifier, "Nothing");
                            this.ReplaceSpecialIdentifier(identifierHeader + inventory.UserRandomItemSpecialIdentifier, "Nothing");
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
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "moderationstrikes", user.ModerationStrikes.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "color", UserV2ViewModel.UserDefaultColor.Equals(user.Color) ? "#000000" : user.Color);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "inchat", ServiceManager.Get<UserService>().IsUserActive(user.ID).ToString());

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "time", user.OnlineViewingTimeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "hours", user.OnlineViewingHoursOnly.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "mins", user.OnlineViewingMinutesOnly.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "accountdays", user.AccountDays.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "accountage", user.AccountAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followdays", user.FollowDays.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followage", user.FollowAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subdays", user.SubscribeDays.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subage", user.SubscribeAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subtier", user.SubscriberTierString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "submonths", user.SubscribeMonths.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subbadge", user.PlatformSubscriberBadgeLink);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isfollower", user.IsFollower.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isregular", user.IsRegular.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "issubscriber", user.IsPlatformSubscriber.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isvip", user.HasRole(UserRoleEnum.TwitchVIP).ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "ismod", user.MeetsRole(UserRoleEnum.Moderator).ToString());

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalstreamswatched", user.TotalStreamsWatched.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalamountdonated", user.TotalAmountDonated.ToCurrencyString());
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

                if (user.HasPlatformData(StreamingPlatformTypeEnum.Glimesh))
                {
                    GlimeshUserPlatformV2Model pUser = user.GetPlatformData<GlimeshUserPlatformV2Model>(StreamingPlatformTypeEnum.Glimesh);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "glimeshid", pUser?.ID);
                }

                string userStreamHeader = identifierHeader + UserSpecialIdentifierHeader + "stream";
                if (this.ContainsSpecialIdentifier(userStreamHeader))
                {
                    if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);
                        if (twitchUser != null)
                        {
                            Twitch.Base.Models.NewAPI.Users.UserModel tUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(twitchUser.ID);
                            if (tUser != null)
                            {
                                Twitch.Base.Models.NewAPI.Channels.ChannelInformationModel tChannel = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetChannelInformation(tUser);
                                Twitch.Base.Models.NewAPI.Streams.StreamModel stream = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetStream(tUser);

                                this.ReplaceSpecialIdentifier(userStreamHeader + "title", tChannel?.title);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "game", tChannel?.game_name);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "islive", (stream != null).ToString());
                            }
                        }
                    }
                    else if (user.Platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeSessionService>().IsConnected)
                    {
                        YouTubeUserPlatformV2Model youtubeUser = user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube);
                        if (youtubeUser != null)
                        {
                            Google.Apis.YouTube.v3.Data.Channel yChannel = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByID(youtubeUser.ID);
                            if (yChannel != null)
                            {

                            }
                        }
                    }
                    else if (user.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().IsConnected)
                    {
                        TrovoUserPlatformV2Model trovoUser = user.GetPlatformData<TrovoUserPlatformV2Model>(StreamingPlatformTypeEnum.Trovo);
                        if (trovoUser != null)
                        {
                            Trovo.Base.Models.Users.UserModel tUser = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetUserByName(trovoUser.Username);
                            if (tUser != null)
                            {
                                Trovo.Base.Models.Channels.ChannelModel tChannel = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetChannelByID(tUser.channel_id);

                                this.ReplaceSpecialIdentifier(userStreamHeader + "title", tChannel?.live_title);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "game", tChannel?.category_name);
                                this.ReplaceSpecialIdentifier(userStreamHeader + "islive", tChannel?.is_live.ToString());
                            }
                        }
                    }
                    else if (user.Platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().IsConnected)
                    {
                        GlimeshUserPlatformV2Model glimeshUser = user.GetPlatformData<GlimeshUserPlatformV2Model>(StreamingPlatformTypeEnum.Glimesh);
                        if (glimeshUser != null)
                        {
                            Glimesh.Base.Models.Channels.ChannelModel gChannel = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetChannelByName(glimeshUser.Username);

                            this.ReplaceSpecialIdentifier(userStreamHeader + "title", gChannel?.title);
                            this.ReplaceSpecialIdentifier(userStreamHeader + "game", gChannel?.stream?.category?.name);
                            this.ReplaceSpecialIdentifier(userStreamHeader + "islive", gChannel?.IsLive.ToString());
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
            if (ServiceManager.Get<TwitchSessionService>().IsConnected && this.ContainsRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredRegexSpecialIdentifier + period.ToString().ToLower()))
            {
                await this.ReplaceNumberBasedRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredRegexSpecialIdentifier + period.ToString().ToLower(), async (total) =>
                {
                    string result = MixItUp.Base.Resources.NoUsersFound;
                    BitsLeaderboardModel leaderboard = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetBitsLeaderboard(period, total);
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
            if (ServiceManager.Get<TwitchSessionService>().UserConnection != null && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier + period.ToString().ToLower()))
            {
                BitsLeaderboardModel leaderboard = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetBitsLeaderboard(period, 1);
                if (leaderboard != null && leaderboard.users != null && leaderboard.users.Count > 0)
                {
                    BitsLeaderboardUserModel bitsUser = leaderboard.users.OrderBy(u => u.rank).First();

                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier + period.ToString().ToLower() + "amount", bitsUser.score.ToString());

                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, bitsUser.user_id);
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
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByID(userID);
                    if (user != null)
                    {
                        await this.HandleUserSpecialIdentifiers(user, userkey);
                    }
                }
            }
        }
    }
}
