using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class ExternalProgramActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.ExternalProgram; } }

        public string FilePath
        {
            get { return this.filePath; }
            set
            {
                this.filePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string filePath;

        public string Arguments
        {
            get { return this.arguments; }
            set
            {
                this.arguments = value;
                this.NotifyPropertyChanged();
            }
        }
        private string arguments;

        public bool ShowWindow
        {
            get { return this.showWindow; }
            set
            {
                this.showWindow = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool showWindow;

        public bool ShellExecute
        {
            get { return this.shellExecute; }
            set
            {
                this.shellExecute = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool shellExecute;

        public bool WaitForFinish
        {
            get { return this.waitForFinish; }
            set
            {
                this.waitForFinish = value;
                this.NotifyPropertyChanged();

                if (!this.WaitForFinish)
                {
                    this.SaveOutput = false;
                }
            }
        }
        private bool waitForFinish;

        public bool SaveOutput
        {
            get { return this.saveOutput; }
            set
            {
                this.saveOutput = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool saveOutput;

        public ExternalProgramActionEditorControlViewModel(ExternalProgramActionModel action)
            : base(action)
        {
            this.FilePath = action.FilePath;
            this.Arguments = action.Arguments;
            this.showWindow = action.ShowWindow;
            this.ShellExecute = action.ShellExecute;
            this.WaitForFinish = action.WaitForFinish;
            this.SaveOutput = action.SaveOutput;
        }

        public ExternalProgramActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ExternalProgramActionMissingFilePath));
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal() { return Task.FromResult<ActionModelBase>(new ExternalProgramActionModel(this.FilePath, this.Arguments, this.ShowWindow, this.ShellExecute, this.WaitForFinish, this.SaveOutput)); }
    }
}
