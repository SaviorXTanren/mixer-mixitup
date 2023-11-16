using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public abstract class GroupedCommandsMainControlViewModelBase : WindowControlViewModelBase
    {
        public static event EventHandler<CommandModelBase> OnCommandAddedEdited = delegate { };

        public static void CommandAddedEdited(CommandModelBase command)
        {
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited(null, command);
        }

        public event EventHandler OnNameFilterChanged = delegate { };

        public ObservableCollection<CommandModelBase> DefaultGroup { get { return this.CommandGroups.FirstOrDefault()?.Commands; } }

        public ObservableCollection<CommandGroupControlViewModel> CommandGroups { get; private set; } = new ObservableCollection<CommandGroupControlViewModel>();

        public bool ShowList { get { return !this.ShowGroups; } }
        public bool ShowGroups { get { return this.CommandGroups.Count > 1; } }

        public string NameFilter
        {
            get { return this.nameFilter; }
            set
            {
                bool changeOccurred = !string.Equals(this.nameFilter, value);

                this.nameFilter = value;
                this.NotifyPropertyChanged();

                if (changeOccurred)
                {
                    this.FilterCommands();
                }
            }
        }
        private string nameFilter;

        public GroupedCommandsMainControlViewModelBase(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void AddCommand(CommandModelBase command)
        {
            this.RemoveCommand(command);

            foreach (CommandGroupControlViewModel group in this.CommandGroups)
            {
                if (string.Equals(group.GroupName, command.GroupName))
                {
                    group.AddCommand(command);
                    this.NotifyProperties();
                    return;
                }
            }

            CommandGroupSettingsModel groupSettings = null;
            if (!string.IsNullOrEmpty(command.GroupName) && ChannelSession.Settings.CommandGroups.ContainsKey(command.GroupName))
            {
                groupSettings = ChannelSession.Settings.CommandGroups[command.GroupName];
            }

            CommandGroupControlViewModel viewModel = new CommandGroupControlViewModel(groupSettings, new List<CommandModelBase>() { command });
            if (groupSettings != null)
            {
                for (int i = 0; i < this.CommandGroups.Count; i++)
                {
                    if (string.Compare(groupSettings.Name, this.CommandGroups[i].DisplayName, ignoreCase: true) < 0)
                    {
                        this.CommandGroups.Insert(i, viewModel);
                        this.NotifyProperties();
                        return;
                    }
                }
            }

            this.CommandGroups.Add(viewModel);
            this.NotifyProperties();
        }

        public void RemoveCommand(CommandModelBase command)
        {
            foreach (CommandGroupControlViewModel group in this.CommandGroups)
            {
                if (group.HasCommand(command))
                {
                    group.RemoveCommand(command);
                    if (!group.HasCommands)
                    {
                        this.CommandGroups.Remove(group);
                    }
                    this.NotifyProperties();
                    return;
                }
            }

            this.NotifyProperties();
        }

        public void ClearNameFilter()
        {
            this.nameFilter = string.Empty;
            this.NotifyPropertyChanged(this.NameFilter);
        }

        public void FullRefresh()
        {
            List<CommandGroupControlViewModel> groups = new List<CommandGroupControlViewModel>();
            IEnumerable<CommandModelBase> commands = this.GetCommands();
            foreach (var group in commands.GroupBy(c => c.GroupName ?? "ZZZZZZZZZZZZZZZZZZZZZZZ").OrderBy(g => g.Key))
            {
                CommandGroupSettingsModel groupSettings = null;
                string groupName = group.First().GroupName;
                if (!string.IsNullOrEmpty(groupName) && ChannelSession.Settings.CommandGroups.ContainsKey(groupName))
                {
                    groupSettings = ChannelSession.Settings.CommandGroups[groupName];
                }
                groups.Add(new CommandGroupControlViewModel(groupSettings, group));
            }
            this.CommandGroups.ClearAndAddRange(groups);
            this.NotifyProperties();
        }

        protected abstract IEnumerable<CommandModelBase> GetCommands();

        protected virtual void FilterCommands()
        {
            foreach (CommandGroupControlViewModel group in this.CommandGroups)
            {
                group.RefreshCommands(this.NameFilter.ToLower());
            }
        }

        protected override Task OnOpenInternal()
        {
            this.FullRefresh();
            return base.OnVisibleInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            if (this.GetCommands().Count() != this.CommandGroups.Sum(cg => cg.Commands.Count))
            {
                this.FullRefresh();
            }
            await base.OnVisibleInternal();
        }

        private void NotifyProperties()
        {
            this.NotifyPropertyChanged("DefaultGroup");
            this.NotifyPropertyChanged("ShowList");
            this.NotifyPropertyChanged("ShowGroups");
        }
    }
}
