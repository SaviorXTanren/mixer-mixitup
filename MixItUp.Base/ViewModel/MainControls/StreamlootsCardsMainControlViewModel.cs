using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube.New;
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
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited += GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited;

            this.StreamlootsManageCollectionCommand = this.CreateCommand((parameters) =>
            {
                if (ServiceManager.Get<TwitchSession>().IsConnected)
                {
                    ServiceManager.Get<IProcessService>().LaunchLink($"https://www.streamloots.com/{ServiceManager.Get<TwitchSession>().StreamerUsername}/manage/cards");
                }
                else if (ServiceManager.Get<YouTubeSession>().IsConnected)
                {
                    ServiceManager.Get<IProcessService>().LaunchLink($"https://www.streamloots.com/{ServiceManager.Get<YouTubeSession>().StreamerID}/manage/cards");
                }
            });
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ServiceManager.Get<CommandService>().StreamlootsCardCommands.ToList();
        }

        private void GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited(object sender, CommandModelBase command)
        {
            if (command.Type == CommandTypeEnum.StreamlootsCard)
            {
                this.AddCommand(command);
            }
        }
    }
}
