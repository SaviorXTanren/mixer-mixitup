using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Games
{
    /// <summary>
    /// Interaction logic for HitmanGameEditorControl.xaml
    /// </summary>
    public partial class HitmanGameEditorControl : GameEditorControlBase
    {
        private HitmanGameCommand existingCommand;

        private CustomCommand startedCommand { get; set; }

        private CustomCommand userJoinCommand { get; set; }

        private CustomCommand hitmanApproachingCommand { get; set; }
        private CustomCommand hitmanAppearsCommand { get; set; }

        private CustomCommand userSuccessCommand { get; set; }
        private CustomCommand userFailCommand { get; set; }

        public HitmanGameEditorControl()
        {
            InitializeComponent();
        }

        public HitmanGameEditorControl(HitmanGameCommand command)
            : this()
        {
            this.existingCommand = command;
        }

        public override async Task<bool> Validate()
        {
            if (!await this.CommandDetailsControl.Validate())
            {
                return false;
            }

            if (!int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit) || timeLimit <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.MinimumParticipantsTextBox.Text, out int minimumParticipants) || minimumParticipants <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Minimum Participants is not a valid number greater than 0");
                return false;
            }

            if (!int.TryParse(this.HitmanTimeLimitTextBox.Text, out int hitmanTimeLimit) || hitmanTimeLimit <= 0)
            {
                await MessageBoxHelper.ShowMessageDialog("The Hitman Time Limit is not a valid number greater than 0");
                return false;
            }

            if (!string.IsNullOrEmpty(this.CustomHitmanNamesFilePathTextBox.Text) && !ChannelSession.Services.FileService.FileExists(this.CustomHitmanNamesFilePathTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("The Custom Hitman Names file you specified does not exist");
                return false;
            }

            return true;
        }

        public override void SaveGameCommand()
        {
            int.TryParse(this.MinimumParticipantsTextBox.Text, out int minimumParticipants);
            int.TryParse(this.TimeLimitTextBox.Text, out int timeLimit);
            int.TryParse(this.HitmanTimeLimitTextBox.Text, out int hitmanTimeLimit);

            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new HitmanGameCommand(this.CommandDetailsControl.GameName, this.CommandDetailsControl.ChatTriggers,
                this.CommandDetailsControl.GetRequirements(), minimumParticipants, timeLimit, this.CustomHitmanNamesFilePathTextBox.Text, hitmanTimeLimit,
                this.startedCommand, this.userJoinCommand, this.hitmanApproachingCommand, this.hitmanAppearsCommand, new GameOutcome("Success", 0, roleProbabilities, this.userSuccessCommand),
                new GameOutcome("Failure", 0, roleProbabilities, this.userFailCommand));
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        protected override Task OnLoaded()
        {
            if (this.existingCommand != null)
            {
                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);

                this.CommandDetailsControl.SetDefaultValues(this.existingCommand);
                this.MinimumParticipantsTextBox.Text = this.existingCommand.MinimumParticipants.ToString();
                this.TimeLimitTextBox.Text = this.existingCommand.TimeLimit.ToString();
                this.HitmanTimeLimitTextBox.Text = this.existingCommand.HitmanTimeLimit.ToString();

                this.startedCommand = this.existingCommand.StartedCommand;

                this.userJoinCommand = this.existingCommand.UserJoinCommand;

                this.hitmanApproachingCommand = this.existingCommand.HitmanApproachingCommand;
                this.hitmanAppearsCommand = this.existingCommand.HitmanAppearsCommand;

                this.userSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
                this.userFailCommand = this.existingCommand.UserFailOutcome.Command;

                this.CustomHitmanNamesFilePathTextBox.Text = this.existingCommand.CustomWordsFilePath;
            }
            else
            {
                this.CommandDetailsControl.SetDefaultValues("Hitman", "hitman", CurrencyRequirementTypeEnum.MinimumAndMaximum, 10, 1000);
                UserCurrencyViewModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault();
                this.MinimumParticipantsTextBox.Text = "2";
                this.TimeLimitTextBox.Text = "30";
                this.HitmanTimeLimitTextBox.Text = "10";

                this.startedCommand = this.CreateBasicChatCommand("@$username has started a game of hitman! Type !hitman in chat to play!");

                this.userJoinCommand = this.CreateBasicChatCommand("You assemble with everyone else, patiently waiting for the hitman to appear...", whisper: true);

                this.hitmanApproachingCommand = this.CreateBasicChatCommand("You can feel the presence of the hitman approaching. Get ready...");
                this.hitmanAppearsCommand = this.CreateBasicChatCommand("It's hitman $gamehitmanname! Quick, type $gamehitmanname in chat!");

                this.userSuccessCommand = this.CreateBasicChatCommand("$gamewinners got hitman $gamehitmanname and walked away with a bounty of $gamepayout " + currency.Name + "!");
                this.userFailCommand = this.CreateBasicChatCommand("No one was quick enough to get hitman $gamehitmanname! Better luck next time...");
            }

            this.GameStartCommandButtonsControl.DataContext = this.startedCommand;
            this.UserJoinedCommandButtonsControl.DataContext = this.userJoinCommand;
            this.HitmanApproachingCommandButtonsControl.DataContext = this.hitmanApproachingCommand;
            this.HitmanAppearsCommandButtonsControl.DataContext = this.hitmanAppearsCommand;
            this.SuccessOutcomeCommandButtonsControl.DataContext = this.userSuccessCommand;
            this.FailOutcomeCommandButtonsControl.DataContext = this.userFailCommand;

            return base.OnLoaded();
        }

        private void CustomHitmanNamesFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Text Files (*.txt)|*.txt|All files (*.*)|*.*");
            if (!string.IsNullOrEmpty(filePath))
            {
                this.CustomHitmanNamesFilePathTextBox.Text = filePath;
            }
        }
    }
}
