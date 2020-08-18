using MixItUp.Base.Actions;
using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Twitch;
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
        public UserRoleEnum Permissions { get; set; }
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
        public PreMadeChatCommand(string name, string command, int cooldown, UserRoleEnum userRole)
            : base(name, command, new RequirementViewModel(userRole: userRole, cooldown: cooldown))
        {
            this.Requirements.Role.MixerRole = userRole;
        }

        public PreMadeChatCommand(string name, List<string> commands, int cooldown, UserRoleEnum userRole)
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

        protected override void TrackTelemetry() { ChannelSession.Services.Telemetry.TrackCommand(this.Type, this.GetType().Name.ToString()); }
    }

    public class MixItUpChatCommand : PreMadeChatCommand
    {
        public MixItUpChatCommand()
            : base("Mix It Up", "mixitup", 5, UserRoleEnum.User)
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
            : base(MixItUp.Base.Resources.Commands, "commands", 0, UserRoleEnum.User)
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
                        await ChannelSession.Services.Chat.SendMessage(text);
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("There are no commands available for you to use.");
                    }
                }
            }));
        }
    }

    public class GamesChatCommand : PreMadeChatCommand
    {
        public GamesChatCommand()
            : base(MixItUp.Base.Resources.Games, "games", 0, UserRoleEnum.User)
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
                        await ChannelSession.Services.Chat.SendMessage(text);
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("There are no games available for you to use.");
                    }
                }
            }));
        }
    }

    public class MixItUpCommandsChatCommand : PreMadeChatCommand
    {
        public MixItUpCommandsChatCommand()
            : base(MixItUp.Base.Resources.MixItUpCommands, "mixitupcommands", 5, UserRoleEnum.User)
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
            : base(MixItUp.Base.Resources.Game, new List<string>() { "game" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.RefreshChannel();
                    if (ChannelSession.TwitchChannelV5 != null)
                    {
                        GameInformation details = await XboxGameChatCommand.GetXboxGameInfo(ChannelSession.TwitchChannelV5.game);
                        if (details == null)
                        {
                            details = await SteamGameChatCommand.GetSteamGameInfo(ChannelSession.TwitchChannelV5.game);
                        }

                        if (details != null)
                        {
                            await ChannelSession.Services.Chat.SendMessage(details.ToString());
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.SendMessage("Game: " + ChannelSession.TwitchChannelV5.game);
                        }
                    }
                }
            }));
        }
    }

    public class TitleChatCommand : PreMadeChatCommand
    {
        public TitleChatCommand()
            : base(MixItUp.Base.Resources.Title, new List<string>() { "title", "stream" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.RefreshChannel();

                    await ChannelSession.Services.Chat.SendMessage("Stream Title: " + ChannelSession.TwitchChannelV5.status);
                }
            }));
        }
    }

    public class UptimeChatCommand : PreMadeChatCommand
    {
        public static Task<DateTimeOffset> GetStartTime()
        {
            DateTimeOffset startTime = DateTimeOffset.MinValue;
            if (ChannelSession.TwitchStreamNewAPI != null)
            {
                startTime = TwitchPlatformService.GetTwitchDateTime(ChannelSession.TwitchStreamNewAPI.started_at).GetValueOrDefault();
            }
            return Task.FromResult(startTime);
        }

        public UptimeChatCommand()
            : base(MixItUp.Base.Resources.Uptime, "uptime", 5, UserRoleEnum.User)
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

    public class FollowAgeChatCommand : PreMadeChatCommand
    {
        public FollowAgeChatCommand()
            : base(MixItUp.Base.Resources.FollowAge, "followage", 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.SendMessage(user.Username + "'s Follow Age: " + user.FollowAgeString);
                }
            }));
        }
    }

    public class SubscribeAgeChatCommand : PreMadeChatCommand
    {
        public SubscribeAgeChatCommand()
            : base(MixItUp.Base.Resources.SubscribeAge, new List<string>() { "subage", "subscribeage" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.SendMessage(user.Username + "'s Subscribe Age: " + user.SubscribeAgeString);
                }
            }));
        }
    }

    public class StreamerAgeChatCommand : PreMadeChatCommand
    {
        public StreamerAgeChatCommand()
            : base(MixItUp.Base.Resources.StreamerAge, new List<string>() { "streamerage", "age" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    await ChannelSession.Services.Chat.SendMessage(user.Username + "'s Streamer Age: " + user.AccountAgeString);
                }
            }));
        }
    }

    public class QuoteChatCommand : PreMadeChatCommand
    {
        public QuoteChatCommand()
            : base(MixItUp.Base.Resources.Quote, new List<string>() { "quote", "quotes" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (ChannelSession.Settings.QuotesEnabled)
                    {
                        if (ChannelSession.Settings.Quotes.Count > 0)
                        {
                            int quoteNumber = 0;
                            UserQuoteViewModel quote = null;

                            if (arguments.Count() == 1)
                            {
                                if (!int.TryParse(arguments.ElementAt(0), out quoteNumber))
                                {
                                    await ChannelSession.Services.Chat.SendMessage("USAGE: !quote [QUOTE NUMBER]");
                                    return;
                                }

                                quote = ChannelSession.Settings.Quotes.SingleOrDefault(q => q.ID == quoteNumber);
                                if (quote == null)
                                {
                                    await ChannelSession.Services.Chat.SendMessage($"Unable to find quote number {quoteNumber}.");
                                }
                            }
                            else if (arguments.Count() == 0)
                            {
                                int quoteIndex = RandomHelper.GenerateRandomNumber(ChannelSession.Settings.Quotes.Count);
                                quote = ChannelSession.Settings.Quotes[quoteIndex];
                            }
                            else 
                            {
                                await ChannelSession.Services.Chat.SendMessage("USAGE: !quote [QUOTE NUMBER]");
                                return;
                            }

                            if (quote != null)
                            {
                                await ChannelSession.Services.Chat.SendMessage(quote.ToString());
                            }
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
            : base(MixItUp.Base.Resources.LastQuote, new List<string>() { "lastquote" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (ChannelSession.Settings.QuotesEnabled)
                    {
                        if (ChannelSession.Settings.Quotes.Count > 0)
                        {
                            UserQuoteViewModel quote = ChannelSession.Settings.Quotes.LastOrDefault();
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
            : base(MixItUp.Base.Resources.AddQuote, new List<string>() { "addquote", "quoteadd" }, 5, UserRoleEnum.Mod)
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

                        UserQuoteViewModel quote = new UserQuoteViewModel(quoteText, DateTimeOffset.Now, ChannelSession.TwitchChannelV5?.game);
                        ChannelSession.Settings.Quotes.Add(quote);
                        await ChannelSession.SaveSettings();

                        GlobalEvents.QuoteAdded(quote);

                        if (ChannelSession.Services.Chat != null)
                        {
                            await ChannelSession.Services.Chat.SendMessage("Added " + quote.ToString());
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Usage: !addquote <FULL QUOTE TEXT>");
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
            : base(MixItUp.Base.Resources.MagicEightBall, new List<string>() { "magic8ball", "8ball" }, 5, UserRoleEnum.User)
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
                    if (products?.FirstOrDefault() is JObject product)
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
            : base(MixItUp.Base.Resources.XboxGame, new List<string>() { "xboxgame" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    string gameName = null;
                    if (arguments.Count() > 0)
                    {
                        gameName = string.Join(" ", arguments);
                    }
                    else
                    {
                        await ChannelSession.RefreshChannel();
                        if (ChannelSession.TwitchChannelV5 != null)
                        {
                            gameName = ChannelSession.TwitchChannelV5.game;
                        }
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
            : base(MixItUp.Base.Resources.SteamGame, new List<string>() { "steamgame", "steam" }, 5, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    string gameName = null;
                    if (arguments.Count() > 0)
                    {
                        gameName = string.Join(" ", arguments);
                    }
                    else
                    {
                        await ChannelSession.RefreshChannel();
                        if (ChannelSession.TwitchChannelV5 != null)
                        {
                            gameName = ChannelSession.TwitchChannelV5.game;
                        }
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
            : base(MixItUp.Base.Resources.SetTitle, "settitle", 5, UserRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (arguments.Count() > 0)
                    {
                        string name = string.Join(" ", arguments);
                        await ChannelSession.TwitchUserConnection.UpdateV5Channel(ChannelSession.TwitchChannelV5, status: name);
                        await ChannelSession.RefreshChannel();
                        await ChannelSession.Services.Chat.SendMessage("Title Updated: " + name);
                        return;
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Usage: !settitle <TITLE NAME>");
                    }
                }
            }));
        }
    }

    public class SetGameChatCommand : PreMadeChatCommand
    {
        public SetGameChatCommand()
            : base(MixItUp.Base.Resources.SetGame, "setgame", 5, UserRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (arguments.Count() > 0)
                    {
                        string name = string.Join(" ", arguments).ToLower();
                        IEnumerable<Twitch.Base.Models.NewAPI.Games.GameModel> games = await ChannelSession.TwitchUserConnection.GetNewAPIGamesByName(name);
                        if (games != null && games.Count() > 0)
                        {
                            Twitch.Base.Models.NewAPI.Games.GameModel game = games.FirstOrDefault(g => g.name.ToLower().Equals(name));
                            if (game == null)
                            {
                                game = games.First();
                            }
                            await ChannelSession.TwitchUserConnection.UpdateV5Channel(ChannelSession.TwitchChannelV5, game: game);
                            await ChannelSession.RefreshChannel();
                            await ChannelSession.Services.Chat.SendMessage("Game Updated: " + game.name);
                            return;
                        }
                        await ChannelSession.Services.Chat.SendMessage("We could not find a game with that name");
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Usage: !setgame <GAME NAME>");
                    }
                }
            }));
        }
    }

    public class SetUserTitleChatCommand : PreMadeChatCommand
    {
        public SetUserTitleChatCommand()
            : base(MixItUp.Base.Resources.SetUserTitle, "setusertitle", 5, UserRoleEnum.Mod)
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

                        UserViewModel targetUser = ChannelSession.Services.User.GetUserByUsername(username, this.platform);
                        if (targetUser != null)
                        {
                            targetUser.Title = string.Join(" ", arguments.Skip(1));
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.SendMessage(username + " could not be found in chat");
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Usage: !settitle <USERNAME> <TITLE NAME>");
                    }
                }
            }));
        }
    }

    public class AddCommandChatCommand : PreMadeChatCommand
    {
        public AddCommandChatCommand()
            : base(MixItUp.Base.Resources.AddCommand, new List<string>() { "addcommand" }, 5, UserRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments.Count() >= 3)
                {
                    string commandTrigger = arguments.ElementAt(0).ToLower();

                    if (!CommandBase.IsValidCommandString(commandTrigger))
                    {
                        await ChannelSession.Services.Chat.SendMessage("ERROR: Command trigger contain an invalid character");
                        return;
                    }

                    foreach (PermissionsCommandBase command in ChannelSession.AllEnabledChatCommands)
                    {
                        if (command.IsEnabled)
                        {
                            if (command.Commands.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase))
                            {
                                await ChannelSession.Services.Chat.SendMessage("ERROR: There already exists an enabled, chat command that uses the command trigger you have specified");
                                return;
                            }
                        }
                    }

                    if (!int.TryParse(arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                    {
                        await ChannelSession.Services.Chat.SendMessage("ERROR: Cooldown must be 0 or greater");
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
                    await ChannelSession.Services.Chat.SendMessage("Usage: !addcommand <COMMAND TRIGGER, NO !> <COOLDOWN> <FULL COMMAND MESSAGE TEXT>");
                }
            }));
        }
    }

    public class UpdateCommandChatCommand : PreMadeChatCommand
    {
        public UpdateCommandChatCommand()
            : base(MixItUp.Base.Resources.UpdateCommand, new List<string>() { "updatecommand", "editcommand" }, 5, UserRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments.Count() >= 2)
                {
                    string commandTrigger = arguments.ElementAt(0).ToLower();

                    PermissionsCommandBase command = ChannelSession.AllEnabledChatCommands.FirstOrDefault(c => c.Commands.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                    if (command == null)
                    {
                        await ChannelSession.Services.Chat.SendMessage("ERROR: Could not find any command with that trigger");
                        return;
                    }

                    if (!int.TryParse(arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                    {
                        await ChannelSession.Services.Chat.SendMessage("ERROR: Cooldown must be 0 or greater");
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
                    await ChannelSession.Services.Chat.SendMessage("Usage: !updatecommand <COMMAND TRIGGER, NO !> <COOLDOWN> [OPTIONAL FULL COMMAND MESSAGE TEXT]");
                }
            }));
        }
    }

    public class DisableCommandChatCommand : PreMadeChatCommand
    {
        public DisableCommandChatCommand()
            : base(MixItUp.Base.Resources.DisableCommand, new List<string>() { "disablecommand" }, 5, UserRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments.Count() == 1)
                {
                    string commandTrigger = arguments.ElementAt(0).ToLower();

                    PermissionsCommandBase command = ChannelSession.AllEnabledChatCommands.FirstOrDefault(c => c.Commands.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                    if (command == null)
                    {
                        await ChannelSession.Services.Chat.SendMessage("ERROR: Could not find any command with that trigger");
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
                    await ChannelSession.Services.Chat.SendMessage("Usage: !disablecommand <COMMAND TRIGGER, NO !>");
                }
            }));
        }
    }

    public class StartGiveawayChatCommand : PreMadeChatCommand
    {
        public StartGiveawayChatCommand()
            : base(MixItUp.Base.Resources.StartGiveaway, "startgiveaway", 5, UserRoleEnum.Streamer)
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
                            await ChannelSession.Services.Chat.SendMessage("ERROR: " + result);
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("Usage: !startgiveaway <GIVEAWAY ITEM>");
                    }
                }
            }));
        }
    }

    public class LinkMixerAccountChatCommand : PreMadeChatCommand
    {
        public static Dictionary<Guid, Guid> LinkedAccounts = new Dictionary<Guid, Guid>();

        public LinkMixerAccountChatCommand()
            : base("linkmixeraccount", "linkmixeraccount", 0, UserRoleEnum.User)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments != null && arguments.Count() == 1)
                {
                    string mixerUsername = arguments.First().Replace("@", "");
#pragma warning disable CS0612 // Type or member is obsolete
                    UserDataModel mixerUserData = ChannelSession.Settings.GetUserDataByUsername(StreamingPlatformTypeEnum.Mixer, mixerUsername);
#pragma warning restore CS0612 // Type or member is obsolete
                    if (mixerUserData != null)
                    {
                        LinkedAccounts[user.ID] = mixerUserData.ID;
                        await ChannelSession.Services.Chat.SendMessage($"@{user.Username} is attempting to link the Mixer account {mixerUserData.MixerUsername} to their {user.Platform} account. Mods can type \"!approvemixeraccount @<TWITCH USERNAME>\" in chat to approve this linking.");
                        return;
                    }
                    await ChannelSession.Services.Chat.SendMessage("There is no Mixer user data for that username");
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage("Usage: !linkmixeraccount <MIXER USERNAME>");
                }
            }));
        }
    }

    public class ApproveMixerAccountChatCommand : PreMadeChatCommand
    {
        public ApproveMixerAccountChatCommand()
            : base("approvemixeraccount", "approvemixeraccount", 0, UserRoleEnum.Mod)
        {
            this.Actions.Add(new CustomAction(async (UserViewModel user, IEnumerable<string> arguments) =>
            {
                if (arguments != null && arguments.Count() == 1)
                {
                    UserViewModel targetUser = ChannelSession.Services.User.GetUserByUsername(arguments.First().Replace("@", ""), user.Platform);
                    if (targetUser != null && LinkMixerAccountChatCommand.LinkedAccounts.ContainsKey(targetUser.ID))
                    {
                        UserDataModel mixerUserData = ChannelSession.Settings.GetUserData(LinkMixerAccountChatCommand.LinkedAccounts[targetUser.ID]);
                        if (mixerUserData != null)
                        {
                            LinkMixerAccountChatCommand.LinkedAccounts.Remove(targetUser.ID);
                            targetUser.Data.MergeData(mixerUserData);

                            ChannelSession.Settings.UserData.Remove(mixerUserData.ID);

                            await ChannelSession.Services.Chat.SendMessage($"The user data from the account {mixerUserData.MixerUsername} on Mixer has been deleted and merged into @{targetUser.Username}.");
                            return;
                        }
                        await ChannelSession.Services.Chat.SendMessage("There is no Mixer user data for that username");
                        return;
                    }
                    await ChannelSession.Services.Chat.SendMessage("The specified Twitch user has not run the !linkmixeraccount command");
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage("Usage: !approvemixeraccount <TWITCH USERNAME>");
                }
            }));
        }
    }

    #region Obsolete Pre-Made Commands

    [Obsolete]
    public class ObsoletePreMadeCommand : PreMadeChatCommand { public ObsoletePreMadeCommand() : base(string.Empty, string.Empty, 0, UserRoleEnum.User) { } }

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
