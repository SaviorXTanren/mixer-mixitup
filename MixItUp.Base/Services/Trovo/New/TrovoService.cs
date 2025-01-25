using MixItUp.Base.Model;
using MixItUp.Base.Model.Trovo;
using MixItUp.Base.Model.Trovo.Category;
using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.New
{
    /// <summary>
    /// https://developer.trovo.live
    /// </summary>
    public class TrovoService : StreamingPlatformServiceBaseNew
    {
        private const string OAuthBaseAddress = "https://open.trovo.live/page/login.html";

        private const string BaseAddressFormat = "https://open-api.trovo.live/openplatform/";

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

        public override string Name { get { return Resources.Trovo; } }

        public override string ClientID { get { return "8FMjuk785AX4FMyrwPTU3B8vYvgHWN33"; } }
        public override string ClientSecret { get { return ServiceManager.Get<SecretsService>().GetSecret("TrovoSecret"); } }

        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.Trovo; } }

        public override bool IsConnected { get; protected set; }

        protected override OAuthTokenModel OAuthToken
        {
            get { return base.OAuthToken; }
            set
            {
                base.OAuthToken = value;
                if (value != null)
                {
                    this.HttpClient.SetAuthorization("OAuth", base.OAuthToken.accessToken);
                }
                else
                {
                    this.HttpClient.RemoveAuthorization();
                }
            }
        }

        public TrovoService(IEnumerable<string> scopes, bool isBotService = false)
            : base(BaseAddressFormat, scopes, isBotService)
        {
            this.HttpClient.DefaultRequestHeaders.Add("Client-ID", this.ClientID);
        }

        public async Task<PrivateUserModel> GetCurrentUser() { return await AsyncRunner.RunAsync(HttpClient.GetAsync<PrivateUserModel>("getuserinfo")); }

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

        public async Task<Result> UpdateChannel(string id, string title = null, string categoryID = null, string langaugeCode = null, ChannelAudienceTypeEnum? audience = null)
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
                if (!response.IsSuccessStatusCode)
                {
                    return new Result(await response.Content.ReadAsStringAsync());
                }

                return new Result();
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

        public async Task<bool> SetGame(string channelID, string gameName)
        {
            IEnumerable<CategoryModel> categories = await this.SearchCategories(gameName, maxResults: 10);
            if (categories != null && categories.Count() > 0)
            {
                string categoryID = categories.FirstOrDefault()?.id;
                if (!string.IsNullOrEmpty(categoryID))
                {
                    Result result = await this.UpdateChannel(channelID, categoryID: categoryID);
                    return result.Success;
                }
            }
            return false;
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

        public async Task SendMessage(string message, string channelID = null)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["content"] = message;
                if (!string.IsNullOrEmpty(channelID))
                {
                    jobj["channel_id"] = channelID;
                }
                await HttpClient.PostAsync("chat/send", AdvancedHttpClient.CreateContentFromObject(jobj));
            });
        }

        public async Task<bool> DeleteMessage(string channelID, string userID, string messageID)
        {
            return await AsyncRunner.RunAsync(HttpClient.DeleteAsync($"channels/{channelID}/messages/{messageID}/users/{userID}"));
        }

        public async Task<bool> ClearChat(string channelID) { return await this.PerformChatCommand(channelID, "clear"); }

        public async Task<bool> ModUser(string channelID, string username) { return await this.PerformChatCommand(channelID, "mod " + username); }

        public async Task<bool> UnmodUser(string channelID, string username) { return await this.PerformChatCommand(channelID, "unmod " + username); }

        public async Task<bool> TimeoutUser(string channelID, string username, int duration) { return await this.PerformChatCommand(channelID, $"ban {username} {duration}"); }

        public async Task<bool> BanUser(string channelID, string username) { return await this.PerformChatCommand(channelID, "ban " + username); }

        public async Task<bool> UnbanUser(string channelID, string username) { return await this.PerformChatCommand(channelID, "unban " + username); }

        public async Task<bool> HostUser(string channelID, string username) { return await this.PerformChatCommand(channelID, "host " + username); }

        public async Task<bool> SlowMode(string channelID, int seconds = 0)
        {
            if (seconds > 0)
            {
                return await this.PerformChatCommand(channelID, "slow " + seconds);
            }
            else
            {
                return await this.PerformChatCommand(channelID, "slowoff");
            }
        }

        public async Task<bool> FollowersMode(string channelID, bool enable)
        {
            if (enable)
            {
                return await this.PerformChatCommand(channelID, "followers");
            }
            else
            {
                return await this.PerformChatCommand(channelID, "followersoff");
            }
        }

        public async Task<bool> SubscriberMode(string channelID, bool enable)
        {
            if (enable)
            {
                return await this.PerformChatCommand(channelID, "subscribers");
            }
            else
            {
                return await this.PerformChatCommand(channelID, "subscribersoff");
            }
        }

        public async Task<bool> AddRole(string channelID, string username, string role) { return await this.PerformChatCommand(channelID, $"addrole {role} {username}"); }

        public async Task<bool> RemoveRole(string channelID, string username, string role) { return await this.PerformChatCommand(channelID, $"removerole {role} {username}"); }

        public async Task<bool> FastClip(string channelID) { return await this.PerformChatCommand(channelID, "fastclip"); }

        /// <summary>
        /// Performs an official Trovo command in the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to perform the command in</param>
        /// <param name="command">The command to perform</param>
        /// <returns>Null if successful, a status message indicating why the command failed to perform</returns>
        public async Task<bool> PerformChatCommand(string channelID, string command)
        {
            return await AsyncRunner.RunAsync(async () =>
            {
                JObject jobj = new JObject();
                jobj["channel_id"] = channelID;
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
                token.ScopeList = OAuthTokenModel.GenerateScopeList(scopes);
                return token;
            }
            return null;
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
                newToken.ScopeList = OAuthToken.ScopeList;
                OAuthToken = newToken;
            }
        }

        private string ConvertClientScopesToString(IEnumerable<string> scopes)
        {
            return string.Join("+", scopes);
        }
    }
}
