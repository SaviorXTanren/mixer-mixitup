using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.ViewModel.Chat;
using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    public static class ChannelSession
    {
        public static PrivatePopulatedUserModel User { get; private set; }
        public static ExpandedChannelModel Channel { get; private set; }

        public static ChannelSettings Settings { get; private set; }

        public static Dictionary<uint, ChatUserViewModel> ChatUsers { get; private set; }
        public static Dictionary<string, InteractiveParticipantModel> InteractiveUsers { get; private set; }

        public static ObservableCollection<CommandBase> ActiveCommands { get; private set; }

        public static InteractiveGameListingModel SelectedGame { get; set; }
        public static InteractiveVersionModel SelectedGameVersion { get; set; }
        public static List<InteractiveConnectedSceneGroupModel> SelectedScenes { get; set; }
        public static InteractiveConnectedSceneGroupModel SelectedScene { get; set; }

        public static void Initialize(PrivatePopulatedUserModel user, ExpandedChannelModel channel)
        {
            ChannelSession.User = user;
            ChannelSession.Channel = channel;

            ChannelSession.ChatUsers = new Dictionary<uint, ChatUserViewModel>();
            ChannelSession.InteractiveUsers = new Dictionary<string, InteractiveParticipantModel>();
            ChannelSession.ActiveCommands = new ObservableCollection<CommandBase>();
            ChannelSession.SelectedScenes = new List<InteractiveConnectedSceneGroupModel>();
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

        public static IEnumerable<T> GetCommands<T>() where T : CommandBase
        {
            return ChannelSession.ActiveCommands.Where(c => c is T).Cast<T>();
        }
    }
}
