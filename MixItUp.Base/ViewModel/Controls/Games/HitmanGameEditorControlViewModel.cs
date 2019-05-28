using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class HitmanGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string MinimumParticipantsString
        {
            get { return this.MinimumParticipants.ToString(); }
            set
            {
                this.MinimumParticipants = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int MinimumParticipants { get; set; } = 2;

        public string TimeLimitString
        {
            get { return this.TimeLimit.ToString(); }
            set
            {
                this.TimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int TimeLimit { get; set; } = 30;

        public string HitmanTimeLimitString
        {
            get { return this.HitmanTimeLimit.ToString(); }
            set
            {
                this.HitmanTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int HitmanTimeLimit { get; set; } = 10;

        public string CustomHitmanNamesFilePath
        {
            get { return this.customHitmanNamesFilePath; }
            set
            {
                this.customHitmanNamesFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customHitmanNamesFilePath;

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }
        public CustomCommand NotEnoughPlayersCommand { get; set; }

        public CustomCommand HitmanApproachingCommand { get; set; }
        public CustomCommand HitmanAppearsCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }
        public CustomCommand UserFailCommand { get; set; }

        public ICommand BrowseCustomHitmanNamesFilePathCommand { get; set; }

        public HitmanGameCommand existingCommand;

        public HitmanGameEditorControlViewModel(UserCurrencyViewModel currency)
            : this()
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of hitman! Type !hitman in chat to play!");

            this.UserJoinCommand = this.CreateBasicChatCommand("You assemble with everyone else, patiently waiting for the hitman to appear...", whisper: true);
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");

            this.HitmanApproachingCommand = this.CreateBasicChatCommand("You can feel the presence of the hitman approaching. Get ready...");
            this.HitmanAppearsCommand = this.CreateBasicChatCommand("It's hitman $gamehitmanname! Quick, type $gamehitmanname in chat!");

            this.UserSuccessCommand = this.CreateBasicChatCommand("$gamewinners got hitman $gamehitmanname and walked away with a bounty of $gamepayout " + currency.Name + "!");
            this.UserFailCommand = this.CreateBasicChatCommand("No one was quick enough to get hitman $gamehitmanname! Better luck next time...");
        }

        public HitmanGameEditorControlViewModel(HitmanGameCommand command)
            : this()
        {
            this.existingCommand = command;

            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;
            this.HitmanTimeLimit = this.existingCommand.HitmanTimeLimit;
            this.CustomHitmanNamesFilePath = this.existingCommand.CustomWordsFilePath;

            this.StartedCommand = this.existingCommand.StartedCommand;

            this.UserJoinCommand = this.existingCommand.UserJoinCommand;
            this.NotEnoughPlayersCommand = this.existingCommand.NotEnoughPlayersCommand;

            this.HitmanApproachingCommand = this.existingCommand.HitmanApproachingCommand;
            this.HitmanAppearsCommand = this.existingCommand.HitmanAppearsCommand;

            this.UserSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
            this.UserFailCommand = this.existingCommand.UserFailOutcome.Command;
        }

        private HitmanGameEditorControlViewModel()
        {
            this.BrowseCustomHitmanNamesFilePathCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Text Files (*.txt)|*.txt|All files (*.*)|*.*");
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.CustomHitmanNamesFilePath = filePath;
                }
                return Task.FromResult(0);
            });
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new HitmanGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, this.CustomHitmanNamesFilePath, this.HitmanTimeLimit,
                this.StartedCommand, this.UserJoinCommand, this.HitmanApproachingCommand, this.HitmanAppearsCommand, new GameOutcome("Success", 0, roleProbabilities, this.UserSuccessCommand),
                new GameOutcome("Failure", 0, roleProbabilities, this.UserFailCommand), this.NotEnoughPlayersCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.TimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (this.MinimumParticipants <= 0)
            {
                await DialogHelper.ShowMessage("The Minimum Participants is not a valid number greater than 0");
                return false;
            }

            if (this.HitmanTimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Hitman Time Limit is not a valid number greater than 0");
                return false;
            }

            if (!string.IsNullOrEmpty(this.CustomHitmanNamesFilePath) && !ChannelSession.Services.FileService.FileExists(this.CustomHitmanNamesFilePath))
            {
                await DialogHelper.ShowMessage("The Custom Hitman Names file you specified does not exist");
                return false;
            }

            return true;
        }
    }
}
