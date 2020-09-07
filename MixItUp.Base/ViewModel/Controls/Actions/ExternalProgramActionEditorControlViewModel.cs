using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
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

        public bool WaitForFinish
        {
            get { return this.waitForFinish; }
            set
            {
                this.waitForFinish = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool waitForFinish;

        public ExternalProgramActionEditorControlViewModel(ExternalProgramActionModel action)
        {
            this.FilePath = action.FilePath;
            this.Arguments = action.Arguments;
            this.showWindow = action.ShowWindow;
            this.WaitForFinish = action.WaitForFinish;
        }

        public ExternalProgramActionEditorControlViewModel() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ExternalProgramActionMissingFilePath));
            }
            return Task.FromResult(new Result());
        }

        public override Task<ActionModelBase> GetAction() { return Task.FromResult<ActionModelBase>(new ExternalProgramActionModel(this.FilePath, this.Arguments, this.ShowWindow, this.WaitForFinish)); }
    }
}
