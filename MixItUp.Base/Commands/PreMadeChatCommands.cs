using Mixer.Base.Model.Broadcast;
using Mixer.Base.Model.Costream;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.User;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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
        public MixerRoleEnum Permissions { get; set; }
        [DataMember]
        public int Cooldown { get; set; }

        public PreMadeChatCommandSettings() { }

        public PreMadeChatCommandSettings(PreMadeChatCommand command)
        {
            this.Name = command.Name;
            this.IsEnabled = command.IsEnabled;
            this.Permissions = command.Requirements.Role.MixerRole;
            this.Cooldown = command.Requirements.Cooldown.Amount;
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
        public PreMadeChatCommand(string name, string command, int cooldown, MixerRoleEnum userRole)
            : base(name, command, new RequirementViewModel(userRole: userRole, cooldown: cooldown))
        {
            this.Requirements.Role.MixerRole = userRole;
        }

        public PreMadeChatCommand(string name, List<string> commands, int cooldown, MixerRoleEnum userRole)
            : base(name, commands, new RequirementViewModel(userRole: userRole, cooldown: cooldown))
        {
            this.Requirements.Role.MixerRole = userRole;
        }

        public void UpdateFromSettings(PreMadeChatCommandSettings settings)
        {
            this.IsEnabled = settings.IsEnabled;
            this.Requirements.Role.MixerRole = settings.Permissions;
            this.Requirements.Cooldown.Amount = settings.Cooldown;
        }
    }

    public class MixItUpChatCommand : PreMadeChatCommand
    {
        public MixItUpChatCommand()
            : base("Mix It Up", "mixitup", 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.SendMessage("This channel uses the Mix It Up app to improve their stream. Check out http://mixitupapp.com for more information!");
                }
            }));
        }
    }

    public class CommandsChatCommand : PreMadeChatCommand
    {
        public CommandsChatCommand()
            : base("Commands", "commands", 0, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    List<string> commandTriggers = new List<string>();
                    foreach (PermissionsCommandBase command in ChannelSession.AllEnabledChatCommands)
                    {
                        if (await command.Requirements.DoesMeetUserRoleRequirement(user))
                        {
                            if (command is ChatCommand && !((ChatCommand)command).IncludeExclamationInCommands)
                            {
                                commandTriggers.AddRange(command.Commands);
                            }
                            else
                            {
                                commandTriggers.AddRange(command.Commands.Select(c => $"!{c}"));
                            }
                        }
                    }

                    if (commandTriggers.Count > 0)
                    {
                        string text = "Available Commands: " + string.Join(", ", commandTriggers.OrderBy(c => c));
                        await ChannelSession.Services.Chat.Whisper(user.UserName, text);
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "There are no commands available for you to use.");
                    }
                }
            }));
        }
    }

    public class GamesChatCommand : PreMadeChatCommand
    {
        public GamesChatCommand()
            : base("Games", "games", 0, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    List<string> commandTriggers = new List<string>();
                    foreach (GameCommandBase command in ChannelSession.Settings.GameCommands)
                    {
                        if (command.IsEnabled && await command.Requirements.DoesMeetUserRoleRequirement(user))
                        {
                            commandTriggers.AddRange(command.Commands.Select(c => $"!{c}"));
                        }
                    }

                    if (commandTriggers.Count > 0)
                    {
                        string text = "Available Games: " + string.Join(", ", commandTriggers.OrderBy(c => c));
                        await ChannelSession.Services.Chat.Whisper(user.UserName, text);
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "There are no games available for you to use.");
                    }
                }
            }));
        }
    }

    public class MixItUpCommandsChatCommand : PreMadeChatCommand
    {
        public MixItUpCommandsChatCommand()
            : base("Mix It Up Commands", "mixitupcommands", 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.SendMessage("All common, Mix It Up chat commands can be found here: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Pre-Made-Chat-Commands. For commands specific to this stream, ask your streamer/moderator.");
                }
            }));
        }
    }

    public class GameInformation
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Uri { get; set; }

        public override string ToString()
        {
            return $"Game: {Name} - {Price} - {Uri}";
        }
    }

    public class GameChatCommand : PreMadeChatCommand
    {
        public GameChatCommand()
            : base("Game", new List<string>() { "game" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.RefreshChannel();

                    GameInformation details = await XboxGameChatCommand.GetXboxGameInfo(ChannelSession.MixerChannel.type.name);
                    if (details == null)
                    {
                        details = await SteamGameChatCommand.GetSteamGameInfo(ChannelSession.MixerChannel.type.name);
                    }

                    if (details != null)
                    {
                        await ChannelSession.Services.Chat.SendMessage(details.ToString());
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Game: " + ChannelSession.MixerChannel.type.name);
                    }
                }
            }));
        }
    }

    public class TitleChatCommand : PreMadeChatCommand
    {
        public TitleChatCommand()
            : base("Title", new List<string>() { "title", "stream" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.Services.Chat.SendMessage("Stream Title: " + ChannelSession.MixerChannel.name);
                }
            }));
        }
    }

    public class UptimeChatCommand : PreMadeChatCommand
    {
        public static async Task<DateTimeOffset> GetStartTime()
        {
            BroadcastModel broadcast = await ChannelSession.MixerUserConnection.GetCurrentBroadcast(ChannelSession.MixerChannel);
            if (broadcast != null && broadcast.online)
            {
                DateTimeOffset startTime = broadcast.startedAt.ToLocalTime();
                if (startTime > DateTimeOffset.MinValue)
                {
                    return startTime;
                }
            }
            return DateTimeOffset.MinValue;
        }

        public UptimeChatCommand()
            : base("Uptime", "uptime", 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    DateTimeOffset startTime = await UptimeChatCommand.GetStartTime();
                    if (startTime > DateTimeOffset.MinValue)
                    {
                        TimeSpan duration = DateTimeOffset.Now.Subtract(startTime);
                        await ChannelSession.Services.Chat.SendMessage("Start Time: " + startTime.ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + (int)duration.TotalHours + duration.ToString("\\:mm"));
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Stream is currently offline");
                    }
                }
            }));
        }
    }

    public class CostreamChatCommand : PreMadeChatCommand
    {
        public static async Task<string> GetCostreamUsers()
        {
            CostreamModel costream = await ChannelSession.MixerUserConnection.GetCurrentCostream();
            if (costream != null && costream.channels != null)
            {
                List<UserModel> costreamUsers = new List<UserModel>();
                foreach (CostreamChannelModel channel in costream.channels)
                {
                    UserModel user = await ChannelSession.MixerUserConnection.GetUser(channel.userId);
                    if (user != null)
                    {
                        costreamUsers.Add(user);
                    }
                }

                if (costreamUsers.Count > 0)
                {
                    return string.Join(", ", costreamUsers.Select(u => "@" + u.username));
                }
            }
            return "@" + ChannelSession.MixerChannel.token;
        }

        public CostreamChatCommand()
            : base("Costream", "costream", 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.SendMessage("Costream Users: " + await CostreamChatCommand.GetCostreamUsers());
                }
            }));
        }
    }

    public class MixerAgeChatCommand : PreMadeChatCommand
    {
        public MixerAgeChatCommand()
            : base("Mixer Age", "mixerage", 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await user.RefreshDetails();
                    await ChannelSession.Services.Chat.SendMessage(user.UserName + "'s Mixer Age: " + user.MixerAgeString);
                }
            }));
        }
    }

    public class FollowAgeChatCommand : PreMadeChatCommand
    {
        public FollowAgeChatCommand()
            : base("Follow Age", "followage", 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await user.RefreshDetails();
                    await ChannelSession.Services.Chat.SendMessage(user.UserName + "'s Follow Age: " + user.FollowAgeString);
                }
            }));
        }
    }

    public class SubscribeAgeChatCommand : PreMadeChatCommand
    {
        public SubscribeAgeChatCommand()
            : base("Subscribe Age", new List<string>() { "subage", "subscribeage" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await user.RefreshDetails();
                    await ChannelSession.Services.Chat.SendMessage(user.UserName + "'s Subscribe Age: " + user.MixerSubscribeAgeString);
                }
            }));
        }
    }

    public class StreamerAgeChatCommand : PreMadeChatCommand
    {
        public StreamerAgeChatCommand()
            : base("Streamer Age", new List<string>() { "streamerage", "age" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await user.RefreshDetails();
                    await ChannelSession.Services.Chat.SendMessage(user.UserName + "'s Streamer Age: " + user.MixerAgeString);
                }
            }));
        }
    }

    public class SparksChatCommand : PreMadeChatCommand
    {
        public SparksChatCommand()
            : base("Sparks", "sparks", 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    UserModel userModel = await ChannelSession.MixerUserConnection.GetUser(user.GetModel());

                    if (arguments.Count() == 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        userModel = await ChannelSession.MixerUserConnection.GetUser(username);
                    }

                    if (userModel != null)
                    {
                        await ChannelSession.Services.Chat.SendMessage(userModel.username + "'s Sparks: " + userModel.sparks);
                    }
                }
            }));
        }
    }

    public class QuoteChatCommand : PreMadeChatCommand
    {
        public QuoteChatCommand()
            : base("Quote", new List<string>() { "quote", "quotes" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
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
                                    await ChannelSession.Services.Chat.Whisper(user.UserName, "USAGE: !quote [QUOTE NUMBER]");
                                    return;
                                }

                                quoteIndex -= 1;

                                if (quoteIndex < 0)
                                {
                                    await ChannelSession.Services.Chat.Whisper(user.UserName, "Quote # must be greater than 0");
                                    return;
                                }

                                if (quoteIndex >= ChannelSession.Settings.UserQuotes.Count)
                                {
                                    await ChannelSession.Services.Chat.Whisper(user.UserName, "There is no quote with a number that high");
                                    return;
                                }
                            }
                            else
                            {
                                quoteIndex = RandomHelper.GenerateRandomNumber(ChannelSession.Settings.UserQuotes.Count);
                            }

                            UserQuoteViewModel quote = ChannelSession.Settings.UserQuotes[quoteIndex];
                            await ChannelSession.Services.Chat.SendMessage(quote.ToString());
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.SendMessage("At least 1 quote must be added for this feature to work");
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Quotes must be enabled for this feature to work");
                    }
                }
            }));
        }
    }

    public class LastQuoteChatCommand : PreMadeChatCommand
    {
        public LastQuoteChatCommand()
            : base("Last Quote", new List<string>() { "lastquote" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (ChannelSession.Settings.QuotesEnabled)
                    {
                        if (ChannelSession.Settings.UserQuotes.Count > 0)
                        {
                            UserQuoteViewModel quote = ChannelSession.Settings.UserQuotes.LastOrDefault();
                            if (quote != null)
                            {
                                await ChannelSession.Services.Chat.SendMessage(quote.ToString());
                                return;
                            }
                        }
                        await ChannelSession.Services.Chat.SendMessage("At least 1 quote must be added for this feature to work");
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Quotes must be enabled for this feature to work");
                    }
                }
            }));
        }
    }

    public class AddQuoteChatCommand : PreMadeChatCommand
    {
        public AddQuoteChatCommand()
            : base("Add Quote", new List<string>() { "addquote", "quoteadd" }, 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Settings.QuotesEnabled)
                {
                    if (arguments.Count() > 0)
                    {
                        StringBuilder quoteBuilder = new StringBuilder();
                        foreach (string arg in arguments)
                        {
                            quoteBuilder.Append(arg + " ");
                        }

                        string quoteText = quoteBuilder.ToString();
                        quoteText = quoteText.Trim(new char[] { ' ', '\'', '\"' });

                        UserQuoteViewModel quote = new UserQuoteViewModel(quoteText, DateTimeOffset.Now, ChannelSession.MixerChannel.type);
                        ChannelSession.Settings.UserQuotes.Add(quote);
                        await ChannelSession.SaveSettings();

                        GlobalEvents.QuoteAdded(quote);

                        if (ChannelSession.Services.Chat != null)
                        {
                            await ChannelSession.Services.Chat.SendMessage("Added " + quote.ToString());
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !addquote <FULL QUOTE TEXT>");
                    }
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage("Quotes must be enabled with Mix It Up for this feature to work");
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
            : base("Magic 8 Ball", new List<string>() { "magic8ball", "8ball" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    int index = RandomHelper.GenerateRandomNumber(this.responses.Count);
                    await ChannelSession.Services.Chat.SendMessage(string.Format("The Magic 8-Ball says: \"{0}\"", this.responses[index]));
                }
            }));
        }
    }

    public class XboxGameChatCommand : PreMadeChatCommand
    {
        public static async Task<GameInformation> GetXboxGameInfo(string gameName)
        {
            gameName = gameName.ToLower();

            string cv = Convert.ToBase64String(Guid.NewGuid().ToByteArray(), 0, 12);

            using (AdvancedHttpClient client = new AdvancedHttpClient("https://displaycatalog.mp.microsoft.com"))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MixItUp");
                client.DefaultRequestHeaders.Add("MS-CV", cv);

                HttpResponseMessage response = await client.GetAsync($"v7.0/productFamilies/Games/products?query={HttpUtility.UrlEncode(gameName)}&$top=1&market=US&languages=en-US&fieldsTemplate=StoreSDK&isAddon=False&isDemo=False&actionFilter=Browse");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JObject jobj = JObject.Parse(result);
                    JArray products = jobj["Products"] as JArray;
                    if (products?.First() is JObject product)
                    {
                        string productId = product["ProductId"]?.Value<string>();
                        string name = product["LocalizedProperties"]?.First()?["ProductTitle"]?.Value<string>();
                        double price = product["DisplaySkuAvailabilities"]?.First()?["Availabilities"]?.First()?["OrderManagementData"]?["Price"]?["ListPrice"]?.Value<double>() ?? 0.0;
                        string uri = $"https://www.microsoft.com/store/apps/{productId}";

                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(productId) && name.ToLower().Contains(gameName))
                        {
                            return new GameInformation { Name = name, Price = price, Uri = uri };
                        }
                    }
                }
            }

            return null;
        }

        public XboxGameChatCommand()
            : base("Xbox Game", new List<string>() { "xboxgame" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    string gameName;
                    if (arguments.Count() > 0)
                    {
                        gameName = string.Join(" ", arguments);
                    }
                    else
                    {
                        await ChannelSession.RefreshChannel();
                        gameName = ChannelSession.MixerChannel.type.name;
                    }

                    GameInformation details = await XboxGameChatCommand.GetXboxGameInfo(gameName);
                    if (details != null)
                    {
                        await ChannelSession.Services.Chat.SendMessage(details.ToString());
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage(string.Format("Could not find the game \"{0}\" on Xbox", gameName));
                    }
                }
            }));
        }
    }

    public class SteamGameChatCommand : PreMadeChatCommand
    {
        private static Dictionary<string, int> steamGameList = new Dictionary<string, int>();

        public static async Task<GameInformation> GetSteamGameInfo(string gameName)
        {
            gameName = gameName.ToLower();

            if (steamGameList.Count == 0)
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient("http://api.steampowered.com/"))
                {
                    HttpResponseMessage response = await client.GetAsync("ISteamApps/GetAppList/v0002");
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
                using (AdvancedHttpClient client = new AdvancedHttpClient("http://store.steampowered.com/"))
                {
                    HttpResponseMessage response = await client.GetAsync("api/appdetails?appids=" + gameID);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(result);
                        if (jobj[gameID.ToString()] != null && jobj[gameID.ToString()]["data"] != null)
                        {
                            jobj = (JObject)jobj[gameID.ToString()]["data"];

                            double price = 0.0;
                            if (jobj["price_overview"] != null && jobj["price_overview"]["final"] != null)
                            {
                                price = (int)jobj["price_overview"]["final"];
                                price = price / 100.0;
                            }

                            string url = string.Format("http://store.steampowered.com/app/{0}", gameID);

                            return new GameInformation { Name = jobj["name"].Value<string>(), Price = price, Uri = url };
                        }
                    }
                }
            }
            return null;
        }

        public SteamGameChatCommand()
            : base("Steam Game", new List<string>() { "steamgame", "steam" }, 5, MixerRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    string gameName;
                    if (arguments.Count() > 0)
                    {
                        gameName = string.Join(" ", arguments);
                    }
                    else
                    {
                        await ChannelSession.RefreshChannel();
                        gameName = ChannelSession.MixerChannel.type.name;
                    }

                    GameInformation details = await SteamGameChatCommand.GetSteamGameInfo(gameName);
                    if (details != null)
                    {
                        await ChannelSession.Services.Chat.SendMessage(details.ToString());
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage(string.Format("Could not find the game \"{0}\" on Steam", gameName));
                    }
                }
            }));
        }
    }

    public class SetTitleChatCommand : PreMadeChatCommand
    {
        private Dictionary<string, int> steamGameList = new Dictionary<string, int>();

        public SetTitleChatCommand()
            : base("Set Title", "settitle", 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (arguments.Count() > 0)
                    {
                        await ChannelSession.MixerUserConnection.UpdateChannel(ChannelSession.MixerChannel.id, name: string.Join(" ", arguments));
                        await ChannelSession.RefreshChannel();
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !settitle <TITLE NAME>");
                    }
                }
            }));
        }
    }

    public class SetGameChatCommand : PreMadeChatCommand
    {
        public SetGameChatCommand()
            : base("Set Game", "setgame", 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (arguments.Count() > 0)
                    {
                        GameTypeModel newGame = null;
                        if (arguments.Count() == 1 && uint.TryParse(arguments.ElementAt(0), out uint gameID))
                        {
                            newGame = await ChannelSession.MixerUserConnection.GetGameType(gameID);
                        }
                        else
                        {
                            string newGameName = string.Join(" ", arguments);
                            IEnumerable<GameTypeModel> games = await ChannelSession.MixerUserConnection.GetGameTypes(newGameName, 25);

                            newGame = games.FirstOrDefault(g => g.name.Equals(newGameName, StringComparison.CurrentCultureIgnoreCase));
                        }

                        if (newGame != null)
                        {
                            await ChannelSession.MixerUserConnection.UpdateChannel(ChannelSession.MixerChannel.id, gameTypeID: newGame.id);
                            await ChannelSession.RefreshChannel();

                            await ChannelSession.Services.Chat.Whisper(user.UserName, "Game Updated: " + newGame.name);
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.Whisper(user.UserName, "We could not find a game with that name/ID");
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !setgame <GAME NAME>");
                    }
                }
            }));
        }
    }

    public class SetAudienceChatCommand : PreMadeChatCommand
    {
        private const string FamilySetting = "family";
        private const string TeenSetting = "teen";
        private const string AdultSettings = "adult";
        private const string Adult18PlusSetting = "18+";

        private Dictionary<string, int> steamGameList = new Dictionary<string, int>();

        public SetAudienceChatCommand()
            : base("Set Audience", "setaudience", 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (arguments.Count() == 1)
                    {
                        string rating = arguments.ElementAt(0);
                        rating = rating.ToLower().Replace(AdultSettings, Adult18PlusSetting);
                        if (rating.Equals(FamilySetting) || rating.Equals(TeenSetting) || rating.Equals(Adult18PlusSetting))
                        {
                            await ChannelSession.MixerUserConnection.UpdateChannel(ChannelSession.MixerChannel.id, ageRating: rating);
                            await ChannelSession.RefreshChannel();

                            return;
                        }
                    }

                    await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !setaudience family|teen|adult");
                }
            }));
        }
    }

    public class SetUserTitleChatCommand : PreMadeChatCommand
    {
        public SetUserTitleChatCommand()
            : base("Set User Title", "setusertitle", 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (arguments.Count() > 1)
                    {
                        string username = arguments.ElementAt(0);
                        if (username.StartsWith("@"))
                        {
                            username = username.Substring(1);
                        }

                        UserModel targetUserModel = await ChannelSession.MixerUserConnection.GetUser(username);
                        if (targetUserModel != null)
                        {
                            UserViewModel targetUser = new UserViewModel(targetUserModel);
                            targetUser.Title = string.Join(" ", arguments.Skip(1));
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.Whisper(user.UserName, username + " is not a valid user");
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !settitle <USERNAME> <TITLE NAME>");
                    }
                }
            }));
        }
    }

    public class AddCommandChatCommand : PreMadeChatCommand
    {
        public AddCommandChatCommand()
            : base("Add Command", new List<string>() { "addcommand" }, 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments.Count() >= 3)
                {
                    string commandTrigger = arguments.ElementAt(0).ToLower();

                    if (!CommandBase.IsValidCommandString(commandTrigger))
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "ERROR: Command trigger contain an invalid character");
                        return;
                    }

                    foreach (PermissionsCommandBase command in ChannelSession.AllEnabledChatCommands)
                    {
                        if (command.IsEnabled)
                        {
                            if (command.Commands.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase))
                            {
                                await ChannelSession.Services.Chat.Whisper(user.UserName, "ERROR: There already exists an enabled, chat command that uses the command trigger you have specified");
                                return;
                            }
                        }
                    }

                    if (!int.TryParse(arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "ERROR: Cooldown must be 0 or greater");
                        return;
                    }

                    StringBuilder commandTextBuilder = new StringBuilder();
                    foreach (string arg in arguments.Skip(2))
                    {
                        commandTextBuilder.Append(arg + " ");
                    }

                    string commandText = commandTextBuilder.ToString();
                    commandText = commandText.Trim(new char[] { ' ', '\'', '\"' });

                    ChatCommand newCommand = new ChatCommand(commandTrigger, commandTrigger, new RequirementViewModel());
                    newCommand.Requirements.Cooldown.Amount = cooldown;
                    newCommand.Actions.Add(new ChatAction(commandText));
                    ChannelSession.Settings.ChatCommands.Add(newCommand);

                    if (ChannelSession.Services.Chat != null)
                    {
                        await ChannelSession.Services.Chat.SendMessage("Added New Command: !" + commandTrigger);

                        ChannelSession.Services.Chat.RebuildCommandTriggers();
                    }
                }
                else
                {
                    await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !addcommand <COMMAND TRIGGER, NO !> <COOLDOWN> <FULL COMMAND MESSAGE TEXT>");
                }
            }));
        }
    }

    public class UpdateCommandChatCommand : PreMadeChatCommand
    {
        public UpdateCommandChatCommand()
            : base("Update Command", new List<string>() { "updatecommand" }, 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments.Count() >= 2)
                {
                    string commandTrigger = arguments.ElementAt(0).ToLower();

                    PermissionsCommandBase command = ChannelSession.AllEnabledChatCommands.FirstOrDefault(c => c.Commands.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                    if (command == null)
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "ERROR: Could not find any command with that trigger");
                        return;
                    }

                    if (!int.TryParse(arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "ERROR: Cooldown must be 0 or greater");
                        return;
                    }

                    command.Requirements.Cooldown.Amount = cooldown;

                    if (arguments.Count() > 2)
                    {
                        StringBuilder commandTextBuilder = new StringBuilder();
                        foreach (string arg in arguments.Skip(2))
                        {
                            commandTextBuilder.Append(arg + " ");
                        }

                        string commandText = commandTextBuilder.ToString();
                        commandText = commandText.Trim(new char[] { ' ', '\'', '\"' });

                        command.Actions.Clear();
                        command.Actions.Add(new ChatAction(commandText));
                    }

                    if (ChannelSession.Services.Chat != null)
                    {
                        await ChannelSession.Services.Chat.SendMessage("Updated Command: !" + commandTrigger);

                        ChannelSession.Services.Chat.RebuildCommandTriggers();
                    }
                }
                else
                {
                    await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !updatecommand <COMMAND TRIGGER, NO !> <COOLDOWN> [OPTIONAL FULL COMMAND MESSAGE TEXT]");
                }
            }));
        }
    }

    public class DisableCommandChatCommand : PreMadeChatCommand
    {
        public DisableCommandChatCommand()
            : base("Disable Command", new List<string>() { "disablecommand" }, 5, MixerRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments.Count() == 1)
                {
                    string commandTrigger = arguments.ElementAt(0).ToLower();

                    PermissionsCommandBase command = ChannelSession.AllEnabledChatCommands.FirstOrDefault(c => c.Commands.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                    if (command == null)
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "ERROR: Could not find any command with that trigger");
                        return;
                    }

                    command.IsEnabled = false;

                    if (ChannelSession.Services.Chat != null)
                    {
                        await ChannelSession.Services.Chat.SendMessage("Disabled Command: !" + commandTrigger);

                        ChannelSession.Services.Chat.RebuildCommandTriggers();
                    }
                }
                else
                {
                    await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !disablecommand <COMMAND TRIGGER, NO !>");
                }
            }));
        }
    }

    public class StartGiveawayChatCommand : PreMadeChatCommand
    {
        public StartGiveawayChatCommand()
            : base("Start Giveaway", "startgiveaway", 5, MixerRoleEnum.Streamer)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (arguments.Count() > 0)
                    {
                        string result = await ChannelSession.Services.GiveawayService.Start(string.Join(" ", arguments));
                        if (!string.IsNullOrEmpty(result))
                        {
                            await ChannelSession.Services.Chat.Whisper(user.UserName, "ERROR: " + result);
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, "Usage: !startgiveaway <GIVEAWAY ITEM>");
                    }
                }
            }));
        }
    }

    #region Obsolete Pre-Made Commands

    [Obsolete]
    public class ObsoletePreMadeCommand : PreMadeChatCommand { public ObsoletePreMadeCommand() : base(string.Empty, string.Empty, 0, MixerRoleEnum.User) { } }

    [Obsolete]
    public class Timeout1ChatCommand : ObsoletePreMadeCommand {  }

    [Obsolete]
    public class Timeout5ChatCommand : ObsoletePreMadeCommand { }

    [Obsolete]
    public class PurgeChatCommand : ObsoletePreMadeCommand { }

    [Obsolete]
    public class BanChatCommand : ObsoletePreMadeCommand { }

    #endregion Obsolete Pre-Made Commands
}
