using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Requirements;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Chat
{
    public class PreMadeChatCommandControlViewModel : UIViewModelBase
    {
        public string Name { get { return this.command.Name; } }

        public string TriggersString { get { return this.command.TriggersString; } }

        public IEnumerable<UserRoleEnum> RoleValues { get { return RoleRequirementViewModel.SelectableUserRoles(); } }
        public UserRoleEnum SelectedRole
        {
            get { return this.command.Requirements.Role.Role; }
            set
            {
                this.command.Requirements.Role.Role = value;
                this.UpdateSetting();
                this.NotifyPropertyChanged();
            }
        }

        public int CooldownString
        {
            get { return int.Parse(this.command.Requirements.Cooldown.IndividualAmount); }
            set
            {
                this.command.Requirements.Cooldown.IndividualAmount = value.ToString();
                this.UpdateSetting();
                this.NotifyPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get { return this.command.IsEnabled; }
            set
            {
                this.command.IsEnabled = value;
                this.UpdateSetting();
                this.NotifyPropertyChanged();
                ChannelSession.Services.Chat.RebuildCommandTriggers();
            }
        }

        public ICommand TestCommand { get; set; }

        private PreMadeChatCommandModelBase command;
        private PreMadeChatCommandSettingsModel setting;

        public PreMadeChatCommandControlViewModel(PreMadeChatCommandModelBase command)
        {
            this.command = command;

            this.setting = ChannelSession.Settings.PreMadeChatCommandSettings.FirstOrDefault(c => c.Name.Equals(this.command.Name));
            if (this.setting == null)
            {
                this.setting = new PreMadeChatCommandSettingsModel(this.command);
                ChannelSession.Settings.PreMadeChatCommandSettings.Add(this.setting);
            }

            this.TestCommand = this.CreateCommand((System.Func<object, System.Threading.Tasks.Task>)(async (parameter) =>
            {
                UserViewModel currentUser = ChannelSession.GetCurrentUser();
                await command.Perform(currentUser, arguments: new List<string>() { "@" + currentUser.Username });
            }));
        }

        private void UpdateSetting()
        {
            if (this.setting != null)
            {
                this.setting.Role = this.command.Requirements.Role.Role;
                this.setting.Cooldown = int.Parse(this.command.Requirements.Cooldown.Amount);
                this.setting.IsEnabled = this.command.IsEnabled;
            }
        }
    }
}
