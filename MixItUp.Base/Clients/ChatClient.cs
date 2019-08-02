using Mixer.Base.Model.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Clients
{
    public interface IChatClient
    {
        ObservableCollection<ChatMessageViewModel> Messages { get; }
        ObservableCollection<UserViewModel> Users { get; }

        Task AddMessage(ChatMessageViewModel message);

        Task DeleteMessage(ChatMessageViewModel message);
        Task DeleteMessage(string id);

        Task ClearMessages();

        Task UsersJoined(IEnumerable<UserViewModel> users);
        Task UsersUpdated(IEnumerable<UserViewModel> users);
        Task UsersLeft(IEnumerable<UserViewModel> users);

        Task PurgeUser(UserViewModel user);
    }

    public class ChatClient : IChatClient
    {
        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ObservableCollection<ChatMessageViewModel>();
        private SemaphoreSlim messagesLock = new SemaphoreSlim(1);
        private LockedDictionary<string, ChatMessageViewModel> messagesLookup = new LockedDictionary<string, ChatMessageViewModel>();

        public ObservableCollection<UserViewModel> Users { get; private set; } = new ObservableCollection<UserViewModel>();
        private SemaphoreSlim usersLock = new SemaphoreSlim(1);
        private LockedDictionary<string, UserViewModel> allUsers = new LockedDictionary<string, UserViewModel>();
        private HashSet<string> userEntranceCommands = new HashSet<string>();
        private const int userJoinLeaveEventsTotalToProcess = 25;
        private Dictionary<string, ChatUserEventModel> userJoinEvents = new Dictionary<string, ChatUserEventModel>();
        private Dictionary<string, ChatUserEventModel> userLeaveEvents = new Dictionary<string, ChatUserEventModel>();

        private SemaphoreSlim whisperNumberLock = new SemaphoreSlim(1);
        private Dictionary<string, int> whisperMap = new Dictionary<string, int>();

        public ChatClient() { }

        public async Task AddMessage(ChatMessageViewModel message)
        {
            await this.messagesLock.WaitAndRelease(() =>
            {
                this.messagesLookup[message.ID] = message;
                this.Messages.Add(message);

                while (this.Messages.Count > ChannelSession.Settings.MaxMessagesInChat)
                {
                    ChatMessageViewModel removedMessage = this.Messages[0];
                    this.messagesLookup.Remove(removedMessage.ID);
                    this.Messages.RemoveAt(0);
                }

                return Task.FromResult(0);
            });
        }

        public async Task ClearMessages()
        {
            await this.messagesLock.WaitAndRelease(() =>
            {
                this.messagesLookup.Clear();
                this.Messages.Clear();

                return Task.FromResult(0);
            });
        }

        public async Task DeleteMessage(ChatMessageViewModel message) { await this.DeleteMessage(message.ID); }

        public async Task DeleteMessage(string id)
        {
            await this.messagesLock.WaitAndRelease(() =>
            {
                if (this.messagesLookup.ContainsKey(id))
                {
                    ChatMessageViewModel removedMessage = this.messagesLookup[id];
                    this.messagesLooku
                }

                this.messagesLookup[message.ID] = message;
                this.Messages.Add(message);

                while (this.Messages.Count > ChannelSession.Settings.MaxMessagesInChat)
                {
                    ChatMessageViewModel removedMessage = this.Messages[0];
                    this.messagesLookup.Remove(removedMessage.ID);
                    this.Messages.RemoveAt(0);
                }

                return Task.FromResult(0);
            });
        }

        public Task PurgeUser(UserViewModel user)
        {
            throw new System.NotImplementedException();
        }

        public async Task UsersJoined(IEnumerable<UserViewModel> users)
        {
            await this.usersLock.WaitAndRelease(() =>
            {
                foreach (UserViewModel user in users)
                {
                    this.Users
                }

                return Task.FromResult(0);
            });
        }

        public Task UsersLeft(IEnumerable<UserViewModel> users)
        {
            throw new System.NotImplementedException();
        }

        public Task UsersUpdated(IEnumerable<UserViewModel> users)
        {
            throw new System.NotImplementedException();
        }
    }
}
