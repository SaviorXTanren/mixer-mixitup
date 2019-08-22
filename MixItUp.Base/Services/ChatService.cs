using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IChatService
    {
        IMixerChatService MixerChatService { get; }

        Task Initialize(IMixerChatService mixerChatService);

        bool DisableChat { get; set; }

        ObservableCollection<ChatMessageViewModel> Messages { get; }

        LockedDictionary<string, UserViewModel> AllUsers { get; }
        IEnumerable<UserViewModel> DisplayUsers { get; }
        event EventHandler DisplayUsersUpdated;

        Task SendMessage(StreamingPlatformTypeEnum platform, string message, bool sendAsStreamer = false);
        Task Whisper(StreamingPlatformTypeEnum platform, UserViewModel user, string message, bool sendAsStreamer = false, bool waitForResponse = false);
        Task Whisper(StreamingPlatformTypeEnum platform, string username, string message, bool sendAsStreamer = false, bool waitForResponse = false);
        Task DeleteMessage(ChatMessageViewModel message);
        Task AddMessage(ChatMessageViewModel message);

        Task ClearMessages();

        Task PurgeUser(UserViewModel user);

        void RebuildCommandTriggers();
    }

    public class ChatService : IChatService
    {
        private const string ChatEventLogDirectoryName = "ChatEventLogs";
        private const string ChatEventLogFileNameFormat = "ChatEventLog-{0}.txt";

        public IMixerChatService MixerChatService { get; private set; }

        public bool DisableChat { get; set; }

        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ObservableCollection<ChatMessageViewModel>();
        private LockedDictionary<string, ChatMessageViewModel> messagesLookup = new LockedDictionary<string, ChatMessageViewModel>();

        public LockedDictionary<string, UserViewModel> AllUsers { get; private set; } = new LockedDictionary<string, UserViewModel>();
        public IEnumerable<UserViewModel> DisplayUsers
        {
            get
            {
                IEnumerable<UserViewModel> users = this.displayUsers.Values;
                users = users.ToList();
                users = users.Take(ChannelSession.Settings.MaxUsersShownInChat);
                return users;
            }
        }
        public event EventHandler DisplayUsersUpdated = delegate { };
        private SortedList<string, UserViewModel> displayUsers = new SortedList<string, UserViewModel>();

        private LockedDictionary<string, ChatCommand> chatCommandTriggers = new LockedDictionary<string, ChatCommand>();

        private HashSet<string> userEntranceCommands = new HashSet<string>();

        private SemaphoreSlim whisperNumberLock = new SemaphoreSlim(1);
        private Dictionary<string, int> whisperMap = new Dictionary<string, int>();

        private string currentChatEventLogFilePath;

        public ChatService() { }

        public async Task Initialize(IMixerChatService mixerChatService)
        {
            this.MixerChatService = mixerChatService;

            this.RebuildCommandTriggers();

            await ChannelSession.Services.FileService.CreateDirectory(ChatEventLogDirectoryName);
            this.currentChatEventLogFilePath = Path.Combine(ChatEventLogDirectoryName, string.Format(ChatEventLogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)));

            this.MixerChatService.OnMessageOccurred += MixerChatService_OnMessageOccurred;
            this.MixerChatService.OnDeleteMessageOccurred += MixerChatService_OnDeleteMessageOccurred;
            this.MixerChatService.OnClearMessagesOccurred += MixerChatService_OnClearMessagesOccurred;
            this.MixerChatService.OnUsersJoinOccurred += MixerChatService_OnUsersJoinOccurred;
            this.MixerChatService.OnUserUpdateOccurred += MixerChatService_OnUserUpdateOccurred;
            this.MixerChatService.OnUsersLeaveOccurred += MixerChatService_OnUsersLeaveOccurred;
            this.MixerChatService.OnUserPurgeOccurred += MixerChatService_OnUserPurgeOccurred;
            this.MixerChatService.OnUserBanOccurred += MixerChatService_OnUserBanOccurred;

            foreach (ChatMessageEventModel messageEvent in await this.MixerChatService.GetChatHistory(50))
            {
                MixerChatMessageViewModel message = new MixerChatMessageViewModel(messageEvent);
                this.messagesLookup[message.ID] = message;
                if (ChannelSession.Settings.LatestChatAtTop)
                {
                    this.Messages.Insert(0, message);
                }
                else
                {
                    this.Messages.Add(message);
                }
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                await ChannelSession.MixerStreamerConnection.GetChatUsers(ChannelSession.MixerChannel, async (collection) =>
                {
                    List<UserViewModel> users = new List<UserViewModel>();
                    foreach (ChatUserModel chatUser in collection)
                    {
                        users.Add(new UserViewModel(chatUser));
                    }
                    await this.UsersJoined(users);
                }, int.MaxValue);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task SendMessage(StreamingPlatformTypeEnum platform, string message, bool sendAsStreamer = false)
        {
            if (platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.SendMessage(message, sendAsStreamer);
            }
        }

        public async Task Whisper(StreamingPlatformTypeEnum platform, UserViewModel user, string message, bool sendAsStreamer = false, bool waitForResponse = false) { await this.Whisper(platform, user.UserName, message, sendAsStreamer); }

        public async Task Whisper(StreamingPlatformTypeEnum platform, string username, string message, bool sendAsStreamer = false, bool waitForResponse = false)
        {
            if (platform == StreamingPlatformTypeEnum.Mixer)
            {
                if (waitForResponse)
                {
                    ChatMessageEventModel messageEvent = await this.MixerChatService.WhisperWithResponse(username, message, sendAsStreamer);
                    if (messageEvent != null)
                    {
                        await this.AddMessage(new MixerChatMessageViewModel(messageEvent));
                    }
                }
                else
                {
                    await this.MixerChatService.Whisper(username, message, sendAsStreamer);
                }
            }
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            message.Delete();
            if (message.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.DeleteMessage(message);
            }
        }

        public async Task ClearMessages()
        {
            this.messagesLookup.Clear();
            this.Messages.Clear();
            await this.MixerChatService.ClearMessages();
        }

        public async Task PurgeUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.PurgeUser(user.UserName);
            }
        }

        public async Task AddMessage(ChatMessageViewModel message)
        {
            Logger.Log(LogLevel.Debug, string.Format("Message Received - {0}", message.ToString()));

            this.messagesLookup[message.ID] = message;
            if (ChannelSession.Settings.LatestChatAtTop)
            {
                this.Messages.Insert(0, message);
            }
            else
            {
                this.Messages.Add(message);
            }

            if (this.Messages.Count > ChannelSession.Settings.MaxMessagesInChat)
            {
                ChatMessageViewModel removedMessage = (ChannelSession.Settings.LatestChatAtTop) ? this.Messages.Last() : this.Messages.First();
                this.messagesLookup.Remove(removedMessage.ID);
                this.Messages.Remove(removedMessage);
            }

            if (message is MixerChatMessageViewModel)
            {
                if (this.DisableChat && !message.ID.Equals(Guid.Empty))
                {
                    Logger.Log(LogLevel.Debug, string.Format("Deleting Message As Chat Disabled - {0}", message.PlainTextMessage));
                    await this.DeleteMessage(message);
                    return;
                }

                if (message.User != null)
                {
                    await message.User.RefreshDetails();
                }
                message.User.UpdateLastActivity();

                if (message.IsWhisper)
                {
                    if (ChannelSession.Settings.TrackWhispererNumber && !message.IsStreamerOrBot)
                    {
                        await this.whisperNumberLock.WaitAndRelease(() =>
                        {
                            if (!whisperMap.ContainsKey(message.User.ID.ToString()))
                            {
                                whisperMap[message.User.ID.ToString()] = whisperMap.Count + 1;
                            }
                            message.User.WhispererNumber = whisperMap[message.User.ID.ToString()];
                            return Task.FromResult(0);
                        });

                        await ChannelSession.Chat.Whisper(message.User.UserName, $"You are whisperer #{message.User.WhispererNumber}.", false);
                    }
                }
                else
                {
                    if (!this.userEntranceCommands.Contains(message.User.ID.ToString()))
                    {
                        this.userEntranceCommands.Add(message.User.ID.ToString());
                        if (message.User.Data.EntranceCommand != null)
                        {
                            await message.User.Data.EntranceCommand.Perform(message.User);
                        }
                    }

                    if (!string.IsNullOrEmpty(message.PlainTextMessage))
                    {
                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>()
                        {
                            { "message", message.PlainTextMessage },
                        };
                        await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerChatMessage), message.User, extraSpecialIdentifiers: specialIdentifiers);
                    }
                }

                if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatWhisperSoundFilePath) && message.IsWhisper)
                {
                    await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatWhisperSoundFilePath, ChannelSession.Settings.NotificationChatWhisperSoundVolume);
                }
                else if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatTaggedSoundFilePath) && message.IsUserTagged)
                {
                    await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatTaggedSoundFilePath, ChannelSession.Settings.NotificationChatTaggedSoundVolume);
                }
                else if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatMessageSoundFilePath))
                {
                    await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatMessageSoundFilePath, ChannelSession.Settings.NotificationChatMessageSoundVolume);
                }

                GlobalEvents.ChatMessageReceived(message);

                if (ChannelSession.IsStreamer && !string.IsNullOrEmpty(message.PlainTextMessage) && message.User != null && !message.User.MixerRoles.Contains(MixerRoleEnum.Banned))
                {
                    if (!ChannelSession.Settings.AllowCommandWhispering && message.IsWhisper)
                    {
                        return;
                    }

                    if (ChannelSession.Settings.IgnoreBotAccountCommands && ChannelSession.MixerBotUser != null && message.User != null && message.User.ID.Equals(ChannelSession.MixerBotUser.id))
                    {
                        return;
                    }

                    if (ChannelSession.Settings.CommandsOnlyInYourStream && !message.IsInUsersChannel)
                    {
                        return;
                    }

                    Logger.Log(LogLevel.Debug, string.Format("Checking Message For Command - {0}", message.ToString()));

                    Dictionary<string, ChatCommand> commandsToCheck = this.chatCommandTriggers.ToDictionary();
                    foreach (ChatCommand command in message.User.Data.CustomCommands)
                    {
                        foreach (string trigger in command.CommandTriggers)
                        {
                            commandsToCheck[trigger] = command;
                        }
                    }

                    IEnumerable<string> messageParts = message.PlainTextMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < messageParts.Count(); i++)
                    {
                        string commandTriggerCheck = string.Join(" ", messageParts.Take(i + 1));
                        if (commandsToCheck.ContainsKey(commandTriggerCheck))
                        {
                            ChatCommand command = commandsToCheck[commandTriggerCheck];
                            if (command.IsEnabled)
                            {
                                Logger.Log(LogLevel.Debug, string.Format("Command Found For Message - {0} - {1}", message.ToString(), command.ToString()));

                                if (command.Requirements.Settings.DeleteChatCommandWhenRun || (ChannelSession.Settings.DeleteChatCommandsWhenRun && !command.Requirements.Settings.DontDeleteChatCommandWhenRun))
                                {
                                    Logger.Log(LogLevel.Debug, string.Format("Deleting Message As Chat Command - {0}", message.PlainTextMessage));
                                    await this.DeleteMessage(message);
                                }

                                IEnumerable<string> arguments = messageParts.Skip(i + 1);
                                await command.Perform(message.User, arguments: arguments);

                                break;
                            }
                        }
                    }
                }
            }
            else if (message is AlertChatMessageViewModel)
            {

            }

            //if (!ModerationHelper.MeetsChatInteractiveParticipationRequirement(message.User) || !ModerationHelper.MeetsChatEmoteSkillsOnlyParticipationRequirement(message.User, message))
            //{
            //    Logger.Log(LogLevel.Debug, string.Format("Deleting Message As User does not meet requirement - {0} - {1}", ChannelSession.Settings.ModerationChatInteractiveParticipation, message.PlainTextMessage));

            //    await this.DeleteMessage(message);

            //    await ModerationHelper.SendChatInteractiveParticipationWhisper(message.User, isChat: true);
            //    return;
            //}

            //string moderationReason = await message.ShouldBeModerated();
            //if (!string.IsNullOrEmpty(moderationReason))
            //{
            //    Logger.Log(LogLevel.Debug, string.Format("Moderation Being Performed - {0}", message.ToString()));

            //    message.ModerationReason = moderationReason;
            //    await this.DeleteMessage(message);
            //}





            //if (ChannelSession.IsStreamer && !message.User.MixerRoles.Contains(MixerRoleEnum.Banned))
            //{
            //    GlobalEvents.ChatCommandMessageReceived(message);

            //    List<PermissionsCommandBase> commandsToCheck = new List<PermissionsCommandBase>(ChannelSession.AllEnabledChatCommands);
            //    commandsToCheck.AddRange(message.User.Data.CustomCommands);

            //    PermissionsCommandBase command = commandsToCheck.FirstOrDefault(c => c.MatchesCommand(message.PlainTextMessage));
            //if (command == null)
            //{
            //    command = commandsToCheck.FirstOrDefault(c => c.ContainsCommand(message.PlainTextMessage));
            //}
            //    if (command != null)
            //    {
            //        Logger.Log(LogLevel.Debug, string.Format("Command Found For Message - {0} - {1}", message.ToString(), command.ToString()));

            //        await command.Perform(message.User, command.GetArgumentsFromText(message.PlainTextMessage));

            //        bool delete = false;
            //        if (ChannelSession.Settings.DeleteChatCommandsWhenRun)
            //        {
            //            if (!command.Requirements.Settings.DontDeleteChatCommandWhenRun)
            //            {
            //                delete = true;
            //            }
            //        }
            //        else if (command.Requirements.Settings.DeleteChatCommandWhenRun)
            //        {
            //            delete = true;
            //        }

            //        if (delete)
            //        {
            //            Logger.Log(LogLevel.Debug, string.Format("Deleting Message As Chat Command - {0}", message.PlainTextMessage));
            //            await this.DeleteMessage(message);
            //        }
            //    }
            //}
        }

        public void RebuildCommandTriggers()
        {
            this.chatCommandTriggers.Clear();
            foreach (ChatCommand command in ChannelSession.Settings.ChatCommands)
            {
                foreach (string trigger in command.CommandTriggers)
                {
                    this.chatCommandTriggers[trigger] = command;
                }
            }
        }

        private async Task UsersJoined(IEnumerable<UserViewModel> users)
        {
            List<AlertChatMessageViewModel> alerts = new List<AlertChatMessageViewModel>();

            foreach (UserViewModel user in users)
            {
                this.AllUsers[user.ID.ToString()] = user;
                this.displayUsers[user.SortableID] = user;

                if (ChannelSession.Settings.ChatShowUserJoinLeave && users.Count() < 5)
                {
                    alerts.Add(new AlertChatMessageViewModel(user.Platform, string.Format("{0} Joined Chat", user.UserName), ChannelSession.Settings.ChatUserJoinLeaveColorScheme));
                }
            }
            this.DisplayUsersUpdated(this, new EventArgs());

            foreach (AlertChatMessageViewModel alert in alerts)
            {
                await this.AddMessage(alert);
            }
        }

        private async Task UsersUpdated(IEnumerable<UserViewModel> users)
        {
            await this.UsersLeft(users);
            await this.UsersJoined(users);
        }

        private async Task UsersLeft(IEnumerable<UserViewModel> users)
        {
            List<AlertChatMessageViewModel> alerts = new List<AlertChatMessageViewModel>();

            foreach (UserViewModel user in users)
            {
                if (this.AllUsers.Remove(user.ID.ToString()))
                {
                    this.displayUsers.Remove(user.UserName);

                    if (ChannelSession.Settings.ChatShowUserJoinLeave && users.Count() < 5)
                    {
                        alerts.Add(new AlertChatMessageViewModel(user.Platform, string.Format("{0} Left Chat", user.UserName), ChannelSession.Settings.ChatUserJoinLeaveColorScheme));
                    }
                }
            }
            this.DisplayUsersUpdated(this, new EventArgs());

            foreach (AlertChatMessageViewModel alert in alerts)
            {
                await this.AddMessage(alert);
            }
        }

        #region Mixer Events

        private async void MixerChatService_OnMessageOccurred(object sender, ChatMessageViewModel message)
        {
            await this.AddMessage(message);
            if (ChannelSession.Settings.SaveChatEventLogs)
            {
                try
                {
                    await ChannelSession.Services.FileService.AppendFile(this.currentChatEventLogFilePath, string.Format("{0} ({1}){2}",
                        message, DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture), Environment.NewLine));
                }
                catch (Exception) { }
            }
        }

        private void MixerChatService_OnDeleteMessageOccurred(object sender, Guid id)
        {
            if (this.messagesLookup.TryGetValue(id.ToString(), out ChatMessageViewModel message))
            {
                message.Delete();
                GlobalEvents.ChatMessageDeleted(id);
            }
        }

        private void MixerChatService_OnClearMessagesOccurred(object sender, EventArgs e)
        {
            this.messagesLookup.Clear();
            this.Messages.Clear();
        }

        private async void MixerChatService_OnUsersJoinOccurred(object sender, IEnumerable<UserViewModel> users)
        {
            await this.UsersJoined(users);
        }

        private async void MixerChatService_OnUserUpdateOccurred(object sender, UserViewModel user)
        {
            await this.UsersUpdated(new List<UserViewModel>() { user });
        }

        private async void MixerChatService_OnUsersLeaveOccurred(object sender, IEnumerable<UserViewModel> users)
        {
            await this.UsersLeft(users);
        }

        private async void MixerChatService_OnUserPurgeOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            foreach (ChatMessageViewModel message in this.Messages.ToList())
            {
                if (message.Platform == StreamingPlatformTypeEnum.Mixer && message.User.Equals(e.Item1))
                {
                    message.Delete(user: e.Item2);
                }
            }

            if (EventCommand.CanUserRunEvent(e.Item1, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge)))
            {
                UserViewModel targetUser = e.Item1;
                UserViewModel modUser = e.Item2;
                if (e.Item2 == null)
                {
                    modUser = new UserViewModel(ChannelSession.MixerStreamerUser);
                }
                await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge), modUser, arguments: new List<string>() { targetUser.UserName });
            }
        }

        private async void MixerChatService_OnUserBanOccurred(object sender, UserViewModel user)
        {
            if (EventCommand.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan)))
            {
                await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan), user);
            }
        }

        #endregion Mixer Events
    }
}
