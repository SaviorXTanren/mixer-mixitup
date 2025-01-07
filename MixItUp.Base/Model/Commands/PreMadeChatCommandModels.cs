using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
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
        public UserRoleEnum UserRole { get; set; }
        [DataMember]
        public int Cooldown { get; set; }

        public PreMadeChatCommandSettingsModel() { }

        public PreMadeChatCommandSettingsModel(PreMadeChatCommandModelBase command)
        {
            this.Name = command.Name;
            this.IsEnabled = command.IsEnabled;
            this.UserRole = command.Requirements.Role.UserRole;
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
            this.Requirements.Role.UserRole = role;
            this.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
            this.Requirements.Cooldown.IndividualAmount = cooldown;
        }

        public void UpdateFromSettings(PreMadeChatCommandSettingsModel settings)
        {
            this.IsEnabled = settings.IsEnabled;
            this.Requirements.Role.UserRole = settings.UserRole;
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
            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandMixItUp, parameters);
        }
    }

    public class CommandsPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public CommandsPreMadeChatCommandModel() : base(MixItUp.Base.Resources.Commands, "commands", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            string groupFilter = (parameters.Arguments != null) ? string.Join(" ", parameters.Arguments) : null;

            List<string> commandTriggers = new List<string>();
            foreach (ChatCommandModel command in ServiceManager.Get<CommandService>().AllEnabledChatAccessibleCommands)
            {
                if (command.IsEnabled && !command.Wildcards)
                {
                    if (string.IsNullOrEmpty(groupFilter) || string.Equals(groupFilter, command.GroupName, StringComparison.OrdinalIgnoreCase))
                    {
                        RoleRequirementModel roleRequirement = command.Requirements.Role;
                        if (roleRequirement != null)
                        {
                            Result result = await roleRequirement.Validate(parameters);
                            if (result.Success)
                            {
                                string firstTrigger = command.Triggers.First();
                                if (command.IncludeExclamation)
                                {
                                    firstTrigger = $"!{firstTrigger}";
                                }

                                commandTriggers.Add(firstTrigger);
                            }
                        }
                    }
                }
            }

            if (commandTriggers.Count > 0)
            {
                string text = MixItUp.Base.Resources.PreMadeChatCommandCommandsHeader + string.Join(", ", commandTriggers.OrderBy(c => c));
                await ServiceManager.Get<ChatService>().SendMessage(text, parameters);
            }
        }
    }

    public class GamesPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public GamesPreMadeChatCommandModel() : base(MixItUp.Base.Resources.Games, "games", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            List<string> commandTriggers = new List<string>();
            foreach (GameCommandModelBase command in ServiceManager.Get<CommandService>().GameCommands)
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
                string text = MixItUp.Base.Resources.PreMadeChatCommandGamesHeader + string.Join(", ", commandTriggers.OrderBy(c => c));
                await ServiceManager.Get<ChatService>().SendMessage(text, parameters);
            }
        }
    }

    public class MixItUpCommandsPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public MixItUpCommandsPreMadeChatCommandModel() : base(MixItUp.Base.Resources.MixItUpCommands, "mixitupcommands", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandMixItUpCommands, parameters);
        }
    }

    public class GamePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public GamePreMadeChatCommandModel() : base(MixItUp.Base.Resources.Game, new HashSet<string>() { "game", "category" }, 5, UserRoleEnum.User) { }

        public static Task<string> GetCurrentGameName(StreamingPlatformTypeEnum platform)
        {
            return Task.FromResult(StreamingPlatforms.GetPlatformSession(platform).StreamCategoryName);
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            string gameName = await GamePreMadeChatCommandModel.GetCurrentGameName(parameters.Platform);
            if (!string.IsNullOrEmpty(gameName))
            {
                GameInformation details = await XboxGamePreMadeChatCommandModel.GetXboxGameInfo(gameName);
                if (details == null)
                {
                    details = await SteamGamePreMadeChatCommandModel.GetSteamGameInfo(gameName);
                }

                if (details != null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(details.ToString(), parameters);
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.CategoryHeader + gameName, parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorHeader + MixItUp.Base.Resources.NoCategoryFound, parameters);
            }
        }
    }

    public class TitlePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public TitlePreMadeChatCommandModel() : base(MixItUp.Base.Resources.Title, new HashSet<string>() { "title", "stream" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            string title = StreamingPlatforms.GetPlatformSession(parameters.Platform).StreamTitle;
            if (!string.IsNullOrEmpty(title))
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.StreamTitleHeader + title, parameters);
            }
        }
    }

    public class UptimePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public static async Task<DateTimeOffset> GetStartTime(StreamingPlatformTypeEnum platform)
        {
            if (StreamingPlatforms.IsValidPlatform(platform))
            {
                StreamingPlatformSessionBase platformService = StreamingPlatforms.GetPlatformSession(platform);
                if (platformService.IsConnected)
                {
                    return platformService.StreamStart;
                }
            }
            else
            {
                foreach (StreamingPlatformTypeEnum p in StreamingPlatforms.GetConnectedPlatforms())
                {
                    DateTimeOffset startTime = await UptimePreMadeChatCommandModel.GetStartTime(p);
                    if (startTime != DateTimeOffset.MinValue)
                    {
                        return startTime;
                    }
                }
            }

            return DateTimeOffset.MinValue;
        }

        public UptimePreMadeChatCommandModel() : base(MixItUp.Base.Resources.Uptime, "uptime", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            DateTimeOffset startTime = await UptimePreMadeChatCommandModel.GetStartTime(parameters.Platform);
            if (startTime > DateTimeOffset.MinValue)
            {
                TimeSpan duration = DateTimeOffset.Now.Subtract(startTime);
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.StartTimeHeader + startTime.ToCorrectLocalTime().ToString("MMMM dd, yyyy - h:mm tt") + ", " +
                    MixItUp.Base.Resources.StreamLengthHeader + (int)duration.TotalHours + duration.ToString("\\:mm"), parameters);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.StreamIsCurrentlyOffline, parameters);
            }
        }
    }

    public class FollowAgePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public FollowAgePreMadeChatCommandModel() : base(MixItUp.Base.Resources.FollowAge, "followage", 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.UserFollowAgeHeader, parameters.User.FullDisplayName) + parameters.User.FollowAgeString, parameters);
        }
    }

    public class SubscribeAgePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SubscribeAgePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SubscribeAge, new HashSet<string>() { "subage", "subscribeage" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.UserSubscribeAgeHeader, parameters.User.FullDisplayName) + parameters.User.SubscribeAgeString, parameters);
        }
    }

    public class StreamerAgePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public StreamerAgePreMadeChatCommandModel() : base(MixItUp.Base.Resources.StreamerAge, new HashSet<string>() { "streamerage", "age" }, 5, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.StreamerAgeHeader, parameters.User.FullDisplayName) + parameters.User.AccountAgeString, parameters);
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

                    if (parameters.Arguments.Count() >= 1)
                    {
                        bool parsedNumber = false;
                        if (parameters.Arguments.Count() == 1 && int.TryParse(parameters.Arguments.ElementAt(0), out quoteNumber))
                        {
                            parsedNumber = true;
                            quote = ChannelSession.Settings.Quotes.SingleOrDefault(q => q.ID == quoteNumber);
                        }

                        if (quote == null)
                        {
                            string searchText = string.Join(" ", parameters.Arguments).ToLower();
                            var applicableQuotes = ChannelSession.Settings.Quotes.Where(q => q.Quote.ToLower().Contains(searchText));
                            if (applicableQuotes.Count() > 0)
                            {
                                quote = applicableQuotes.Random();
                            }
                        }

                        if (quote == null)
                        {
                            if (parsedNumber)
                            {
                                await ServiceManager.Get<ChatService>().SendMessage(String.Format(MixItUp.Base.Resources.PreMadeChatCommandQuoteUnableToFind, quoteNumber), parameters);
                            }
                            else
                            {
                                await ServiceManager.Get<ChatService>().SendMessage(String.Format(MixItUp.Base.Resources.PreMadeChatCommandQuoteUnableToFindText, string.Join(" ", parameters.Arguments)), parameters);
                            }
                            return;
                        }
                    }
                    else if (parameters.Arguments.Count() == 0)
                    {
                        int quoteIndex = RandomHelper.GenerateRandomNumber(ChannelSession.Settings.Quotes.Count);
                        quote = ChannelSession.Settings.Quotes[quoteIndex];
                    }
                    else
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandQuoteUsage, parameters);
                        return;
                    }

                    if (quote != null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(quote.ToString(), parameters);
                    }
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandQuotesNotEnabled, parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandQuotesNotEnabled, parameters);
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
                        await ServiceManager.Get<ChatService>().SendMessage(quote.ToString(), parameters);
                        return;
                    }
                }
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandQuotesNotEnabled, parameters);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandQuotesNotEnabled, parameters);
            }
        }
    }

    public class AddQuotePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public AddQuotePreMadeChatCommandModel() : base(MixItUp.Base.Resources.AddQuote, new HashSet<string>() { "addquote", "quoteadd" }, 5, UserRoleEnum.Moderator) { }

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
                    quoteText = quoteText.Trim();

                    char[] quoteCharacters = new char[] { '\'', '\"' };
                    if (quoteText.First() == quoteText.Last() && quoteCharacters.Contains(quoteText.First()) && quoteCharacters.Contains(quoteText.Last()))
                    {
                        quoteText = quoteText.Trim(quoteCharacters);
                    }

                    UserQuoteModel quote = new UserQuoteModel(UserQuoteViewModel.GetNextQuoteNumber(), quoteText, DateTimeOffset.Now, await GamePreMadeChatCommandModel.GetCurrentGameName(parameters.Platform));
                    ChannelSession.Settings.Quotes.Add(quote);
                    await ChannelSession.SaveSettings();

                    UserQuoteModel.QuoteAdded(quote);

                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.QuoteAddedHeader + quote.ToString(), parameters);
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandAddQuoteUsage, parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandQuotesNotEnabled, parameters);
            }
        }
    }

    public class DeleteQuotePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public DeleteQuotePreMadeChatCommandModel() : base(MixItUp.Base.Resources.DeleteQuote, new HashSet<string>() { "deletequote", "quotedelete" }, 5, UserRoleEnum.Moderator) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (!ChannelSession.Settings.QuotesEnabled || ChannelSession.Settings.Quotes.Count == 0)
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandQuotesNotEnabled, parameters);
                return;
            }

            int quoteNumber = 0;
            if (parameters.Arguments.Count() != 1 || !int.TryParse(parameters.Arguments.ElementAt(0), out quoteNumber))
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandDeleteQuoteUsage, parameters);
                return;
            }

            UserQuoteModel quote = ChannelSession.Settings.Quotes.SingleOrDefault(q => q.ID == quoteNumber);
            if (quote == null)
            {
                await ServiceManager.Get<ChatService>().SendMessage(String.Format(MixItUp.Base.Resources.PreMadeChatCommandQuoteUnableToFind, quoteNumber), parameters);
                return;
            }

            ChannelSession.Settings.Quotes.Remove(quote);

            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.QuoteDeletedHeader + quote.ToString(), parameters);
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
            await ServiceManager.Get<ChatService>().SendMessage(string.Format("The Magic 8-Ball says: \"{0}\"", this.responses[index]), parameters);
        }
    }

    public class GameInformation
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Uri { get; set; }

        public override string ToString()
        {
            return $"{MixItUp.Base.Resources.GameHeader} {Name} - {Price} - {Uri}";
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
                gameName = await GamePreMadeChatCommandModel.GetCurrentGameName(parameters.Platform);
            }

            GameInformation details = await XboxGamePreMadeChatCommandModel.GetXboxGameInfo(gameName);
            if (details != null)
            {
                await ServiceManager.Get<ChatService>().SendMessage(details.ToString(), parameters);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorCouldNotFindGame, parameters);
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
                gameName = await GamePreMadeChatCommandModel.GetCurrentGameName(parameters.Platform);
            }

            GameInformation details = await SteamGamePreMadeChatCommandModel.GetSteamGameInfo(gameName);
            if (details != null)
            {
                await ServiceManager.Get<ChatService>().SendMessage(details.ToString(), parameters);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorCouldNotFindGame, parameters);
            }
        }
    }

    public class SetTitlePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SetTitlePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SetTitle, "settitle", 5, UserRoleEnum.Moderator) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() > 0)
            {
                string name = string.Join(" ", parameters.Arguments);
                await StreamingPlatforms.ForEachPlatform(async (p) =>
                {
                    StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(p);
                    if (session.IsConnected)
                    {
                        await session.SetStreamTitle(name);
                        await session.RefreshDetails();
                    }
                });
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.TitleUpdatedHeader + name, parameters);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandSetTitleUsage, parameters);
            }
        }
    }

    public class SetGamePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SetGamePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SetGame, new HashSet<string>() { "setgame", "setcategory" }, 5, UserRoleEnum.Moderator) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() > 0)
            {
                string name = string.Join(" ", parameters.Arguments).ToLower();
                Dictionary<StreamingPlatformTypeEnum, Result> results = new Dictionary<StreamingPlatformTypeEnum, Result>();

                await StreamingPlatforms.ForEachPlatform(async (p) =>
                {
                    StreamingPlatformSessionBase session = StreamingPlatforms.GetPlatformSession(p);
                    if (session.IsConnected)
                    {
                        results[p] = await session.SetStreamCategory(name);
                        await session.RefreshDetails();
                    }
                });

                if (results.Count > 0 && results.All(r => r.Value.Success))
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.CategroryUpdatedHeader + name, parameters);
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorFailedToUpdateCategory, parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandSetGameUsage, parameters);
            }
        }
    }

    public class SetUserTitlePreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public SetUserTitlePreMadeChatCommandModel() : base(MixItUp.Base.Resources.SetUserTitle, "setusertitle", 5, UserRoleEnum.Moderator) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() > 1)
            {
                string username = parameters.Arguments.ElementAt(0);

                UserV2ViewModel targetUser = ServiceManager.Get<UserService>().GetActiveUserByPlatform(parameters.Platform, platformUsername: username);
                if (targetUser != null)
                {
                    targetUser.CustomTitle = string.Join(" ", parameters.Arguments.Skip(1));
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.UserNotFound, parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandSetUserTitleUsage, parameters);
            }
        }
    }

    public class AddCommandPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public AddCommandPreMadeChatCommandModel() : base(MixItUp.Base.Resources.AddCommand, "addcommand", 5, UserRoleEnum.Moderator) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() >= 3)
            {
                string commandTrigger = parameters.Arguments.ElementAt(0).ToLower();

                if (!ChatCommandModel.IsValidCommandTrigger(commandTrigger))
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorHeader + MixItUp.Base.Resources.ChatCommandInvalidTriggers, parameters);
                    return;
                }

                foreach (CommandModelBase command in ServiceManager.Get<CommandService>().AllEnabledChatAccessibleCommands)
                {
                    if (command.IsEnabled)
                    {
                        if (command.Triggers.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorHeader + string.Format(MixItUp.Base.Resources.ChatCommandTriggerAlreadyExists, commandTrigger), parameters);
                            return;
                        }
                    }
                }

                if (!int.TryParse(parameters.Arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ValidCooldownAmountMustBeSpecified, parameters);
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
                newCommand.Requirements.Role.UserRole = UserRoleEnum.User;
                newCommand.Requirements.Cooldown.Type = CooldownTypeEnum.Standard;
                newCommand.Requirements.Cooldown.IndividualAmount = cooldown;
                newCommand.Actions.Add(new ChatActionModel(commandText));
                ChannelSession.Settings.SetCommand(newCommand);
                ServiceManager.Get<CommandService>().ChatCommands.Add(newCommand);

                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.CommandAddedHeader + commandTrigger, parameters);

                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandAddCommandUsage, parameters);
            }
        }
    }

    public class UpdateCommandPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public UpdateCommandPreMadeChatCommandModel() : base(MixItUp.Base.Resources.UpdateCommand, new HashSet<string>() { "updatecommand", "editcommand" }, 5, UserRoleEnum.Moderator) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() >= 2)
            {
                string commandTrigger = parameters.Arguments.ElementAt(0).ToLower();

                CommandModelBase command = ServiceManager.Get<CommandService>().AllEnabledChatAccessibleCommands.FirstOrDefault(c => c.Triggers.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                if (command == null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorHeader + MixItUp.Base.Resources.CouldNotFindCommand, parameters);
                    return;
                }

                if (!int.TryParse(parameters.Arguments.ElementAt(1), out int cooldown) || cooldown < 0)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ValidCooldownAmountMustBeSpecified, parameters);
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

                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.UpdatedCommandHeader + commandTrigger, parameters);
                ServiceManager.Get<ChatService>().RebuildCommandTriggers();

                ChannelSession.Settings.SetCommand(command);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandUpdateCommandUsage, parameters);
            }
        }
    }

    public class DisableCommandPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public DisableCommandPreMadeChatCommandModel() : base(MixItUp.Base.Resources.DisableCommand, "disablecommand", 5, UserRoleEnum.Moderator) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count() == 1)
            {
                string commandTrigger = parameters.Arguments.ElementAt(0).ToLower();

                CommandModelBase command = ServiceManager.Get<CommandService>().AllEnabledChatAccessibleCommands.FirstOrDefault(c => c.Triggers.Contains(commandTrigger, StringComparer.InvariantCultureIgnoreCase));
                if (command == null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorHeader + MixItUp.Base.Resources.CouldNotFindCommand, parameters);
                    return;
                }

                command.IsEnabled = false;

                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.DisabledCommandHeader + commandTrigger, parameters);

                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandDisableCommandUsage, parameters);
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
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.ErrorHeader + result, parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.PreMadeChatCommandStartGiveawayUsage, parameters);
            }
        }
    }


    public class LinkAccountPreMadeChatCommandModel : PreMadeChatCommandModelBase
    {
        public static Dictionary<Guid, Guid> LinkedAccounts = new Dictionary<Guid, Guid>();

        public LinkAccountPreMadeChatCommandModel() : base("Link Account", "linkaccount", 0, UserRoleEnum.User) { }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments != null && parameters.Arguments.Count() >= 2)
            {
                string platformName = parameters.Arguments.First();
                StreamingPlatformTypeEnum platform = EnumHelper.GetEnumValueFromString<StreamingPlatformTypeEnum>(platformName);
                
                if (!StreamingPlatforms.SupportedPlatforms.Contains(platform) || platform == parameters.Platform || !StreamingPlatforms.GetPlatformSession(platform).IsConnected)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.LinkAccountCommandErrorUnsupportedPlatform, platformName), parameters);
                    return;
                }

                string username = UserService.SanitizeUsername(string.Join(" ", parameters.Arguments.Skip(1)));
                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(platform, platformUsername: username, performPlatformSearch: true);
                if (user == null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.LinkAccountCommandErrorUserNotFound, username), parameters);
                    return;
                }

                if (LinkedAccounts.ContainsKey(user.ID) && LinkedAccounts[user.ID] == parameters.User.ID)
                {
                    LinkedAccounts.Remove(user.ID);
                    UserV2ViewModel.MergeUserData(user, parameters.User);
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.LinkAccountCommandAccountsLinkedSuccessfully, parameters);
                }
                else
                {
                    LinkedAccounts[parameters.User.ID] = user.ID;
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.LinkAccountCommandPleaseConfirmLink, parameters.Platform, parameters.User.Username), parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.LinkAccountCommandUsage, parameters);
            }
        }
    }
}
