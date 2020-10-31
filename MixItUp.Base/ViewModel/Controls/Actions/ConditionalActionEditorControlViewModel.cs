using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public class ConditionalClauseViewModel : UIViewModelBase
    {
        public ConditionalClauseModel Clause { get; set; }

        public ICommand DeleteCommand { get; set; }

        private ConditionalActionEditorControlViewModel viewModel;

        public ConditionalClauseViewModel(ConditionalActionEditorControlViewModel viewModel) : this(new ConditionalClauseModel(), viewModel) { }

        public ConditionalClauseViewModel(ConditionalClauseModel clause, ConditionalActionEditorControlViewModel viewModel)
        {
            this.Clause = clause;
            this.viewModel = viewModel;

            this.DeleteCommand = this.CreateCommand((parameter) =>
            {
                this.viewModel.Clauses.Remove(this);
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

        public IEnumerable<ConditionalComparisionTypeEnum> ComparisonTypes { get { return EnumHelper.GetEnumList<ConditionalComparisionTypeEnum>(); } }

        public ConditionalComparisionTypeEnum ComparisionType
        {
            get { return this.Clause.ComparisionType; }
            set
            {
                this.Clause.ComparisionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsValue2Definable");
                this.NotifyPropertyChanged("IsBetweenOperatorSelected");
                this.NotifyPropertyChanged("IsBetweenOperatorNotSelected");
            }
        }

        public bool IsValue2Definable { get { return this.ComparisionType != ConditionalComparisionTypeEnum.Replaced && this.ComparisionType != ConditionalComparisionTypeEnum.NotReplaced; } }

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
    }

    public class ConditionalActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        private const string SingleActionCommandType = "SingleAction";

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

        public ObservableCollection<ConditionalClauseViewModel> Clauses { get; private set; } = new ObservableCollection<ConditionalClauseViewModel>();

        public IEnumerable<string> SuccessTriggerTypes
        {
            get
            {
                List<CommandTypeEnum> commandTypes = new List<CommandTypeEnum>(EnumHelper.GetEnumList<CommandTypeEnum>());
                commandTypes.Remove(CommandTypeEnum.Custom);

                List<string> results = new List<string>(EnumHelper.GetEnumNames(commandTypes));
                results.Sort();
                results.Add(SingleActionCommandType);

                return results;
            }
        }

        public string SelectedSuccessTriggerType
        {
            get { return this.selectedSuccessTriggerType; }
            set
            {
                this.selectedSuccessTriggerType = value;

                this.SelectedCommand = null;

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowCommands");
                this.NotifyPropertyChanged("Commands");
                this.NotifyPropertyChanged("SelectedCommand");
                this.NotifyPropertyChanged("ShowActions");
                this.NotifyPropertyChanged("Actions");
                this.NotifyPropertyChanged("SelectedAction");
            }
        }
        private string selectedSuccessTriggerType;

        public bool ShowCommands { get { return !string.Equals(this.SelectedSuccessTriggerType, SingleActionCommandType); } }

        public IEnumerable<CommandModelBase> Commands
        {
            get
            {
                List<CommandModelBase> commands = new List<CommandModelBase>();
                if (this.ShowCommands)
                {
                    CommandTypeEnum commandType = EnumHelper.GetEnumValueFromString<CommandTypeEnum>(this.SelectedSuccessTriggerType);
                    if (commandType == CommandTypeEnum.PreMade)
                    {
                        return ChannelSession.PreMadeChatCommands.OrderBy(c => c.Name);
                    }
                    else
                    {
                        return ChannelSession.AllCommands.Where(c => c.Type == commandType).OrderBy(c => c.Name);
                    }
                }
                return commands;
            }
        }

        public CommandModelBase SelectedCommand
        {
            get { return this.selectedCommand; }
            set
            {
                this.selectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase selectedCommand;

        public bool ShowActions { get { return !this.ShowCommands; } }

        public IEnumerable<ActionTypeEnum> Actions
        {
            get
            {
                List<ActionTypeEnum> actions = new List<ActionTypeEnum>(EnumHelper.GetEnumList<ActionTypeEnum>());
                actions.Remove(ActionTypeEnum.Custom);
                return actions;
            }
        }

        public ActionTypeEnum SelectedAction
        {
            get { return this.selectedAction; }
            set
            {
                this.selectedAction = value;
                this.NotifyPropertyChanged();
            }
        }
        private ActionTypeEnum selectedAction;

        public ConditionalActionEditorControlViewModel(ConditionalActionModel action)
            : base(action)
        {
            this.CaseSensitive = action.CaseSensitive;
            this.SelectedOperatorType = action.Operator;

            foreach (ConditionalClauseModel clause in action.Clauses)
            {
                this.Clauses.Add(new ConditionalClauseViewModel(clause, this));
            }

            if (action.CommandID != Guid.Empty)
            {
                // TODO
            }
            else if (action.Action != null)
            {
                this.SelectedAction = action.Action.Type;
                // TODO
            }
        }

        public ConditionalActionEditorControlViewModel()
            : base()
        {
            this.Clauses.Add(new ConditionalClauseViewModel(this));
        }

        protected override async Task OnLoadedInternal()
        {
            this.AddClauseCommand = this.CreateCommand((parameter) =>
            {
                this.Clauses.Add(new ConditionalClauseViewModel(this));
                return Task.FromResult(0);
            });
            await base.OnLoadedInternal();
        }

        public override Task<Result> Validate()
        {
            foreach (ConditionalClauseViewModel clause in this.Clauses)
            {
                if (!clause.Validate())
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.ConditionalActionClauseMissingValue));
                }
            }

            if (string.IsNullOrEmpty(this.SelectedSuccessTriggerType))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ConditionalActionMissingSuccessTrigger));
            }

            if (this.ShowCommands)
            {
                // TODO
            }
            else if (this.ShowActions)
            {
                // TODO
                return Task.FromResult(new Result(MixItUp.Base.Resources.ConditionalActionSuccessTriggerActionHasErrors));
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowCommands)
            {
                // TODO
            }
            else if (this.ShowActions)
            {
                // TODO
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
