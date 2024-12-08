using MixItUp.Base.Model;
using MixItUp.Base.Model.Trovo;
using MixItUp.Base.Model.Trovo.Category;
using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.New
{
    public class StreamerTrovoService : TrovoService
    {
        public override IEnumerable<string> Scopes { get; protected set; } = new List<string>()
        {
            "chat_connect",
            "chat_send_self",
            "send_to_my_channel",
            "manage_messages",

            "channel_details_self",
            "channel_update_self",
            "channel_subscriptions",

            "user_details_self",
        };

        public IDictionary<string, TrovoChatEmoteViewModel> ChannelEmotes { get { return channelEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> channelEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public IDictionary<string, TrovoChatEmoteViewModel> EventEmotes { get { return eventEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> eventEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public IDictionary<string, TrovoChatEmoteViewModel> GlobalEmotes { get { return globalEmotes; } }
        private Dictionary<string, TrovoChatEmoteViewModel> globalEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        public override bool IsEnabled { get { return this.GetAuthenticationSettings()?.IsEnabled ?? false; } }

        public StreamerTrovoService()
        {
            this.Client = new TrovoClient(isFullClient: true);
        }

        public override async Task<Result> Initialize()
        {
            Result result = await Initialize();
            if (!result.Success)
            {
                return result;
            }

            ChatEmotePackageModel emotePackage = await GetPlatformAndChannelEmotes(ChannelID);
            if (emotePackage != null)
            {
                if (emotePackage.customizedEmotes?.channel != null)
                {
                    foreach (ChannelChatEmotesModel channel in emotePackage.customizedEmotes.channel)
                    {
                        foreach (ChatEmoteModel emote in channel.emotes)
                        {
                            channelEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                        }
                    }
                }

                if (emotePackage.eventEmotes != null)
                {
                    foreach (EventChatEmoteModel emote in emotePackage.eventEmotes)
                    {
                        eventEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                    }
                }

                if (emotePackage.globalEmotes != null)
                {
                    foreach (GlobalChatEmoteModel emote in emotePackage.globalEmotes)
                    {
                        globalEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                    }
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Failed to get available Trovo emotes");
            }

            return new Result();
        }
    }

    public class BotTrovoService : TrovoService
    {
        public override IEnumerable<string> Scopes { get; protected set; } = new List<string>()
        {
            "chat_connect",
            "chat_send_self",
            "end_to_my_channel",
            "manage_messages",

            "user_details_self",
        };

        public override bool IsEnabled { get { return this.GetAuthenticationSettings()?.IsBotEnabled ?? false; } }

        public BotTrovoService()
        {
            this.Client = new TrovoClient();

            this.channelIDToConnectTo = this.GetAuthenticationSettings()?.ChannelID ?? string.Empty;
        }
    }

    /// <summary>
    /// https://trovo.live/policy/apis-developer-doc.html
    /// </summary>
    public abstract class TrovoService : StreamingPlatformServiceBaseNew
    {
        private const string OAuthBaseAddress = "https://open.trovo.live/page/login.html";

        private const string TrovoRestAPIBaseAddressFormat = "https://open-api.trovo.live/openplatform/";

        private const int MaxMessageLength = 500;

        public static DateTimeOffset GetTrovoDateTime(string dateTime)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateTime) && long.TryParse(dateTime, out long seconds))
                {
                    DateTimeOffset result = DateTimeOffsetExtensions.FromUTCUnixTimeSeconds(seconds);
                    if (result > DateTimeOffset.MinValue)
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{dateTime} - {ex}");
            }
            return DateTimeOffset.MinValue;
        }

        internal static string ConvertClientScopesToString(IEnumerable<string> scopes)
        {
            return string.Join("+", scopes);
        }

        public override string Name { get { return Resources.Trovo; } }

        public override string ClientID { get { return "8FMjuk785AX4FMyrwPTU3B8vYvgHWN33"; } }
        public override string ClientSecret { get { return ServiceManager.Get<SecretsService>().GetSecret("TrovoSecret"); } }

        public override bool IsConnected { get; protected set; }

        public override string UserID { get { return User?.userId; } }
        public override string Username { get { return User?.userName; } }
        public override string ChannelID { get { return User?.channelId; } }
        public override string ChannelLink { get { return string.Format("trovo.live/{0}", Username?.ToLower()); } }

        public override StreamingPlatformAccountModel Account
        {
            get
            {
                return new StreamingPlatformAccountModel()
                {
                    ID = UserID,
                    Username = Username,
                    AvatarURL = User?.profilePic
                };
            }
        }

        public PrivateUserModel User { get; private set; }
        public ChannelModel Channel { get; private set; }
        public PrivateUserModel Bot { get; private set; }

        public TrovoClient Client { get; protected set; }

        public bool IsBotService { get { return !string.IsNullOrEmpty(this.channelIDToConnectTo); } }
        public string ChatChannelID { get { return this.IsBotService ? this.channelIDToConnectTo : this.ChannelID; } }
        protected string channelIDToConnectTo;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        public TrovoService() : base(TrovoRestAPIBaseAddressFormat) { }

        public override async Task<Result> Initialize()
        {
            User = await GetUser();
            if (User == null)
            {
                return new Result(Resources.TrovoFailedToGetUserData);
            }

            Channel = await GetChannelByID(ChannelID);
            if (Channel == null)
            {
                return new Result(Resources.TrovoFailedToGetChannelData);
            }

            string chatToken = this.IsBotService ? await this.GetChatToken(this.channelIDToConnectTo) : await this.GetChatToken();
            if (string.IsNullOrEmpty(chatToken))
            {
                return new Result(Resources.TrovoChatConnectionCouldNotBeEstablished);
            }

            Result result = await Client.Connect(chatToken);
            if (!result.Success)
            {
                await Client.Disconnect();
                return result;
            }

            return new Result();
        }

        public override async Task Disconnect()
        {
            await Client.Disconnect();
        }

        public async Task<PrivateUserModel> GetUser() { return await AsyncRunner.RunAsync(HttpClient.GetAsync<PrivateUserModel>("getuserinfo")); }

        public async Task<UserModel> GetUserByName(string username)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["user"] = new JArray { username };

                UsersModel result = await HttpClient.PostAsync<UsersModel>("getusers", AdvancedHttpClient.CreateContentFromObject(jobj));
                if (result?.users != null)
                {
                    return result.users.FirstOrDefault();
                }
                return null;
            });
        }

        public async Task<IEnumerable<ChannelFollowerModel>> GetFollowers(string channelID, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ChannelFollowersModel> response = await PostPagedCursorAsync<ChannelFollowersModel>($"channels/{channelID}/followers", maxResults, maxLimit: 100);

                List<ChannelFollowerModel> result = new List<ChannelFollowerModel>();
                foreach (ChannelFollowersModel r in response)
                {
                    result.AddRange(r.follower);
                }
                return result;
            });
        }

        public async Task<IEnumerable<ChannelSubscriberModel>> GetSubscribers(string channelID, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ChannelSubscribersModel> response = await GetPagedOffsetAsync<ChannelSubscribersModel>($"channels/{channelID}/subscriptions", maxResults, maxLimit: 100);

                List<ChannelSubscriberModel> result = new List<ChannelSubscriberModel>();
                foreach (ChannelSubscribersModel r in response)
                {
                    result.AddRange(r.subscriptions);
                }
                return result;
            });
        }

        public async Task<ChannelModel> GetChannelByID(string channelID)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject requestParameters = new JObject();
                requestParameters["channel_id"] = channelID;

                return await HttpClient.PostAsync<ChannelModel>("channels/id", AdvancedHttpClient.CreateContentFromObject(requestParameters));
            });
        }

        public async Task<bool> UpdateChannel(string id, string title = null, string categoryID = null, string langaugeCode = null, ChannelAudienceTypeEnum? audience = null)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["channel_id"] = id;
                if (!string.IsNullOrEmpty(title)) { jobj["live_title"] = title; }
                if (!string.IsNullOrEmpty(categoryID)) { jobj["category_id"] = categoryID; }
                if (!string.IsNullOrEmpty(langaugeCode)) { jobj["language_code"] = langaugeCode; }
                if (audience != null) { jobj["audi_type"] = audience.ToString(); }

                HttpResponseMessage response = await HttpClient.PostAsync("channels/update", AdvancedHttpClient.CreateContentFromObject(jobj));
                return response.IsSuccessStatusCode;
            });
        }

        public async Task<IEnumerable<CategoryModel>> SearchCategories(string query, int maxResults = 1)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["query"] = query;
                jobj["limit"] = maxResults;

                CategoriesModel categories = await HttpClient.PostAsync<CategoriesModel>("searchcategory", AdvancedHttpClient.CreateContentFromObject(jobj));
                if (categories != null)
                {
                    return categories.category_info;
                }
                return null;
            });
        }

        public async Task<ChatEmotePackageModel> GetPlatformEmotes() { return await AsyncRunner.RunAsync(GetEmotes()); }

        public async Task<ChatEmotePackageModel> GetPlatformAndChannelEmotes(string channelID) { return await AsyncRunner.RunAsync(GetEmotes(new List<string>() { channelID })); }

        public async Task<ChatViewersModel> GetViewers(string channelID, int maxResults = 1000)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                IEnumerable<ChatViewersInternalModel> viewers = await PostPagedCursorAsync<ChatViewersInternalModel>($"channels/{channelID}/viewers", maxResults);

                ChatViewersModel result = new ChatViewersModel();
                foreach (ChatViewersInternalModel viewer in viewers)
                {
                    result.ace.viewers.AddRange(viewer.chatters.ace.viewers);
                    result.aceplus.viewers.AddRange(viewer.chatters.aceplus.viewers);
                    result.admins.viewers.AddRange(viewer.chatters.admins.viewers);
                    result.all.viewers.AddRange(viewer.chatters.all.viewers);
                    result.creators.viewers.AddRange(viewer.chatters.creators.viewers);
                    result.editors.viewers.AddRange(viewer.chatters.editors.viewers);
                    result.followers.viewers.AddRange(viewer.chatters.followers.viewers);
                    result.moderators.viewers.AddRange(viewer.chatters.moderators.viewers);
                    result.subscribers.viewers.AddRange(viewer.chatters.subscribers.viewers);
                    result.supermods.viewers.AddRange(viewer.chatters.supermods.viewers);
                    result.VIPS.viewers.AddRange(viewer.chatters.VIPS.viewers);
                    result.wardens.viewers.AddRange(viewer.chatters.wardens.viewers);

                    foreach (var kvp in viewer.custome_roles)
                    {
                        if (!result.CustomRoles.ContainsKey(kvp.Key))
                        {
                            result.CustomRoles[kvp.Key] = new ChatViewersRoleGroupModel();
                        }

                        ChatViewersRoleGroupModel group = kvp.Value.ToObject<ChatViewersRoleGroupModel>();
                        result.CustomRoles[kvp.Key].viewers.AddRange(group.viewers);
                    }

                    foreach (var kvp in viewer.custom_roles)
                    {
                        if (!result.CustomRoles.ContainsKey(kvp.Key))
                        {
                            result.CustomRoles[kvp.Key] = new ChatViewersRoleGroupModel();
                        }

                        ChatViewersRoleGroupModel group = kvp.Value.ToObject<ChatViewersRoleGroupModel>();
                        result.CustomRoles[kvp.Key].viewers.AddRange(group.viewers);
                    }
                }

                if (viewers.Count() > 0)
                {
                    result.Total = viewers.First().total;
                }

                return result;
            });
        }

        public async Task<IEnumerable<TopChannelModel>> GetTopChannels(int maxResults = 1, string categoryID = null)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                Dictionary<string, object> parameters = null;
                if (!string.IsNullOrEmpty(categoryID))
                {
                    parameters = new Dictionary<string, object>();
                    parameters["category_id"] = categoryID;
                }

                IEnumerable<TopChannelsModel> response = await PostPagedTokenAsync<TopChannelsModel>("gettopchannels", maxResults, maxLimit: 100, parameters: parameters);

                List<TopChannelModel> results = new List<TopChannelModel>();
                foreach (TopChannelsModel r in response)
                {
                    if (r != null && r.top_channels_lists != null)
                    {
                        results.AddRange(r.top_channels_lists);
                    }
                }

                return results;
            });
        }

        public async Task<string> GetChatToken()
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = await HttpClient.GetJObjectAsync("chat/token");
                if (jobj != null && jobj.ContainsKey("token"))
                {
                    return jobj["token"].ToString();
                }
                return null;
            });
        }

        public async Task<string> GetChatToken(string channelID)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = await HttpClient.GetJObjectAsync($"chat/channel-token/{channelID}");
                if (jobj != null && jobj.ContainsKey("token"))
                {
                    return jobj["token"].ToString();
                }
                return null;
            });
        }

        public async Task SendMessage(string message)
        {
            try
            {
                await messageSemaphore.WaitAsync();

                string subMessage = null;
                do
                {
                    message = ChatService.SplitLargeMessage(message, MaxMessageLength, out subMessage);

                    await AsyncRunner.RunAsync(async () =>
                    {
                        JObject jobj = new JObject();
                        jobj["content"] = message;
                        if (this.IsBotService)
                        {
                            jobj["channel_id"] = this.ChatChannelID;
                        }
                        await HttpClient.PostAsync("chat/send", AdvancedHttpClient.CreateContentFromObject(jobj));
                    });

                    message = subMessage;
                    await Task.Delay(500);
                }
                while (!string.IsNullOrEmpty(message));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                messageSemaphore.Release();
            }
        }

        public async Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            string channelID = !string.IsNullOrEmpty(this.channelIDToConnectTo) ? this.channelIDToConnectTo : this.ChannelID;
            return await AsyncRunner.RunAsync(HttpClient.DeleteAsync($"channels/{channelID}/messages/{message.ID}/users/{message.User?.PlatformID}"));
        }

        public async Task<bool> ClearChat() { return await this.PerformChatCommand("clear"); }

        public async Task<bool> ModUser(string username) { return await this.PerformChatCommand("mod " + username); }

        public async Task<bool> UnmodUser(string username) { return await this.PerformChatCommand("unmod " + username); }

        public async Task<bool> TimeoutUser(string username, int duration) { return await this.PerformChatCommand($"ban {username} {duration}"); }

        public async Task<bool> BanUser(string username) { return await this.PerformChatCommand("ban " + username); }

        public async Task<bool> UnbanUser(string username) { return await this.PerformChatCommand("unban " + username); }

        public async Task<bool> HostUser(string username) { return await this.PerformChatCommand("host " + username); }

        public async Task<bool> SlowMode(int seconds = 0)
        {
            if (seconds > 0)
            {
                return await this.PerformChatCommand("slow " + seconds);
            }
            else
            {
                return await this.PerformChatCommand("slowoff");
            }
        }

        public async Task<bool> FollowersMode(bool enable)
        {
            if (enable)
            {
                return await this.PerformChatCommand("followers");
            }
            else
            {
                return await this.PerformChatCommand("followersoff");
            }
        }

        public async Task<bool> SubscriberMode(bool enable)
        {
            if (enable)
            {
                return await this.PerformChatCommand("subscribers");
            }
            else
            {
                return await this.PerformChatCommand("subscribersoff");
            }
        }

        public async Task<bool> AddRole(string username, string role) { return await this.PerformChatCommand($"addrole {role} {username}"); }

        public async Task<bool> RemoveRole(string username, string role) { return await this.PerformChatCommand($"removerole {role} {username}"); }

        public async Task<bool> FastClip() { return await this.PerformChatCommand("fastclip"); }

        /// <summary>
        /// Performs an official Trovo command in the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to perform the command in</param>
        /// <param name="command">The command to perform</param>
        /// <returns>Null if successful, a status message indicating why the command failed to perform</returns>
        public async Task<bool> PerformChatCommand(string command)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["channel_id"] = this.ChatChannelID;
                jobj["command"] = command;

                jobj = await HttpClient.PostAsync<JObject>("channels/command", AdvancedHttpClient.CreateContentFromObject(jobj));
                if (jobj != null)
                {
                    JToken success = jobj.SelectToken("is_success");
                    JToken message = jobj.SelectToken("display_msg");
                    if (success != null && Equals(false, success))
                    {
                        string result = message.ToString();
                        if (!string.IsNullOrEmpty(result))
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(result, StreamingPlatformTypeEnum.Trovo);
                            return false;
                        }
                    }
                }
                return true;
            });
        }

        private async Task<ChatEmotePackageModel> GetEmotes(IEnumerable<string> channelIDs = null)
        {
            JObject jobj = new JObject();
            jobj["emote_type"] = channelIDs != null && channelIDs.Count() > 0 ? 0 : 2;
            jobj["channel_id"] = new JArray(channelIDs);

            ChatEmotesModel result = await HttpClient.PostAsync<ChatEmotesModel>("getemotes", AdvancedHttpClient.CreateContentFromObject(jobj));
            if (result != null)
            {
                return result.channels;
            }
            return null;
        }

        /// <summary>
        /// Performs a GET REST request using the provided request URI for paged offset data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <param name="maxLimit">The maximum limit of results that can be returned in a single request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        private async Task<IEnumerable<T>> GetPagedOffsetAsync<T>(string requestUri, int maxResults = 1, int maxLimit = -1) where T : PageDataResponseModel
        {
            if (!requestUri.Contains("?"))
            {
                requestUri += "?";
            }
            else
            {
                requestUri += "&";
            }

            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            if (maxLimit > 0)
            {
                queryParameters["limit"] = maxLimit.ToString();
            }

            List<T> results = new List<T>();
            int lastCount = -1;
            int totalCount = 0;
            do
            {
                if (totalCount > 0)
                {
                    queryParameters["offset"] = totalCount.ToString();
                }
                T data = await HttpClient.GetAsync<T>(requestUri + string.Join("&", queryParameters.Select(kvp => kvp.Key + "=" + kvp.Value)));

                lastCount = -1;
                if (data != null)
                {
                    results.Add(data);
                    lastCount = data.GetItemCount();
                    totalCount += lastCount;
                }
            }
            while (totalCount < maxResults && lastCount > 0 && lastCount < totalCount);

            return results;
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI for paged cursor data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <param name="maxLimit">The maximum limit of results that can be returned in a single request</param>
        /// <param name="parameters">Optional parameters to include in the request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        private async Task<IEnumerable<T>> PostPagedTokenAsync<T>(string requestUri, int maxResults = 1, int maxLimit = -1, Dictionary<string, object> parameters = null) where T : PageDataResponseModel
        {
            JObject requestParameters = new JObject();
            if (maxLimit > 0)
            {
                requestParameters["limit"] = maxLimit;
            }
            requestParameters["after"] = true;

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    requestParameters[kvp.Key] = kvp.Value.ToString();
                }
            }

            List<T> results = new List<T>();
            string token = null;
            int cursor = -1;
            int count = 0;
            do
            {
                if (!string.IsNullOrEmpty(token) && cursor > 0)
                {
                    requestParameters["token"] = token;
                    requestParameters["cursor"] = cursor;
                }
                T data = await HttpClient.PostAsync<T>(requestUri, AdvancedHttpClient.CreateContentFromObject(requestParameters));

                if (data != null)
                {
                    results.Add(data);
                    count += data.GetItemCount();

                    if (data.cursor < data.total_page)
                    {
                        token = data.token;
                        cursor = data.cursor;
                    }
                    else
                    {
                        token = null;
                        cursor = -1;
                    }
                }
            }
            while (count < maxResults && !string.IsNullOrEmpty(token));

            return results;
        }

        /// <summary>
        /// Performs a POST REST request using the provided request URI for paged cursor data.
        /// </summary>
        /// <param name="requestUri">The request URI to use</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <param name="maxLimit">The maximum limit of results that can be returned in a single request</param>
        /// <param name="parameters">Optional parameters to include in the request</param>
        /// <returns>A type-casted object of the contents of the response</returns>
        private async Task<IEnumerable<T>> PostPagedCursorAsync<T>(string requestUri, int maxResults = 1, int maxLimit = -1, Dictionary<string, object> parameters = null) where T : PageDataResponseModel
        {
            JObject requestParameters = new JObject();
            if (maxLimit > 0)
            {
                requestParameters["limit"] = maxLimit;
            }

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    requestParameters[kvp.Key] = new JObject(kvp.Value);
                }
            }

            List<T> results = new List<T>();
            int cursor = -1;
            int count = 0;
            do
            {
                if (cursor > 0)
                {
                    requestParameters["cursor"] = cursor;
                }
                T data = await HttpClient.PostAsync<T>(requestUri, AdvancedHttpClient.CreateContentFromObject(requestParameters));

                if (data != null)
                {
                    results.Add(data);
                    count += data.GetItemCount();
                    if (data.cursor < data.total_page)
                    {
                        cursor = data.cursor;
                    }
                    else
                    {
                        cursor = -1;
                    }
                }
            }
            while (count < maxResults && cursor >= 0);

            return results;
        }

        protected override async Task RefreshOAuthToken()
        {
            JObject content = new JObject()
            {
                { "client_secret", ClientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", OAuthToken.refreshToken }
            };

            OAuthTokenModel newToken = await HttpClient.PostAsync<OAuthTokenModel>("refreshtoken", AdvancedHttpClient.CreateContentFromObject(content));
            if (newToken != null)
            {
                newToken.clientID = OAuthToken.clientID;
                newToken.authorizationCode = OAuthToken.authorizationCode;
                newToken.ScopeList = OAuthToken.ScopeList;
                OAuthToken = newToken;
            }
        }

        protected async override Task<string> GetAuthorizationCodeURL(IEnumerable<string> scopes, string state, bool forceApprovalPrompt = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", ClientID },
                { "response_type", LocalOAuthHttpListenerServer.AUTHORIZATION_CODE_URL_PARAMETER },
                { "scope", ConvertClientScopesToString(scopes) },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
                { "state", state },
            };

            if (forceApprovalPrompt)
            {
                parameters.Add("force_verify", "force");
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());
            return OAuthBaseAddress + "?" + await content.ReadAsStringAsync();
        }

        protected async override Task<OAuthTokenModel> RequestOAuthToken(string authorizationCode, IEnumerable<string> scopes, string state)
        {
            JObject content = new JObject()
            {
                { "client_id", ClientID },
                { "client_secret", ClientSecret },
                { "code", authorizationCode },
                { "grant_type", "authorization_code" },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
            };

            OAuthTokenModel token = await HttpClient.PostAsync<OAuthTokenModel>("exchangetoken", AdvancedHttpClient.CreateContentFromObject(content));
            if (token != null)
            {
                token.clientID = ClientID;
                token.authorizationCode = authorizationCode;
                token.ScopeList = string.Join(",", scopes ?? new List<string>());
                return token;
            }
            return null;
        }
    }
}
