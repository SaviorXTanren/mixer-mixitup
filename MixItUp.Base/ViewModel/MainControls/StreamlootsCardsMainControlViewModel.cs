using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class StreamlootsCardsMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public ICommand StreamlootsManageCollectionCommand { get; set; }

        public StreamlootsCardsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.StreamlootsManageCollectionCommand = this.CreateCommand((parameters) =>
            {
                if (ChannelSession.TwitchUserConnection != null)
                {
                    ProcessHelper.LaunchLink($"https://www.streamloots.com/{ChannelSession.TwitchUserNewAPI.login}/manage/cards");
                }
            });
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.StreamlootsCardCommands.ToList();
        }
    }
}
