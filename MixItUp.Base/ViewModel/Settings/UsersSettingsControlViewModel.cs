using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModel.Requirements;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings
{
    public class UserTitleViewModel : UIViewModelBase
    {
        public string Name { get { return this.Title.Name; } }

        public UserRoleEnum Role { get { return this.Title.Role; } }

        public string RoleString { get { return this.Title.RoleString; } }

        public int Months { get { return this.Title.Months; } }

        public string MonthsString { get { return this.Title.MonthsString; } }

        public ICommand DeleteCommand { get; set; }

        public UserTitleModel Title { get; private set; }

        private UsersSettingsControlViewModel viewModel;

        public UserTitleViewModel(UsersSettingsControlViewModel viewModel, UserTitleModel title)
        {
            this.viewModel = viewModel;
            this.Title = title;

            this.DeleteCommand = this.CreateCommand((parameter) =>
            {
                this.viewModel.DeleteTitle(this);
                return Task.FromResult(0);
            });
        }
    }

    public class UsersSettingsControlViewModel : UIViewModelBase
    {
        public int RegularMinimumHours
        {
            get { return ChannelSession.Settings.RegularUserMinimumHours; }
            set
            {
                if (value >= 0)
                {
                    ChannelSession.Settings.RegularUserMinimumHours = value;
                }
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<UserTitleViewModel> Titles { get; set; } = new ObservableCollection<UserTitleViewModel>();

        public string TitleName
        {
            get { return this.titleName; }
            set
            {
                this.titleName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string titleName;

        public IEnumerable<UserRoleEnum> Roles { get; private set; } = RoleRequirementViewModel.SelectableUserRoles();
        public UserRoleEnum SelectedRole
        {
            get { return this.selectedRole; }
            set
            {
                this.selectedRole = value;
                this.NotifyPropertyChanged();

                if (!this.CanSelectMinimumMonths)
                {
                    this.MinimumMonths = 0;
                }

                this.NotifyPropertyChanged("MinimumMonths");
                this.NotifyPropertyChanged("CanSelectMinimumMonths");
            }
        }
        private UserRoleEnum selectedRole = UserRoleEnum.User;

        public int MinimumMonths
        {
            get { return this.minimumMonths; }
            set
            {
                if (value >= 0)
                {
                    this.minimumMonths = value;
                }
                this.NotifyPropertyChanged();
            }
        }
        private int minimumMonths;
        public bool CanSelectMinimumMonths { get { return this.SelectedRole == UserRoleEnum.Follower || SelectedRole == UserRoleEnum.Subscriber; } }

        public ICommand AddCommand { get; set; }

        public GenericButtonSettingsOptionControlViewModel ClearMixerUserData { get; set; }

        public GenericButtonSettingsOptionControlViewModel ClearUserData { get; set; }

        public UsersSettingsControlViewModel()
        {
            this.ClearMixerUserData = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.ClearMixerUserDataHeader, MixItUp.Base.Resources.ClearMixerUserData, this.CreateCommand(async (parameter) =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ClearAllMixerUserDataWarning))
                {
                    ChannelSession.Settings.ClearMixerUserData();
                    await ChannelSession.SaveSettings();
                    GlobalEvents.RestartRequested();
                }
            }));

            this.ClearUserData = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.ClearAllUserDataHeader, MixItUp.Base.Resources.ClearUserData, this.CreateCommand(async (parameter) =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ClearAllUserDataWarning))
                {
                    await ChannelSession.Settings.ClearAllUserData();
                    await ChannelSession.SaveSettings();
                    GlobalEvents.RestartRequested();
                }
            }));

            this.RefreshTitleList();

            this.AddCommand = this.CreateCommand(async (parameter) =>
            {
                if (string.IsNullOrEmpty(this.TitleName))
                {
                    await DialogHelper.ShowMessage("A name for the title must be specified");
                    return;
                }

                if (this.CanSelectMinimumMonths && this.MinimumMonths < 0)
                {
                    await DialogHelper.ShowMessage("A valid amount of months for the title must be specified");
                    return;
                }

                if (this.Titles.Any(t => t.Name.Equals(this.TitleName)))
                {
                    await DialogHelper.ShowMessage("A title with the same name already exists");
                    return;
                }

                UserTitleViewModel existingTitle = this.Titles.FirstOrDefault(t => t.Role.Equals(this.SelectedRole));
                if (existingTitle != null)
                {
                    if (existingTitle.Role == UserRoleEnum.Follower || existingTitle.Role == UserRoleEnum.Subscriber)
                    {
                        if (existingTitle.Months == this.MinimumMonths)
                        {
                            await DialogHelper.ShowMessage("A title with the same role & months already exists");
                            return;
                        }
                    }
                    else
                    {
                        await DialogHelper.ShowMessage("A title with the same role already exists");
                        return;
                    }
                }

                ChannelSession.Settings.UserTitles.Add(new UserTitleModel(this.TitleName, this.SelectedRole, this.MinimumMonths));
                this.RefreshTitleList();
            });
        }

        public void DeleteTitle(UserTitleViewModel title)
        {
            this.Titles.Remove(title);
            ChannelSession.Settings.UserTitles.Remove(title.Title);
            this.RefreshTitleList();
        }

        private void RefreshTitleList()
        {
            this.Titles.Clear();
            foreach (UserTitleModel title in ChannelSession.Settings.UserTitles.OrderBy(t => t.Role).ThenBy(t => t.Months))
            {
                this.Titles.Add(new UserTitleViewModel(this, title));
            }
        }
    }
}
