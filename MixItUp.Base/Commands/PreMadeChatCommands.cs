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
                    await ChannelSession.Chat.SendMessage("This channel uses the Mix It Up app to improve their stream. Check out https://github.com/SaviorXTanren/mixer-mixitup for more information!");
                }
            }));
        }
    }

    public class CommandsChatCommand : PreMadeChatCommand
    {
        public CommandsChatCommand()
            : base("Commands", "commands", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    await ChannelSession.Chat.SendMessage("All common chat commands can be found here: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Pre-Made-Chat-Commands. For commands specific to this stream, ask your streamer/moderator.");
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
        public UptimeChatCommand()
            : base("Uptime", "uptime", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    IEnumerable<StreamSessionsAnalyticModel> sessions = await ChannelSession.Connection.GetStreamSessions(ChannelSession.Channel, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1)));
                    sessions = sessions.OrderBy(s => s.dateTime);
                    if (sessions.Count() > 0 && sessions.Last().duration == null)
                    {
                        StreamSessionsAnalyticModel session = sessions.Last();
                        TimeSpan duration = DateTimeOffset.Now.Subtract(session.dateTime);
                        await ChannelSession.Chat.SendMessage("Start Time: " + session.dateTime.ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + duration.ToString("h\\:mm"));
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("Stream is currently offline");
                    }
                }
            }));
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
            : base("Quote", "quote", UserRole.User, 5)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Chat != null)
                {
                    if (ChannelSession.Settings.QuotesEnabled && ChannelSession.Settings.Quotes.Count > 0)
                    {
                        Random random = new Random();
                        int index = random.Next(ChannelSession.Settings.Quotes.Count);
                        string quote = ChannelSession.Settings.Quotes[index];

                        await ChannelSession.Chat.SendMessage("Quote #" + (index + 1) + ": \"" + quote + "\"");
                    }
                    else
                    {
                        await ChannelSession.Chat.SendMessage("Quotes must be enabled with Mix It Up for this feature to work");
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

                    string quote = quoteBuilder.ToString();
                    quote = quote.Trim(new char[] { ' ', '\'', '\"' });

                    ChannelSession.Settings.Quotes.Add(quote);
                    GlobalEvents.QuoteAdded(quote);

                    if (ChannelSession.Chat != null)
                    {
                        await ChannelSession.Chat.SendMessage("Added Quote: \"" + quote + "\"");
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

                        UserViewModel bannedUser = ChatClientWrapper.ChatUsers.Values.FirstOrDefault(u => u.UserName.Equals(username));
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

    public class RouletteSpinChatCommand : PreMadeChatCommand
    {
        public static Task MessageRouletteSpinResults(string result, uint userID, int bet)
        {
            //result = result.ToLower();
            //if (ChannelSession.Settings.CurrencyAcquisition.Enabled && ChannelSession.Settings.UserData.ContainsKey(userID))
            //{
            //    UserDataViewModel user = ChannelSession.Settings.UserData[userID];
            //    if (result.Equals("lose"))
            //    {
            //        await ChannelSession.Chat.SendMessage(string.Format("Sorry @{0}, you lost the spin!", user.UserName));
            //    }
            //    else if (result.Equals("win"))
            //    {
            //        int winAmount = bet * 2;
            //        user.CurrencyAmount += winAmount;
            //        await ChannelSession.Chat.SendMessage(string.Format("Congrats @{0}, you won the spin and got {1} {2}!", user.UserName,
            //            winAmount, ChannelSession.Settings.CurrencyAcquisition.Name));
            //    }
            //    else if (result.Equals("bonus"))
            //    {
            //        int winAmount = bet * 3;
            //        user.CurrencyAmount += winAmount;
            //        await ChannelSession.Chat.SendMessage(string.Format("Congrats @{0}, you won the BONUS spin and got {1} {2}!", user.UserName,
            //            winAmount, ChannelSession.Settings.CurrencyAcquisition.Name));
            //    }
            //}
            return Task.FromResult(0);
        }

        public RouletteSpinChatCommand()
            : base("Roulette Spin", new List<string>() { "spin", "roulette" }, UserRole.User, 10)
        {
            //this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            //{
            //    if (ChannelSession.Settings.CurrencyAcquisition.Enabled && ChannelSession.Chat != null)
            //    {
            //        if (arguments.Count() == 1)
            //        {
            //            int bet = 0;
            //            if (!int.TryParse(arguments.ElementAt(0), out bet) || bet <= 0)
            //            {
            //                await ChannelSession.Chat.Whisper(user.UserName, "Spin bet amount must be greater than 0");
            //                return;
            //            }

            //            if (await this.CheckForRequiredCurrency(user, bet))
            //            {
            //                user.Data.CurrencyAmount -= bet;
            //                if (ChannelSession.Services.OverlayServer != null)
            //                {
            //                    await ChannelSession.Services.OverlayServer.SetRouletteWheel(new OverlayRouletteWheel() { userID = user.ID, bet = bet });
            //                }
            //                else
            //                {
            //                    Random random = new Random();
            //                    int resultNumber = random.Next(0, 8);
            //                    string result = "lose";
            //                    if (resultNumber == 7)
            //                    {
            //                        result = "bonus";
            //                    }
            //                    else if (resultNumber >= 4)
            //                    {
            //                        result = "win";
            //                    }
            //                    await RouletteSpinChatCommand.MessageRouletteSpinResults(result, user.ID, bet);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            await ChannelSession.Chat.Whisper(user.UserName, "Usage: !spin <AMOUNT>");
            //        }
            //    }
            //}));
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
