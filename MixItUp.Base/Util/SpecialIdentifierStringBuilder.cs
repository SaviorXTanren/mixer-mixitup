using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Patronage;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Util
{
    public class RandomUserSpecialIdentiferGroup
    {
        public UserViewModel RandomUser { get; set; }
        public UserViewModel RandomFollower { get; set; }
        public UserViewModel RandomSubscriber { get; set; }
    }

    public class SpecialIdentifierStringBuilder
    {
        public const string SpecialIdentifierHeader = "$";

        public const string UptimeSpecialIdentifierHeader = "uptime";
        public const string StartSpecialIdentifierHeader = "start";

        public const string Top10SpecialIdentifierHeader = "top10";
        public const string UserSpecialIdentifierHeader = "user";
        public const string ArgSpecialIdentifierHeader = "arg";
        public const string StreamerSpecialIdentifierHeader = "streamer";
        public const string TargetSpecialIdentifierHeader = "target";
        public const string RandomSpecialIdentifierHeader = "random";
        public const string RandomFollowerSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "follower";
        public const string RandomSubscriberSpecialIdentifierHeader = RandomSpecialIdentifierHeader + "sub";
        public const string RandomNumberSpecialIdentifier = RandomSpecialIdentifierHeader + "number";
        public const string FeaturedChannelsSpecialIdentifer = "featuredchannels";
        public const string CostreamUsersSpecialIdentifier = "costreamusers";

        public const string StreamSpecialIdentifierHeader = "stream";
        public const string StreamHostCountSpecialIdentifier = StreamSpecialIdentifierHeader + "hostcount";

        public const string MilestoneSpecialIdentifierHeader = "milestone";

        public const string CurrentSongIdentifierHeader = "currentsong";
        public const string NextSongIdentifierHeader = "nextsong";

        public const string DonationSourceSpecialIdentifier = "donationsource";
        public const string DonationAmountNumberSpecialIdentifier = "donationamountnumber";
        public const string DonationAmountNumberDigitsSpecialIdentifier = "donationamountnumberdigits";
        public const string DonationAmountSpecialIdentifier = "donationamount";
        public const string DonationMessageSpecialIdentifier = "donationmessage";
        public const string DonationImageSpecialIdentifier = "donationimage";

        public const string ExtraLifeSpecialIdentifierHeader = "extralife";

        public const string UnicodeSpecialIdentifierHeader = "unicode";

        public const string InteractiveTextBoxTextEntrySpecialIdentifierHelpText = "User Text Entered = " + SpecialIdentifierStringBuilder.SpecialIdentifierHeader +
            SpecialIdentifierStringBuilder.ArgSpecialIdentifierHeader + "1text";

        private static Dictionary<string, string> CustomSpecialIdentifiers = new Dictionary<string, string>();

        private static Dictionary<Guid, RandomUserSpecialIdentiferGroup> RandomUserSpecialIdentifierGroups = new Dictionary<Guid, RandomUserSpecialIdentiferGroup>();

        public static void AddCustomSpecialIdentifier(string specialIdentifier, string replacement)
        {
            SpecialIdentifierStringBuilder.CustomSpecialIdentifiers[specialIdentifier] = replacement;
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

        public static string ConvertToSpecialIdentifier(string text)
        {
            StringBuilder specialIdentifier = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]))
                {
                    specialIdentifier.Append(text[i]);
                }
            }
            return specialIdentifier.ToString().ToLower();
        }

        public static async Task AssignRandomUserSpecialIdentifierGroup(Guid id)
        {
            SpecialIdentifierStringBuilder.RandomUserSpecialIdentifierGroups[id] = new RandomUserSpecialIdentiferGroup();
            IEnumerable<UserViewModel> users = await ChannelSession.ActiveUsers.GetAllWorkableUsers();
            users = users.Where(u => !u.ID.Equals(ChannelSession.User.id));
            if (users.Count() > 0)
            {
                SpecialIdentifierStringBuilder.RandomUserSpecialIdentifierGroups[id].RandomUser = users.ElementAt(RandomHelper.GenerateRandomNumber(users.Count()));
                users = users.Where(u => u.IsFollower);
                if (users.Count() > 0)
                {
                    SpecialIdentifierStringBuilder.RandomUserSpecialIdentifierGroups[id].RandomFollower = users.ElementAt(RandomHelper.GenerateRandomNumber(users.Count()));
                    users = users.Where(u => u.GetsSubscriberBenefits);
                    if (users.Count() > 0)
                    {
                        SpecialIdentifierStringBuilder.RandomUserSpecialIdentifierGroups[id].RandomSubscriber = users.ElementAt(RandomHelper.GenerateRandomNumber(users.Count()));
                    }
                }
            }
        }

        public static void ClearRandomUserSpecialIdentifierGroup(Guid id) { SpecialIdentifierStringBuilder.RandomUserSpecialIdentifierGroups.Remove(id); }

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

        private string text;
        private Guid randomUserSpecialIdentifierGroupID;
        private bool encode;

        public SpecialIdentifierStringBuilder(string text, Guid randomUserSpecialIdentifierGroupID, bool encode = false)
        {
            this.text = text;
            this.randomUserSpecialIdentifierGroupID = randomUserSpecialIdentifierGroupID;
            this.encode = encode;
        }

        public async Task ReplaceCommonSpecialModifiers(UserViewModel user, IEnumerable<string> arguments = null)
        {
            foreach (string counter in ChannelSession.Counters.Keys)
            {
                this.ReplaceSpecialIdentifier(counter, ChannelSession.Counters[counter].ToString());
            }

            foreach (var kvp in SpecialIdentifierStringBuilder.CustomSpecialIdentifiers)
            {
                this.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
            }

            this.ReplaceSpecialIdentifier("timedigits", DateTimeOffset.Now.ToString("HHmm"));
            this.ReplaceSpecialIdentifier("datetime", DateTimeOffset.Now.ToString("g"));
            this.ReplaceSpecialIdentifier("date", DateTimeOffset.Now.ToString("d"));
            this.ReplaceSpecialIdentifier("time", DateTimeOffset.Now.ToString("t"));
            this.ReplaceSpecialIdentifier("linebreak", Environment.NewLine);

            if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.Top10SpecialIdentifierHeader))
            {
                Dictionary<uint, UserDataViewModel> allUsersDictionary = ChannelSession.Settings.UserData.ToDictionary();
                allUsersDictionary.Remove(ChannelSession.Channel.user.id);

                IEnumerable<UserDataViewModel> allUsers = allUsersDictionary.Select(kvp => kvp.Value);
                allUsers = allUsers.Where(u => !u.IsCurrencyRankExempt);

                foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                {
                    if (this.ContainsSpecialIdentifier(currency.Top10SpecialIdentifier))
                    {
                        List<string> currencyUserList = new List<string>();
                        int userPosition = 1;
                        foreach (UserDataViewModel currencyUser in allUsers.OrderByDescending(u => u.GetCurrencyAmount(currency)).Take(10))
                        {
                            currencyUserList.Add($"#{userPosition}) {currencyUser.UserName} - {currencyUser.GetCurrencyAmount(currency)}");
                            userPosition++;
                        }

                        if (currencyUserList.Count > 0)
                        {
                            this.ReplaceSpecialIdentifier(currency.Top10SpecialIdentifier, string.Join(", ", currencyUserList));
                        }
                        else
                        {
                            this.ReplaceSpecialIdentifier(currency.Top10SpecialIdentifier, "No users found.");
                        }
                    }
                }

                if (this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.Top10SpecialIdentifierHeader + "time"))
                {
                    List<string> timeUserList = new List<string>();
                    int userPosition = 1;
                    foreach (UserDataViewModel timeUser in allUsers.OrderByDescending(u => u.ViewingMinutes).Take(10))
                    {
                        timeUserList.Add($"#{userPosition}) {timeUser.UserName} - {timeUser.ViewingTimeShortString}");
                        userPosition++;
                    }

                    if (timeUserList.Count > 0)
                    {
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.Top10SpecialIdentifierHeader + "time", string.Join(", ", timeUserList));
                    }
                    else
                    {
                        this.ReplaceSpecialIdentifier(SpecialIdentifierStringBuilder.Top10SpecialIdentifierHeader + "time", "No users found.");
                    }
                }
            }

            if (this.ContainsSpecialIdentifier(CurrentSongIdentifierHeader))
            {
                SongRequestItem song = null;

                if (ChannelSession.Services.SongRequestService != null && ChannelSession.Services.SongRequestService.IsEnabled)
                {
                    song = await ChannelSession.Services.SongRequestService.GetCurrentlyPlaying();
                }

                if (song != null)
                {
                    this.ReplaceSpecialIdentifier(CurrentSongIdentifierHeader + "title", song.Name);
                    this.ReplaceSpecialIdentifier(CurrentSongIdentifierHeader + "username", song.User.UserName);
                    this.ReplaceSpecialIdentifier(CurrentSongIdentifierHeader + "albumimage", song.AlbumImage ?? string.Empty);
                }
                else
                {
                    this.ReplaceSpecialIdentifier(CurrentSongIdentifierHeader + "title", "No Song");
                    this.ReplaceSpecialIdentifier(CurrentSongIdentifierHeader + "username", "Nobody");
                    this.ReplaceSpecialIdentifier(CurrentSongIdentifierHeader + "albumimage", string.Empty);
                }
            }

            if (this.ContainsSpecialIdentifier(NextSongIdentifierHeader))
            {
                SongRequestItem song = null;

                if (ChannelSession.Services.SongRequestService != null && ChannelSession.Services.SongRequestService.IsEnabled)
                {
                    song = await ChannelSession.Services.SongRequestService.GetNextTrack();
                }

                if (song != null)
                {
                    this.ReplaceSpecialIdentifier(NextSongIdentifierHeader + "title", song.Name);
                    this.ReplaceSpecialIdentifier(NextSongIdentifierHeader + "username", song.User.UserName);
                    this.ReplaceSpecialIdentifier(NextSongIdentifierHeader + "albumimage", song.AlbumImage ?? string.Empty);
                }
                else
                {
                    this.ReplaceSpecialIdentifier(NextSongIdentifierHeader + "title", "No Song");
                    this.ReplaceSpecialIdentifier(NextSongIdentifierHeader + "username", "Nobody");
                    this.ReplaceSpecialIdentifier(NextSongIdentifierHeader + "albumimage", string.Empty);
                }
            }

            if (this.ContainsSpecialIdentifier(UptimeSpecialIdentifierHeader) || this.ContainsSpecialIdentifier(StartSpecialIdentifierHeader))
            {
                DateTimeOffset startTime = await UptimeChatCommand.GetStartTime();
                if (startTime > DateTimeOffset.MinValue)
                {
                    TimeSpan duration = DateTimeOffset.Now.Subtract(startTime);

                    this.ReplaceSpecialIdentifier(StartSpecialIdentifierHeader + "datetime", startTime.ToString("g"));
                    this.ReplaceSpecialIdentifier(StartSpecialIdentifierHeader + "date", startTime.ToString("d"));
                    this.ReplaceSpecialIdentifier(StartSpecialIdentifierHeader + "time", startTime.ToString("t"));

                    this.ReplaceSpecialIdentifier(UptimeSpecialIdentifierHeader + "total", (int)duration.TotalHours + duration.ToString("\\:mm"));
                    this.ReplaceSpecialIdentifier(UptimeSpecialIdentifierHeader + "hours", ((int)duration.TotalHours).ToString());
                    this.ReplaceSpecialIdentifier(UptimeSpecialIdentifierHeader + "minutes", duration.ToString("mm"));
                    this.ReplaceSpecialIdentifier(UptimeSpecialIdentifierHeader + "seconds", duration.ToString("ss"));
                }
            }

            if (this.ContainsSpecialIdentifier(CostreamUsersSpecialIdentifier))
            {
                this.ReplaceSpecialIdentifier(CostreamUsersSpecialIdentifier, await CostreamChatCommand.GetCostreamUsers());
            }

            if (ChannelSession.Services.Twitter != null && this.ContainsSpecialIdentifier("tweet"))
            {
                IEnumerable<Tweet> tweets = await ChannelSession.Services.Twitter.GetLatestTweets();
                if (tweets.Count() > 0)
                {
                    Tweet latestTweet = tweets.FirstOrDefault();
                    DateTimeOffset latestTweetLocalTime = latestTweet.DateTime.ToLocalTime();

                    this.ReplaceSpecialIdentifier("tweetlatesturl", latestTweet.TweetLink);
                    this.ReplaceSpecialIdentifier("tweetlatesttext", latestTweet.Text);
                    this.ReplaceSpecialIdentifier("tweetlatestdatetime", latestTweetLocalTime.ToString("g"));
                    this.ReplaceSpecialIdentifier("tweetlatestdate", latestTweetLocalTime.ToString("d"));
                    this.ReplaceSpecialIdentifier("tweetlatesttime", latestTweetLocalTime.ToString("t"));

                    Tweet streamTweet = tweets.FirstOrDefault(t => t.Links.Any(l => l.ToLower().Contains(string.Format("mixer.com/{0}", ChannelSession.User.username.ToLower()))));
                    if (streamTweet != null)
                    {
                        DateTimeOffset streamTweetLocalTime = streamTweet.DateTime.ToLocalTime();
                        this.ReplaceSpecialIdentifier("tweetstreamurl", streamTweet.TweetLink);
                        this.ReplaceSpecialIdentifier("tweetstreamtext", streamTweet.Text);
                        this.ReplaceSpecialIdentifier("tweetstreamdatetime", streamTweetLocalTime.ToString("g"));
                        this.ReplaceSpecialIdentifier("tweetstreamdate", streamTweetLocalTime.ToString("d"));
                        this.ReplaceSpecialIdentifier("tweetstreamtime", streamTweetLocalTime.ToString("t"));
                    }
                }
            }

            if (ChannelSession.Services.Spotify != null && this.ContainsSpecialIdentifier("spotify"))
            {
                SpotifyUserProfile profile = await ChannelSession.Services.Spotify.GetCurrentProfile();
                if (profile != null)
                {
                    this.ReplaceSpecialIdentifier("spotifyprofileurl", profile.Link);
                }

                SpotifyCurrentlyPlaying currentlyPlaying = await ChannelSession.Services.Spotify.GetCurrentlyPlaying();
                if (currentlyPlaying != null)
                {
                    this.ReplaceSpecialIdentifier("spotifycurrentlyplaying", currentlyPlaying.ToString());
                }
            }

            if (ChannelSession.Services.ExtraLife.IsConnected() && this.ContainsSpecialIdentifier(ExtraLifeSpecialIdentifierHeader))
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

            if (this.ContainsSpecialIdentifier(FeaturedChannelsSpecialIdentifer))
            {
                IEnumerable<ExpandedChannelModel> featuredChannels = await ChannelSession.Connection.GetFeaturedChannels();
                if (featuredChannels != null)
                {
                    this.ReplaceSpecialIdentifier(FeaturedChannelsSpecialIdentifer, string.Join(", ", featuredChannels.Select(c => "@" + c.user.username)));
                }
            }

            if (this.ContainsSpecialIdentifier(StreamSpecialIdentifierHeader))
            {
                ChannelDetailsModel details = await ChannelSession.Connection.GetChannelDetails(ChannelSession.Channel);
                if (details != null)
                {
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "title", details.name);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "agerating", details.audience);
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "followcount", details.numFollowers.ToString());
                    this.ReplaceSpecialIdentifier(StreamSpecialIdentifierHeader + "subcount", details.numSubscribers.ToString());
                }

                if (this.ContainsSpecialIdentifier(StreamHostCountSpecialIdentifier))
                {
                    IEnumerable<ChannelAdvancedModel> hosters = await ChannelSession.Connection.GetHosters(ChannelSession.Channel);
                    if (hosters != null)
                    {
                        this.ReplaceSpecialIdentifier(StreamHostCountSpecialIdentifier, hosters.Count().ToString());
                    }
                }
            }

            if (this.ContainsSpecialIdentifier(MilestoneSpecialIdentifierHeader))
            {
                PatronageStatusModel patronageStatus = await ChannelSession.Connection.GetPatronageStatus(ChannelSession.Channel);
                if (patronageStatus != null)
                {
                    PatronagePeriodModel patronagePeriod = await ChannelSession.Connection.GetPatronagePeriod(patronageStatus);
                    if (patronagePeriod != null)
                    {
                        IEnumerable<PatronageMilestoneModel> patronageMilestones = patronagePeriod.milestoneGroups.SelectMany(mg => mg.milestones);

                        PatronageMilestoneModel patronageMilestone = patronageMilestones.FirstOrDefault(m => m.id == patronageStatus.currentMilestoneId);
                        if (patronageMilestone != null)
                        {
                            double milestoneReward = Math.Round(((double)patronageMilestone.reward) / 100.0, 2);
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "amount", patronageMilestone.target.ToString());
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "remainingamount", (patronageMilestone.target - patronageStatus.patronageEarned).ToString());
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "reward", string.Format("{0:C}", milestoneReward));
                        }

                        PatronageMilestoneModel patronageNextMilestone = patronageMilestones.FirstOrDefault(m => m.id == (patronageStatus.currentMilestoneId + 1));
                        if (patronageNextMilestone != null)
                        {
                            double milestoneNextReward = Math.Round(((double)patronageNextMilestone.reward) / 100.0, 2);
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "nextamount", patronageNextMilestone.target.ToString());
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "remainingnextamount", (patronageNextMilestone.target - patronageStatus.patronageEarned).ToString());
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "nextreward", string.Format("{0:C}", milestoneNextReward));
                        }

                        PatronageMilestoneModel patronageFinalMilestone = patronageMilestones.OrderByDescending(m => m.id).FirstOrDefault();
                        if (patronageNextMilestone != null)
                        {
                            double milestoneFinalReward = Math.Round(((double)patronageFinalMilestone.reward) / 100.0, 2);
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "finalamount", patronageFinalMilestone.target.ToString());
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "remainingfinalamount", (patronageFinalMilestone.target - patronageStatus.patronageEarned).ToString());
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "finalreward", string.Format("{0:C}", milestoneFinalReward));
                        }

                        IEnumerable<PatronageMilestoneModel> patronageMilestonesEarned = patronageMilestones.Where(m => m.target <= patronageStatus.patronageEarned);
                        if (patronageMilestonesEarned.Count() > 0)
                        {
                            long patronageEarnedReward = patronageMilestonesEarned.Max(m => m.reward);
                            double patronageEarnedRewardDollars = Math.Round(((double)patronageEarnedReward) / 100.0, 2);

                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "earnedamount", patronageStatus.patronageEarned.ToString());
                            this.ReplaceSpecialIdentifier(MilestoneSpecialIdentifierHeader + "earnedreward", string.Format("{0:C}", patronageEarnedRewardDollars));
                        }
                    }
                }
            }

            if (this.ContainsSpecialIdentifier(UserSpecialIdentifierHeader))
            {
                await this.HandleUserSpecialIdentifiers(user, string.Empty);
            }

            if (arguments != null)
            {
                for (int i = 0; i < arguments.Count(); i++)
                {
                    string currentArgumentSpecialIdentifierHeader = ArgSpecialIdentifierHeader + (i + 1);
                    if (this.ContainsSpecialIdentifier(currentArgumentSpecialIdentifierHeader))
                    {
                        UserViewModel argUser = await this.GetUserFromArgument(arguments.ElementAt(i));
                        if (argUser != null)
                        {
                            await this.HandleUserSpecialIdentifiers(argUser, currentArgumentSpecialIdentifierHeader);
                        }

                        this.ReplaceSpecialIdentifier(currentArgumentSpecialIdentifierHeader + "text", arguments.ElementAt(i));
                    }
                }

                this.ReplaceSpecialIdentifier("allargs", string.Join(" ", arguments));
            }

            if (this.ContainsSpecialIdentifier(TargetSpecialIdentifierHeader))
            {
                UserViewModel targetUser = null;
                if (arguments.Count() > 0)
                {
                    targetUser = await this.GetUserFromArgument(arguments.ElementAt(0));
                }

                if (targetUser == null)
                {
                    targetUser = user;
                }

                await this.HandleUserSpecialIdentifiers(targetUser, TargetSpecialIdentifierHeader);
            }

            if (this.ContainsSpecialIdentifier(StreamerSpecialIdentifierHeader))
            {
                await this.HandleUserSpecialIdentifiers(new UserViewModel(ChannelSession.Channel.user), StreamerSpecialIdentifierHeader);
            }

            if (this.ContainsSpecialIdentifier(RandomSpecialIdentifierHeader))
            {
                if (this.randomUserSpecialIdentifierGroupID != Guid.Empty && RandomUserSpecialIdentifierGroups.ContainsKey(this.randomUserSpecialIdentifierGroupID))
                {
                    if (RandomUserSpecialIdentifierGroups[randomUserSpecialIdentifierGroupID].RandomUser != null && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomSpecialIdentifierHeader + "user"))
                    {
                        await this.HandleUserSpecialIdentifiers(RandomUserSpecialIdentifierGroups[randomUserSpecialIdentifierGroupID].RandomUser, RandomSpecialIdentifierHeader);
                    }

                    if (RandomUserSpecialIdentifierGroups[randomUserSpecialIdentifierGroupID].RandomFollower != null && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomFollowerSpecialIdentifierHeader + "user"))
                    {
                        await this.HandleUserSpecialIdentifiers(RandomUserSpecialIdentifierGroups[randomUserSpecialIdentifierGroupID].RandomFollower, RandomFollowerSpecialIdentifierHeader);
                    }

                    if (RandomUserSpecialIdentifierGroups[randomUserSpecialIdentifierGroupID].RandomSubscriber != null && this.ContainsSpecialIdentifier(SpecialIdentifierStringBuilder.RandomSubscriberSpecialIdentifierHeader + "user"))
                    {
                        await this.HandleUserSpecialIdentifiers(RandomUserSpecialIdentifierGroups[randomUserSpecialIdentifierGroupID].RandomSubscriber, RandomSubscriberSpecialIdentifierHeader);
                    }
                }

                if (this.ContainsSpecialIdentifier(RandomNumberSpecialIdentifier))
                {
                    this.ReplaceNumberBasedSpecialIdentifier(RandomNumberSpecialIdentifier, (maxNumber) =>
                    {
                        int number = RandomHelper.GenerateRandomNumber(maxNumber) + 1;
                        return number.ToString();
                    });
                }
            }

            if (this.ContainsSpecialIdentifier(UnicodeSpecialIdentifierHeader))
            {
                this.ReplaceNumberBasedSpecialIdentifier(UnicodeSpecialIdentifierHeader, (number) =>
                {
                    char uChar = (char)number;
                    return uChar.ToString();
                });
            }
        }

        public void ReplaceSpecialIdentifier(string identifier, string replacement)
        {
            replacement = (replacement == null) ? string.Empty : replacement;
            if (encode)
            {
                replacement = HttpUtility.UrlEncode(replacement);
            }
            this.text = this.text.Replace(SpecialIdentifierHeader + identifier, replacement);
        }

        public bool ContainsSpecialIdentifier(string identifier)
        {
            return this.text.Contains(SpecialIdentifierHeader + identifier);
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
                await user.RefreshDetails();

                if (ChannelSession.Settings.UserData.ContainsKey(user.ID))
                {
                    UserDataViewModel userData = ChannelSession.Settings.UserData[user.ID];

                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values.OrderByDescending(c => c.UserAmountSpecialIdentifier))
                    {
                        UserCurrencyDataViewModel currencyData = userData.GetCurrency(currency);
                        UserRankViewModel rank = currencyData.GetRank();
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserRankNameSpecialIdentifier, rank.Name);
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountSpecialIdentifier, currencyData.Amount.ToString());
                    }

                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "time", userData.ViewingTimeString);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "hours", userData.ViewingHoursString);
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "mins", userData.ViewingMinutesString);

                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "moderationstrikes", userData.ModerationStrikes.ToString());
                }

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "primaryrole", user.PrimaryRoleString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "avatar", user.AvatarLink);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "url", "https://www.mixer.com/" + user.UserName);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "name", user.UserName);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "id", user.ID.ToString());
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "sparks", user.Sparks.ToString());

                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "mixerage", user.MixerAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followage", user.FollowAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "subage", user.SubscribeAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "submonths", user.SubscribeMonths.ToString());

                if (this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followers") || this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "game") ||
                    this.ContainsSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "channel"))
                {
                    ExpandedChannelModel channel = await ChannelSession.Connection.GetChannel(user.UserName);

                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "followers", channel?.numFollowers.ToString() ?? "0");

                    if (channel.type != null)
                    {
                        this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "gameimage", channel.type.coverUrl);
                        this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "game", channel.type.name.ToString());
                    }

                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "channelid", channel.id.ToString());
                    this.ReplaceSpecialIdentifier(identifierHeader + UserSpecialIdentifierHeader + "channellive", channel.online.ToString());
                }
            }
        }

        private async Task<UserViewModel> GetUserFromArgument(string argument)
        {
            string username = argument.Replace("@", "");
            UserModel argUserModel = await ChannelSession.Connection.GetUser(username);
            if (argUserModel != null)
            {
                return new UserViewModel(argUserModel);
            }
            return null;
        }

        private void ReplaceNumberBasedSpecialIdentifier(string header, Func<int, string> replacer)
        {
            int startIndex = 0;
            do
            {
                startIndex = this.GetFirstInstanceOfSpecialIdentifier(header, startIndex);
                if (startIndex >= 0)
                {
                    int endIndex = 0;
                    for (endIndex = startIndex + header.Length + 1; endIndex < this.text.Length; endIndex++)
                    {
                        if (!char.IsDigit(this.text[endIndex]))
                        {
                            break;
                        }
                    }

                    if (endIndex <= this.text.Length)
                    {
                        string specialIdentifier = this.text.Substring(startIndex, endIndex - startIndex).Replace(SpecialIdentifierHeader, "");
                        if (int.TryParse(specialIdentifier.Replace(header, ""), out int number) && number > 0)
                        {
                            this.ReplaceSpecialIdentifier(specialIdentifier, replacer(number));
                        }
                        else
                        {
                            startIndex = endIndex;
                        }
                    }
                    else
                    {
                        startIndex = endIndex;
                    }
                }
            } while (startIndex > 0);
        }
    }
}
