using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.ViewModel;
using Mixer.Base.ViewModel.Chat;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public static PrivatePopulatedUserModel User { get; private set; }
        public static ExpandedChannelModel Channel { get; private set; }

        public static ChannelSettings Settings { get; private set; }

        public static LockedDictionary<uint, ChatUserViewModel> ChatUsers { get; private set; }
        public static LockedDictionary<string, InteractiveParticipantModel> InteractiveUsers { get; private set; }

        public static void Initialize(PrivatePopulatedUserModel user, ExpandedChannelModel channel)
        {
            ChannelSession.User = user;
            ChannelSession.Channel = channel;

            ChannelSession.ChatUsers = new LockedDictionary<uint, ChatUserViewModel>();
            ChannelSession.InteractiveUsers = new LockedDictionary<string, InteractiveParticipantModel>();
        }

        public static async Task LoadSettings() { ChannelSession.Settings = await ChannelSettings.LoadSettings(ChannelSession.Channel); }

        public static async Task SaveSettings() { await ChannelSession.Settings.SaveSettings(); }

        public static async Task RefreshUser()
        {
            if (ChannelSession.User != null)
            {
                ChannelSession.User = await MixerAPIHandler.MixerConnection.Users.GetCurrentUser();
            }
        }

        public static async Task RefreshChannel()
        {
            if (ChannelSession.Channel != null)
            {
                ChannelSession.Channel = await MixerAPIHandler.MixerConnection.Channels.GetChannel(ChannelSession.Channel.user.username);
            }
        }

        public static UserViewModel GetCurrentUser() { return new UserViewModel(User.id, User.username); }
    }
}
