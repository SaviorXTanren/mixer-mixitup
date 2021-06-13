using MixItUp.Base.Commands;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class PreMadeChatCommandSettingsModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public UserRoleEnum Role { get; set; }
        [DataMember]
        public int Cooldown { get; set; }

        public PreMadeChatCommandSettingsModel() { }

        public PreMadeChatCommandSettingsModel(PreMadeChatCommandModelBase command)
        {
            this.Name = command.Name;
            this.IsEnabled = command.IsEnabled;
            this.Role = command.Requirements.Role.Role;
            this.Cooldown = command.Requirements.Cooldown.IndividualAmount;
        }
    }

    public abstract class PreMadeChatCommandModelBase : ChatCommandModel
    {
        public PreMadeChatCommandModelBase(string name, string trigger, int cooldown, UserRoleEnum role) : this(name, new HashSet<string>() { trigger }, cooldown, role) { }

        public PreMadeChatCommandModelBase(string name, HashSet<string> triggers, int cooldown, UserRoleEnum role)
            : base(name, CommandTypeEnum.PreMade, triggers, includeExclamation: true, wildcards: false)
        {
            this.Requirements.AddBasicRequirements();
            this.Requirements.Role.Role = role;
            this.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
            this.Requirements.Cooldown.IndividualAmount = cooldown;
        }

        public void UpdateFromSettings(PreMadeChatCommandSettingsModel settings)
        {
            this.IsEnabled = settings.IsEnabled;
            this.Requirements.Role.Role = settings.Role;
            this.Requirements.Cooldown.IndividualAmount = settings.Cooldown;
        }

        public override bool HasCustomRun { get { return true; } }

        public override HashSet<ActionTypeEnum> GetActionTypesInCommand(HashSet<Guid> commandIDs = null)
        {
            return new HashSet<ActionTypeEnum>() { ActionTypeEnum.Chat };
        }
    }

    public class MixItUpPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public MixItUpPreMadeChatCommandModel() : base("Mix It Up", "mixitup", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ServiceManager.Get<ChatService>().SendMessage("This channel uses the Mix It Up app to improve their stream. Check out http://mixitupapp.com for more information!", parameters.Platform);
        }
    }

    public class CommandsPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public CommandsPreMadeChatCommandModel() : base(MixItUp.Base.Resources.Commands, "commands", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            List<string> commandTriggers = new List<string>();
            foreach (ChatCommandModel command in ChannelSession.Services.Command.AllEnabledChatAccessibleCommands)
            {
                if (command.IsEnabled)
                {
                    RoleRequirementModel roleRequirement = command.Requirements.Role;
                    if (roleRequirement != null)
                    {
                        Result result = await roleRequirement.Validate(parameters);
                        if (result.Success)
                        {
                            if (command.IncludeExclamation)
                            {
                                commandTriggers.AddRange(command.Triggers.Select(c => $"!{c}"));
                            }
                            else
                            {
                                commandTriggers.AddRange(command.Triggers);
                            }
                        }
                    }
                }
            }

            if (commandTriggers.Count > 0)
            {
                string text = "Available Commands: " + string.Join(", ", commandTriggers.OrderBy(c => c));
                await ServiceManager.Get<ChatService>().SendMessage(text, parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("There are no commands available for you to use.", parameters.Platform);
            }
        }
    }

    public class GamesPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public GamesPreMadeChatCommandModel() : base(MixItUp.Base.Resources.Games, "games", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            List<string> commandTriggers = new List<string>();
            foreach (GameCommandModelBase command in ChannelSession.Services.Command.GameCommands)
            {
                if (command.IsEnabled)
                {
                    RoleRequirementModel roleRequirement = command.Requirements.Role;
                    if (roleRequirement != null)
                    {
                        Result result = await roleRequirement.Validate(parameters);
                        if (result.Success)
                        {
                            commandTriggers.AddRange(command.Triggers.Select(c => $"!{c}"));
                        }
                    }
                }
            }

            if (commandTriggers.Count > 0)
            {
                string text = "Available Games: " + string.Join(", ", commandTriggers.OrderBy(c => c));
                await ServiceManager.Get<ChatService>().SendMessage(text, parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("There are no games available for you to use.", parameters.Platform);
            }
        }
    }

    public class MixItUpCommandsPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public MixItUpCommandsPreMadeChatCommandModel() : base(MixItUp.Base.Resources.MixItUpCommands, "mixitupcommands", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ServiceManager.Get<ChatService>().SendMessage("All common, Mix It Up chat commands can be found here: https://github.com/SaviorXTanren/mixer-mixitup/wiki/Pre-Made-Chat-Commands. For commands specific to this stream, ask your streamer/moderator.", parameters.Platform);
        }
    }

    public class GamePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public GamePreMadeChatCommandModel() : base(MixItUp.Base.Resources.Game, "game", 5, UserRoleEnum.User) { }

        public static async Task<string> GetCurrentGameName()
        {
            string gameName = null;
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                await ServiceManager.Get<TwitchSessionService>().RefreshChannel();
                gameName = ServiceManager.Get<TwitchSessionService>().ChannelV5.game;
            }
            else if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
            {
                await ServiceManager.Get<GlimeshSessionService>().RefreshChannel();
                //gameName = ServiceManager.Get<GlimeshSessionService>().Channel?
            }
            return gameName;
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            string gameName = await GamePreMadeChatCommandModel.GetCurrentGameName();
            if (!string.IsNullOrEmpty(gameName))
            {
                GameInformation details = await XboxGamePreMadeChatCommandModel.GetXboxGameInfo(gameName);
                if (details == null)
                {
                    details = await SteamGamePreMadeChatCommandModel.GetSteamGameInfo(gameName);
                }

                if (details != null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(details.ToString(), parameters.Platform);
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage("Game: " + gameName, parameters.Platform);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("No Game Found", parameters.Platform);
            }
        }
    }

    public class TitlePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public TitlePreMadeChatCommandModel() : base(MixItUp.Base.Resources.Title, new HashSet<string>() { "title", "stream" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            string title = null;
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                await ServiceManager.Get<TwitchSessionService>().RefreshChannel();
                title = ServiceManager.Get<TwitchSessionService>().ChannelV5?.status;
            }
            else if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
            {
                await ServiceManager.Get<GlimeshSessionService>().RefreshChannel();
                title = ServiceManager.Get<GlimeshSessionService>().Channel?.title;
            }

            if (!string.IsNullOrEmpty(title))
            {
                await ServiceManager.Get<ChatService>().SendMessage("Stream Title: " + title, parameters.Platform);
            }
        }
    }

    public class UptimePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public static async Task<DateTimeOffset> GetStartTime()
        {
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                await ServiceManager.Get<GlimeshSessionService>().RefreshChannel();
                if (ServiceManager.Get<TwitchSessionService>().StreamIsLive)
                {
                    return TwitchPlatformService.GetTwitchDateTime(ServiceManager.Get<TwitchSessionService>().StreamV5.created_at);
                }
            }
            else if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
            {
                await ServiceManager.Get<GlimeshSessionService>().RefreshChannel();
                return GlimeshPlatformService.GetGlimeshDateTime(ServiceManager.Get<GlimeshSessionService>().Channel?.stream?.startedAt);
            }
            return DateTimeOffset.MinValue;
        }

        public UptimePreMadeChatCommandModel() : base(MixItUp.Base.Resources.Uptime, "uptime", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            DateTimeOffset startTime = await UptimePreMadeChatCommandModel.GetStartTime();
            if (startTime > DateTimeOffset.MinValue)
            {
                TimeSpan duration = DateTimeOffset.Now.Subtract(startTime);
                await ServiceManager.Get<ChatService>().SendMessage("Start Time: " + startTime.ToCorrectLocalTime().ToString("MMMM dd, yyyy - h:mm tt") + ", Stream Length: " + (int)duration.TotalHours + duration.ToString("\\:mm"), parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Stream is currently offline", parameters.Platform);
            }
        }
    }

    public class FollowAgePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public FollowAgePreMadeChatCommandModel() : base(MixItUp.Base.Resources.FollowAge, "followage", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ChannelSession.Services.Chat.SendMessage(parameters.User.FullDisplayName + "'s Follow Age: " + parameters.User.FollowAgeString);
        }
    }

    public class SubscribeAgePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SubscribeAgePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SubscribeAge, new HashSet<string>() { "subage", "subscribeage" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ChannelSession.Services.Chat.SendMessage(parameters.User.FullDisplayName + "'s Subscribe Age: " + parameters.User.SubscribeAgeString);
        }
    }

    public class StreamerAgePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public StreamerAgePreMadeChatCommandModel() : base(MixItUp.Base.Resources.StreamerAge, new HashSet<string>() { "streamerage", "age" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ChannelSession.Services.Chat.SendMessage(parameters.User.FullDisplayName + "'s Streamer Age: " + parameters.User.AccountAgeString);
        }
    }

    public class QuotePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public QuotePreMadeChatCommandModel() : base(MixItUp.Base.Resources.Quote, new HashSet<string>() { "quote", "quotes" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.QuotesEnabled)
            {
                if (ChannelSession.Settings.Quotes.Count > 0)
                {
                    int quoteNumber = 0;
                    UserQuoteModel quote = null;

                    if (parameters.Arguments.Count() == 1)
                    {
                        if (!int.TryParse(parameters.Arguments.ElementAt(0), out quoteNumber))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage("USAGE: !quote [QUOTE NUMBER]", parameters.Platform);
                            return;
                        }

                        quote = ChannelSession.Settings.Quotes.SingleOrDefault(q => q.ID == quoteNumber);
                        if (quote == null)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage($"Unable to find quote number {quoteNumber}.", parameters.Platform);
                        }
                    }
                    else if (parameters.Arguments.Count() == 0)
                    {
                        int quoteIndex = RandomHelper.GenerateRandomNumber(ChannelSession.Settings.Quotes.Count);
                        quote = ChannelSession.Settings.Quotes[quoteIndex];
                    }
                    else
                    {
                        await ServiceManager.Get<ChatService>().SendMessage("USAGE: !quote [QUOTE NUMBER]", parameters.Platform);
                        return;
                    }

                    if (quote != null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(quote.ToString(), parameters.Platform);
                    }
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage("At least 1 quote must be added for this feature to work", parameters.Platform);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Quotes must be enabled for this feature to work", parameters.Platform);
            }
        }
    }

    public class LastQuotePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public LastQuotePreMadeChatCommandModel() : base(MixItUp.Base.Resources.LastQuote, new HashSet<string>() { "lastquote" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.QuotesEnabled)
            {
                if (ChannelSession.Settings.Quotes.Count > 0)
                {
                    UserQuoteModel quote = ChannelSession.Settings.Quotes.LastOrDefault();
                    if (quote != null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(quote.ToString(), parameters.Platform);
                        return;
                    }
                }
                await ServiceManager.Get<ChatService>().SendMessage("At least 1 quote must be added for this feature to work", parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Quotes must be enabled for this feature to work", parameters.Platform);
            }
        }
    }

    public class AddQuotePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public AddQuotePreMadeChatCommandModel() : base(MixItUp.Base.Resources.AddQuote, new HashSet<string>() { "addquote", "quoteadd" }, 5, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (ChannelSession.Settings.QuotesEnabled)
            {
                if (parameters.Arguments.Count() > 0)
                {
                    StringBuilder quoteBuilder = new StringBuilder();
                    foreach (string arg in parameters.Arguments)
                    {
                        quoteBuilder.Append(arg + " ");
                    }

                    string quoteText = quoteBuilder.ToString();
                    quoteText = quoteText.Trim(new char[] { ' ', '\'', '\"' });

                    UserQuoteModel quote = new UserQuoteModel(UserQuoteViewModel.GetNextQuoteNumber(), quoteText, DateTimeOffset.Now, await GamePreMadeChatCommandModel.GetCurrentGameName());
                    ChannelSession.Settings.Quotes.Add(quote);
                    await ChannelSession.SaveSettings();

                    GlobalEvents.QuoteAdded(quote);

                    await ServiceManager.Get<ChatService>().SendMessage("Added " + quote.ToString(), parameters.Platform);
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage("Usage: !addquote <FULL QUOTE TEXT>", parameters.Platform);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Quotes must be enabled with Mix It Up for this feature to work", parameters.Platform);
            }
        }
    }

    public class Magic8BallPreMadeChatCommandModel : PreMadeChatCommandModelBase
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

        public Magic8BallPreMadeChatCommandModel() : base(MixItUp.Base.Resources.MagicEightBall, new HashSet<string>() { "magic8ball", "8ball" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            int index = RandomHelper.GenerateRandomNumber(this.responses.Count);
            await ServiceManager.Get<ChatService>().SendMessage(string.Format("The Magic 8-Ball says: \"{0}\"", this.responses[index]), parameters.Platform);
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

    public class XboxGamePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public static async Task<GameInformation> GetXboxGameInfo(string gameName)
        {
            try
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
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public XboxGamePreMadeChatCommandModel() : base(MixItUp.Base.Resources.XboxGame, "xboxgame", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            string gameName = null;
            if (parameters.Arguments.Count() > 0)
            {
                gameName = string.Join(" ", parameters.Arguments);
            }
            else
            {
                gameName = await GamePreMadeChatCommandModel.GetCurrentGameName();
            }

            GameInformation details = await XboxGamePreMadeChatCommandModel.GetXboxGameInfo(gameName);
            if (details != null)
            {
                await ServiceManager.Get<ChatService>().SendMessage(details.ToString(), parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(string.Format("Could not find the game \"{0}\" on Xbox", gameName), parameters.Platform);
            }
        }
    }

    public class SteamGamePreMadeChatCommandModel : PreMadeChatCommandModelBase
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
                            SteamGamePreMadeChatCommandModel.steamGameList[game["name"].ToString().ToLower()] = (int)game["appid"];
                        }
                    }
                }
            }

            int gameID = -1;
            if (SteamGamePreMadeChatCommandModel.steamGameList.ContainsKey(gameName))
            {
                gameID = SteamGamePreMadeChatCommandModel.steamGameList[gameName];
            }
            else
            {
                string foundGame = SteamGamePreMadeChatCommandModel.steamGameList.Keys.FirstOrDefault(g => g.Contains(gameName));
                if (foundGame != null)
                {
                    gameID = SteamGamePreMadeChatCommandModel.steamGameList[foundGame];
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

        public SteamGamePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SteamGame, "steamgame", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            string gameName = null;
            if (parameters.Arguments.Count() > 0)
            {
                gameName = string.Join(" ", parameters.Arguments);
            }
            else
            {
                gameName = await GamePreMadeChatCommandModel.GetCurrentGameName();
            }

            GameInformation details = await SteamGamePreMadeChatCommandModel.GetSteamGameInfo(gameName);
            if (details != null)
            {
                await ServiceManager.Get<ChatService>().SendMessage(details.ToString(), parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(string.Format("Could not find the game \"{0}\" on Steam", gameName), parameters.Platform);
            }
        }
    }

    public class SetTitlePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SetTitlePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SetTitle, "settitle", 5, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() > 0)
            {
                string name = string.Join(" ", parameters.Arguments);
                if (ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    await ServiceManager.Get<TwitchSessionService>().UserConnection.UpdateV5Channel(ServiceManager.Get<TwitchSessionService>().ChannelV5, status: name);
                    await ServiceManager.Get<TwitchSessionService>().RefreshChannel();
                    await ServiceManager.Get<ChatService>().SendMessage("Title Updated: " + name, parameters.Platform);
                }
                // TODO
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !settitle <TITLE NAME>", parameters.Platform);
            }
        }
    }

    public class SetGamePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SetGamePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SetGame, "setgame", 5, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() > 0)
            {
                string name = string.Join(" ", parameters.Arguments).ToLower();
                IEnumerable<Twitch.Base.Models.NewAPI.Games.GameModel> games = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIGamesByName(name);
                if (games != null && games.Count() > 0)
                {
                    Twitch.Base.Models.NewAPI.Games.GameModel game = games.FirstOrDefault(g => g.name.ToLower().Equals(name));
                    if (game == null)
                    {
                        game = games.First();
                    }

                    // TODO
                    if (ServiceManager.Get<TwitchSessionService>().IsConnected)
                    {
                        await ServiceManager.Get<TwitchSessionService>().UserConnection.UpdateV5Channel(ServiceManager.Get<TwitchSessionService>().ChannelV5, game: game);
                        await ServiceManager.Get<TwitchSessionService>().RefreshChannel();
                        await ServiceManager.Get<ChatService>().SendMessage("Game Updated: " + game.name, parameters.Platform);
                    }
                    return;
                }
                await ServiceManager.Get<ChatService>().SendMessage("We could not find a game with that name", parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !setgame <GAME NAME>", parameters.Platform);
            }
        }
    }

    public class SetUserTitlePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SetUserTitlePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SetUserTitle, "setusertitle", 5, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() > 1)
            {
                string username = parameters.Arguments.ElementAt(0);
                if (username.StartsWith("@"))
                {
                    username = username.Substring(1);
                }

                UserViewModel targetUser = ChannelSession.Services.User.GetActiveUserByUsername(username, parameters.Platform);
                if (targetUser != null)
                {
                    targetUser.Title = string.Join(" ", parameters.Arguments.Skip(1));
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage(username + " could not be found in chat", parameters.Platform);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !settitle <USERNAME> <TITLE NAME>", parameters.Platform);
            }
        }
    }

    public class AddCommandPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public AddCommandPreMadeChatCommandModel() : base(MixItUp.Base.Resources.AddCommand, "addcommand", 5, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() >= 3)
            {
                string commandTrigger = parameters.Arguments.ElementAt(0).ToLower();

                if (!ChatCommandModel.IsValidCommandTrigger(commandTrigger))
                {
                    await ServiceManager.Get<ChatService>().SendMessage("ERROR: Command trigger contain an invalid character", parameters.Platform);
                    return;
                }

                foreach (CommandModelBase command in ChannelSession.Services.Command.AllEnabledChatAccessibleCommands)
                {
                    if (command.IsEnabled)
                    {
                        if (command.Triggers.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage("ERROR: There already exists an enabled, chat command that uses the command trigger you have specified", parameters.Platform);
                            return;
                        }
                    }
                }

                if (!int.TryParse(parameters.Arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                {
                    await ServiceManager.Get<ChatService>().SendMessage("ERROR: Cooldown must be 0 or greater", parameters.Platform);
                    return;
                }

                StringBuilder commandTextBuilder = new StringBuilder();
                foreach (string arg in parameters.Arguments.Skip(2))
                {
                    commandTextBuilder.Append(arg + " ");
                }

                string commandText = commandTextBuilder.ToString();
                commandText = commandText.Trim(new char[] { ' ', '\'', '\"' });

                ChatCommandModel newCommand = new ChatCommandModel(commandTrigger, new HashSet<string>() { commandTrigger }, includeExclamation: true, wildcards: false);
                newCommand.Requirements.AddBasicRequirements();
                newCommand.Requirements.Role.Role = UserRoleEnum.User;
                newCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                newCommand.Requirements.Cooldown.IndividualAmount = cooldown;
                newCommand.Actions.Add(new ChatActionModel(commandText));
                ChannelSession.Settings.SetCommand(newCommand);
                ChannelSession.Services.Command.ChatCommands.Add(newCommand);

                await ServiceManager.Get<ChatService>().SendMessage("Added New Command: !" + commandTrigger, parameters.Platform);

                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !addcommand <COMMAND TRIGGER, NO !> <COOLDOWN> <FULL COMMAND MESSAGE TEXT>", parameters.Platform);
            }
        }
    }

    public class UpdateCommandPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public UpdateCommandPreMadeChatCommandModel() : base(MixItUp.Base.Resources.UpdateCommand, new HashSet<string>() { "updatecommand", "editcommand" }, 5, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() >= 2)
            {
                string commandTrigger = parameters.Arguments.ElementAt(0).ToLower();

                CommandModelBase command = ChannelSession.Services.Command.AllEnabledChatAccessibleCommands.FirstOrDefault(c => c.Triggers.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                if (command == null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage("ERROR: Could not find any command with that trigger", parameters.Platform);
                    return;
                }

                if (!int.TryParse(parameters.Arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                {
                    await ServiceManager.Get<ChatService>().SendMessage("ERROR: Cooldown must be 0 or greater", parameters.Platform);
                    return;
                }

                if (command.Requirements.Cooldown != null)
                {
                    command.Requirements.Cooldown.IndividualAmount = cooldown;
                }

                if (parameters.Arguments.Count() > 2)
                {
                    StringBuilder commandTextBuilder = new StringBuilder();
                    foreach (string arg in parameters.Arguments.Skip(2))
                    {
                        commandTextBuilder.Append(arg + " ");
                    }

                    string commandText = commandTextBuilder.ToString();
                    commandText = commandText.Trim(new char[] { ' ', '\'', '\"' });

                    command.Actions.Clear();
                    command.Actions.Add(new ChatActionModel(commandText));
                }

                await ServiceManager.Get<ChatService>().SendMessage("Updated Command: !" + commandTrigger, parameters.Platform);
                ServiceManager.Get<ChatService>().RebuildCommandTriggers();

                ChannelSession.Settings.SetCommand(command);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !updatecommand <COMMAND TRIGGER, NO !> <COOLDOWN> [OPTIONAL FULL COMMAND MESSAGE TEXT]", parameters.Platform);
            }
        }
    }

    public class DisableCommandPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public DisableCommandPreMadeChatCommandModel() : base(MixItUp.Base.Resources.DisableCommand, "disablecommand", 5, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() == 1)
            {
                string commandTrigger = parameters.Arguments.ElementAt(0).ToLower();

                CommandModelBase command = ChannelSession.Services.Command.AllEnabledChatAccessibleCommands.FirstOrDefault(c => c.Triggers.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                if (command == null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage("ERROR: Could not find any command with that trigger", parameters.Platform);
                    return;
                }

                command.IsEnabled = false;

                await ServiceManager.Get<ChatService>().SendMessage("Disabled Command: !" + commandTrigger, parameters.Platform);

                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !disablecommand <COMMAND TRIGGER, NO !>", parameters.Platform);
            }
        }
    }

    public class StartGiveawayPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public StartGiveawayPreMadeChatCommandModel() : base(MixItUp.Base.Resources.StartGiveaway, "startgiveaway", 5, UserRoleEnum.Streamer) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() > 0)
            {
                string result = await ServiceManager.Get<GiveawayService>().Start(string.Join(" ", parameters.Arguments));
                if (!string.IsNullOrEmpty(result))
                {
                    await ServiceManager.Get<ChatService>().SendMessage("ERROR: " + result, parameters.Platform);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !startgiveaway <GIVEAWAY ITEM>", parameters.Platform);
            }
        }
    }

    public class LinkMixerAccountPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public static Dictionary<Guid, Guid> LinkedAccounts = new Dictionary<Guid, Guid>();

        public LinkMixerAccountPreMadeChatCommandModel() : base("Link Mixer Account", "linkmixeraccount", 0, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments != null && parameters.Arguments.Count() == 1)
            {
                // TODO
//                string mixerUsername = parameters.Arguments.First().Replace("@", "");
//#pragma warning disable CS0612 // Type or member is obsolete
//                UserDataModel mixerUserData = ChannelSession.Settings.GetUserDataByUsername(StreamingPlatformTypeEnum.Mixer, mixerUsername);
//#pragma warning restore CS0612 // Type or member is obsolete
//                if (mixerUserData != null)
//                {
//                    LinkedAccounts[parameters.User.ID] = mixerUserData.ID;
//                    await ChannelSession.Services.Chat.SendMessage($"@{parameters.User.Username} is attempting to link the Mixer account {mixerUserData.MixerUsername} to their {parameters.User.Platform} account. Mods can type \"!approvemixeraccount @<TWITCH USERNAME>\" in chat to approve this linking.");
//                    return;
//                }
//                await ChannelSession.Services.Chat.SendMessage("There is no Mixer user data for that username");
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !linkmixeraccount <MIXER USERNAME>", parameters.Platform);
            }
        }
    }

    public class ApproveMixerAccountPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public ApproveMixerAccountPreMadeChatCommandModel() : base("Approve Mixer Account", "approvemixeraccount", 0, UserRoleEnum.Mod) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments != null && parameters.Arguments.Count() == 1)
            {
                UserViewModel targetUser = ChannelSession.Services.User.GetActiveUserByUsername(parameters.Arguments.First().Replace("@", ""), parameters.User.Platform);
                if (targetUser != null && LinkMixerAccountPreMadeChatCommandModel.LinkedAccounts.ContainsKey(targetUser.ID))
                {
                    UserDataModel mixerUserData = ChannelSession.Settings.GetUserData(LinkMixerAccountPreMadeChatCommandModel.LinkedAccounts[targetUser.ID]);
                    if (mixerUserData != null)
                    {
                        LinkMixerAccountPreMadeChatCommandModel.LinkedAccounts.Remove(targetUser.ID);
                        targetUser.Data.MergeData(mixerUserData);

                        ChannelSession.Settings.UserData.Remove(mixerUserData.ID);

                        await ServiceManager.Get<ChatService>().SendMessage($"The user data from the account {mixerUserData.MixerUsername} on Mixer has been deleted and merged into @{targetUser.Username}.", parameters.Platform);
                        return;
                    }
                    await ServiceManager.Get<ChatService>().SendMessage("There is no Mixer user data for that username", parameters.Platform);
                    return;
                }
                await ServiceManager.Get<ChatService>().SendMessage("The specified Twitch user has not run the !linkmixeraccount command", parameters.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage("Usage: !approvemixeraccount <TWITCH USERNAME>", parameters.Platform);
            }
        }
    }
}
