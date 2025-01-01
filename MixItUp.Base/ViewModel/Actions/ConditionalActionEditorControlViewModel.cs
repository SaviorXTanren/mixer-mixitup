using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Actions
{
    public class ConditionalClauseViewModel : UIViewModelBase
    {
        public ICommand DeleteCommand { get; set; }

        private ConditionalActionEditorControlViewModel viewModel;

        public ConditionalClauseViewModel(ConditionalActionEditorControlViewModel viewModel)
        {
            this.viewModel = viewModel;

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.Clauses.Remove(this);
            });
        }

        public ConditionalClauseViewModel(ConditionalClauseModel clause, ConditionalActionEditorControlViewModel viewModel)
            : this(viewModel)
        {
            this.ComparisionType = clause.ComparisionType;
            this.Value1 = clause.Value1;
            this.Value2 = clause.Value2;
            this.Value3 = clause.Value3;
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

        public IEnumerable<ConditionalComparisionTypeEnum> ComparisonTypes { get { return EnumHelper.GetEnumList<ConditionalComparisionTypeEnum>(); } }

        public ConditionalComparisionTypeEnum ComparisionType
        {
            get { return this.comparisionType; }
            set
            {
                this.comparisionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsValue2Definable");
                this.NotifyPropertyChanged("IsNormalComparision");
                this.NotifyPropertyChanged("IsBetweenOperatorSelected");
                this.NotifyPropertyChanged("IsRegexMatch");
            }
        }
        private ConditionalComparisionTypeEnum comparisionType;

        public bool IsValue2Definable { get { return this.ComparisionType != ConditionalComparisionTypeEnum.Replaced && this.ComparisionType != ConditionalComparisionTypeEnum.NotReplaced; } }

        public bool IsNormalComparision { get { return !this.IsBetweenOperatorSelected && !this.IsRegexMatch; } }
        public bool IsBetweenOperatorSelected { get { return this.ComparisionType == ConditionalComparisionTypeEnum.Between; } }
        public bool IsRegexMatch { get { return this.ComparisionType == ConditionalComparisionTypeEnum.RegexMatch; } }

        public string Value1
        {
            get { return this.value1; }
            set
            {
                this.value1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value1 = string.Empty;

        public string Value2
        {
            get { return this.value2; }
            set
            {
                this.value2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value2 = string.Empty;

        public string Value3
        {
            get { return this.value3; }
            set
            {
                this.value3 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value3 = string.Empty;

        public bool Validate()
        {
            if (this.ComparisionType == ConditionalComparisionTypeEnum.Replaced || this.ComparisionType == ConditionalComparisionTypeEnum.NotReplaced)
            {
                return !string.IsNullOrEmpty(this.Value1);
            }
            else if (this.ComparisionType == ConditionalComparisionTypeEnum.Between)
            {
                return !string.IsNullOrEmpty(this.Value1) && !string.IsNullOrEmpty(this.Value2) && !string.IsNullOrEmpty(this.Value3);
            }
            else
            {
                return (!string.IsNullOrEmpty(this.Value1)) || (!string.IsNullOrEmpty(this.Value2));
            }
        }

        public ConditionalClauseModel GetModel() { return new ConditionalClauseModel(this.ComparisionType, this.Value1, this.Value2, this.Value3); }
    }

    public class ConditionalActionEditorControlViewModel : GroupActionEditorControlViewModel
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Conditional; } }

        public bool CaseSensitive
        {
            get { return this.caseSensitive; }
            set
            {
                this.caseSensitive = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool caseSensitive;

        public IEnumerable<ConditionalOperatorTypeEnum> OperatorTypes { get { return EnumHelper.GetEnumList<ConditionalOperatorTypeEnum>(); } }

        public ConditionalOperatorTypeEnum SelectedOperatorType
        {
            get { return this.selectedOperatorType; }
            set
            {
                this.selectedOperatorType = value;
                this.NotifyPropertyChanged();
            }
        }
        private ConditionalOperatorTypeEnum selectedOperatorType;

        public ICommand AddClauseCommand { get; private set; }

        public bool RepeatWhileTrue
        {
            get { return this.repeatWhileTrue; }
            set
            {
                this.repeatWhileTrue = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool repeatWhileTrue;

        public ObservableCollection<ConditionalClauseViewModel> Clauses { get; private set; } = new ObservableCollection<ConditionalClauseViewModel>();

        public ConditionalActionEditorControlViewModel(ConditionalActionModel action)
            : base(action)
        {
            this.CaseSensitive = action.CaseSensitive;
            this.SelectedOperatorType = action.Operator;
            this.RepeatWhileTrue = action.RepeatWhileTrue;

            this.Clauses.AddRange(action.Clauses.Select(c => new ConditionalClauseViewModel(c, this)));
        }

        public ConditionalActionEditorControlViewModel()
            : base()
        {
            this.Clauses.Add(new ConditionalClauseViewModel(this));
        }

        protected override async Task OnOpenInternal()
        {
            this.AddClauseCommand = this.CreateCommand(() =>
            {
                this.Clauses.Add(new ConditionalClauseViewModel(this));
            });

            await base.OnOpenInternal();
        }

        public override async Task<Result> Validate()
        {
            if (this.Clauses.Count == 0)
            {
                return new Result(MixItUp.Base.Resources.ConditionalActionAtLeastOneClause);
            }

            foreach (ConditionalClauseViewModel clause in this.Clauses)
            {
                if (!clause.Validate())
                {
                    return new Result(MixItUp.Base.Resources.ConditionalActionClauseMissingValue);
                }
            }
            return await base.Validate();
        }

        protected override async Task<ActionModelBase> GetActionInternal()
        {
            return new ConditionalActionModel(this.CaseSensitive, this.SelectedOperatorType, this.RepeatWhileTrue, this.Clauses.Select(c => c.GetModel()), await this.ActionEditorList.GetActions());
        }
    }
}
