using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
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
                this.NotifyPropertyChanged("IsBetweenOperatorSelected");
                this.NotifyPropertyChanged("IsBetweenOperatorNotSelected");
            }
        }
        private ConditionalComparisionTypeEnum comparisionType;

        public bool IsValue2Definable { get { return this.ComparisionType != ConditionalComparisionTypeEnum.Replaced && this.ComparisionType != ConditionalComparisionTypeEnum.NotReplaced; } }

        public bool IsBetweenOperatorSelected { get { return this.ComparisionType == ConditionalComparisionTypeEnum.Between; } }
        public bool IsBetweenOperatorNotSelected { get { return !this.IsBetweenOperatorSelected; } }

        public string Value1
        {
            get { return this.value1; }
            set
            {
                this.value1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value1;

        public string Value2
        {
            get { return this.value2; }
            set
            {
                this.value2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value2;

        public string Value3
        {
            get { return this.value3; }
            set
            {
                this.value3 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value3;

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
                return !(string.IsNullOrEmpty(this.Value1) && string.IsNullOrEmpty(this.Value2));
            }
        }

        public ConditionalClauseModel GetModel() { return new ConditionalClauseModel(this.ComparisionType, this.Value1, this.Value2, this.Value3); }
    }

    public class ConditionalActionEditorControlViewModel : ActionEditorControlViewModelBase
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

        public ThreadSafeObservableCollection<ConditionalClauseViewModel> Clauses { get; private set; } = new ThreadSafeObservableCollection<ConditionalClauseViewModel>();

        public ICommand ImportActionsCommand { get; private set; }

        public ActionEditorListControlViewModel ActionEditorList { get; set; } = new ActionEditorListControlViewModel();

        private List<ActionModelBase> subActions = new List<ActionModelBase>();

        public ConditionalActionEditorControlViewModel(ConditionalActionModel action)
            : base(action)
        {
            this.CaseSensitive = action.CaseSensitive;
            this.SelectedOperatorType = action.Operator;

            this.Clauses.AddRange(action.Clauses.Select(c => new ConditionalClauseViewModel(c, this)));

            foreach (ActionModelBase subAction in action.Actions)
            {
                subActions.Add(subAction);
            }
        }

        public ConditionalActionEditorControlViewModel()
            : base()
        {
            this.Clauses.Add(new ConditionalClauseViewModel(this));
        }

        protected override async Task OnLoadedInternal()
        {
            this.AddClauseCommand = this.CreateCommand(() =>
            {
                this.Clauses.Add(new ConditionalClauseViewModel(this));
            });

            this.ImportActionsCommand = this.CreateCommand(async () =>
            {
                await this.ImportActionsFromCommand(await CommandEditorWindowViewModelBase.ImportCommandFromFile());
            });

            foreach (ActionModelBase subAction in subActions)
            {
                await this.ActionEditorList.AddAction(subAction);
            }
            subActions.Clear();

            await base.OnLoadedInternal();
        }

        public override async Task<Result> Validate()
        {
            foreach (ConditionalClauseViewModel clause in this.Clauses)
            {
                if (!clause.Validate())
                {
                    return new Result(MixItUp.Base.Resources.ConditionalActionClauseMissingValue);
                }
            }

            IEnumerable<Result> results = await this.ActionEditorList.ValidateActions();
            if (results.Any(r => !r.Success))
            {
                return new Result(results.Where(r => !r.Success));
            }
            return new Result();
        }

        public async Task ImportActionsFromCommand(CommandModelBase command)
        {
            if (command != null)
            {
                foreach (ActionModelBase action in command.Actions)
                {
                    await this.ActionEditorList.AddAction(action);
                }
            }
        }

        protected override async Task<ActionModelBase> GetActionInternal()
        {
            return new ConditionalActionModel(this.CaseSensitive, this.SelectedOperatorType, this.Clauses.Select(c => c.GetModel()), await this.ActionEditorList.GetActions());
        }
    }
}
