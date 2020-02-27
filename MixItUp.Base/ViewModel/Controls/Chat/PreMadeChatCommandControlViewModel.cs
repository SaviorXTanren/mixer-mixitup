using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Chat
{
    public class PreMadeChatCommandControlViewModel : UIViewModelBase
    {
        public string Name { get { return this.command.Name; } }

        public string CommandsString { get { return this.command.CommandsString; } }

        public IEnumerable<UserRoleEnum> PermissionsValues { get { return RoleRequirementViewModel.BasicUserRoleAllowedValues; } }
        public UserRoleEnum SelectedPermission
        {
            get { return this.command.Requirements.Role.MixerRole; }
            set
            {
                this.command.Requirements.Role.MixerRole = value;
                this.UpdateSetting();
                this.NotifyPropertyChanged();
            }
        }

        public string CooldownString
        {
            get { return this.command.Requirements.Cooldown.Amount.ToString(); }
            set
            {
                this.command.Requirements.Cooldown.Amount = this.GetPositiveIntFromString(value);
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

        private PreMadeChatCommand command;
        private PreMadeChatCommandSettings setting;

        public PreMadeChatCommandControlViewModel(PreMadeChatCommand command)
        {
            this.command = command;

            this.setting = ChannelSession.Settings.PreMadeChatCommandSettings.FirstOrDefault(c => c.Name.Equals(this.command.Name));
            if (this.setting == null)
            {
                this.setting = new PreMadeChatCommandSettings(this.command);
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
                this.setting.Permissions = this.command.Requirements.Role.MixerRole;
                this.setting.Cooldown = this.command.Requirements.Cooldown.Amount;
                this.setting.IsEnabled = this.command.IsEnabled;
            }
        }
    }
}
