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
    public class PolyPopVariableViewModel : UIViewModelBase
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

        private PolyPopActionEditorControlViewModel viewModel;

        public PolyPopVariableViewModel(PolyPopActionEditorControlViewModel viewModel, string name, string value)
            : this(viewModel)
        {
            this.Name = name;
            this.Value = value;
        }

        public PolyPopVariableViewModel(PolyPopActionEditorControlViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DeleteVariableCommand = this.CreateCommand(() =>
            {
                this.viewModel.Variables.Remove(this);
            });
        }
    }

    public class PolyPopActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.PolyPop; } }

        public bool PolyPopNotEnabled { get { return !ServiceManager.Get<PolyPopService>().IsConnected; } }

        public string AlertName
        {
            get { return this.alertName; }
            set
            {
                this.alertName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string alertName;

        public ICommand AddVariableCommand { get; private set; }

        public ObservableCollection<PolyPopVariableViewModel> Variables { get; private set; } = new ObservableCollection<PolyPopVariableViewModel>();

        public PolyPopActionEditorControlViewModel(PolyPopActionModel action)
            : base(action)
        {
            this.AlertName = action.AlertName;
            this.Variables.AddRange(action.Variables.Select(kvp => new PolyPopVariableViewModel(this, kvp.Key, kvp.Value)));
        }

        public PolyPopActionEditorControlViewModel() : base() { }

        protected override async Task OnOpenInternal()
        {
            this.AddVariableCommand = this.CreateCommand(() =>
            {
                this.Variables.Add(new PolyPopVariableViewModel(this));
            });
            await base.OnOpenInternal();
        }

        public override Task<Result> Validate()
        {
            foreach (PolyPopVariableViewModel variable in this.Variables)
            {
                if (string.IsNullOrEmpty(variable.Name))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.PolyPopActionMissingVariableName));
                }
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();
            foreach (PolyPopVariableViewModel variable in this.Variables)
            {
                variables.Add(variable.Name, variable.Value);
            }
            return Task.FromResult<ActionModelBase>(new PolyPopActionModel(this.AlertName, variables));
        }
    }
}
