using MixItUp.Base.Model.Commands;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class TwitchChannelPointsMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public TwitchChannelPointsMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.TwitchChannelPointsCommands.ToList();
        }
    }
}
