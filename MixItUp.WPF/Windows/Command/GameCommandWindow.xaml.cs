using MaterialDesignThemes.Wpf;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Games;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Windows.Command
{
    public enum GameTypeEnum
    {
        [Name("Single Player")]
        SinglePlayer,
        [Name("Individual Probability")]
        IndividualProbabilty,
        [Name("Only One Winner")]
        OnlyOneWinner,
        [Name("User Charity")]
        UserCharity,
    }

    /// <summary>
    /// Interaction logic for GameCommandWindow.xaml
    /// </summary>
    public partial class GameCommandWindow : LoadingWindowBase
    {
        private GameCommandBase command;

        private ObservableCollection<GameOutcomeCommandControl> outcomeCommandControls = new ObservableCollection<GameOutcomeCommandControl>();
        private ObservableCollection<GameOutcomeGroupControl> outcomeGroupControls = new ObservableCollection<GameOutcomeGroupControl>();

        public GameCommandWindow() : this(null) { }

        public GameCommandWindow(GameCommandBase command)
        {
            this.command = command;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public GameCommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.GameLowestRoleAllowedComboBox.ItemsSource = ChatCommand.PermissionsAllowedValues;
            this.GameLowestRoleAllowedComboBox.SelectedIndex = 0;

            this.CurrencyTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;
            this.CurrencyRequirementTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<CurrencyRequirementTypeEnum>();

            this.GameTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<GameTypeEnum>();

            this.AddOutcomeRankGroupButton.IsEnabled = (ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank).Count() > 0);

            this.OutcomeCommandsItemsControl.ItemsSource = this.outcomeCommandControls;
            this.OutcomeGroupsItemsControl.ItemsSource = this.outcomeGroupControls;

            await this.GameStartedCommandControl.Initialize(this, null);
            await this.GameEndedCommandControl.Initialize(this, null);
            await this.UserJoinedCommandControl.Initialize(this, null);
            await this.NotEnoughUsersCommandControl.Initialize(this, null);

            await this.UserParticipatedCommandControl.Initialize(this, null);

            await this.LoseLeftoverProbabilityCommandControl.Initialize(this, null);

            if (this.command != null)
            {
                this.GameNameTextBox.Text = this.command.Name;
                this.GameChatCommandTextBox.Text = this.command.CommandsString;

                this.GameCooldownTextBox.Text = this.command.Cooldown.ToString();
                this.GameLowestRoleAllowedComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Permissions);

                if (this.command is SinglePlayerGameCommand)
                {
                    this.GameTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GameTypeEnum.SinglePlayer);
                }
                else if (this.command is IndividualProbabilityGameCommand)
                {
                    this.GameTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GameTypeEnum.IndividualProbabilty);

                    IndividualProbabilityGameCommand ipCommand = (IndividualProbabilityGameCommand)this.command;

                    this.GameLengthTextBox.Text = ipCommand.GameLength.ToString();
                    this.GameMinimumParticipantsTextBox.Text = ipCommand.MinimumParticipants.ToString();

                    await this.GameStartedCommandControl.Initialize(this, ipCommand.GameStartedCommand);
                    await this.GameEndedCommandControl.Initialize(this, ipCommand.GameEndedCommand);
                    await this.UserJoinedCommandControl.Initialize(this, ipCommand.UserJoinedCommand);
                    await this.NotEnoughUsersCommandControl.Initialize(this, ipCommand.NotEnoughUsersCommand);
                }
                else if (this.command is OnlyOneWinnerGameCommand)
                {
                    this.GameTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GameTypeEnum.OnlyOneWinner);

                    OnlyOneWinnerGameCommand oowCommand = (OnlyOneWinnerGameCommand)this.command;

                    this.GameLengthTextBox.Text = oowCommand.GameLength.ToString();
                    this.GameMinimumParticipantsTextBox.Text = oowCommand.MinimumParticipants.ToString();

                    await this.GameStartedCommandControl.Initialize(this, oowCommand.GameStartedCommand);
                    await this.GameEndedCommandControl.Initialize(this, oowCommand.GameEndedCommand);
                    await this.UserJoinedCommandControl.Initialize(this, oowCommand.UserJoinedCommand);
                    await this.NotEnoughUsersCommandControl.Initialize(this, oowCommand.NotEnoughUsersCommand);
                }
                else if (this.command is UserCharityGameCommand)
                {
                    this.GameTypeComboBox.SelectedItem = EnumHelper.GetEnumName(GameTypeEnum.UserCharity);

                    UserCharityGameCommand rucComand = (UserCharityGameCommand)this.command;

                    this.GiveToRandomUserCharityToggleButton.IsChecked = rucComand.GiveToRandomUser;

                    await this.UserParticipatedCommandControl.Initialize(this, rucComand.UserParticipatedCommand);
                }

                this.CurrencyTypeComboBox.SelectedItem = this.command.CurrencyRequirement.GetCurrency();
                this.CurrencyRequirementTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.CurrencyRequirementType);
                this.CurrencyMinimumCostTextBox.Text = this.command.CurrencyRequirement.RequiredAmount.ToString();
                this.CurrencyMaximumCostTextBox.Text = this.command.CurrencyRequirement.MaximumAmount.ToString();

                if (this.command is OutcomeGameCommandBase)
                {
                    OutcomeGameCommandBase oCommand = (OutcomeGameCommandBase)this.command;

                    await this.LoseLeftoverProbabilityCommandControl.Initialize(this, oCommand.LoseLeftoverCommand);

                    this.outcomeCommandControls.Clear();
                    foreach (GameOutcome outcome in oCommand.Outcomes)
                    {
                        GameOutcomeCommandControl outcomeControl = new GameOutcomeCommandControl( outcome);
                        await outcomeControl.Initialize(this);
                        this.outcomeCommandControls.Add(outcomeControl);
                    }

                    this.outcomeGroupControls.Clear();
                    foreach (GameOutcomeGroup group in oCommand.Groups)
                    {
                        this.outcomeGroupControls.Add(new GameOutcomeGroupControl(group));
                    }
                }
            }
            else
            {
                this.GameCooldownTextBox.Text = "0";
            }
        }

        private IEnumerable<string> GetCommandStrings() { return new List<string>(this.GameChatCommandTextBox.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)); }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.GameNameTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("Name is missing");
                    return;
                }

                if (string.IsNullOrEmpty(this.GameChatCommandTextBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("Commands is missing");
                    return;
                }

                if (this.GameChatCommandTextBox.Text.Any(c => !Char.IsLetterOrDigit(c) && !Char.IsWhiteSpace(c)))
                {
                    await MessageBoxHelper.ShowMessageDialog("Commands can only contain letters and numbers");
                    return;
                }

                foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
                {
                    if (this.command != command && this.GameNameTextBox.Text.Equals(command.Name))
                    {
                        await MessageBoxHelper.ShowMessageDialog("There already exists a chat command with the same name");
                        return;
                    }
                }

                IEnumerable<string> commandStrings = this.GetCommandStrings();
                if (commandStrings.GroupBy(c => c).Where(g => g.Count() > 1).Count() > 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Each command string must be unique");
                    return;
                }

                foreach (PermissionsCommandBase command in ChannelSession.AllChatCommands)
                {
                    if (command.IsEnabled && this.GetExistingCommand() != command)
                    {
                        if (commandStrings.Any(c => command.Commands.Contains(c)))
                        {
                            await MessageBoxHelper.ShowMessageDialog("There already exists a chat command that uses one of the command strings you have specified");
                            return;
                        }
                    }
                }

                int cooldown = 0;
                if (!int.TryParse(this.GameCooldownTextBox.Text, out cooldown) || cooldown < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Cooldown must be 0 or greater");
                    return;
                }

                if (this.GameLowestRoleAllowedComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A permission level must be selected");
                    return;
                }

                if (this.GameTypeComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A game type must be selected");
                    return;
                }
                GameTypeEnum gameType = EnumHelper.GetEnumValueFromString<GameTypeEnum>((string)this.GameTypeComboBox.SelectedItem);

                if (this.CurrencyTypeComboBox.SelectedIndex < 0 || this.CurrencyRequirementTypeComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A currency type and requirement must be selected");
                    return;
                }

                int minimum = 0;
                int maximum = 0;

                CurrencyRequirementTypeEnum currencyRequirementType = EnumHelper.GetEnumValueFromString<CurrencyRequirementTypeEnum>((string)this.CurrencyRequirementTypeComboBox.SelectedItem);
                if (currencyRequirementType != CurrencyRequirementTypeEnum.NoCurrencyCost && (!int.TryParse(this.CurrencyMinimumCostTextBox.Text, out minimum) || minimum < 0))
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency minimum must be 0 or greater");
                    return;
                }

                if (currencyRequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum && (!int.TryParse(this.CurrencyMaximumCostTextBox.Text, out maximum) || maximum <= minimum))
                {
                    await MessageBoxHelper.ShowMessageDialog("The currency maximum must be greater than the minimum");
                    return;
                }

                int gameLength = 0;
                int minParticipants = 0;
                if (this.MultiplayerGameDetailsGrid.Visibility == Visibility.Visible)
                {
                    if (!int.TryParse(this.GameLengthTextBox.Text, out gameLength) || gameLength < 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("Game length must be 0 or greater");
                        return;
                    }

                    if (!int.TryParse(this.GameMinimumParticipantsTextBox.Text, out minParticipants) || minParticipants < 1)
                    {
                        await MessageBoxHelper.ShowMessageDialog("Minimum participants must be 1 or greater");
                        return;
                    }
                }

                List<GameOutcome> outcomes = new List<GameOutcome>();
                List<GameOutcomeGroup> outcomeGroups = new List<GameOutcomeGroup>();
                if (gameType == GameTypeEnum.SinglePlayer || gameType == GameTypeEnum.IndividualProbabilty)
                {
                    foreach (GameOutcomeCommandControl commandControl in this.outcomeCommandControls)
                    {
                        GameOutcome outcome = await commandControl.GetOutcome();
                        if (outcome == null)
                        {
                            return;
                        }
                        outcomes.Add(outcome);
                    }

                    if (outcomes.Select(o => o.Name).GroupBy(n => n).Where(g => g.Count() > 1).Count() > 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("Each outcome must have a unique name");
                        return;
                    }

                    foreach (GameOutcomeGroupControl groupControl in this.outcomeGroupControls)
                    {
                        GameOutcomeGroup group = await groupControl.GetOutcomeGroup();
                        if (group == null)
                        {
                            return;
                        }
                        outcomeGroups.Add(group);
                        for (int i = 0; i < outcomes.Count; i++)
                        {
                            group.Probabilities[i].OutcomeName = outcomes[i].Name;
                        }
                    }
                }

                UserRole permissionsRole = EnumHelper.GetEnumValueFromString<UserRole>((string)this.GameLowestRoleAllowedComboBox.SelectedItem);

                if (this.command != null)
                {
                    ChannelSession.Settings.GameCommands.Remove(this.command);
                }

                UserCurrencyRequirementViewModel currencyRequirement = new UserCurrencyRequirementViewModel((UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem, minimum, maximum);
                if (gameType == GameTypeEnum.SinglePlayer)
                {
                    this.command = new SinglePlayerGameCommand(this.GameNameTextBox.Text, this.GetCommandStrings(), permissionsRole, cooldown, currencyRequirement, currencyRequirementType, outcomes,
                        outcomeGroups, this.LoseLeftoverProbabilityCommandControl.GetCommand());
                }
                else if (gameType == GameTypeEnum.IndividualProbabilty)
                {
                    IndividualProbabilityGameCommand ipCommand = new IndividualProbabilityGameCommand(this.GameNameTextBox.Text, this.GetCommandStrings(), permissionsRole, cooldown,
                        currencyRequirement, currencyRequirementType, outcomes, outcomeGroups, this.LoseLeftoverProbabilityCommandControl.GetCommand(), gameLength, minParticipants);
                    ipCommand.GameStartedCommand = this.GameStartedCommandControl.GetCommand();
                    ipCommand.GameEndedCommand = this.GameEndedCommandControl.GetCommand();
                    ipCommand.UserJoinedCommand = this.UserJoinedCommandControl.GetCommand();
                    ipCommand.NotEnoughUsersCommand = this.NotEnoughUsersCommandControl.GetCommand();
                    this.command = ipCommand;
                }
                else if (gameType == GameTypeEnum.OnlyOneWinner)
                {
                    OnlyOneWinnerGameCommand oowCommand = new OnlyOneWinnerGameCommand(this.GameNameTextBox.Text, this.GetCommandStrings(), permissionsRole, cooldown, currencyRequirement,
                        gameLength, minParticipants);
                    oowCommand.GameStartedCommand = this.GameStartedCommandControl.GetCommand();
                    oowCommand.GameEndedCommand = this.GameEndedCommandControl.GetCommand();
                    oowCommand.UserJoinedCommand = this.UserJoinedCommandControl.GetCommand();
                    oowCommand.NotEnoughUsersCommand = this.NotEnoughUsersCommandControl.GetCommand();
                    this.command = oowCommand;
                }
                else if (gameType == GameTypeEnum.UserCharity)
                {
                    UserCharityGameCommand ucCommand = new UserCharityGameCommand(this.GameNameTextBox.Text, this.GetCommandStrings(), permissionsRole, cooldown, currencyRequirement,
                        currencyRequirementType, this.GiveToRandomUserCharityToggleButton.IsChecked.GetValueOrDefault());
                    ucCommand.UserParticipatedCommand = this.UserParticipatedCommandControl.GetCommand();
                    this.command = ucCommand;
                }

                ChannelSession.Settings.GameCommands.Add(this.command);

                await ChannelSession.SaveSettings();

                this.Close();
            });
        }

        private async void GameTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (this.GameTypeComboBox.SelectedIndex >= 0)
                {
                    this.CurrencyGrid.IsEnabled = true;

                    this.MultiplayerGameDetailsGrid.Visibility = Visibility.Collapsed;
                    this.OutcomesDetailsGrid.Visibility = Visibility.Collapsed;
                    this.UserCharityGameGrid.Visibility = Visibility.Collapsed;

                    GameTypeEnum gameType = EnumHelper.GetEnumValueFromString<GameTypeEnum>((string)this.GameTypeComboBox.SelectedItem);

                    if (gameType == GameTypeEnum.OnlyOneWinner)
                    {
                        this.CurrencyRequirementTypeComboBox.SelectedItem = EnumHelper.GetEnumName(CurrencyRequirementTypeEnum.RequiredAmount);
                        this.CurrencyRequirementTypeComboBox.IsEnabled = false;
                    }
                    else
                    {
                        this.CurrencyRequirementTypeComboBox.SelectedIndex = -1;
                        this.CurrencyRequirementTypeComboBox.IsEnabled = true;
                    }

                    if (gameType == GameTypeEnum.IndividualProbabilty || gameType == GameTypeEnum.OnlyOneWinner)
                    {
                        this.MultiplayerGameDetailsGrid.Visibility = Visibility.Visible;
                    }

                    if (gameType == GameTypeEnum.SinglePlayer || gameType == GameTypeEnum.IndividualProbabilty)
                    {
                        this.OutcomesDetailsGrid.Visibility = Visibility.Visible;

                        this.outcomeCommandControls.Clear();

                        GameOutcomeCommandControl outcomeControl = new GameOutcomeCommandControl(new GameOutcome("Win"));
                        await outcomeControl.Initialize(this);
                        this.outcomeCommandControls.Add(outcomeControl);

                        this.outcomeGroupControls.Clear();

                        GameOutcomeGroup userGroup = new GameOutcomeGroup(UserRole.User);
                        userGroup.Probabilities.Add(new GameOutcomeProbability(50, 25));
                        this.outcomeGroupControls.Add(new GameOutcomeGroupControl(userGroup));

                        GameOutcomeGroup subscriberGroup = new GameOutcomeGroup(UserRole.Subscriber);
                        subscriberGroup.Probabilities.Add(new GameOutcomeProbability(50, 25));
                        this.outcomeGroupControls.Add(new GameOutcomeGroupControl(subscriberGroup));

                        GameOutcomeGroup modGroup = new GameOutcomeGroup(UserRole.Mod);
                        modGroup.Probabilities.Add(new GameOutcomeProbability(50, 25));
                        this.outcomeGroupControls.Add(new GameOutcomeGroupControl(modGroup));
                    }

                    if (gameType == GameTypeEnum.UserCharity)
                    {
                        this.UserCharityGameGrid.Visibility = Visibility.Visible;
                    }
                }
            });
        }

        private void AddProbabilitiesForOutcome()
        {
            foreach (GameOutcomeGroupControl outcomeGroupControl in this.outcomeGroupControls)
            {
                outcomeGroupControl.AddProbability(new GameOutcomeProbabilityControl());
            }
        }

        private void CurrencyTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CurrencyTypeComboBox.SelectedIndex >= 0)
            {
                this.CurrencyRequirementTypeComboBox.IsEnabled = true;
            }
        }

        private void CurrencyRequirementTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CurrencyRequirementTypeComboBox.SelectedIndex >= 0)
            {
                CurrencyRequirementTypeEnum requirementType = EnumHelper.GetEnumValueFromString<CurrencyRequirementTypeEnum>((string)this.CurrencyRequirementTypeComboBox.SelectedItem);
                if (requirementType == CurrencyRequirementTypeEnum.NoCurrencyCost)
                {
                    this.CurrencyCostsGrid.IsEnabled = false;
                }
                else
                {
                    this.CurrencyCostsGrid.IsEnabled = true;
                    this.CurrencyMaximumCostTextBox.IsEnabled = (requirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum);

                    if (requirementType == CurrencyRequirementTypeEnum.RequiredAmount)
                    {
                        HintAssist.SetHint(this.CurrencyMinimumCostTextBox, "Required Amount");
                    }
                    else
                    {
                        HintAssist.SetHint(this.CurrencyMinimumCostTextBox, "Minimum Amount");
                    }
                }
            }
        }

        private void AddOutcomeRankGroupButton_Click(object sender, RoutedEventArgs e)
        {
            GameOutcomeGroupControl groupControl = new GameOutcomeGroupControl();
            this.outcomeGroupControls.Add(groupControl);
            foreach (GameOutcomeCommandControl commandControl in this.outcomeCommandControls)
            {
                groupControl.AddProbability(new GameOutcomeProbabilityControl());
            }
        }

        private async void AddOutcomeProbabilityButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                GameOutcomeCommandControl outcomeControl = new GameOutcomeCommandControl();
                await outcomeControl.Initialize(this);
                this.outcomeCommandControls.Add(outcomeControl);
                this.AddProbabilitiesForOutcome();
            });
        }

        private void RandomUserCharityToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
