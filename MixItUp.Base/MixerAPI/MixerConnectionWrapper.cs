using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Interactive;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Game;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class MixerConnectionWrapper : MixerRequestWrapperBase
    {
        public MixerConnection Connection { get; private set; }

        public MixerConnectionWrapper(MixerConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<ChatClientWrapper> CreateChatClient(ChannelModel channel)
        {
            ChatClient client = await this.RunAsync(ChatClient.CreateFromChannel(this.Connection, channel));
            if (client != null)
            {
                return new ChatClientWrapper(client);
            }
            return null;
        }

        public async Task<ConstellationClientWrapper> CreateConstellationClient()
        {
            ConstellationClient client = await this.RunAsync(ConstellationClient.Create(this.Connection));
            if (client != null)
            {
                return new ConstellationClientWrapper(client);
            }
            return null;
        }

        public async Task<InteractiveClientWrapper> CreateInteractiveClient(ChannelModel channel, InteractiveGameListingModel game)
        {
            InteractiveClient client = await this.RunAsync(InteractiveClient.CreateFromChannel(this.Connection, channel, game));
            if (client != null)
            {
                return new InteractiveClientWrapper(client);
            }
            return null;
        }

        public async Task<UserModel> GetUser(string username) { return await this.RunAsync(this.Connection.Users.GetUser(username)); }

        public async Task<UserModel> GetUser(uint userID) { return await this.RunAsync(this.Connection.Users.GetUser(userID)); }

        public async Task<UserWithChannelModel> GetUser(UserModel user) { return await this.RunAsync(this.Connection.Users.GetUser(user)); }

        public async Task<IEnumerable<UserWithGroupsModel>> GetUsersWithRoles(ChannelModel channel, UserRole role) { return await this.RunAsync(this.Connection.Channels.GetUsersWithRoles(channel, role.ToString())); }

        public async Task<PrivatePopulatedUserModel> GetCurrentUser() { return await this.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<ChatUserModel> GetChatUser(ChannelModel channel, uint userID) { return await this.RunAsync(this.Connection.Chats.GetUser(channel, userID)); }

        public async Task<IEnumerable<ChatUserModel>> GetChatUsers(ChannelModel channel) { return await this.RunAsync(this.Connection.Chats.GetUsers(channel)); }

        public async Task<ExpandedChannelModel> GetChannel(string name) { return await this.RunAsync(this.Connection.Channels.GetChannel(name)); }

        public async Task<ChannelModel> UpdateChannel(ChannelModel channel) { return await this.RunAsync(this.Connection.Channels.UpdateChannel(channel)); }

        public async Task<IEnumerable<GameTypeModel>> GetGameTypes(string name, uint maxResults = 1) { return await this.RunAsync(this.Connection.GameTypes.GetGameTypes(name, maxResults)); }

        public async Task<DateTimeOffset?> CheckIfFollows(ChannelModel channel, UserModel user) { return await this.RunAsync(this.Connection.Channels.CheckIfFollows(ChannelSession.Channel, user)); }

        public async Task<Dictionary<UserModel, DateTimeOffset?>> CheckIfFollows(ChannelModel channel, IEnumerable<UserModel> users) { return await this.RunAsync(this.Connection.Channels.CheckIfFollows(ChannelSession.Channel, users)); }

        public async Task<IEnumerable<StreamSessionsAnalyticModel>> GetStreamSessions(ChannelModel channel, DateTimeOffset startTime) { return await this.RunAsync(this.Connection.Channels.GetStreamSessions(ChannelSession.Channel, startTime)); }

        public async Task AddUserRoles(ChannelModel channel, UserModel user, IEnumerable<UserRole> roles) { await this.RunAsync(this.Connection.Channels.UpdateUserRoles(ChannelSession.Channel, user, roles.Select(r => EnumHelper.GetEnumName(r)), null)); }

        public async Task RemoveUserRoles(ChannelModel channel, UserModel user, IEnumerable<UserRole> roles) { await this.RunAsync(this.Connection.Channels.UpdateUserRoles(ChannelSession.Channel, user, null, roles.Select(r => EnumHelper.GetEnumName(r)))); } 

        public async Task<IEnumerable<InteractiveGameListingModel>> GetOwnedInteractiveGames(ChannelModel channel) { return await this.RunAsync(this.Connection.Interactive.GetOwnedInteractiveGames(channel)); }

        public async Task<InteractiveGameListingModel> CreateInteractiveGame(ChannelModel channel, UserModel user, string name) { return await this.RunAsync(InteractiveGameHelper.CreateInteractive2Game(this.Connection, channel, user, name, null)); }

        public async Task<InteractiveGameVersionModel> GetInteractiveGameVersion(InteractiveGameVersionModel version) { return await this.RunAsync(this.Connection.Interactive.GetInteractiveGameVersion(version)); }

        public async Task UpdateInteractiveGameVersion(InteractiveGameVersionModel version) { await this.RunAsync(this.Connection.Interactive.UpdateInteractiveGameVersion(version)); }
    }
}
