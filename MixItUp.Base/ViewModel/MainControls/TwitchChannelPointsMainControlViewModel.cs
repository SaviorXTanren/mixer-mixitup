using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Twitch.Base.Models.NewAPI.ChannelPoints;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class TwitchChannelPointsMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        private const string RewardCreationErrorTooManyRewards = "CREATE_CUSTOM_REWARD_TOO_MANY_REWARDS";
        private const string RewardCreationErrorDuplicateReward = "CREATE_CUSTOM_REWARD_DUPLICATE_REWARD";

        public ICommand CreateChannelPointRewardCommand { get; set; }

        public ICommand ChannelPointsEditorCommand { get; set; }

        public TwitchChannelPointsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited += GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited;

            this.CreateChannelPointRewardCommand = this.CreateCommand(async () =>
            {
                if (!ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.TwitchAccountMustBeConnectedToUseThisFeature);
                    return;
                }

                string name = await DialogHelper.ShowTextEntry(MixItUp.Base.Resources.ChannelPointRewardName);
                if (!string.IsNullOrEmpty(name))
                {
                    Result<CustomChannelPointRewardModel> reward = await ServiceManager.Get<TwitchSessionService>().UserConnection.CreateCustomChannelPointRewards(ServiceManager.Get<TwitchSessionService>().User, new UpdatableCustomChannelPointRewardModel()
                    {
                        title = name,
                        cost = 1,
                        is_enabled = true,
                    });

                    if (reward.Success)
                    {
                        this.AddCommand(new TwitchChannelPointsCommandModel(reward.Value.title, reward.Value.id));

                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.CreateChannelPointRewardSuccess);
                    }
                    else
                    {
                        string message = reward.Message;
                        try
                        {
                            JObject jobj = JObject.Parse(reward.Message);
                            if (jobj.ContainsKey("message"))
                            {
                                message = jobj["message"].ToString();
                                if (string.Equals(message, RewardCreationErrorTooManyRewards, StringComparison.OrdinalIgnoreCase))
                                {
                                    message = MixItUp.Base.Resources.TwitchChannelPointRewardCreationErrorTooManyRewards;
                                }
                                else if (string.Equals(message, RewardCreationErrorDuplicateReward, StringComparison.OrdinalIgnoreCase))
                                {
                                    message = MixItUp.Base.Resources.TwitchChannelPointRewardCreationErrorRewardAlreadyExists;
                                }
                            }
                        }
                        catch (Exception) { }

                        await DialogHelper.ShowMessage(string.Format(MixItUp.Base.Resources.CreateChannelPointRewardFailure, message));
                    }
                }
            });

            this.ChannelPointsEditorCommand = this.CreateCommand(() =>
            {
                if (ServiceManager.Get<TwitchSessionService>().IsConnected)
                {
                    ServiceManager.Get<IProcessService>().LaunchLink($"https://dashboard.twitch.tv/u/{ServiceManager.Get<TwitchSessionService>().Username}/viewer-rewards/channel-points");
                }
            });
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ServiceManager.Get<CommandService>().TwitchChannelPointsCommands.ToList();
        }

        private void GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited(object sender, CommandModelBase command)
        {
            if (command.Type == CommandTypeEnum.TwitchChannelPoints)
            {
                this.AddCommand(command);
            }
        }
    }
}
