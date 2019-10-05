using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        public ObservableCollection<ConditionalClauseViewModel> Clauses { get; set; } = new ObservableCollection<ConditionalClauseViewModel>();

        private ConditionalAction action;

        public ConditionalActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public ConditionalActionControl(ActionContainerControl containerControl, ConditionalAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.OperatorTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<ConditionalOperatorTypeEnum>();
            if (this.action != null)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (this.action.Clauses.Count == 0 && !string.IsNullOrEmpty(this.action.Value1))
                {
                    StoreCommandUpgrader.UpdateConditionalAction(new List<ActionBase>() { this.action });
                }
#pragma warning restore CS0612 // Type or member is obsolete

                this.IgnoreCasingToggleButton.IsChecked = this.action.IgnoreCase;
                this.OperatorTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.Operator);
                foreach (ConditionalClauseModel clause in this.action.Clauses)
                {
                    this.Clauses.Add(new ConditionalClauseViewModel(clause, this));
                }
                this.Clauses.First().CanBeRemoved = false;
                this.CommandReference.Command = this.action.GetCommand();
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
            if (this.OperatorTypeComboBox.SelectedIndex >= 0 && this.Clauses.Count >= 0 && this.CommandReference.Command != null)
            {
                ConditionalOperatorTypeEnum op = EnumHelper.GetEnumValueFromString<ConditionalOperatorTypeEnum>((string)this.OperatorTypeComboBox.SelectedItem);
                if (this.Clauses.All(c => c.Validate()))
                {
                    return new ConditionalAction(this.IgnoreCasingToggleButton.IsChecked.GetValueOrDefault(), op, this.Clauses.Select(c => c.Clause), this.CommandReference.Command);
                }
            }
            return null;
        }

        private void AddClauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Clauses.Add(new ConditionalClauseViewModel(new ConditionalClauseModel(), this));
        }
    }
}
