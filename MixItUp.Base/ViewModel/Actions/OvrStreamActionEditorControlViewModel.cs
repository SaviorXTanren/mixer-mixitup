using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Actions
{
    public class OvrStreamVariableViewModel : UIViewModelBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public string Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value;

        public ICommand DeleteVariableCommand { get; private set; }

        public ObservableCollection<OvrStreamVariable> KnownVariables { get { return this.viewModel.KnownVariables; } set { } }

        private OvrStreamActionEditorControlViewModel viewModel;

        public OvrStreamVariableViewModel(OvrStreamActionEditorControlViewModel viewModel, string name, string value)
            : this(viewModel)
        {
            this.Name = name;
            this.Value = value;
        }

        public OvrStreamVariableViewModel(OvrStreamActionEditorControlViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DeleteVariableCommand = this.CreateCommand(() =>
            {
                this.viewModel.Variables.Remove(this);
            });
        }
    }

    public class OvrStreamActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.OvrStream; } }

        public IEnumerable<OvrStreamActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<OvrStreamActionTypeEnum>(); } }

        public OvrStreamActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowVariablesGrid");
            }
        }
        private OvrStreamActionTypeEnum selectedActionType;

        public bool OvrStreamNotEnabled { get { return !ServiceManager.Get<IOvrStreamService>().IsConnected; } }

        public ObservableCollection<OvrStreamTitle> Titles { get; private set; } = new ObservableCollection<OvrStreamTitle>();

        public OvrStreamTitle SelectedTitle
        {
            get { return this.selectedTitle; }
            set
            {
                this.selectedTitle = value;
                this.NotifyPropertyChanged();


                if (this.SelectedTitle != null)
                {
                    this.KnownVariables.ClearAndAddRange(this.SelectedTitle.Variables);
                }
                else
                {
                    this.KnownVariables.Clear();
                }
            }
        }
        private OvrStreamTitle selectedTitle;

        public string TitleName
        {
            get { return this.titleName; }
            set
            {
                this.titleName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string titleName;

        public bool ShowVariablesGrid { get { return this.SelectedActionType == OvrStreamActionTypeEnum.PlayTitle || this.SelectedActionType == OvrStreamActionTypeEnum.UpdateVariables; } }

        public ObservableCollection<OvrStreamVariable> KnownVariables { get; private set; } = new ObservableCollection<OvrStreamVariable>();

        public ICommand AddVariableCommand { get; private set; }

        public ObservableCollection<OvrStreamVariableViewModel> Variables { get; private set; } = new ObservableCollection<OvrStreamVariableViewModel>();

        public OvrStreamActionEditorControlViewModel(OvrStreamActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            this.TitleName = action.TitleName;
            if (this.ShowVariablesGrid)
            {
                this.Variables.AddRange(action.Variables.Select(kvp => new OvrStreamVariableViewModel(this, kvp.Key, kvp.Value)));
            }
        }

        public OvrStreamActionEditorControlViewModel() : base() { }

        protected override async Task OnOpenInternal()
        {
            this.AddVariableCommand = this.CreateCommand(() =>
            {
                this.Variables.Add(new OvrStreamVariableViewModel(this));
            });

            if (ServiceManager.Get<IOvrStreamService>().IsConnected)
            {
                IEnumerable<OvrStreamTitle> titles = await ServiceManager.Get<IOvrStreamService>().GetTitles();
                if (titles != null)
                {
                    this.Titles.AddRange(titles);
                }
            }
            await base.OnOpenInternal();
        }

        public override Task<Result> Validate()
        {
            if (this.ShowVariablesGrid)
            {
                foreach (OvrStreamVariableViewModel variable in this.Variables)
                {
                    if (string.IsNullOrEmpty(variable.Name))
                    {
                        return Task.FromResult(new Result(MixItUp.Base.Resources.OvrStreamActionMissingVariableName));
                    }
                }
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowVariablesGrid)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>();
                foreach (OvrStreamVariableViewModel variable in this.Variables)
                {
                    variables.Add(variable.Name, variable.Value);
                }
                return Task.FromResult<ActionModelBase>(new OvrStreamActionModel(this.SelectedActionType, this.TitleName, variables));
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new OvrStreamActionModel(this.SelectedActionType, this.TitleName));
            }
        }
    }
}