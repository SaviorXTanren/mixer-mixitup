using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
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

        public IEnumerable<ConditionalComparisionTypeEnum> ComparisonTypes 
        { 
            get
            {
                var comparisonTypes = System.Enum.GetValues(typeof(ConditionalComparisionTypeEnum))
                    .Cast<ConditionalComparisionTypeEnum>()
                    .Where(v => !EnumHelper.IsObsolete(v));
                return comparisonTypes;
            }
        }

        public ConditionalComparisionTypeEnum ComparisionType
        {
            get { return this.Clause.ComparisionType; }
            set
            {
                this.Clause.ComparisionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsBetweenOperatorSelected");
                this.NotifyPropertyChanged("IsBetweenOperatorNotSelected");
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
        private const string SingleActionCommandType = "SingleAction";

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

        private ContentControl ActionControlContentControl;
        private ActionContentContainerControl actionContentContainerControl;

        public ConditionalActionControl() : base() { InitializeComponent(); }

        public ConditionalActionControl(ConditionalAction action) : this()
        {
            this.action = action;
        }

        public override Task OnLoaded()
        {
            this.ActionControlContentControl = (ContentControl)this.GetByUid("ActionControlContentControl");

            var commandTypeStrings = System.Enum.GetValues(typeof(CommandTypeEnum))
                    .Cast<CommandTypeEnum>()
                    .Where(v => !EnumHelper.IsObsolete(v) && v != CommandTypeEnum.Game && v != CommandTypeEnum.Remote && v != CommandTypeEnum.Custom)
                    .Select(v => EnumHelper.GetEnumName(v))
                    .ToList();

            commandTypeStrings.Add("SingleAction");
            this.CommandTypeComboBox.ItemsSource = commandTypeStrings;

            var actionTypes = System.Enum.GetValues(typeof(ActionTypeEnum))
                    .Cast<ActionTypeEnum>()
                    .Where(v => !EnumHelper.IsObsolete(v) && v != ActionTypeEnum.Custom);

            this.SingleActionNameComboBox.ItemsSource = actionTypes;

            var operatorTypes = System.Enum.GetValues(typeof(ConditionalOperatorTypeEnum))
                    .Cast<ConditionalOperatorTypeEnum>()
                    .Where(v => !EnumHelper.IsObsolete(v));

            this.OperatorTypeComboBox.ItemsSource = operatorTypes;
            if (this.action != null)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (this.action.Clauses.Count == 0 && !string.IsNullOrEmpty(this.action.Value1))
                {
                    StoreCommandUpgrader.UpdateConditionalAction(new List<ActionBase>() { this.action });
                }
#pragma warning restore CS0612 // Type or member is obsolete

                this.CaseSensitiveToggleButton.IsChecked = !this.action.IgnoreCase;
                this.OperatorTypeComboBox.SelectedItem = this.action.Operator;
                foreach (ConditionalClauseModel clause in this.action.Clauses)
                {
                    this.Clauses.Add(new ConditionalClauseViewModel(clause, this));
                }
                this.Clauses.First().CanBeRemoved = false;
                this.Command = this.action.GetCommand();

                if (this.action.Action != null)
                {
                    this.CommandTypeComboBox.SelectedItem = SingleActionCommandType;
                    this.SingleActionNameComboBox.SelectedItem = this.action.Action.Type;
                    this.ActionControlContentControl.Visibility = Visibility.Visible;
                    this.ActionControlContentControl.Content = this.actionContentContainerControl = new ActionContentContainerControl(this.action.Action);
                }
            }
            else
            {
                this.OperatorTypeComboBox.SelectedItem = ConditionalOperatorTypeEnum.And;
                this.Clauses.Add(new ConditionalClauseViewModel(new ConditionalClauseModel(), this) { CanBeRemoved = false });
            }
            this.ClausesItemsControl.ItemsSource = this.Clauses;

            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.OperatorTypeComboBox.SelectedIndex >= 0 && this.Clauses.Count >= 0)
            {
                ConditionalOperatorTypeEnum op = (ConditionalOperatorTypeEnum)this.OperatorTypeComboBox.SelectedItem;
                if (this.Clauses.All(c => c.Validate()))
                {
                    if (this.Command != null)
                    {
                        return new ConditionalAction(!this.CaseSensitiveToggleButton.IsChecked.GetValueOrDefault(), op, this.Clauses.Select(c => c.Clause), this.Command);
                    }
                    else
                    {
                        if (this.actionContentContainerControl != null)
                        {
                            ActionBase action = this.actionContentContainerControl.GetAction();
                            if (action != null)
                            {
                                return new ConditionalAction(!this.CaseSensitiveToggleButton.IsChecked.GetValueOrDefault(), op, this.Clauses.Select(c => c.Clause), action);
                            }
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
            this.CommandNameComboBox.Visibility = Visibility.Collapsed;
            this.SingleActionNameComboBox.Visibility = Visibility.Collapsed;
            this.ActionControlContentControl.Visibility = Visibility.Collapsed;

            if (this.CommandTypeComboBox.SelectedIndex >= 0)
            {
                string selection = (string)this.CommandTypeComboBox.SelectedItem;
                if (selection.Equals(SingleActionCommandType))
                {
                    this.Command = null;

                    this.SingleActionNameComboBox.SelectedIndex = -1;
                    this.SingleActionNameComboBox.Visibility = Visibility.Visible;
                }
                else
                {
                    this.SingleActionNameComboBox.SelectedIndex = -1;
                    this.ActionControlContentControl.Content = this.actionContentContainerControl = null;

                    CommandTypeEnum commandType = EnumHelper.GetEnumValueFromString<CommandTypeEnum>((string)this.CommandTypeComboBox.SelectedItem);
                    IEnumerable<CommandBase> commands = ChannelSession.AllEnabledCommands.Where(c => c.Type == commandType);
                    if (commandType == CommandTypeEnum.Chat)
                    {
                        commands = commands.Where(c => !(c is PreMadeChatCommand));
                    }
                    this.CommandNameComboBox.ItemsSource = commands.OrderBy(c => c.Name);
                    this.CommandNameComboBox.Visibility = Visibility.Visible;
                }
            }
        }

        private void CommandNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CommandNameComboBox.SelectedIndex >= 0 && this.Command != this.CommandNameComboBox.SelectedItem)
            {
                this.Command = (CommandBase)this.CommandNameComboBox.SelectedItem;
            }
        }

        private void SingleActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SingleActionNameComboBox.SelectedIndex >= 0)
            {
                ActionTypeEnum actionType = (ActionTypeEnum)this.SingleActionNameComboBox.SelectedItem;
                this.ActionControlContentControl.Visibility = Visibility.Visible;
                this.ActionControlContentControl.Content = this.actionContentContainerControl = new ActionContentContainerControl(actionType);
            }
        }
    }
}
