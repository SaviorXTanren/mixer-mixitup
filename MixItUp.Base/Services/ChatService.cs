using Mixer.Base.Model.Chat;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IChatService
    {
        Task Initialize(MixerChatService mixerChatService);

        bool DisableChat { get; set; }

        ObservableCollection<ChatMessageViewModel> Messages { get; }
        ObservableCollection<UserViewModel> DisplayUsers { get; }
        LockedDictionary<string, UserViewModel> AllUsers { get; }

        Task SendMessage(PlatformTypeEnum platform, string message, bool sendAsStreamer = false);
        Task DeleteMessage(ChatMessageViewModel message);

        Task ClearMessages();

        Task PurgeUser(UserViewModel user);
    }

    public class ChatService : IChatService
    {
        public bool DisableChat { get; set; }

        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ObservableCollection<ChatMessageViewModel>();
        private LockedDictionary<string, ChatMessageViewModel> messagesLookup = new LockedDictionary<string, ChatMessageViewModel>();

        public ObservableCollection<UserViewModel> DisplayUsers { get; private set; } = new ObservableCollection<UserViewModel>();
        public LockedDictionary<string, UserViewModel> AllUsers { get; private set; } = new LockedDictionary<string, UserViewModel>();
        private SemaphoreSlim usersLock = new SemaphoreSlim(1);
        private Dictionary<string, MixerRoleEnum> cachedUserRoles = new Dictionary<string, MixerRoleEnum>();
        private HashSet<string> userEntranceCommands = new HashSet<string>();

        private SemaphoreSlim whisperNumberLock = new SemaphoreSlim(1);
        private Dictionary<string, int> whisperMap = new Dictionary<string, int>();

        private MixerChatService mixerChatService;

        public ChatService() { }

        public async Task Initialize(MixerChatService mixerChatService)
        {
            this.mixerChatService = mixerChatService;

            this.mixerChatService.OnMessageOccurred += MixerChatService_OnMessageOccurred;
            this.mixerChatService.OnDeleteMessageOccurred += MixerChatService_OnDeleteMessageOccurred;
            this.mixerChatService.OnClearMessagesOccurred += MixerChatService_OnClearMessagesOccurred;
            this.mixerChatService.OnUsersJoinOccurred += MixerChatService_OnUsersJoinOccurred;
            this.mixerChatService.OnUserUpdateOccurred += MixerChatService_OnUserUpdateOccurred;
            this.mixerChatService.OnUsersLeaveOccurred += MixerChatService_OnUsersLeaveOccurred;
            this.mixerChatService.OnUserPurgeOccurred += MixerChatService_OnUserPurgeOccurred;

            foreach (ChatMessageEventModel message in await this.mixerChatService.GetChatHistory(50))
            {
                await this.AddMessage(new ChatMessageViewModel(message));
            }
        }

        public async Task SendMessage(PlatformTypeEnum platform, string message, bool sendAsStreamer = false)
        {
            if (platform == PlatformTypeEnum.Mixer)
            {
                await this.mixerChatService.SendMessage(message, sendAsStreamer);
            }
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            message.IsDeleted = true;
            if (message.Platform == PlatformTypeEnum.Mixer)
            {
                await this.mixerChatService.DeleteMessage(message);
            }
        }

        public async Task ClearMessages()
        {
            this.messagesLookup.Clear();
            this.Messages.Clear();
            await this.mixerChatService.ClearMessages();
        }

        public async Task PurgeUser(UserViewModel user)
        {
            if (user.Platform == PlatformTypeEnum.Mixer)
            {
                await this.mixerChatService.PurgeUser(user.UserName);
            }
        }

        private async Task AddMessage(ChatMessageViewModel message)
        {
            //if (message.User != null)
            //{
            //    await message.User.RefreshDetails();
            //}
            //message.User.UpdateLastActivity();

            Util.Logger.LogDiagnostic(string.Format("Message Received - {0}", message.ToString()));

            if (!message.IsWhisper && !this.userEntranceCommands.Contains(message.User.ID.ToString()))
            {
                this.userEntranceCommands.Add(message.User.ID.ToString());
                if (message.User.Data.EntranceCommand != null)
                {
                    await message.User.Data.EntranceCommand.Perform(message.User);
                }
            }

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

            if (this.DisableChat && !message.ID.Equals(Guid.Empty))
            {
                Util.Logger.LogDiagnostic(string.Format("Deleting Message As Chat Disabled - {0}", message.PlainTextMessage));
                await this.DeleteMessage(message);
                return;
            }

            //if (!ModerationHelper.MeetsChatInteractiveParticipationRequirement(message.User) || !ModerationHelper.MeetsChatEmoteSkillsOnlyParticipationRequirement(message.User, message))
            //{
            //    Util.Logger.LogDiagnostic(string.Format("Deleting Message As User does not meet requirement - {0} - {1}", ChannelSession.Settings.ModerationChatInteractiveParticipation, message.PlainTextMessage));

            //    await this.DeleteMessage(message);

            //    await ModerationHelper.SendChatInteractiveParticipationWhisper(message.User, isChat: true);
            //    return;
            //}

            //string moderationReason = await message.ShouldBeModerated();
            //if (!string.IsNullOrEmpty(moderationReason))
            //{
            //    Util.Logger.LogDiagnostic(string.Format("Moderation Being Performed - {0}", message.ToString()));

            //    message.ModerationReason = moderationReason;
            //    await this.DeleteMessage(message);
            //}

            //if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatWhisperSoundFilePath) && message.IsWhisper)
            //{
            //    await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatWhisperSoundFilePath, ChannelSession.Settings.NotificationChatWhisperSoundVolume);
            //}
            //else if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatTaggedSoundFilePath) && message.IsUserTagged)
            //{
            //    await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatTaggedSoundFilePath, ChannelSession.Settings.NotificationChatTaggedSoundVolume);
            //}
            //else if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatMessageSoundFilePath))
            //{
            //    await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatMessageSoundFilePath, ChannelSession.Settings.NotificationChatMessageSoundVolume);
            //}

            //if (message.IsWhisper && ChannelSession.Settings.TrackWhispererNumber && !message.IsStreamerOrBot())
            //{
            //    await this.whisperNumberLock.WaitAndRelease(() =>
            //    {
            //        if (!whisperMap.ContainsKey(message.User.ID.ToString()))
            //        {
            //            whisperMap[message.User.ID.ToString()] = whisperMap.Count + 1;
            //        }
            //        message.User.WhispererNumber = whisperMap[message.User.ID.ToString()];
            //        return Task.FromResult(0);
            //    });

            //    await ChannelSession.Chat.Whisper(message.User.UserName, $"You are whisperer #{message.User.WhispererNumber}.", false);
            //}

            //GlobalEvents.ChatMessageReceived(message);

            //Util.Logger.LogDiagnostic(string.Format("Checking Message For Command - {0}", message.ToString()));

            //if (!ChannelSession.Settings.AllowCommandWhispering && message.IsWhisper)
            //{
            //    return;
            //}

            //if (ChannelSession.MixerBotUser != null && ChannelSession.Settings.IgnoreBotAccountCommands && message.User != null && message.User.ID.Equals(ChannelSession.MixerBotUser.id))
            //{
            //    return;
            //}

            //if (ChannelSession.Settings.CommandsOnlyInYourStream && !message.IsInUsersChannel)
            //{
            //    return;
            //}

            //if (ChannelSession.IsStreamer && !message.User.MixerRoles.Contains(MixerRoleEnum.Banned))
            //{
            //    GlobalEvents.ChatCommandMessageReceived(message);

            //    List<PermissionsCommandBase> commandsToCheck = new List<PermissionsCommandBase>(ChannelSession.AllEnabledChatCommands);
            //    commandsToCheck.AddRange(message.User.Data.CustomCommands);

            //    PermissionsCommandBase command = commandsToCheck.FirstOrDefault(c => c.MatchesCommand(message.PlainTextMessage));
            //    if (command == null)
            //    {
            //        command = commandsToCheck.FirstOrDefault(c => c.ContainsCommand(message.PlainTextMessage));
            //    }

            //    if (command != null)
            //    {
            //        Util.Logger.LogDiagnostic(string.Format("Command Found For Message - {0} - {1}", message.ToString(), command.ToString()));

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
            //            Util.Logger.LogDiagnostic(string.Format("Deleting Message As Chat Command - {0}", message.PlainTextMessage));
            //            await this.DeleteMessage(message);
            //        }
            //    }
            //}
        }

        private async Task UsersJoined(IEnumerable<UserViewModel> users)
        {
            await this.usersLock.WaitAndRelease(() =>
            {
                foreach (UserViewModel user in users)
                {
                    this.AllUsers[user.ID.ToString()] = user;
                    if (this.DisplayUsers.Count <= ChannelSession.Settings.MaxUsersShownInChat)
                    {
                        this.cachedUserRoles[user.ID.ToString()] = user.PrimarySortableRole;

                        int insertIndex = 0;
                        for (insertIndex = 0; insertIndex < this.DisplayUsers.Count; insertIndex++)
                        {
                            UserViewModel compareUser = this.DisplayUsers[insertIndex];
                            if (compareUser != null)
                            {
                                if (!this.cachedUserRoles.ContainsKey(compareUser.ID.ToString()))
                                {
                                    this.cachedUserRoles[compareUser.ID.ToString()] = compareUser.PrimarySortableRole;
                                }

                                if (this.cachedUserRoles[compareUser.ID.ToString()] == this.cachedUserRoles[user.ID.ToString()])
                                {
                                    if (!string.IsNullOrEmpty(user.UserName) && compareUser.UserName.CompareTo(user.UserName) > 0)
                                    {
                                        break;
                                    }
                                }
                                else if (this.cachedUserRoles[compareUser.ID.ToString()] < this.cachedUserRoles[user.ID.ToString()])
                                {
                                    break;
                                }
                            }
                        }

                        if (insertIndex < this.DisplayUsers.Count)
                        {
                            this.DisplayUsers.Insert(insertIndex, user);
                        }
                        else
                        {
                            this.DisplayUsers.Add(user);
                        }
                    }
                }
                return Task.FromResult(0);
            });
        }

        private async Task UsersUpdated(IEnumerable<UserViewModel> users)
        {
            await this.UsersLeft(users);
            await this.UsersJoined(users);
        }

        private async Task UsersLeft(IEnumerable<UserViewModel> users)
        {
            await this.usersLock.WaitAndRelease(() =>
            {
                foreach (UserViewModel user in users)
                {
                    if (this.AllUsers.ContainsKey(user.ID.ToString()))
                    {
                        this.DisplayUsers.Remove(user);
                        this.AllUsers.Remove(user.ID.ToString());
                    }
                }
                return Task.FromResult(0);
            });
        }

        #region Mixer Events

        private async void MixerChatService_OnMessageOccurred(object sender, ChatMessageViewModel message)
        {
            await this.AddMessage(message);
        }

        private void MixerChatService_OnDeleteMessageOccurred(object sender, Guid id)
        {
            if (this.messagesLookup.TryGetValue(id.ToString(), out ChatMessageViewModel message))
            {
                message.IsDeleted = true;
                GlobalEvents.ChatMessageDeleted(id);
            }
        }

        private async void MixerChatService_OnClearMessagesOccurred(object sender, EventArgs e)
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
                if (message.User.Equals(e.Item1))
                {
                    message.IsDeleted = true;
                    if (e.Item2 != null)
                    {
                        message.DeletedBy = e.Item2.UserName;
                    }
                }
            }

            if (ChannelSession.Constellation.CanUserRunEvent(e.Item1, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge)))
            {
                ChannelSession.Constellation.LogUserRunEvent(e.Item1, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge));
                UserViewModel targetUser = e.Item1;
                UserViewModel modUser = e.Item2;
                if (e.Item2 == null)
                {
                    modUser = new UserViewModel(ChannelSession.MixerStreamerUser);
                }
                await ChannelSession.Constellation.RunEventCommand(ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge)), modUser, arguments: new List<string>() { targetUser.UserName });
            }
        }

        #endregion Mixer Events
    }
}
