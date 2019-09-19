using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Chat.Mixer;
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
using System.Text.RegularExpressions;
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

        Dictionary<string, MixrElixrEmoteModel> MixrElixrEmotes { get; }

        LockedDictionary<string, UserViewModel> AllUsers { get; }
        IEnumerable<UserViewModel> DisplayUsers { get; }
        event EventHandler DisplayUsersUpdated;

        event EventHandler<Dictionary<string, uint>> OnPollEndOccurred;

        Task SendMessage(string message, bool sendAsStreamer = false);
        Task Whisper(UserViewModel user, string message, bool sendAsStreamer = false, bool waitForResponse = false);
        Task Whisper(string username, string message, bool sendAsStreamer = false, bool waitForResponse = false);

        Task DeleteMessage(ChatMessageViewModel message);
        Task ClearMessages();

        Task StartPoll(string question, IEnumerable<string> answers, uint lengthInSeconds);

        Task TimeoutUser(UserViewModel user, uint durationInSeconds);
        Task PurgeUser(UserViewModel user);

        Task ModUser(UserViewModel user);
        Task UnmodUser(UserViewModel user);

        Task BanUser(UserViewModel user);
        Task UnbanUser(UserViewModel user);

        void RebuildCommandTriggers();

        Task AddMessage(ChatMessageViewModel message);
    }

    public class ChatService : IChatService
    {
        private const string ChatEventLogDirectoryName = "ChatEventLogs";
        private const string ChatEventLogFileNameFormat = "ChatEventLog-{0}.txt";

        public IMixerChatService MixerChatService { get; private set; }

        public bool DisableChat { get; set; }

        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ObservableCollection<ChatMessageViewModel>();
        private LockedDictionary<string, ChatMessageViewModel> messagesLookup = new LockedDictionary<string, ChatMessageViewModel>();

        public Dictionary<string, MixrElixrEmoteModel> MixrElixrEmotes { get; private set; } = new Dictionary<string, MixrElixrEmoteModel>();

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

        public event EventHandler<Dictionary<string, uint>> OnPollEndOccurred = delegate { };

        private LockedDictionary<string, PermissionsCommandBase> chatCommandTriggers = new LockedDictionary<string, PermissionsCommandBase>();
        private LockedDictionary<string, PermissionsCommandBase> chatCommandWildcardTriggers = new LockedDictionary<string, PermissionsCommandBase>();

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

            if (ChannelSession.Settings.ShowMixrElixrEmotes)
            {
                IEnumerable<MixrElixrEmoteModel> emotes = await ChannelSession.Services.MixrElixr.GetChannelEmotes(ChannelSession.MixerChannel);
                if (emotes != null)
                {
                    foreach (MixrElixrEmoteModel emote in emotes)
                    {
                        this.MixrElixrEmotes[emote.code] = emote;
                    }
                }
            }

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

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.Mixer;
            if (platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.SendMessage(message, sendAsStreamer);
            }
        }

        public async Task Whisper(UserViewModel user, string message, bool sendAsStreamer = false, bool waitForResponse = false) { await this.Whisper(user.UserName, message, sendAsStreamer); }

        public async Task Whisper(string username, string message, bool sendAsStreamer = false, bool waitForResponse = false)
        {
            StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.Mixer;
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

        public async Task StartPoll(string question, IEnumerable<string> answers, uint lengthInSeconds)
        {
            StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.Mixer;
            if (platform == StreamingPlatformTypeEnum.Mixer)
            {
                this.MixerChatService.OnPollEndOccurred += MixerChatService_OnPollEndOccurred;
                await this.MixerChatService.StartPoll(question, answers, lengthInSeconds);
            }
        }

        public async Task PurgeUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.PurgeUser(user.UserName);
            }
        }

        public async Task TimeoutUser(UserViewModel user, uint durationInSeconds)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.TimeoutUser(user.UserName, durationInSeconds);
            }
        }

        public async Task ModUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.ModUser(user);
            }
        }

        public async Task UnmodUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.UnmodUser(user);
            }
        }

        public async Task BanUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.BanUser(user);
            }
        }

        public async Task UnbanUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Mixer)
            {
                await this.MixerChatService.UnbanUser(user);
            }
        }

        public void RebuildCommandTriggers()
        {
            this.chatCommandTriggers.Clear();
            this.chatCommandWildcardTriggers.Clear();
            foreach (ChatCommand command in ChannelSession.Settings.ChatCommands)
            {
                if (command.ContainsWildcards)
                {
                    foreach (string trigger in command.CommandTriggers)
                    {
                        this.chatCommandWildcardTriggers[trigger] = command;
                    }
                }
                else
                {
                    foreach (string trigger in command.CommandTriggers)
                    {
                        this.chatCommandTriggers[trigger] = command;
                    }
                }
            }

            foreach (GameCommandBase command in ChannelSession.Settings.GameCommands)
            {
                foreach (string trigger in command.CommandTriggers)
                {
                    this.chatCommandTriggers[trigger] = command;
                }
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

                        await ChannelSession.Services.Chat.Whisper(message.User.UserName, $"You are whisperer #{message.User.WhispererNumber}.", false);
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

                    if (await message.CheckForModeration())
                    {
                        await this.DeleteMessage(message);
                        return;
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

                    Dictionary<string, PermissionsCommandBase> commandsToCheck = this.chatCommandTriggers.ToDictionary();
                    foreach (PermissionsCommandBase command in message.User.Data.CustomCommands)
                    {
                        foreach (string trigger in command.CommandTriggers)
                        {
                            commandsToCheck[trigger] = command;
                        }
                    }

                    if (commandsToCheck.Keys.Count() > 0)
                    {
                        bool commandTriggered = false;

                        int maxTriggerLength = commandsToCheck.Keys.Max(t => t.Length);
                        IEnumerable<string> messageParts = message.PlainTextMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < messageParts.Count(); i++)
                        {
                            string commandTriggerCheck = string.Join(" ", messageParts.Take(i + 1));
                            if (commandTriggerCheck.Length > maxTriggerLength)
                            {
                                break;  // Early shorting logic
                            }

                            if (commandsToCheck.ContainsKey(commandTriggerCheck))
                            {
                                await this.RunCommand(message, commandsToCheck[commandTriggerCheck], messageParts.Skip(i + 1));
                                commandTriggered = true;
                                break;
                            }
                        }

                        if (!commandTriggered && this.chatCommandWildcardTriggers.Count > 0)
                        {
                            foreach (var commandToCheck in this.chatCommandWildcardTriggers.Keys.ToList())
                            {
                                Match match = Regex.Match(message.PlainTextMessage, commandToCheck);
                                if (match != null && match.Success)
                                {
                                    IEnumerable<string> arguments = message.PlainTextMessage.Substring(match.Index, match.Length).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    await this.RunCommand(message, this.chatCommandWildcardTriggers[commandToCheck], arguments);
                                    commandTriggered = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (message is AlertChatMessageViewModel)
            {

            }
        }

        private async Task RunCommand(ChatMessageViewModel message, PermissionsCommandBase command, IEnumerable<string> arguments)
        {
            if (command.IsEnabled)
            {
                Logger.Log(LogLevel.Debug, string.Format("Command Found For Message - {0} - {1}", message.ToString(), command.ToString()));

                if (command.Requirements.Settings.DeleteChatCommandWhenRun || (ChannelSession.Settings.DeleteChatCommandsWhenRun && !command.Requirements.Settings.DontDeleteChatCommandWhenRun))
                {
                    Logger.Log(LogLevel.Debug, string.Format("Deleting Message As Chat Command - {0}", message.PlainTextMessage));
                    await this.DeleteMessage(message);
                }

                await command.Perform(message.User, arguments: arguments);
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
                    message.Delete(user: e.Item2, reason: "Purged");
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

        private void MixerChatService_OnPollEndOccurred(object sender, ChatPollEventModel pollResults)
        {
            this.MixerChatService.OnPollEndOccurred -= MixerChatService_OnPollEndOccurred;

            Dictionary<string, uint> results = new Dictionary<string, uint>();
            foreach (string answer in pollResults.answers)
            {
                results[answer] = 0;
                if (pollResults.responses.ContainsKey(answer))
                {
                    results[answer] = pollResults.responses[answer].ToObject<uint>();
                }
            }

            this.OnPollEndOccurred(this, results);
        }

        #endregion Mixer Events
    }
}
