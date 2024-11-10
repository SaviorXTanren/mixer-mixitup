using MixItUp.Base.Model.Trovo;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.API
{
    /// <summary>
    /// The APIs for chat-based services.
    /// </summary>
    public class ChatService : TrovoServiceBase
    {
        private class ChatEmotePackageWrapperModel
        {
            public ChatEmotePackageModel channels { get; set; }
        }

        private class ChatViewersInternalModel : PageDataResponseModel
        {
            public string nickname { get; set; }

            public string live_title { get; set; }

            public ChatViewersRolesModel chatters { get; set; } = new ChatViewersRolesModel();

            public JObject custom_roles { get; set; } = new JObject();

            public JObject custome_roles { get; set; } = new JObject();

            public override int GetItemCount() { return this.total; }
        }

        /// <summary>
        /// Creates an instance of the ChatService.
        /// </summary>
        /// <param name="connection">The Trovo connection to use</param>
        public ChatService(TrovoConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the chat token for the currently authenticated channel.
        /// </summary>
        /// <returns>The chat token</returns>
        public async Task<string> GetToken()
        {
            JObject jobj = await this.GetJObjectAsync("chat/token");
            if (jobj != null && jobj.ContainsKey("token"))
            {
                return jobj["token"].ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets the chat token for the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to get a token for</param>
        /// <returns>The chat token</returns>
        public async Task<string> GetToken(string channelID)
        {
            Validator.ValidateString(channelID, "channelID");

            JObject jobj = await this.GetJObjectAsync($"chat/channel-token/{channelID}");
            if (jobj != null && jobj.ContainsKey("token"))
            {
                return jobj["token"].ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets the set of available emotes for the currently authenticated user.
        /// </summary>
        /// <param name="channelIDs">The optional list of IDs of the channels to get emotes for</param>
        /// <returns>The set of available emotes</returns>
        public async Task<ChatEmotePackageModel> GetEmotes(IEnumerable<string> channelIDs = null)
        {
            JObject jobj = new JObject();
            jobj["emote_type"] = (channelIDs != null && channelIDs.Count() > 0) ? 0 : 2;
            jobj["channel_id"] = new JArray(channelIDs);

            ChatEmotePackageWrapperModel result = await this.PostAsync<ChatEmotePackageWrapperModel>("getemotes", AdvancedHttpClient.CreateContentFromObject(jobj));
            if (result != null)
            {
                return result.channels;
            }
            return null;
        }

        /// <summary>
        /// Gets the current viewers of the specified channel ID
        /// </summary>
        /// <param name="channelID">The ID of the channel</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The current viewers of the channel</returns>
        public async Task<ChatViewersModel> GetViewers(string channelID, int maxResults = 1)
        {
            Validator.ValidateString(channelID, "channelID");
            IEnumerable<ChatViewersInternalModel> viewers = await this.PostPagedCursorAsync<ChatViewersInternalModel>($"channels/{channelID}/viewers", maxResults);

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
        }

        /// <summary>
        /// Sends a message to the currently authenticated channel.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable Task</returns>
        public async Task SendMessage(string message)
        {
            Validator.ValidateString(message, "message");

            JObject jobj = new JObject();
            jobj["content"] = message;
            await this.PostAsync("chat/send", AdvancedHttpClient.CreateContentFromObject(jobj));
        }

        /// <summary>
        /// Sends a message to the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to send to</param>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable Task</returns>
        public async Task SendMessage(string channelID, string message)
        {
            Validator.ValidateString(channelID, "channelID");
            Validator.ValidateString(message, "message");

            JObject jobj = new JObject();
            jobj["channel_id"] = channelID;
            jobj["content"] = message;
            await this.PostAsync("chat/send", AdvancedHttpClient.CreateContentFromObject(jobj));
        }

        /// <summary>
        /// Deletes the specified message in the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to delete</param>
        /// <param name="messageID">The ID of the message to delete</param>
        /// <param name="userID">The ID of the user who sent the message</param>
        /// <returns>Whether the delete was successful</returns>
        public async Task<bool> DeleteMessage(string channelID, string messageID, string userID)
        {
            Validator.ValidateString(channelID, "channelID");
            Validator.ValidateString(messageID, "messageID");
            Validator.ValidateString(userID, "userID");

            return await this.DeleteAsync($"channels/{channelID}/messages/{messageID}/users/{userID}");
        }

        /// <summary>
        /// Performs an official Trovo command in the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to perform the command in</param>
        /// <param name="command">The command to perform</param>
        /// <returns>Null if successful, a status message indicating why the command failed to perform</returns>
        public async Task<string> PerformChatCommand(string channelID, string command)
        {
            Validator.ValidateString(channelID, "channelID");
            Validator.ValidateString(command, "command");

            JObject jobj = new JObject();
            jobj["channel_id"] = channelID;
            jobj["command"] = command;

            jobj = await this.PostAsync<JObject>("channels/command", AdvancedHttpClient.CreateContentFromObject(jobj));
            if (jobj != null)
            {
                JToken success = jobj.SelectToken("is_success");
                JToken message = jobj.SelectToken("display_msg");
                if (success != null && bool.Equals(false, success))
                {
                    return message.ToString();
                }
            }
            return null;
        }
    }
}
