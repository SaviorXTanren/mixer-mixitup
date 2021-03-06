using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ThreadSafeObservableCollection<OvrStreamVariable> KnownVariables { get { return this.viewModel.KnownVariables; } set { } }

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
            this.DeleteVariableCommand = this.CreateCommand((parameter) =>
            {
                this.viewModel.Variables.Remove(this);
                return Task.FromResult(0);
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

        public bool OvrStreamNotEnabled { get { return !ChannelSession.Services.OvrStream.IsConnected; } }

        public ThreadSafeObservableCollection<OvrStreamTitle> Titles { get; private set; } = new ThreadSafeObservableCollection<OvrStreamTitle>();

        public OvrStreamTitle SelectedTitle
        {
            get { return this.selectedTitle; }
            set
            {
                this.selectedTitle = value;
                this.NotifyPropertyChanged();

                this.KnownVariables.Clear();
                if (this.SelectedTitle != null)
                {
                    foreach (OvrStreamVariable variable in this.SelectedTitle.Variables)
                    {
                        this.KnownVariables.Add(variable);
                    }
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

        public ThreadSafeObservableCollection<OvrStreamVariable> KnownVariables { get; private set; } = new ThreadSafeObservableCollection<OvrStreamVariable>();

        public ICommand AddVariableCommand { get; private set; }

        public ThreadSafeObservableCollection<OvrStreamVariableViewModel> Variables { get; private set; } = new ThreadSafeObservableCollection<OvrStreamVariableViewModel>();

        public OvrStreamActionEditorControlViewModel(OvrStreamActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            this.TitleName = action.TitleName;
            if (this.ShowVariablesGrid)
            {
                foreach (var kvp in action.Variables)
                {
                    this.Variables.Add(new OvrStreamVariableViewModel(this, kvp.Key, kvp.Value));
                }
            }
        }

        public OvrStreamActionEditorControlViewModel() : base() { }

        protected override async Task OnLoadedInternal()
        {
            this.AddVariableCommand = this.CreateCommand((parameter) =>
            {
                this.Variables.Add(new OvrStreamVariableViewModel(this));
                return Task.FromResult(0);
            });

            if (ChannelSession.Services.OvrStream.IsConnected)
            {
                IEnumerable<OvrStreamTitle> titles = await ChannelSession.Services.OvrStream.GetTitles();
                if (titles != null)
                {
                    foreach (OvrStreamTitle title in titles)
                    {
                        this.Titles.Add(title);
                    }
                }
            }
            await base.OnLoadedInternal();
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
