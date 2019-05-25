using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class ActionGroupMainControlViewModel : MainControlViewModelBase
    {
        public ObservableCollection<CommandGroupControlViewModel> ActionGroupCommands { get; private set; } = new ObservableCollection<CommandGroupControlViewModel>();

        public string NameFilter
        {
            get { return this.nameFilter; }
            set
            {
                this.nameFilter = value;
                this.NotifyPropertyChanged();

                foreach (CommandGroupControlViewModel group in this.ActionGroupCommands)
                {
                    group.RefreshCommands(this.NameFilter.ToLower());
                }
            }
        }
        private string nameFilter;

        public ActionGroupMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void AddCommand(ActionGroupCommand command)
        {
            foreach (CommandGroupControlViewModel group in this.ActionGroupCommands)
            {
                if (string.Equals(group.GroupName, command.GroupName))
                {
                    group.AddCommand(command);
                    return;
                }
            }

            CommandGroupSettings groupSettings = null;
            if (!string.IsNullOrEmpty(command.GroupName) && ChannelSession.Settings.CommandGroups.ContainsKey(command.GroupName))
            {
                groupSettings = ChannelSession.Settings.CommandGroups[command.GroupName];
            }
            this.ActionGroupCommands.Add(new CommandGroupControlViewModel(groupSettings, new List<CommandBase>() { command }));
        }

        public void RemoveCommand(ActionGroupCommand command)
        {
            foreach (CommandGroupControlViewModel group in this.ActionGroupCommands)
            {
                group.RemoveCommand(command);
            }
        }

        protected override Task OnLoadedInternal()
        {
            this.FullRefresh();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            return base.OnVisibleInternal();
        }

        private void FullRefresh()
        {
            this.ActionGroupCommands.Clear();
            IEnumerable<ActionGroupCommand> commands = ChannelSession.Settings.ActionGroupCommands.ToList();
            foreach (var group in commands.GroupBy(c => c.GroupName ?? string.Empty).OrderByDescending(g => !string.IsNullOrEmpty(g.Key)))
            {
                CommandGroupSettings groupSettings = null;
                string groupName = group.First().GroupName;
                if (!string.IsNullOrEmpty(groupName) && ChannelSession.Settings.CommandGroups.ContainsKey(groupName))
                {
                    groupSettings = ChannelSession.Settings.CommandGroups[groupName];
                }
                this.ActionGroupCommands.Add(new CommandGroupControlViewModel(groupSettings, group));
            }
        }
    }
}
