using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Controls.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public abstract class GroupedCommandsMainControlViewModelBase : WindowControlViewModelBase
    {
        public ObservableCollection<CommandGroupControlViewModel> CommandGroups { get; private set; } = new ObservableCollection<CommandGroupControlViewModel>();

        public string NameFilter
        {
            get { return this.nameFilter; }
            set
            {
                this.nameFilter = value;
                this.NotifyPropertyChanged();
                this.FilterCommands();
            }
        }
        private string nameFilter;

        public GroupedCommandsMainControlViewModelBase(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void AddCommand(CommandModelBase command)
        {
            foreach (CommandGroupControlViewModel group in this.CommandGroups)
            {
                if (string.Equals(group.GroupName, command.GroupName))
                {
                    group.AddCommand(command);
                    return;
                }
            }

            CommandGroupSettingsModel groupSettings = null;
            if (!string.IsNullOrEmpty(command.GroupName) && ChannelSession.Settings.CommandGroups.ContainsKey(command.GroupName))
            {
                groupSettings = ChannelSession.Settings.CommandGroups[command.GroupName];
            }
            this.CommandGroups.Add(new CommandGroupControlViewModel(groupSettings, new List<CommandModelBase>() { command }));
        }

        public void RemoveCommand(CommandModelBase command)
        {
            foreach (CommandGroupControlViewModel group in this.CommandGroups)
            {
                group.RemoveCommand(command);
            }
        }

        public void FullRefresh()
        {
            this.CommandGroups.Clear();
            IEnumerable<CommandModelBase> commands = this.GetCommands();
            foreach (var group in commands.GroupBy(c => c.GroupName ?? "ZZZZZZZZZZZZZZZZZZZZZZZ").OrderBy(g => g.Key))
            {
                CommandGroupSettingsModel groupSettings = null;
                string groupName = group.First().GroupName;
                if (!string.IsNullOrEmpty(groupName) && ChannelSession.Settings.CommandGroups.ContainsKey(groupName))
                {
                    groupSettings = ChannelSession.Settings.CommandGroups[groupName];
                }
                this.CommandGroups.Add(new CommandGroupControlViewModel(groupSettings, group));
            }
        }

        protected abstract IEnumerable<CommandModelBase> GetCommands();

        protected virtual void FilterCommands()
        {
            foreach (CommandGroupControlViewModel group in this.CommandGroups)
            {
                group.RefreshCommands(this.NameFilter.ToLower());
            }
        }

        protected override Task OnLoadedInternal()
        {
            this.FullRefresh();
            return base.OnVisibleInternal();
        }
    }
}
