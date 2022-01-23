using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Twitch.Base.Models.NewAPI.Bits;
using Twitch.Base.Models.NewAPI.Channels;
using Twitch.Base.Services.NewAPI;

namespace MixItUp.Base.Util
{
    public class SpecialIdentifierStringBuilder
    {
        public const string SpecialIdentifierHeader = "$";

        public const string SpecialIdentifierNumberRegexPattern = "\\d+";
        public const string SpecialIdentifierNumberRangeRegexPattern = "\\d+:\\d+";

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

        public static string ConvertScorpBotText(string text)
        {
            text = text.Replace("$user", "@$username");
            text = text.Replace("$url", "$userurl");
            text = text.Replace("$hours", "$userhours");
            text = text.Replace("$game", "$usergame");

            for (int i = 1; i < 10; i++)
            {
                text = text.Replace("$target" + i, "@$arg" + i + "username");
            }
            text = text.Replace("$target", "@$targetusername");

            text = text.Replace("$randuser", "@$randomusername");

            text = text.Replace("$msg", "$allargs");

            text = text.Replace("$mygame", "$streamerusergame");
            text = text.Replace("$title", "$streamtitle");
            text = text.Replace("$status", "$streamtitle");

            if (text.Contains("$randnum("))
            {
                text = ReplaceParameterVariablesEntries(text, "$randnum(", "$randomnumber");
            }

            if (text.Contains("$tophours("))
            {
                text = ReplaceParameterVariablesEntries(text, "$tophours(", "$top", "time");
            }

            return text;
        }

        public static string ConvertStreamlabsChatBotText(string text)
        {
            text = text.Replace("$targetid", "$targetuserid");
            text = text.Replace("$targetname", "$targetusername");
            text = text.Replace("$touserid", "$targetuserid");
            text = text.Replace("$tousername", "$targetusername");

            text = text.Replace("$randuserid", "$randomuserid");
            text = text.Replace("$randusername", "$randomusername");

            text = text.Replace("$mychannel", "$streameruserid");
            text = text.Replace("$mychannelname", "$streamerusername");

            for (int i = 1; i < 10; i++)
            {
                text = text.Replace("$arg" + i, "$arg" + i + "text");
                text = text.Replace("$argl" + i, "$arg" + i + "text");
                text = text.Replace("$num" + i, "$arg" + i + "text");
            }

            text = text.Replace("$points", "$userpoints");
            text = text.Replace("$pointstext", "$userpoints");

            if (text.Contains("$randnum("))
            {
                text = ReplaceParameterVariablesEntries(text, "$randnum(", "$randomnumber");
            }

            if (text.Contains("$toppoints("))
            {
                text = ReplaceParameterVariablesEntries(text, "$toppoints(", "$top", "points");
            }

            if (text.Contains("$tophours("))
            {
                text = ReplaceParameterVariablesEntries(text, "$tophours(", "$top", "time");
            }

            text = text.Replace("$rank", "$userrankname");
            text = text.Replace("$hours", "$userhours");

            text = text.Replace("$url", "$targetuserurl");
            text = text.Replace("$game", "$targetusergame");

            text = text.Replace("$myurl", "$streameruserurl");
            text = text.Replace("$mygame", "$streamerusergame");

            text = text.Replace("$uptime", "$uptimetotal");
            text = text.Replace("$followercount", "$streameruserfollowers");
            text = text.Replace("$subcount", "$streamsubcount");

            return text;
        }

        public static bool IsValidSpecialIdentifier(string text)
        {
            return !string.IsNullOrEmpty(text) && text.All(c => Char.IsLetterOrDigit(c));
        }

        public static string ConvertToSpecialIdentifier(string text, int maxLength = 0)
        {
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

        public static async Task<IEnumerable<UserDataModel>> GetUserOrderedCurrencyList(CurrencyModel currency)
        {
            IEnumerable<UserDataModel> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
            return applicableUsers.OrderByDescending(u => currency.GetAmount(u));
        }

        public static async Task<IEnumerable<UserDataModel>> GetAllNonExemptUsers()
        {
            await ChannelSession.Settings.LoadAllUserData();

            List<UserDataModel> exemptUsers = new List<UserDataModel>(ChannelSession.Settings.UserData.Values.Where(u => !u.IsCurrencyRankExempt));
            exemptUsers.Remove(ChannelSession.GetCurrentUser().Data);
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
                        ssService = ChannelSession.Services.OBSStudio;
                        break;
                    case Model.Actions.StreamingSoftwareTypeEnum.XSplit:
                        ssService = ChannelSession.Services.XSplit;
                        break;
                    case Model.Actions.StreamingSoftwareTypeEnum.StreamlabsOBS:
                        ssService = ChannelSession.Services.StreamlabsOBS;
                        break;
                }

                string currentScene = "Unknown";
                if (ssService != null)
                {
                    currentScene = await ssService.GetCurrentScene();
                }

                this.ReplaceSpecialIdentifier("streamcurrentscene", currentScene);
            }

            int gameQueueCount = 0;
            if (ChannelSession.Services.GameQueueService != null && ChannelSession.Services.GameQueueService.IsEnabled)
            {
                gameQueueCount = ChannelSession.Services.GameQueueService.Queue.Count();
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
                        IEnumerable<UserDataModel> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
                        List<string> timeUserList = new List<string>();
                        int userPosition = 1;
                        foreach (UserDataModel timeUser in applicableUsers.OrderByDescending(u => u.ViewingMinutes).Take(total))
                        {
                            timeUserList.Add($"#{userPosition}) {timeUser.Username} - {timeUser.ViewingTimeShortString}");
                            userPosition++;
                        }

                        string result = "No users found.";
                        if (timeUserList.Count > 0)
                        {
                            result = string.Join(", ", timeUserList);
                        }
                        return result;
                    });
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TopTimeSpecialIdentifier))
                {
                    IEnumerable<UserDataModel> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
                    UserDataModel topUserData = applicableUsers.Top(u => u.ViewingMinutes);
                    UserViewModel topUser = ChannelSession.Services.User.GetActiveUserByID(topUserData.ID);
                    if (topUser == null)
                    {
                        topUser = new UserViewModel(topUserData);
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
                            foreach (UserDataModel userData in (await SpecialIdentifierStringBuilder.GetUserOrderedCurrencyList(currency)).Take(total))
                            {
                                currencyUserList.Add($"#{userPosition}) {userData.Username} - {currency.GetAmount(userData)}");
                                userPosition++;
                            }

                            string result = "No users found.";
                            if (currencyUserList.Count > 0)
                            {
                                result = string.Join(", ", currencyUserList);
                            }
                            return result;
                        });
                    }

                    if (this.ContainsSpecialIdentifier(currency.TopUserSpecialIdentifier))
                    {
                        IEnumerable<UserDataModel> applicableUsers = await SpecialIdentifierStringBuilder.GetAllNonExemptUsers();
                        UserDataModel topUserData = applicableUsers.Top(u => currency.GetAmount(u));
                        UserViewModel topUser = ChannelSession.Services.User.GetActiveUserByID(topUserData.ID);
                        if (topUser == null)
                        {
                            topUser = new UserViewModel(topUserData);
                        }
                        await this.HandleUserSpecialIdentifiers(topUser, currency.TopSpecialIdentifier);
                    }
                }
            }

            if (this.ContainsSpecialIdentifier(QuoteSpecialIdentifierHeader) && ChannelSession.Settings.QuotesEnabled && ChannelSession.Settings.Quotes.Count > 0)
            {
                UserQuoteModel quote = ChannelSession.Settings.Quotes.PickRandom();
                if (quote != null)
                {
                    this.ReplaceSpecialIdentifier(QuoteSpecialIdentifierHeader + "random", quote.ToString());
                }

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

            if (ChannelSession.Services.Twitter.IsConnected && this.ContainsSpecialIdentifier("tweet"))
            {
                IEnumerable<Tweet> tweets = await ChannelSession.Services.Twitter.GetLatestTweets();
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

            if (ChannelSession.Services.ExtraLife.IsConnected && this.ContainsSpecialIdentifier(ExtraLifeSpecialIdentifierHeader))
            {
                ExtraLifeTeam team = await ChannelSession.Services.ExtraLife.GetTeam();

                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "teamdonationgoal", team.fundraisingGoal.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "teamdonationcount", team.numDonations.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "teamdonationamount", team.sumDonations.ToString());

                ExtraLifeTeamParticipant participant = await ChannelSession.Services.ExtraLife.GetParticipant();

                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "userdonationgoal", participant.fundraisingGoal.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "userdonationcount", participant.numDonations.ToString());
                this.ReplaceSpecialIdentifier(ExtraLifeSpecialIdentifierHeader + "userdonationamount", participant.sumDonations.ToString());
            }

            if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader))
            {
                if (ChannelSession.TwitchUserConnection != null)
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

                    if (ChannelSession.TwitchStreamIsLive)
                    {
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "viewercount", ChannelSession.TwitchStreamNewAPI.viewer_count.ToString());
                    }
                    if (ChannelSession.TwitchUserNewAPI != null)
                    {
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "viewscount", ChannelSession.TwitchUserNewAPI.view_count.ToString());
                    }
                    if (ChannelSession.TwitchChannelInformation != null)
                    {
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "title", ChannelSession.TwitchChannelInformation.title);
                        this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "game", ChannelSession.TwitchChannelInformation.game_name);
                        if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount"))
                        {
                            long followCount = await ChannelSession.TwitchUserConnection.GetFollowerCount(ChannelSession.TwitchUserNewAPI);
                            this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followercount", followCount.ToString());
                        }
                        if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount"))
                        {
                            long subCount = await ChannelSession.TwitchUserConnection.GetSubscriberCount(ChannelSession.TwitchUserNewAPI);
                            this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subscribercount", subCount.ToString());
                        }
                    }
                }

                this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "chattercount", ChannelSession.Services.Chat.AllUsers.Count.ToString());
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
                        UserViewModel argUser = await ChannelSession.Services.User.GetUserFullSearch(parameters.Platform, userID: null, parameters.Arguments.ElementAt(i));
                        if (argUser != null)
                        {
                            await this.HandleUserSpecialIdentifiers(argUser, currentArgumentSpecialIdentifierHeader);
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
                await this.HandleUserSpecialIdentifiers(ChannelSession.GetCurrentUser(), StreamerSpecialIdentifierHeader);
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

                IEnumerable<UserViewModel> workableUsers = ChannelSession.Services.User.GetAllWorkableUsers();
                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomSpecialIdentifierHeader + "user"))
                {
                    if (workableUsers != null && workableUsers.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(workableUsers.ElementAt(RandomHelper.GenerateRandomNumber(workableUsers.Count())), RandomSpecialIdentifierHeader);
                    }
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomFollowerSpecialIdentifierHeader))
                {
                    IEnumerable<UserViewModel> users = workableUsers.Where(u => u.IsFollower);
                    if (users != null && users.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(users.ElementAt(RandomHelper.GenerateRandomNumber(users.Count())), RandomFollowerSpecialIdentifierHeader);
                    }
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomSubscriberSpecialIdentifierHeader))
                {
                    IEnumerable<UserViewModel> users = workableUsers.Where(u => u.IsSubscriber);
                    if (users != null && users.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(users.ElementAt(RandomHelper.GenerateRandomNumber(users.Count())), RandomSubscriberSpecialIdentifierHeader);
                    }
                }

                if (this.ContainsRegexSpecialIdentifier(SpecialIdentifierStringBuilder.RandomRegularSpecialIdentifierHeader))
                {
                    IEnumerable<UserViewModel> users = workableUsers.Where(u => u.IsRegular);
                    if (users != null && users.Count() > 0)
                    {
                        await this.HandleUserSpecialIdentifiers(users.ElementAt(RandomHelper.GenerateRandomNumber(users.Count())), RandomRegularSpecialIdentifierHeader);
                    }
                }
            }

            await this.HandleLatestSpecialIdentifier(SpecialIdentifierStringBuilder.LatestFollowerUserData);
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
            this.text = this.text.Replace(((includeSpecialIdentifierHeader) ? SpecialIdentifierHeader : string.Empty) + identifier, replacement);
        }

        public bool ContainsSpecialIdentifier(string identifier)
        {
            return this.text.Contains(SpecialIdentifierHeader + identifier);
        }

        public bool ContainsRegexSpecialIdentifier(string identifier)
        {
            return Regex.IsMatch(this.text, "\\" + SpecialIdentifierHeader + identifier);
        }

        public int GetFirstInstanceOfSpecialIdentifier(string identifier, int startIndex)
        {
            return this.text.IndexOf(SpecialIdentifierHeader + identifier, startIndex);
        }

        public override string ToString() { return this.text; }

        private async Task HandleUserSpecialIdentifiers(UserViewModel user, string identifierHeader)
        {
            if (user != null && this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader))
            {
                await user.RefreshDetails(force: true);

                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values.OrderByDescending(c => c.UserAmountSpecialIdentifier))
                {
                    if (this.ContainsSpecialIdentifier(identifierHeader + currency.UserAmountSpecialIdentifier))
                    {
                        if (this.ContainsSpecialIdentifier(identifierHeader + currency.UserPositionSpecialIdentifier))
                        {
                            List<UserDataModel> sortedUsers = (await SpecialIdentifierStringBuilder.GetUserOrderedCurrencyList(currency)).ToList();
                            int index = sortedUsers.IndexOf(user.Data);
                            this.ReplaceSpecialIdentifier(identifierHeader + currency.UserPositionSpecialIdentifier, (index + 1).ToString());
                        }

                        int amount = currency.GetAmount(user.Data);
                        RankModel rank = currency.GetRank(user.Data);
                        RankModel nextRank = currency.GetNextRank(user.Data);

                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserRankNextNameSpecialIdentifier, nextRank.Name);
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountNextDisplaySpecialIdentifier, nextRank.Amount.ToString("N0"));
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountNextSpecialIdentifier, nextRank.Amount.ToString());

                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserRankNameSpecialIdentifier, rank.Name);
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountDisplaySpecialIdentifier, amount.ToString("N0"));
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
                            var quantity = inventory.GetAmount(user.Data, item);
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
                        this.ReplaceSpecialIdentifier(identifierHeader + streamPass.UserLevelSpecialIdentifier, streamPass.GetLevel(user.Data).ToString());
                        this.ReplaceSpecialIdentifier(identifierHeader + streamPass.UserPointsDisplaySpecialIdentifier, streamPass.GetAmount(user.Data).ToString("N0"));
                        this.ReplaceSpecialIdentifier(identifierHeader + streamPass.UserAmountSpecialIdentifier, streamPass.GetAmount(user.Data).ToString());
                    }
                }

                if (this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "gamequeueposition"))
                {
                    string gameQueuePosition = "Not In Queue";
                    if (ChannelSession.Services.GameQueueService != null && ChannelSession.Services.GameQueueService.IsEnabled)
                    {
                        int position = ChannelSession.Services.GameQueueService.GetUserPosition(user);
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
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "displayroles", user.RolesLocalizedString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "primaryrole", user.PrimaryRoleString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "title", user.Title);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "moderationstrikes", user.Data.ModerationStrikes.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "color", UserViewModel.UserDefaultColor.Equals(user.Color) ? "#000000" : user.Color);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "inchat", ChannelSession.Services.User.IsUserActive(user.ID).ToString());

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "twitchid", user.TwitchID);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "twitchcolor", user.Data.TwitchColor);

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "time", user.Data.ViewingTimeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "hours", user.Data.ViewingHoursString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "mins", user.Data.ViewingMinutesString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "accountage", user.AccountAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followage", user.FollowAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subage", user.SubscribeAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subtier", user.SubscribeTierString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "submonths", user.SubscribeMonths.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subbadge", user.SubscriberBadgeLink);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isfollower", user.IsFollower.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isregular", user.IsRegular.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "issubscriber", user.IsPlatformSubscriber.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "isvip", user.UserRoles.Contains(UserRoleEnum.VIP).ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "ismod", user.UserRoles.Contains(UserRoleEnum.Mod).ToString());

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalstreamswatched", user.Data.TotalStreamsWatched.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalamountdonated", user.Data.TotalAmountDonated.ToCurrencyString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalsubsgifted", user.Data.TotalSubsGifted.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalsubsreceived", user.Data.TotalSubsReceived.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalbitscheered", user.Data.TotalBitsCheered.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalchatmessagessent", user.Data.TotalChatMessageSent.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totaltimestagged", user.Data.TotalTimesTagged.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalcommandsrun", user.Data.TotalCommandsRun.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "totalmonthssubbed", user.Data.TotalMonthsSubbed.ToString());

                string userStreamHeader = identifierHeader + UserSpecialIdentifierHeader + "stream";
                if (this.ContainsSpecialIdentifier(userStreamHeader))
                {
                    if (user.Platform.HasFlag(StreamingPlatformTypeEnum.Twitch))
                    {
                        ChannelInformationModel channel = await ChannelSession.TwitchUserConnection.GetChannelInformation(user.GetTwitchNewAPIUserModel());
                        if (channel != null)
                        {
                            this.ReplaceSpecialIdentifier(userStreamHeader + "title", channel.title);
                            this.ReplaceSpecialIdentifier(userStreamHeader + "game", channel.game_name);
                        }
                    }
                }

                if (ChannelSession.Services.Patreon.IsConnected)
                {
                    string tierName = "Not Subscribed";
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
            if (ChannelSession.TwitchUserConnection != null && this.ContainsRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredRegexSpecialIdentifier + period.ToString().ToLower()))
            {
                await this.ReplaceNumberBasedRegexSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredRegexSpecialIdentifier + period.ToString().ToLower(), async (total) =>
                {
                    string result = "No users found.";
                    BitsLeaderboardModel leaderboard = await ChannelSession.TwitchUserConnection.GetBitsLeaderboard(period, total);
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
            if (ChannelSession.TwitchUserConnection != null && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier + period.ToString().ToLower()))
            {
                BitsLeaderboardModel leaderboard = await ChannelSession.TwitchUserConnection.GetBitsLeaderboard(period, 1);
                if (leaderboard != null && leaderboard.users != null && leaderboard.users.Count > 0)
                {
                    BitsLeaderboardUserModel bitsUser = leaderboard.users.OrderBy(u => u.rank).First();

                    this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.TopBitsCheeredSpecialIdentifier + period.ToString().ToLower() + "amount", bitsUser.score.ToString());

                    UserViewModel user = ChannelSession.Services.User.GetActiveUserByPlatformID(StreamingPlatformTypeEnum.Twitch, bitsUser.user_id);
                    if (user == null)
                    {
                        user = await UserViewModel.Create(new Twitch.Base.Models.NewAPI.Users.UserModel()
                        {
                            id = bitsUser.user_id,
                            login = bitsUser.user_name
                        });
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
                    if (!ChannelSession.Settings.UserData.TryGetValue(userID, out UserDataModel userData))
                    {
                        userData = await ChannelSession.Settings.GetUserDataByID(userID);
                    }

                    if (userData != null)
                    {
                        await this.HandleUserSpecialIdentifiers(new UserViewModel(userData), userkey);
                    }
                }
            }
        }

        private async Task ReplaceNumberBasedRegexSpecialIdentifier(string regex, Func<int, Task<string>> replacer)
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

        private async Task ReplaceNumberRangeBasedRegexSpecialIdentifier(string regex, Func<int, int, Task<string>> replacer)
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
    }
}
