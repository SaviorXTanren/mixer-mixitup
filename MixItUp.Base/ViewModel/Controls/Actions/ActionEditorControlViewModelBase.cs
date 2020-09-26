using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public abstract class ActionEditorControlViewModelBase : UIViewModelBase
    {
        public abstract ActionTypeEnum Type { get; }

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

        public ICommand PlayCommand { get; private set; }
        public ICommand MoveUpCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand HelpCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                this.enabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool enabled = true;

        public ActionEditorControlViewModelBase(ActionModelBase action)
        {
            this.Name = action.Name;
            this.Enabled = action.Enabled;
        }

        public ActionEditorControlViewModelBase() { }

        protected override Task OnLoadedInternal()
        {
            this.PlayCommand = this.CreateCommand((parameter) =>
            {
                return Task.FromResult(0);
            });

            this.MoveUpCommand = this.CreateCommand((parameter) =>
            {
                return Task.FromResult(0);
            });

            this.MoveDownCommand = this.CreateCommand((parameter) =>
            {
                return Task.FromResult(0);
            });

            this.CopyCommand = this.CreateCommand((parameter) =>
            {
                return Task.FromResult(0);
            });

            this.HelpCommand = this.CreateCommand((parameter) =>
            {
                return Task.FromResult(0);
            });

            this.DeleteCommand = this.CreateCommand((parameter) =>
            {
                return Task.FromResult(0);
            });

            return Task.FromResult(0);
        }

        public virtual Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public abstract Task<ActionModelBase> GetAction();
    }
}
