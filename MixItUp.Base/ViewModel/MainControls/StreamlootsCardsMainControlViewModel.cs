using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class StreamlootsCardsMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public ICommand ChannelPointsEditorCommand { get; set; }

        public StreamlootsCardsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.ChannelPointsEditorCommand = this.CreateCommand((parameters) =>
            {
                if (ChannelSession.TwitchUserConnection != null)
                {
                    ProcessHelper.LaunchLink(this.GetChannelPointsEditorURL());
                }
            });
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.TwitchChannelPointsCommands.ToList();
        }

        private string GetChannelPointsEditorURL()
        {
            if (ChannelSession.TwitchUserConnection != null)
            {
                return $"https://dashboard.twitch.tv/u/{ChannelSession.TwitchUserNewAPI.login}/viewer-rewards/channel-points";
            }
            return null;
        }
    }
}
