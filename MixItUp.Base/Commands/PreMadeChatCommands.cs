using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.User;
using Mixer.Base.Web;
using MixItUp.Base.Actions;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class PreMadeChatCommandSettings
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public UserRole Permissions { get; set; }
        [DataMember]
        public int Cooldown { get; set; }

        public PreMadeChatCommandSettings() { }

        public PreMadeChatCommandSettings(PreMadeChatCommand command)
        {
            this.Name = command.Name;
            this.IsEnabled = command.IsEnabled;
            this.Permissions = command.Permissions;
            this.Cooldown = command.Cooldown;
        }
    }

    internal class CustomAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CustomAction.asyncSemaphore; } }

        private Func<UserViewModel, IEnumerable<string>, Task> action;

        internal CustomAction(Func<UserViewModel, IEnumerable<string>, Task> action) : base(ActionTypeEnum.Custom) { this.action = action; }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments) { await this.action(user, arguments); }
    }

    public class PreMadeChatCommand : ChatCommand
    {
        public PreMadeChatCommand(string name, string command, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyCost = null)
            : base(name, command, lowestAllowedRole, cooldown, currencyCost)
        { }

        public PreMadeChatCommand(string name, List<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyCost = null)
            : base(name, commands, lowestAllowedRole, cooldown, currencyCost)
        { }

        public void UpdateFromSettings(PreMadeChatCommandSettings settings)
        {
            this.IsEnabled = settings.IsEnabled;
            this.Permissions = settings.Permissions;
            this.Cooldown = settings.Cooldown;
        }

        public string GetMixerAge(UserModel user)
        {
            return user.username + "'s Mixer Age: " + user.createdAt.GetValueOrDefault().GetAge();
        }

        public string GetFollowAge(UserModel user, DateTimeOffset followDate)
        {
            return user.username + "'s Follow Age: " + followDate.GetAge();
        }

        public string GetSubscribeAge(UserModel user, DateTimeOffset subscribeDate)
        {
            return user.username + "'s Subscribe Age: " + subscribeDate.GetAge();
        }
    }

    public class MixItUpChatCommand : PreMadeChatCommand
    {
        public MixItUpChatCommand()
            : base("Mix It Up", "mixitup", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    await ChannelSession.Chat.SendMessage("This channel uses the Mix It Up app to improve their stream. Check out http://mixitupapp.com for more information!");
                }
            }));
        }
    }

    public class CommandsChatCommand : PreMadeChatCommand
    {
        public CommandsChatCommand()
            : base("Commands", "commands", UserRole.User, 0)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    IEnumerable<PermissionsCommandBase> commands = ChannelSession.AllChatCommands;
                    commands = commands.Where(c => user.PrimaryRole >= c.Permissions);
                    if (commands.Count() > 0)
                    {
                        IEnumerable<string> commandTriggers = commands.SelectMany(c => c.Commands);
                        commandTriggers = commandTriggers.OrderBy(c => c);

                        string text = "Available Commands: !" + string.Join(", !", commandTriggers);
                        await ChannelSession.Chat.Whisper(user.UserName, text);
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "There are no commands available for you to use.");
                    }
                }
            }));
        }
    }

    public class MixItUpCommandsChatCommand : PreMadeChatCommand
    {
        public MixItUpCommandsChatCommand()
            : base("Mix It Up Commands", "mixitupcommands", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    await ChannelSession.Chat.SendMessage("All common, Mix It Up chat commands can be found here: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Pre-Made-Chat-Commands. For commands specific to this stream, ask your streamer/moderator.");
                }
            }));
        }
    }

    public class GameChatCommand : PreMadeChatCommand
    {
        public GameChatCommand()
            : base("Game", new List<string>() { "game" }, UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    await ChannelSession.RefreshChannel();

                    string details = await SteamGameChatCommand.GetSteamGameInfo(ChannelSession.Channel.type.name);
                    if (!string.IsNullOrEmpty(details))
                    {
                        await ChannelSession.Chat.SendMessage(details);
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("Game: " + ChannelSession.Channel.type.name);
                    }
                }
            }));
        }
    }

    public class TitleChatCommand : PreMadeChatCommand
    {
        public TitleChatCommand()
            : base("Title", new List<string>() { "title", "stream" }, UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.Chat.SendMessage("Stream Title: " + ChannelSession.Channel.name);
                }
            }));
        }
    }

    public class UptimeChatCommand : PreMadeChatCommand
    {
        private static StreamSessionsAnalyticModel latestSession;
        private static DateTimeOffset streamStartDateTime = DateTimeOffset.MinValue;

        public static void SetUptime(DateTimeOffset dateTime)
        {
            UptimeChatCommand.streamStartDateTime = dateTime;
        }

        public UptimeChatCommand()
            : base("Uptime", "uptime", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (UptimeChatCommand.streamStartDateTime > DateTimeOffset.MinValue)
                    {
                        TimeSpan duration = DateTimeOffset.Now.Subtract(UptimeChatCommand.streamStartDateTime);
                        await ChannelSession.Chat.SendMessage("Start Time: " + UptimeChatCommand.streamStartDateTime.ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + duration.ToString("h\\:mm"));
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("Stream is currently offline");
                    }
                }
            }));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                while (true)
                {
                    await this.GetLatestStreamSession();
                    await Task.Delay(60000);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task GetLatestStreamSession()
        {
            IEnumerable<StreamSessionsAnalyticModel> sessions = await ChannelSession.Connection.GetStreamSessions(ChannelSession.Channel, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1)));
            sessions = sessions.OrderBy(s => s.dateTime);
            if (sessions.Count() > 0 && sessions.Last().duration == null)
            {
                StreamSessionsAnalyticModel latestSession = sessions.Last();
                UptimeChatCommand.SetUptime(latestSession.dateTime);
            }
        }
    }

    public class MixerAgeChatCommand : PreMadeChatCommand
    {
        public MixerAgeChatCommand()
            : base("Mixer Age", "mixerage", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    UserModel userModel = await ChannelSession.Connection.GetUser(user.GetModel());
                    await ChannelSession.Chat.SendMessage(this.GetMixerAge(userModel));
                }
            }));
        }
    }

    public class FollowAgeChatCommand : PreMadeChatCommand
    {
        public FollowAgeChatCommand()
            : base("Follow Age", "followage", UserRole.Follower, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    DateTimeOffset? followDate = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, user.GetModel());
                    if (followDate != null)
                    {
                        await ChannelSession.Chat.SendMessage(this.GetFollowAge(user.GetModel(), followDate.GetValueOrDefault()));
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "You are not currently following this channel");
                    }
                }
            }));
        }
    }

    public class SubscribeAgeChatCommand : PreMadeChatCommand
    {
        public SubscribeAgeChatCommand()
            : base("Subscribe Age", new List<string>() { "subage", "subscribeage" }, UserRole.Subscriber, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (user.SubscribeDate == null)
                    {
                        await user.SetSubscribeDate();
                    }

                    if (user.SubscribeDate != null)
                    {
                        await ChannelSession.Chat.SendMessage(this.GetSubscribeAge(user.GetModel(), user.SubscribeDate.GetValueOrDefault()));
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "You are not currently subscribed to this channel");
                    }
                }
            }));
        }
    }

    public class StreamerAgeChatCommand : PreMadeChatCommand
    {
        public StreamerAgeChatCommand()
            : base("Streamer Age", new List<string>() { "streamerage", "age" }, UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    await ChannelSession.Chat.SendMessage(this.GetMixerAge(ChannelSession.Channel.user));
                }
            }));
        }
    }

    public class SparksChatCommand : PreMadeChatCommand
    {
        public SparksChatCommand()
            : base("Sparks", "sparks", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    UserWithChannelModel userModel = await ChannelSession.Connection.GetUser(user.GetModel());
                    await ChannelSession.Chat.SendMessage(user.UserName + "'s Sparks: " + userModel.sparks);
                }
            }));
        }
    }

    public class QuoteChatCommand : PreMadeChatCommand
    {
        public QuoteChatCommand()
            : base("Quote", new List<string>() { "quote", "quotes" }, UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (ChannelSession.Settings.QuotesEnabled)
                    {
                        if (ChannelSession.Settings.UserQuotes.Count > 0)
                        {
                            int quoteIndex = 0;
                            if (arguments.Count() == 1)
                            {
                                if (!int.TryParse(arguments.ElementAt(0), out quoteIndex))
                                {
                                    await ChannelSession.Chat.Whisper(user.UserName, "USAGE: !quote [QUOTE NUMBER]");
                                    return;
                                }

                                quoteIndex -= 1;

                                if (quoteIndex < 0)
                                {
                                    await ChannelSession.Chat.Whisper(user.UserName, "Quote # must be greater than 0");
                                    return;
                                }

                                if (quoteIndex >= ChannelSession.Settings.UserQuotes.Count)
                                {
                                    await ChannelSession.Chat.Whisper(user.UserName, "There is no quote with a number that high");
                                    return;
                                }
                            }
                            else
                            {
                                Random random = new Random();
                                quoteIndex = random.Next(ChannelSession.Settings.UserQuotes.Count);
                            }

                            UserQuoteViewModel quote = ChannelSession.Settings.UserQuotes[quoteIndex];
                            await ChannelSession.Chat.SendMessage("Quote #" + (quoteIndex + 1) + ": " + quote.ToString());
                        }
                        else
                        {
                            await ChannelSession.Chat.SendMessage("At least 1 quote must be added for this feature to work");
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("Quotes must be enabled for this feature to work");
                    }
                }
            }));
        }
    }

    public class AddQuoteChatCommand : PreMadeChatCommand
    {
        public AddQuoteChatCommand()
            : base("Add Quote", new List<string>() { "addquote", "quoteadd" }, UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Settings.QuotesEnabled)
                {
                    StringBuilder quoteBuilder = new StringBuilder();
                    foreach (string arg in arguments)
                    {
                        quoteBuilder.Append(arg + " ");
                    }

                    string quoteText = quoteBuilder.ToString();
                    quoteText = quoteText.Trim(new char[] { ' ', '\'', '\"' });

                    UserQuoteViewModel quote = new UserQuoteViewModel(quoteText, DateTimeOffset.Now, ChannelSession.Channel.type);
                    ChannelSession.Settings.UserQuotes.Add(quote);
                    await ChannelSession.SaveSettings();

                    GlobalEvents.QuoteAdded(quote);

                    if (ChannelSession.Chat != null)
                    {
                        await ChannelSession.Chat.SendMessage("Added Quote: \"" + quote.ToString() + "\"");
                    }
                }
                else
                {
                    await ChannelSession.Chat.SendMessage("Quotes must be enabled with Mix It Up for this feature to work");
                }
            }));
        }
    }

    public class Timeout1ChatCommand : PreMadeChatCommand
    {
        public Timeout1ChatCommand()
            : base("Timeout 1", "timeout1", UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await ChannelSession.Chat.Whisper(username, "You have been timed out for 1 minute");
                        await ChannelSession.Chat.TimeoutUser(username, 60);
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Usage: !timeout1 @USERNAME");
                    }
                }
            }));
        }
    }

    public class Timeout5ChatCommand : PreMadeChatCommand
    {
        public Timeout5ChatCommand()
            : base("Timeout 5", "timeout5", UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await ChannelSession.Chat.Whisper(username, "You have been timed out for 5 minutes");
                        await ChannelSession.Chat.TimeoutUser(username, 300);
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Usage: !timeout5 @USERNAME");
                    }
                }
            }));
        }
    }

    public class PurgeChatCommand : PreMadeChatCommand
    {
        public PurgeChatCommand()
            : base("Purge", "purge", UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        await ChannelSession.Chat.PurgeUser(username);
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Usage: !purge @USERNAME");
                    }
                }
            }));
        }
    }

    public class BanChatCommand : PreMadeChatCommand
    {
        public BanChatCommand()
            : base("Ban", "ban", UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        UserViewModel bannedUser = ChannelSession.Chat.ChatUsers.Values.FirstOrDefault(u => u.UserName.Equals(username));
                        if (bannedUser != null)
                        {
                            await ChannelSession.Connection.AddUserRoles(ChannelSession.Channel, bannedUser.GetModel(), new List<UserRole>() { UserRole.Banned });
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Usage: !ban @USERNAME");
                    }
                }
            }));
        }
    }

    public class Magic8BallChatCommand : PreMadeChatCommand
    {
        private List<String> responses = new List<string>()
        {
            "It is certain",
            "It is decidedly so",
            "Without a doubt",
            "Yes definitely",
            "You may rely on it",
            "As I see it, yes",
            "Most likely",
            "Outlook good",
            "Yes",
            "Signs point to yes",
            "Reply hazy try again",
            "Ask again later",
            "Better not tell you now",
            "Cannot predict now",
            "Concentrate and ask again",
            "Don't count on it",
            "My reply is no",
            "My sources say no",
            "Outlook not so good",
            "Very doubtful",
            "Ask your mother",
            "Ask your father",
            "Come back later, I'm sleeping",
            "Yeah...sure, whatever",
            "Hahaha...no...",
            "I don't know, blame @SaviorXTanren..."
        };

        public Magic8BallChatCommand()
            : base("Magic 8 Ball", new List<string>() { "magic8ball", "8ball" }, UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    Random random = new Random();
                    int index = random.Next(0, this.responses.Count);
                    await ChannelSession.Chat.SendMessage(string.Format("The Magic 8-Ball says: \"{0}\"", this.responses[index]));
                }
            }));
        }
    }

    public class SteamGameChatCommand : PreMadeChatCommand
    {
        private static Dictionary<string, int> steamGameList = new Dictionary<string, int>();

        public static async Task<string> GetSteamGameInfo(string gameName)
        {
            gameName = gameName.ToLower();

            if (steamGameList.Count == 0)
            {
                using (HttpClientWrapper client = new HttpClientWrapper())
                {
                    HttpResponseMessage response = await client.GetAsync("http://api.steampowered.com/ISteamApps/GetAppList/v0002/");
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(result);
                        JToken list = jobj["applist"]["apps"];
                        JArray games = (JArray)list;
                        foreach (JToken game in games)
                        {
                            SteamGameChatCommand.steamGameList[game["name"].ToString().ToLower()] = (int)game["appid"];
                        }
                    }
                }
            }

            int gameID = -1;
            if (SteamGameChatCommand.steamGameList.ContainsKey(gameName))
            {
                gameID = SteamGameChatCommand.steamGameList[gameName];
            }
            else
            {
                string foundGame = SteamGameChatCommand.steamGameList.Keys.FirstOrDefault(g => g.Contains(gameName));
                if (foundGame != null)
                {
                    gameID = SteamGameChatCommand.steamGameList[foundGame];
                }
            }

            if (gameID > 0)
            {
                using (HttpClientWrapper client = new HttpClientWrapper())
                {
                    HttpResponseMessage response = await client.GetAsync("http://store.steampowered.com/api/appdetails?appids=" + gameID);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(result);
                        jobj = (JObject)jobj[gameID.ToString()]["data"];

                        double price = (int)jobj["price_overview"]["final"];
                        price = price / 100.0;

                        string url = string.Format("http://store.steampowered.com/app/{0}", gameID);

                        return string.Format("Game: {0} - ${1} - {2}", jobj["name"], price, url);
                    }
                }
            }
            return null;
        }

        public SteamGameChatCommand()
            : base("Steam Game", new List<string>() { "steamgame", "steam" }, UserRole.User, 30)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    string gameName;
                    if (arguments.Count() > 0)
                    {
                        gameName = string.Join(" ", arguments);
                    }
                    else
                    {
                        await ChannelSession.RefreshChannel();
                        gameName = ChannelSession.Channel.type.name;
                    }

                    string details = await SteamGameChatCommand.GetSteamGameInfo(gameName);
                    if (!string.IsNullOrEmpty(details))
                    {
                        await ChannelSession.Chat.SendMessage(details);
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage(string.Format("Could not find the game \"{0}\" on Steam", gameName));
                    }
                }
            }));
        }
    }

    public class SetTitleChatCommand : PreMadeChatCommand
    {
        private Dictionary<string, int> steamGameList = new Dictionary<string, int>();

        public SetTitleChatCommand()
            : base("Set Title", "settitle", UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (arguments.Count() > 0)
                    {
                        string newTitle = string.Join(" ", arguments);
                        ChannelSession.Channel.name = newTitle;
                        await ChannelSession.Connection.UpdateChannel(ChannelSession.Channel);
                        await ChannelSession.RefreshChannel();
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Usage: !settitle <TITLE NAME>");
                    }
                }
            }));
        }
    }

    public class SetGameChatCommand : PreMadeChatCommand
    {
        private Dictionary<string, int> steamGameList = new Dictionary<string, int>();

        public SetGameChatCommand()
            : base("Set Game", "setgame", UserRole.Mod, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (arguments.Count() > 0)
                    {
                        string newGameName = string.Join(" ", arguments);
                        IEnumerable<GameTypeModel> games = await ChannelSession.Connection.GetGameTypes(newGameName);

                        GameTypeModel newGame = games.FirstOrDefault(g => g.name.Equals(newGameName, StringComparison.CurrentCultureIgnoreCase));
                        if (newGame != null)
                        {
                            ChannelSession.Channel.typeId = newGame.id;
                            await ChannelSession.Connection.UpdateChannel(ChannelSession.Channel);
                            await ChannelSession.RefreshChannel();
                        }
                        else
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "We could not find a game with that name");
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Usage: !setgame <GAME NAME>");
                    }
                }
            }));
        }
    }
}
