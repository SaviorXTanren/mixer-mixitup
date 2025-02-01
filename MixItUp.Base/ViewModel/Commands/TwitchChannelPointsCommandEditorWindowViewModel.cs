using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.ChannelPoints;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class TwitchChannelPointsCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public ObservableCollection<CustomChannelPointRewardModel> ChannelPointRewards { get; set; } = new ObservableCollection<CustomChannelPointRewardModel>();

        public CustomChannelPointRewardModel ChannelPointReward
        {
            get { return this.channelPointReward; }
            set
            {
                this.channelPointReward = value;
                this.NotifyPropertyChanged();

                this.Name = (this.channelPointReward != null) ? this.channelPointReward.title : string.Empty;
            }
        }
        private CustomChannelPointRewardModel channelPointReward;

        private Guid existingChannelPointRewardID = Guid.Empty;

        public TwitchChannelPointsCommandEditorWindowViewModel(TwitchChannelPointsCommandModel existingCommand)
            : base(existingCommand)
        {
            this.existingChannelPointRewardID = existingCommand.ChannelPointRewardID;
        }

        public TwitchChannelPointsCommandEditorWindowViewModel() : base(CommandTypeEnum.TwitchChannelPoints) { }

        public override Task<Result> Validate()
        {
            if (this.ChannelPointReward == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ChannelPointRewardMissing));
            }

            return Task.FromResult(new Result());
        }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return TwitchChannelPointsCommandModel.GetChannelPointTestSpecialIdentifiers(); }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new TwitchChannelPointsCommandModel(this.ChannelPointReward.title, this.ChannelPointReward.id)); }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            ((TwitchChannelPointsCommandModel)command).ChannelPointRewardID = this.ChannelPointReward.id;
        }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().TwitchChannelPointsCommands.Remove((TwitchChannelPointsCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().TwitchChannelPointsCommands.Add((TwitchChannelPointsCommandModel)command);
            return Task.CompletedTask;
        }

        protected override async Task OnOpenInternal()
        {
            if (ServiceManager.Get<TwitchSession>().IsConnected)
            {
                IEnumerable<CustomChannelPointRewardModel> rewards = await ServiceManager.Get<TwitchSession>().StreamerService.GetCustomChannelPointRewards(ServiceManager.Get<TwitchSession>().StreamerModel);
                if (rewards != null && rewards.Count() > 0)
                {
                    foreach (CustomChannelPointRewardModel channelPoint in rewards.OrderBy(c => c.title))
                    {
                        this.ChannelPointRewards.Add(channelPoint);
                    }

                    if (this.existingChannelPointRewardID != Guid.Empty)
                    {
                        this.ChannelPointReward = this.ChannelPointRewards.FirstOrDefault(c => c.id.Equals(this.existingChannelPointRewardID));
                    }
                    else if (!string.IsNullOrEmpty(this.Name))
                    {
                        this.ChannelPointReward = this.ChannelPointRewards.FirstOrDefault(c => c.title.Equals(this.Name));
                    }
                }
            }
            await base.OnOpenInternal();
        }
    }
}
