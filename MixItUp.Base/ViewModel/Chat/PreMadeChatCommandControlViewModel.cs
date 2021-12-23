using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Chat
{
    public class PreMadeChatCommandControlViewModel : UIViewModelBase
    {
        public string Name { get { return this.command.Name; } }

        public string TriggersString { get { return this.command.TriggersString; } }

        public IEnumerable<UserRoleEnum> RoleValues { get { return UserRoles.All; } }
        public UserRoleEnum SelectedRole
        {
            get { return this.command.Requirements.Role.UserRole; }
            set
            {
                this.command.Requirements.Role.UserRole = value;
                this.UpdateSetting();
                this.NotifyPropertyChanged();
            }
        }

        public int CooldownString
        {
            get { return this.command.Requirements.Cooldown.IndividualAmount; }
            set
            {
                this.command.Requirements.Cooldown.IndividualAmount = value;
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
                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
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

            this.TestCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CommandService>().Queue(command, new CommandParametersModel(ChannelSession.User, arguments: new List<string>() { "@" + ChannelSession.User.Username }));
            });
        }

        private void UpdateSetting()
        {
            if (this.setting != null)
            {
                this.setting.UserRole = this.command.Requirements.Role.UserRole;
                this.setting.Cooldown = this.command.Requirements.Cooldown.Amount;
                this.setting.IsEnabled = this.command.IsEnabled;
            }
        }
    }
}
