using Mixer.Base.Model.Game;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class SpecialIdentifierStringBuilder
    {
        public const string SpecialIdentifierHeader = "$";

        private const string ArgSpecialIdentifierHeader = "arg";
        private const string RandomSpecialIdentifierHeader = "random";
        private const string StreamerSpecialIdentifierHeader = "streamer";
        private const string RandomNumberSpecialIdentifier = "randomnumber";

        private static Dictionary<string, string> CustomSpecialIdentifiers = new Dictionary<string, string>();

        public static void AddCustomSpecialIdentifier(string specialIdentifier, string replacement)
        {
            SpecialIdentifierStringBuilder.CustomSpecialIdentifiers[specialIdentifier] = replacement;
        }

        public static string ConvertScorpBotText(string text)
        {
            text = text.Replace("$user", "@$username");
            text = text.Replace("$url", "$userurl");
            text = text.Replace("$hours", "$userhours");

            text = text.Replace("$target", "@$arg1username");
            for (int i = 1; i < 10; i++)
            {
                text = text.Replace("$target" + i, "@$arg" + i + "username");
            }

            text = text.Replace("$randuser", "@$randomusername");

            text = text.Replace("$msg", "$allargs");

            return text;
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

        private string text;

        public SpecialIdentifierStringBuilder(string text) { this.text = text; }

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

            this.ReplaceSpecialIdentifier("datetime", DateTimeOffset.Now.ToString("g"));
            this.ReplaceSpecialIdentifier("date", DateTimeOffset.Now.ToString("d"));
            this.ReplaceSpecialIdentifier("time", DateTimeOffset.Now.ToString("t"));

            if (this.ContainsSpecialIdentifier("uptime") || this.ContainsSpecialIdentifier("starttime"))
            {
                DateTimeOffset startTime = await UptimeChatCommand.GetStartTime();
                if (startTime > DateTimeOffset.MinValue)
                {
                    TimeSpan duration = DateTimeOffset.Now.Subtract(startTime);

                    this.ReplaceSpecialIdentifier("startdatetime", startTime.ToString("g"));
                    this.ReplaceSpecialIdentifier("startdate", startTime.ToString("d"));
                    this.ReplaceSpecialIdentifier("starttime", startTime.ToString("t"));

                    this.ReplaceSpecialIdentifier("uptimetotal", duration.ToString("h\\:mm"));
                    this.ReplaceSpecialIdentifier("uptimehours", duration.ToString("%h"));
                    this.ReplaceSpecialIdentifier("uptimeminutes", duration.ToString("mm"));
                    this.ReplaceSpecialIdentifier("uptimeseconds", duration.ToString("ss"));
                }
            }

            if (ChannelSession.Services.Twitter != null && this.ContainsSpecialIdentifier("tweet"))
            {
                IEnumerable<Tweet> tweets = await ChannelSession.Services.Twitter.GetLatestTweets();
                if (tweets.Count() > 0)
                {
                    this.ReplaceSpecialIdentifier("tweetlatesturl", tweets.FirstOrDefault().TweetLink);
                    this.ReplaceSpecialIdentifier("tweetlatesttext", tweets.FirstOrDefault().Text);

                    Tweet streamTweet = tweets.FirstOrDefault(t => t.Links.Any(l => l.ToLower().Contains(string.Format("mixer.com/{0}", ChannelSession.User.username.ToLower()))));
                    if (streamTweet != null)
                    {
                        this.ReplaceSpecialIdentifier("tweetstreamurl", streamTweet.TweetLink);
                        this.ReplaceSpecialIdentifier("tweetstreamtext", streamTweet.Text);
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

            await this.HandleUserSpecialIdentifiers(user, string.Empty);

            if (arguments != null)
            {
                for (int i = 0; i < arguments.Count(); i++)
                {
                    string username = arguments.ElementAt(i);
                    username = username.Replace("@", "");

                    UserModel argUserModel = await ChannelSession.Connection.GetUser(username);
                    if (argUserModel != null)
                    {
                        UserViewModel argUser = new UserViewModel(argUserModel);
                        await this.HandleUserSpecialIdentifiers(argUser, ArgSpecialIdentifierHeader + (i + 1));
                    }

                    this.ReplaceSpecialIdentifier(ArgSpecialIdentifierHeader + (i + 1) + "text", arguments.ElementAt(i));
                }

                this.ReplaceSpecialIdentifier("allargs", string.Join(" ", arguments));
            }

            await this.HandleUserSpecialIdentifiers(new UserViewModel(ChannelSession.Channel.user), StreamerSpecialIdentifierHeader);

            await this.HandleUserSpecialIdentifiers(ChannelSession.ChannelUsers.PickRandom(), RandomSpecialIdentifierHeader);
            if (this.ContainsSpecialIdentifier(RandomNumberSpecialIdentifier))
            {
                int startIndex = 0;
                do
                {
                    startIndex = this.GetFirstInstanceOfSpecialIdentifier(RandomNumberSpecialIdentifier, startIndex);
                    if (startIndex >= 0)
                    {
                        int endIndex = 0;
                        for (endIndex = startIndex + RandomNumberSpecialIdentifier.Length + 1; endIndex < this.text.Length; endIndex++)
                        {
                            if (!char.IsDigit(this.text[endIndex]))
                            {
                                break;
                            }
                        }

                        if (endIndex < this.text.Length)
                        {
                            string randomSI = this.text.Substring(startIndex, endIndex - startIndex).Replace(SpecialIdentifierHeader, "");
                            if (int.TryParse(randomSI.Replace(RandomNumberSpecialIdentifier, ""), out int randomNumberMax) && randomNumberMax > 0)
                            {
                                Random random = new Random();
                                int randomNumber = (random.Next() % randomNumberMax) + 1;
                                this.ReplaceSpecialIdentifier(randomSI, randomNumber.ToString());
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

        public void ReplaceSpecialIdentifier(string identifier, string replacement)
        {
            replacement = (replacement == null) ? string.Empty : replacement;
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
            if (user != null)
            {
                await user.SetDetails();

                if (ChannelSession.Settings.UserData.ContainsKey(user.ID))
                {
                    UserDataViewModel userData = ChannelSession.Settings.UserData[user.ID];

                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        UserCurrencyDataViewModel currencyData = userData.GetCurrency(currency);
                        UserRankViewModel rank = currencyData.GetRank();
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserRankNameSpecialIdentifier, rank.Name);
                        this.ReplaceSpecialIdentifier(identifierHeader + currency.UserAmountSpecialIdentifier, currencyData.Amount.ToString());
                    }

                    this.ReplaceSpecialIdentifier(identifierHeader + "usertime", userData.ViewingTimeString);
                    this.ReplaceSpecialIdentifier(identifierHeader + "userhours", userData.ViewingHoursString);
                    this.ReplaceSpecialIdentifier(identifierHeader + "usermins", userData.ViewingMinutesString);
                }

                if (this.ContainsSpecialIdentifier(identifierHeader + "usergame"))
                {
                    GameTypeModel game = await ChannelSession.Connection.GetGameType(user.GameTypeID);
                    if (game != null)
                    {
                        this.ReplaceSpecialIdentifier(identifierHeader + "usergame", game.name.ToString());
                    }
                    else
                    {
                        this.ReplaceSpecialIdentifier(identifierHeader + "usergame", "Unknown");
                    }
                }

                this.ReplaceSpecialIdentifier(identifierHeader + "userfollowage", user.FollowAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + "usersubage", user.SubscribeAgeString);
                this.ReplaceSpecialIdentifier(identifierHeader + "usersubmonths", user.SubscribeMonths.ToString());

                this.ReplaceSpecialIdentifier(identifierHeader + "useravatar", user.AvatarLink);
                this.ReplaceSpecialIdentifier(identifierHeader + "userurl", "https://www.mixer.com/" + user.UserName);
                this.ReplaceSpecialIdentifier(identifierHeader + "username", user.UserName);
                this.ReplaceSpecialIdentifier(identifierHeader + "userid", user.ID.ToString());
            }
        }
    }
}
