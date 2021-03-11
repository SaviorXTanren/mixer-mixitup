using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.ChannelPoints;

namespace MixItUp.Base.ViewModel.Commands
{
    public class TwitchChannelPointsCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public ThreadSafeObservableCollection<string> ChannelPointRewards { get; set; } = new ThreadSafeObservableCollection<string>();

        public TwitchChannelPointsCommandEditorWindowViewModel(TwitchChannelPointsCommandModel existingCommand) : base(existingCommand) { }

        public TwitchChannelPointsCommandEditorWindowViewModel() : base(CommandTypeEnum.TwitchChannelPoints) { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new TwitchChannelPointsCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ChannelSession.TwitchChannelPointsCommands.Remove((TwitchChannelPointsCommandModel)this.existingCommand);
            ChannelSession.TwitchChannelPointsCommands.Add((TwitchChannelPointsCommandModel)command);
            return Task.FromResult(0);
        }

        protected override async Task OnLoadedInternal()
        {
            IEnumerable<CustomChannelPointRewardModel> customChannelPointRewards = await ChannelSession.TwitchUserConnection.GetCustomChannelPointRewards(ChannelSession.TwitchUserNewAPI);
            if (customChannelPointRewards != null)
            {
                this.ChannelPointRewards.AddRange(customChannelPointRewards.Select(c => c.title));
            }

            await base.OnLoadedInternal();
        }
    }
}
