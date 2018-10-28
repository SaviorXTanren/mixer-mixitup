using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.API;
using MixItUp.Base.Services;
using MixItUp.Base.Statistics;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MixItUp.Desktop")]

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public const string ClientID = "5e3140d0719f5842a09dd2700befbfc100b5a246e35f2690";

        public const string DefaultOBSStudioConnection = "ws://127.0.0.1:4444";

        private const string DefaultEmoticonsManifest = "https://mixer.com/_latest/assets/emoticons/manifest.json";
        private const string DefaultEmoticonsLinkFormat = "https://mixer.com/_latest/assets/emoticons/{0}.png";

        //                                 Source             Text
        private static readonly Dictionary<string, Dictionary<string, EmoticonImage>> builtinEmoticons = new Dictionary<string, Dictionary<string, EmoticonImage>>();
        private static readonly Dictionary<string, Dictionary<string, EmoticonImage>> externalEmoticons = new Dictionary<string, Dictionary<string, EmoticonImage>>();
        private static readonly Dictionary<string, Dictionary<string, EmoticonImage>> userEmoticons = new Dictionary<string, Dictionary<string, EmoticonImage>>();
        private static readonly Dictionary<string, Dictionary<string, EmoticonImage>> botEmoticons = new Dictionary<string, Dictionary<string, EmoticonImage>>();

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.channel__clip__create__self,
            OAuthClientScopeEnum.channel__details__self,
            OAuthClientScopeEnum.channel__update__self,
            OAuthClientScopeEnum.channel__analytics__self,

            OAuthClientScopeEnum.interactive__manage__self,
            OAuthClientScopeEnum.interactive__robot__self,

            OAuthClientScopeEnum.user__details__self,
        };

        public static readonly List<OAuthClientScopeEnum> ModeratorScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.user__details__self,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.user__details__self,
        };

        public static SecretManagerService SecretManager { get; internal set; }

        public static MixerConnectionWrapper Connection { get; private set; }
        public static MixerConnectionWrapper BotConnection { get; private set; }

        public static PrivatePopulatedUserModel User { get; private set; }
        public static PrivatePopulatedUserModel BotUser { get; private set; }
        public static ExpandedChannelModel Channel { get; private set; }

        public static UserContainerViewModel ActiveUsers { get; private set; }

        public static IChannelSettings Settings { get; private set; }

        public static ChatClientWrapper Chat { get; private set; }
        public static InteractiveClientWrapper Interactive { get; private set; }
        public static ConstellationClientWrapper Constellation { get; private set; }

        public static StatisticsTracker Statistics { get; private set; }

        public static ServicesHandlerBase Services { get; private set; }

        public static List<PreMadeChatCommand> PreMadeChatCommands { get; private set; }

        public static bool GameQueueEnabled { get; set; }
        public static LockedList<UserViewModel> GameQueue { get; private set; }

        public static LockedDictionary<string, double> Counters { get; private set; }

        public static IEnumerable<PermissionsCommandBase> AllEnabledChatCommands
        {
            get
            {
                return ChannelSession.AllChatCommands.Where(c => c.IsEnabled);
            }
        }

        public static IEnumerable<PermissionsCommandBase> AllChatCommands
        {
            get
            {
                List<PermissionsCommandBase> commands = new List<PermissionsCommandBase>();
                commands.AddRange(ChannelSession.PreMadeChatCommands);
                commands.AddRange(ChannelSession.Settings.ChatCommands);
                commands.AddRange(ChannelSession.Settings.GameCommands);
                return commands;
            }
        }

        public static IEnumerable<CommandBase> AllEnabledCommands
        {
            get
            {
                return ChannelSession.AllCommands.Where(c => c.IsEnabled);
            }
        }

        public static IEnumerable<CommandBase> AllCommands
        {
            get
            {
                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(ChannelSession.AllChatCommands);
                commands.AddRange(ChannelSession.Settings.EventCommands);
                commands.AddRange(ChannelSession.Settings.InteractiveCommands);
                commands.AddRange(ChannelSession.Settings.TimerCommands);
                commands.AddRange(ChannelSession.Settings.ActionGroupCommands);
                commands.AddRange(ChannelSession.Settings.RemoteCommands);
                return commands;
            }
        }

        public static bool IsStreamer
        {
            get
            {
                if (ChannelSession.User != null && ChannelSession.Channel != null)
                {
                    return ChannelSession.User.id == ChannelSession.Channel.user.id;
                }
                return false;
            }
        }

        public static void Initialize(ServicesHandlerBase serviceHandler)
        {
            try
            {
                Type mixItUpSecretsType = Type.GetType("MixItUp.Base.MixItUpSecrets");
                if (mixItUpSecretsType != null)
                {
                    ChannelSession.SecretManager = (SecretManagerService)Activator.CreateInstance(mixItUpSecretsType);
                }
            }
            catch (Exception ex) { Util.Logger.Log(ex); }

            if (ChannelSession.SecretManager == null)
            {
                ChannelSession.SecretManager = new SecretManagerService();
            }

            ChannelSession.ActiveUsers = new UserContainerViewModel();

            ChannelSession.PreMadeChatCommands = new List<PreMadeChatCommand>();
            ChannelSession.GameQueue = new LockedList<UserViewModel>();

            ChannelSession.Counters = new LockedDictionary<string, double>();

            ChannelSession.Services = serviceHandler;

            ChannelSession.Chat = new ChatClientWrapper();
            ChannelSession.Constellation = new ConstellationClientWrapper();
            ChannelSession.Interactive = new InteractiveClientWrapper();

            ChannelSession.Statistics = new StatisticsTracker();
        }

        public static async Task<bool> ConnectUser(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.ClientID, scopes, false, loginSuccessHtmlPageFilePath: OAuthServiceBase.LoginRedirectPageFileName);
                if (connection != null)
                {
                    ChannelSession.Connection = new MixerConnectionWrapper(connection);
                    return await ChannelSession.InitializeInternal((channelName == null), channelName);
                }
            }
            catch (Exception ex)
            {
                Util.Logger.Log(ex);
            }
            return false;
        }

        public static async Task<bool> ConnectUser(IChannelSettings settings)
        {
            bool result = false;

            ChannelSession.Settings = settings;

            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(settings.OAuthToken);
                if (connection != null)
                {
                    ChannelSession.Connection = new MixerConnectionWrapper(connection);
                    result = await ChannelSession.InitializeInternal(ChannelSession.Settings.IsStreamer, ChannelSession.Settings.Channel.user.username);
                }
            }
            catch (RestServiceRequestException ex)
            {
                Util.Logger.Log(ex);
                result = await ChannelSession.ConnectUser(ChannelSession.StreamerScopes, settings.Channel.user.username);
            }
            catch (Exception ex)
            {
                Util.Logger.Log(ex);
            }

            return result;
        }

        public static async Task<bool> ConnectBot()
        {
            try
            {
                MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.ClientID, ChannelSession.BotScopes, false, loginSuccessHtmlPageFilePath: OAuthServiceBase.LoginRedirectPageFileName);
                if (connection != null)
                {
                    ChannelSession.BotConnection = new MixerConnectionWrapper(connection);
                    return await ChannelSession.InitializeBotInternal();
                }
            }
            catch (Exception ex)
            {
                Util.Logger.Log(ex);
            }
            return false;
        }

        public static async Task<bool> ConnectBot(IChannelSettings settings)
        {
            bool result = true;

            if (settings.BotOAuthToken != null)
            {
                try
                {
                    MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(settings.BotOAuthToken);
                    if (connection != null)
                    {
                        ChannelSession.BotConnection = new MixerConnectionWrapper(connection);
                        result = await ChannelSession.InitializeBotInternal();
                    }
                }
                catch (RestServiceRequestException)
                {
                    settings.BotOAuthToken = null;
                    return false;
                }
            }

            return result;
        }

        public static async Task DisconnectBot()
        {
            ChannelSession.BotConnection = null;
            await ChannelSession.Chat.DisconnectBot();
        }

        public static async Task Close()
        {
            await ChannelSession.Services.Close();

            await ChannelSession.Chat.Disconnect();
            await ChannelSession.DisconnectBot();

            await ChannelSession.Constellation.Disconnect();
        }

        public static async Task SaveSettings()
        {
            await ChannelSession.Services.Settings.Save(ChannelSession.Settings);
        }

        public static async Task RefreshUser()
        {
            if (ChannelSession.User != null)
            {
                PrivatePopulatedUserModel user = await ChannelSession.Connection.GetCurrentUser();
                if (user != null)
                {
                    ChannelSession.User = user;
                }
            }
        }

        public static async Task RefreshChannel()
        {
            if (ChannelSession.Channel != null)
            {
                ExpandedChannelModel channel = await ChannelSession.Connection.GetChannel(ChannelSession.Channel.user.username);
                if (channel != null)
                {
                    ChannelSession.Channel = channel;
                }
            }
        }

        public static async Task<UserViewModel> GetCurrentUser()
        {
            UserViewModel user = await ChannelSession.ActiveUsers.GetUserByID(ChannelSession.User.id);
            if (user == null)
            {
                user = new UserViewModel(ChannelSession.User);
            }
            return user;
        }

        public static void DisconnectionOccurred(string serviceName)
        {
            Util.Logger.Log(serviceName + " Service disconnection occurred");
            GlobalEvents.ServiceDisconnect(serviceName);
        }

        public static void ReconnectionOccurred(string serviceName)
        {
            Util.Logger.Log(serviceName + " Service reconnection successful");
            GlobalEvents.ServiceReconnect(serviceName);
        }

        public static async Task EnsureEmoticonForMessageAsync(ChatMessageDataModel message)
        {
            if (message.source.Equals("external") && Uri.IsWellFormedUriString(message.pack, UriKind.Absolute))
            {
                string imageFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(message.pack));

                if (!externalEmoticons.ContainsKey(message.pack))
                {
                    externalEmoticons.Add(message.pack, new Dictionary<string, EmoticonImage>());
                    if (!File.Exists(imageFilePath))
                    {
                        using (WebClient client = new WebClient())
                        {
                            await Task.Run(() =>
                            {
                                client.DownloadFile(new Uri(message.pack), imageFilePath);
                            });
                        }
                    }
                }

                if (!externalEmoticons[message.pack].ContainsKey(message.text))
                {
                    externalEmoticons[message.pack][message.text] = new EmoticonImage
                    {
                        Name = message.text,
                        FilePath = imageFilePath,
                        X = message.coords.x,
                        Y = message.coords.y,
                        Width = message.coords.width,
                        Height = message.coords.height,
                    };
                }
            }
        }

        public static EmoticonImage GetEmoticonForMessage(ChatMessageDataModel message)
        {
            if (message.type.Equals("emoticon", StringComparison.InvariantCultureIgnoreCase))
            {
                Dictionary<string, Dictionary<string, EmoticonImage>> emoticons = null;
                switch (message.source.ToLower())
                {
                    case "external":
                        emoticons = externalEmoticons;
                        break;
                    case "builtin":
                        emoticons = builtinEmoticons;
                        break;
                }

                if (emoticons != null && emoticons.ContainsKey(message.pack) && emoticons[message.pack].ContainsKey(message.text))
                {
                    return emoticons[message.pack][message.text];
                }
            }
            return null;
        }

        public static IEnumerable<EmoticonImage> FindMatchingEmoticonsForUser(string text)
        {
            return FindMatchingEmoticons(text, userEmoticons);
        }

        public static IEnumerable<EmoticonImage> FindMatchingEmoticonsForBot(string text)
        {
            return FindMatchingEmoticons(text, botEmoticons);
        }

        private static IEnumerable<EmoticonImage> FindMatchingEmoticons(string text, Dictionary<string, Dictionary<string, EmoticonImage>> storage)
        {
            List<EmoticonImage> matchedImages = new List<EmoticonImage>();
            if (text.Length == 1 && char.IsLetterOrDigit(text[0]))
            {
                // Short circuit for very short searches that start with letters or digits
                return matchedImages;
            }

            // User specific emoticons
            foreach (var kvp in storage)
            {
                matchedImages.AddRange(kvp.Value.Where(v => v.Key.StartsWith(text, StringComparison.InvariantCultureIgnoreCase)).Select(v => v.Value));
            }

            // Builtin emoticons (added last to put them at the end)
            foreach (var kvp in builtinEmoticons)
            {
                matchedImages.AddRange(kvp.Value.Where(v => v.Key.StartsWith(text, StringComparison.InvariantCultureIgnoreCase)).Select(v => v.Value));
            }

            return matchedImages.Distinct();
        }

        private static async Task<bool> InitializeInternal(bool isStreamer, string channelName = null)
        {
            await ChannelSession.Services.InitializeTelemetryService();

            PrivatePopulatedUserModel user = await ChannelSession.Connection.GetCurrentUser();
            if (user != null)
            {
                ExpandedChannelModel channel = await ChannelSession.Connection.GetChannel((channelName == null) ? user.username : channelName);
                if (channel != null)
                {
                    ChannelSession.User = user;
                    ChannelSession.Channel = channel;

                    if (ChannelSession.Settings == null)
                    {
                        ChannelSession.Settings = await ChannelSession.Services.Settings.Create(channel, isStreamer);
                    }
                    await ChannelSession.Services.Settings.Initialize(ChannelSession.Settings);

                    if (isStreamer && ChannelSession.Settings.Channel != null && ChannelSession.User.id != ChannelSession.Settings.Channel.userId)
                    {
                        GlobalEvents.ShowMessageBox("The account you are logged in as on Mixer does not match the account for this settings. Please log in as the correct account on Mixer.");
                        ChannelSession.Settings.OAuthToken.accessToken = string.Empty;
                        ChannelSession.Settings.OAuthToken.refreshToken = string.Empty;
                        ChannelSession.Settings.OAuthToken.expiresIn = 0;
                        return false;
                    }

                    ChannelSession.Settings.Channel = channel;

                    ChannelSession.Services.Telemetry.SetUserId(ChannelSession.Settings.TelemetryUserId);

                    ChannelSession.Connection.Initialize();

                    if (!await ChannelSession.Chat.Connect() || !await ChannelSession.Constellation.Connect())
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP))
                    {
                        await ChannelSession.Services.InitializeOBSWebsocket();
                    }
                    if (ChannelSession.Settings.EnableStreamlabsOBSConnection)
                    {
                        await ChannelSession.Services.InitializeStreamlabsOBSService();
                    }
                    if (ChannelSession.Settings.EnableXSplitConnection)
                    {
                        await ChannelSession.Services.InitializeXSplitServer();
                    }

                    if (ChannelSession.Settings.EnableOverlay)
                    {
                        await ChannelSession.Services.InitializeOverlayServer();
                    }

                    if (ChannelSession.Settings.EnableDeveloperAPI)
                    {
                        await ChannelSession.Services.InitializeDeveloperAPI();
                    }

                    if (ChannelSession.Settings.StreamlabsOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeStreamlabs();
                    }
                    if (ChannelSession.Settings.GameWispOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeGameWisp();
                    }
                    if (ChannelSession.Settings.GawkBoxOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeGawkBox();
                    }
                    if (ChannelSession.Settings.TwitterOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeTwitter();
                    }
                    if (ChannelSession.Settings.SpotifyOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeSpotify();
                    }
                    if (ChannelSession.Settings.DiscordOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeDiscord();
                    }
                    if (ChannelSession.Settings.TiltifyOAuthToken != null)
                    {
                        await ChannelSession.Services.InitializeTiltify();
                    }

                    foreach (CommandBase command in ChannelSession.AllEnabledCommands)
                    {
                        foreach (ActionBase action in command.Actions)
                        {
                            if (action is CounterAction)
                            {
                                await ((CounterAction)action).SetCounterValue();
                            }
                        }
                    }

                    if (ChannelSession.Settings.DefaultInteractiveGame > 0)
                    {
                        IEnumerable<InteractiveGameListingModel> games = await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel);
                        InteractiveGameListingModel game = games.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultInteractiveGame));
                        if (game != null)
                        {
                            if (!await ChannelSession.Interactive.Connect(game))
                            {
                                await ChannelSession.Interactive.Disconnect();
                            }
                        }
                        else
                        {
                            ChannelSession.Settings.DefaultInteractiveGame = 0;
                        }
                    }

                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        if (currency.ShouldBeReset())
                        {
                            await currency.Reset();
                        }
                    }

                    if (ChannelSession.Settings.ModerationResetStrikesOnLaunch)
                    {
                        foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values)
                        {
                            userData.ModerationStrikes = 0;
                            ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
                        }
                    }

                    await ChannelSession.LoadUserEmoticons();

                    await ChannelSession.SaveSettings();

                    await ChannelSession.Services.Settings.PerformBackupIfApplicable(ChannelSession.Settings);

                    ChannelSession.Services.Telemetry.TrackLogin();
                    if (ChannelSession.Settings.IsStreamer)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () => { await ChannelSession.Services.MixItUpService.SendUserFeatureEvent(new UserFeatureEvent(ChannelSession.User.id)); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }

                    GlobalEvents.OnRankChanged += GlobalEvents_OnRankChanged;

                    return true;
                }
            }
            return false;
        }

        private static async Task<bool> InitializeBotInternal()
        {
            PrivatePopulatedUserModel user = await ChannelSession.BotConnection.GetCurrentUser();
            if (user != null)
            {
                ChannelSession.BotUser = user;

                ChannelSession.BotConnection.Initialize();

                await ChannelSession.Chat.ConnectBot();

                await ChannelSession.LoadBotEmoticons();

                await ChannelSession.SaveSettings();

                return true;
            }
            return false;
        }

        private static async void GlobalEvents_OnRankChanged(object sender, UserCurrencyDataViewModel currency)
        {
            if (currency.Currency.RankChangedCommand != null)
            {
                UserViewModel user = await ChannelSession.ActiveUsers.GetUserByID(currency.User.ID);
                if (user != null)
                {
                    await currency.Currency.RankChangedCommand.Perform(user);
                }
            }
        }

        private static async Task LoadUserEmoticons()
        {
            // Read Manifest (built in)
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", $"MixItUp/{System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString()} (Web call from Mix It Up; https://mixitupapp.com; support@mixitupapp.com)");

                using (HttpResponseMessage response = await httpClient.GetAsync(ChannelSession.DefaultEmoticonsManifest))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Dictionary<string, BuiltinEmoticonPack> manifest = JsonConvert.DeserializeObject<Dictionary<string, BuiltinEmoticonPack>>(await response.Content.ReadAsStringAsync());

                        foreach (KeyValuePair<string, BuiltinEmoticonPack> pack in manifest)
                        {
                            string imageLink = string.Format(ChannelSession.DefaultEmoticonsLinkFormat, pack.Key);
                            string imageFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(imageLink));

                            if (!builtinEmoticons.ContainsKey(pack.Key))
                            {
                                builtinEmoticons.Add(pack.Key, new Dictionary<string, EmoticonImage>());
                                if (!File.Exists(imageFilePath))
                                {
                                    using (WebClient client = new WebClient())
                                    {
                                        await Task.Run(() =>
                                        {
                                            client.DownloadFile(new Uri(imageLink), imageFilePath);
                                        });
                                    }
                                }
                            }

                            foreach (KeyValuePair<string, EmoticonGroupModel> emoticon in pack.Value.emoticons)
                            {
                                builtinEmoticons[pack.Key][emoticon.Key] = new EmoticonImage
                                {
                                    Name = emoticon.Key,
                                    FilePath = imageFilePath,
                                    X = emoticon.Value.x,
                                    Y = emoticon.Value.y,
                                    Width = emoticon.Value.width,
                                    Height = emoticon.Value.height,
                                };
                            }
                        }
                    }
                }
            }

            await LoadEmoticons(ChannelSession.User, userEmoticons);
        }

        private static async Task LoadBotEmoticons()
        {
            await LoadEmoticons(ChannelSession.BotUser, botEmoticons);
        }

        private class BuiltinEmoticonPack
        {
            public string[] authors = null;
            public bool @default = false;
            public string name = null;
            public Dictionary<string, EmoticonGroupModel> emoticons = new Dictionary<string, EmoticonGroupModel>();
        }

        private static async Task LoadEmoticons(UserModel user, Dictionary<string, Dictionary<string, EmoticonImage>> storage)
        {
            List<EmoticonPackModel> userPacks = (await ChannelSession.Connection.GetEmoticons(ChannelSession.Channel, user)).ToList();
            foreach (EmoticonPackModel userPack in userPacks)
            {
                string imageFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(userPack.url));

                if (!storage.ContainsKey(userPack.channelId.ToString()))
                {
                    storage.Add(userPack.channelId.ToString(), new Dictionary<string, EmoticonImage>());
                    if (!File.Exists(imageFilePath))
                    {
                        using (WebClient client = new WebClient())
                        {
                            await Task.Run(() =>
                            {
                                client.DownloadFile(new Uri(userPack.url), imageFilePath);
                            });
                        }
                    }
                }

                foreach (KeyValuePair<string, EmoticonGroupModel> emoticon in userPack.emoticons)
                {
                    storage[userPack.channelId.ToString()][emoticon.Key] = new EmoticonImage
                    {
                        Name = emoticon.Key,
                        FilePath = imageFilePath,
                        X = emoticon.Value.x,
                        Y = emoticon.Value.y,
                        Width = emoticon.Value.width,
                        Height = emoticon.Value.height,
                    };
                }
            }
        }
    }
}