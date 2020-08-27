using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.Actions;
using MixItUp.Base.ViewModel.Requirements;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Commands
{
    public abstract class CommandEditorWindowViewModelBase : WindowViewModelBase
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

        public RequirementsSetViewModel Requirements { get; set; } = new RequirementsSetViewModel();

        public ObservableCollection<ActionEditorControlViewModelBase> Actions { get; set; } = new ObservableCollection<ActionEditorControlViewModelBase>();

        public ObservableCollection<ActionTypeEnum> ActionTypes { get; set; } = new ObservableCollection<ActionTypeEnum>();
        public ActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
            }
        }
        private ActionTypeEnum selectedActionType;
        public ICommand AddCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        private CommandModelBase existingCommand;

        public CommandEditorWindowViewModelBase(CommandModelBase existingCommand)
        {
            this.existingCommand = existingCommand;

            this.Name = this.existingCommand.Name;
            
            foreach (ActionModelBase action in this.existingCommand.Actions)
            {
                switch (action.Type)
                {
                    case ActionTypeEnum.Chat: this.Actions.Add(new ChatActionEditorControlViewModel((ChatActionModel)action)); break;
                }
            }
        }

        public CommandEditorWindowViewModelBase()
        {
            List<ActionTypeEnum> actionTypes = new List<ActionTypeEnum>(EnumHelper.GetEnumList<ActionTypeEnum>());
            actionTypes.Remove(ActionTypeEnum.Custom);
            foreach (ActionTypeEnum hiddenActions in ChannelSession.Settings.ActionsToHide)
            {
                actionTypes.Remove(hiddenActions);
            }

            foreach (ActionTypeEnum actionType in actionTypes.OrderBy(a => a.ToString()))
            {
                this.ActionTypes.Add(actionType);
            }

            this.AddCommand = this.CreateCommand((parameter) =>
            {
                if (this.ActionTypes.Contains(this.SelectedActionType))
                {
                    switch (this.SelectedActionType)
                    {
                        case ActionTypeEnum.Chat: this.Actions.Add(new ChatActionEditorControlViewModel()); break;
                    }
                }
                return Task.FromResult(0);
            });

            this.SaveCommand = this.CreateCommand(async (parameter) =>
            {
                List<Result> results = new List<Result>();

                results.Add(await this.Validate());
                results.AddRange(await this.Requirements.Validate());
                foreach (ActionEditorControlViewModelBase action in this.Actions)
                {
                    results.Add(await action.Validate());
                }

                if (results.Any(r => !r.Success))
                {
                    StringBuilder error = new StringBuilder();
                    error.AppendLine(MixItUp.Base.Resources.TheFollowingErrorsMustBeFixed);
                    error.AppendLine();
                    foreach (Result result in results)
                    {
                        if (!result.Success)
                        {
                            error.AppendLine(" - " + result.Message);
                        }
                    }
                    await DialogHelper.ShowMessage(error.ToString());
                    return;
                }

                await this.Save();
            });
        }

        public abstract Task<Result> Validate();

        public abstract Task Save();
    }
}
