using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModels;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Actions
{
    public class ConditionalClauseViewModel : UIViewModelBase
    {
        public ConditionalClauseModel Clause { get; set; }

        public ICommand DeleteCommand { get; set; }

        private ConditionalActionControl actionControl;

        public ConditionalClauseViewModel(ConditionalClauseModel clause, ConditionalActionControl actionControl)
        {
            this.Clause = clause;
            this.actionControl = actionControl;

            this.DeleteCommand = this.CreateCommand((parameter) =>
            {
                this.actionControl.Clauses.Remove(this);
                return Task.FromResult(0);
            });
        }

        public bool CanBeRemoved
        {
            get { return this.canBeRemoved; }
            set
            {
                this.canBeRemoved = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool canBeRemoved = true;

        public IEnumerable<string> ComparisonTypes { get { return EnumHelper.GetEnumNames<ConditionalComparisionTypeEnum>(); } }

        public string ComparisionTypeString
        {
            get { return EnumHelper.GetEnumName(this.ComparisionType); }
            set
            {
                this.ComparisionType = EnumHelper.GetEnumValueFromString<ConditionalComparisionTypeEnum>(value);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsBetweenOperatorSelected");
                this.NotifyPropertyChanged("IsBetweenOperatorNotSelected");
            }
        }

        public ConditionalComparisionTypeEnum ComparisionType
        {
            get { return this.Clause.ComparisionType; }
            set
            {
                this.Clause.ComparisionType = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool IsBetweenOperatorSelected { get { return this.ComparisionType == ConditionalComparisionTypeEnum.Between; } }
        public bool IsBetweenOperatorNotSelected { get { return !this.IsBetweenOperatorSelected; } }

        public string Value1
        {
            get { return this.Clause.Value1; }
            set
            {
                this.Clause.Value1 = value;
                this.NotifyPropertyChanged();
            }
        }
        public string Value2
        {
            get { return this.Clause.Value2; }
            set
            {
                this.Clause.Value2 = value;
                this.NotifyPropertyChanged();
            }
        }
        public string Value3
        {
            get { return this.Clause.Value3; }
            set
            {
                this.Clause.Value3 = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool Validate()
        {
            if (this.ComparisionType == ConditionalComparisionTypeEnum.Between)
            {
                return !string.IsNullOrEmpty(this.Value1) && !string.IsNullOrEmpty(this.Value2) && !string.IsNullOrEmpty(this.Value3);
            }
            else
            {
                return !(string.IsNullOrEmpty(this.Value1) && string.IsNullOrEmpty(this.Value2));
            }
        }
    }

    /// <summary>
    /// Interaction logic for ConditionalActionControl.xaml
    /// </summary>
    public partial class ConditionalActionControl : ActionControlBase
    {
        private const string SingleActionCommandType = "Single Action";

        public ObservableCollection<ConditionalClauseViewModel> Clauses { get; set; } = new ObservableCollection<ConditionalClauseViewModel>();

        private ConditionalAction action;


        public CommandBase Command
        {
            get { return this.command; }
            set
            {
                this.command = value;
                if (this.Command != null)
                {
                    this.CommandTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.Command.Type);
                    this.CommandNameComboBox.SelectedItem = this.Command;
                }
            }
        }
        private CommandBase command;

        private ActionBase singleAction;

        private ActionContentContainerControl ActionControlContentControl;

        public ConditionalActionControl() : base() { InitializeComponent(); }

        public ConditionalActionControl(ConditionalAction action) : this()
        {
            this.action = action;
            this.singleAction = this.action.Action;
        }

        public override Task OnLoaded()
        {
            this.ActionControlContentControl = (ActionContentContainerControl)this.GetByUid("ActionControlContentControl");

            List<CommandTypeEnum> commandTypes = EnumHelper.GetEnumList<CommandTypeEnum>().ToList();
            commandTypes.Remove(CommandTypeEnum.Game);
            commandTypes.Remove(CommandTypeEnum.Remote);
            commandTypes.Remove(CommandTypeEnum.Custom);

            List<string> commandTypeStrings = EnumHelper.GetEnumNames(commandTypes).ToList();
            commandTypeStrings.Add(SingleActionCommandType);
            this.CommandTypeComboBox.ItemsSource = commandTypeStrings;

            this.OperatorTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ConditionalOperatorTypeEnum>();
            if (this.action != null)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (this.action.Clauses.Count == 0 && !string.IsNullOrEmpty(this.action.Value1))
                {
                    StoreCommandUpgrader.UpdateConditionalAction(new List<ActionBase>() { this.action });
                }
#pragma warning restore CS0612 // Type or member is obsolete

                this.CaseSensitiveToggleButton.IsChecked = !this.action.IgnoreCase;
                this.OperatorTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Operator);
                foreach (ConditionalClauseModel clause in this.action.Clauses)
                {
                    this.Clauses.Add(new ConditionalClauseViewModel(clause, this));
                }
                this.Clauses.First().CanBeRemoved = false;
                this.Command = this.action.GetCommand();

                if (this.singleAction != null)
                {
                    this.CommandTypeComboBox.SelectedItem = SingleActionCommandType;
                    this.CommandNameComboBox.SelectedItem = EnumHelper.GetEnumName(this.singleAction.Type);
                    this.ActionControlContentControl.ResetAction();
                    this.ActionControlContentControl.AssignAction(this.singleAction);
                    this.singleAction = null;
                }
            }
            else
            {
                this.OperatorTypeComboBox.SelectedItem = EnumHelper.GetEnumName(ConditionalOperatorTypeEnum.And);
                this.Clauses.Add(new ConditionalClauseViewModel(new ConditionalClauseModel(), this) { CanBeRemoved = false });
            }
            this.ClausesItemsControl.ItemsSource = this.Clauses;

            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.OperatorTypeComboBox.SelectedIndex >= 0 && this.Clauses.Count >= 0)
            {
                ConditionalOperatorTypeEnum op = EnumHelper.GetEnumValueFromString<ConditionalOperatorTypeEnum>((string)this.OperatorTypeComboBox.SelectedItem);
                if (this.Clauses.All(c => c.Validate()))
                {
                    if (this.Command != null)
                    {
                        return new ConditionalAction(!this.CaseSensitiveToggleButton.IsChecked.GetValueOrDefault(), op, this.Clauses.Select(c => c.Clause), this.Command);
                    }
                    else
                    {
                        ActionBase action = this.ActionControlContentControl.GetAction();
                        if (action != null)
                        {
                            return new ConditionalAction(!this.CaseSensitiveToggleButton.IsChecked.GetValueOrDefault(), op, this.Clauses.Select(c => c.Clause), action);
                        }
                    }
                }
            }
            return null;
        }

        private void AddClauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Clauses.Add(new ConditionalClauseViewModel(new ConditionalClauseModel(), this));
        }

        private void CommandTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandTypeComboBox.SelectedIndex >= 0)
            {
                string selection = (string)this.CommandTypeComboBox.SelectedItem;
                List<string> names = new List<string>();
                if (selection.Equals(SingleActionCommandType))
                {
                    names.AddRange(EnumHelper.GetEnumNames<ActionTypeEnum>());
                }
                else
                {
                    IEnumerable<CommandBase> commands = this.GetCommandsForType(selection);
                    names.AddRange(commands.Select(c => c.Name));
                }
                this.CommandNameComboBox.ItemsSource = names.OrderBy(c => c);
                this.CommandNameComboBox.SelectedIndex = -1;
                this.CommandNameComboBox.IsEnabled = true;
            }
        }

        private void CommandNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandTypeComboBox.SelectedIndex >= 0 && this.CommandNameComboBox.SelectedIndex >= 0)
            {
                string selection = (string)this.CommandTypeComboBox.SelectedItem;
                string name = (string)this.CommandNameComboBox.SelectedItem;

                this.ActionControlContentControl.Visibility = Visibility.Collapsed;
                if (selection.Equals(SingleActionCommandType))
                {
                    ActionTypeEnum actionType = EnumHelper.GetEnumValueFromString<ActionTypeEnum>(name);
                    this.ActionControlContentControl.AssignAction(actionType);
                    this.ActionControlContentControl.Visibility = Visibility.Visible;
                }
                else
                {
                    IEnumerable<CommandBase> commands = this.GetCommandsForType(selection);
                    this.Command = commands.FirstOrDefault(c => c.Name.Equals(name));
                }
            }
        }

        private IEnumerable<CommandBase> GetCommandsForType(string selection)
        {
            CommandTypeEnum commandType = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(selection);
            IEnumerable<CommandBase> commands = ChannelSession.AllEnabledCommands.Where(c => c.Type == commandType);
            if (commandType == CommandTypeEnum.Chat)
            {
                commands = commands.Where(c => !(c is PreMadeChatCommand));
            }
            return commands;
        }
    }
}
