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
    public class SAMMIVariableViewModel : UIViewModelBase
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ICommand DeleteVariableCommand { get; private set; }

        private SAMMIActionEditorControlViewModel viewModel;

        public SAMMIVariableViewModel(string name, string value, SAMMIActionEditorControlViewModel viewModel)
            : this(viewModel)
        {
            this.Name = name;
            this.Value = value;
        }

        public SAMMIVariableViewModel(SAMMIActionEditorControlViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DeleteVariableCommand = this.CreateCommand(() =>
            {
                this.viewModel.ButtonVariables.Remove(this);
            });
        }
    }

    public class SAMMIActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.SAMMI; } }

        public IEnumerable<SAMMIActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<SAMMIActionTypeEnum>(); } }

        public SAMMIActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();

                this.NotifyPropertyChanged(nameof(this.ShowButtonGrid));
                this.NotifyPropertyChanged(nameof(this.ShowGlobalVariableGrid));
            }
        }
        private SAMMIActionTypeEnum selectedActionType = SAMMIActionTypeEnum.TriggerButton;

        public bool ShowButtonGrid { get { return this.SelectedActionType == SAMMIActionTypeEnum.TriggerButton || this.SelectedActionType == SAMMIActionTypeEnum.ReleaseButton; } }

        public string ButtonID
        {
            get { return this.buttonID; }
            set
            {
                this.buttonID = value;
                this.NotifyPropertyChanged();
            }
        }
        private string buttonID;

        public ObservableCollection<SAMMIVariableViewModel> ButtonVariables { get; private set; } = new ObservableCollection<SAMMIVariableViewModel>();

        public bool ShowGlobalVariableGrid { get { return this.SelectedActionType == SAMMIActionTypeEnum.SetGlobalVariable; } }

        public string GlobalVariableName
        {
            get { return this.globalVariableName; }
            set
            {
                this.globalVariableName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string globalVariableName;

        public string GlobalVariableValue
        {
            get { return this.globalVariableValue; }
            set
            {
                this.globalVariableValue = value;
                this.NotifyPropertyChanged();
            }
        }
        private string globalVariableValue;

        public ICommand AddVariableCommand { get; private set; }

        public SAMMIActionEditorControlViewModel(SAMMIActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowButtonGrid)
            {
                this.ButtonID = action.ButtonID;
                foreach (var kvp in action.ButtonVariables)
                {
                    this.ButtonVariables.Add(new SAMMIVariableViewModel(kvp.Key, kvp.Value, this));
                }
            }
            else if (this.ShowGlobalVariableGrid)
            {
                this.GlobalVariableName = action.GlobalVariableName;
                this.GlobalVariableValue = action.GlobalVariableValue;
            }

            this.Initialize();
        }

        public SAMMIActionEditorControlViewModel() : base() { this.Initialize(); }

        public override Task<Result> Validate()
        {
            if (this.ShowButtonGrid)
            {
                if (string.IsNullOrWhiteSpace(this.ButtonID))
                {
                    return Task.FromResult<Result>(new Result(Resources.SAMMIActionMissingButtonID));
                }

                foreach (SAMMIVariableViewModel variable in this.ButtonVariables)
                {
                    if (string.IsNullOrWhiteSpace(variable.Name) || string.IsNullOrWhiteSpace(variable.Value))
                    {
                        return Task.FromResult<Result>(new Result(Resources.SAMMIActionMissingVariable));
                    }
                }
            }
            else if (this.ShowGlobalVariableGrid)
            {
                if (string.IsNullOrWhiteSpace(this.GlobalVariableName) || string.IsNullOrWhiteSpace(this.GlobalVariableValue))
                {
                    return Task.FromResult<Result>(new Result(Resources.SAMMIActionMissingVariable));
                }
            }
            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowButtonGrid)
            {
                return Task.FromResult<ActionModelBase>(new SAMMIActionModel(this.SelectedActionType, this.ButtonID, this.ButtonVariables.ToDictionary(v => v.Name, v => v.Value)));
            }
            else if (this.ShowGlobalVariableGrid)
            {
                return Task.FromResult<ActionModelBase>(new SAMMIActionModel(this.SelectedActionType, this.GlobalVariableName, this.GlobalVariableValue));
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        private void Initialize()
        {
            this.AddVariableCommand = this.CreateCommand(() =>
            {
                this.ButtonVariables.Add(new SAMMIVariableViewModel(this));
            });
        }
    }
}
