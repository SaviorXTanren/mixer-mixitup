using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings.Generic;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings
{
    public class UserTitleViewModel : UIViewModelBase
    {
        public string Name { get { return this.Title.Name; } }

        public UserRoleEnum Role { get { return this.Title.UserRole; } }

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

            this.DeleteCommand = this.CreateCommand(() =>
            {
                this.viewModel.DeleteTitle(this);
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

        public GenericToggleSettingsOptionControlViewModel ExplicitUserRoleRequirements { get; set; }

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

        public IEnumerable<UserRoleEnum> Roles { get; private set; } = UserRoles.All;
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

        public GenericButtonSettingsOptionControlViewModel ClearUserDataRange { get; set; }
        public GenericButtonSettingsOptionControlViewModel ClearAllUserData { get; set; }

        public UsersSettingsControlViewModel()
        {
            this.ExplicitUserRoleRequirements = new GenericToggleSettingsOptionControlViewModel(
                MixItUp.Base.Resources.ExplicitUserRoleRequirements,
                ChannelSession.Settings.ExplicitUserRoleRequirements,
                async (value) =>
                {
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.ExplicitUserRoleRequirementsTooltip);
                    ChannelSession.Settings.ExplicitUserRoleRequirements = value;
                },
                MixItUp.Base.Resources.ExplicitUserRoleRequirementsTooltip);

            this.ClearUserDataRange = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.ClearUserDataRangeHeader, MixItUp.Base.Resources.ClearUserDataRange, this.CreateCommand(async () =>
            {
                string output = await DialogHelper.ShowTextEntry(MixItUp.Base.Resources.TimeDays, "0", MixItUp.Base.Resources.ClearUserDataRangeWarning);
                if (!string.IsNullOrEmpty(output) && int.TryParse(output, out int days) && days > 0)
                {
                    await ServiceManager.Get<UserService>().ClearUserDataRange(days);
                    await ChannelSession.SaveSettings();
                    ChannelSession.RestartRequested();
                }
            }));

            this.ClearAllUserData = new GenericButtonSettingsOptionControlViewModel(MixItUp.Base.Resources.ClearAllUserDataHeader, MixItUp.Base.Resources.ClearAllUserData, this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ClearAllUserDataWarning))
                {
                    await ServiceManager.Get<UserService>().ClearAllUserData();
                    await ChannelSession.SaveSettings();
                    ChannelSession.RestartRequested();
                }
            }));

            this.RefreshTitleList();

            this.AddCommand = this.CreateCommand(async () =>
            {
                if (string.IsNullOrEmpty(this.TitleName))
                {
                    await DialogHelper.ShowMessage(Resources.CreateTitleErrorNoTitle);
                    return;
                }

                if (this.CanSelectMinimumMonths && this.MinimumMonths < 0)
                {
                    await DialogHelper.ShowMessage(Resources.CreateTitleErrorInvalidMonths);
                    return;
                }

                if (this.Titles.Any(t => t.Name.Equals(this.TitleName)))
                {
                    await DialogHelper.ShowMessage(Resources.CreateTitleErrorDuplicateTitle);
                    return;
                }

                UserTitleViewModel existingTitle = this.Titles.FirstOrDefault(t => t.Role.Equals(this.SelectedRole));
                if (existingTitle != null)
                {
                    if (existingTitle.Role == UserRoleEnum.Follower || existingTitle.Role == UserRoleEnum.YouTubeSubscriber || existingTitle.Role == UserRoleEnum.Subscriber)
                    {
                        if (existingTitle.Months == this.MinimumMonths)
                        {
                            await DialogHelper.ShowMessage(Resources.CreateTitleErrorDuplicateRoleMonths);
                            return;
                        }
                    }
                    else
                    {
                        await DialogHelper.ShowMessage(Resources.CreateTitleErrorDuplicateRole);
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
            this.Titles.ClearAndAddRange(ChannelSession.Settings.UserTitles.OrderBy(t => t.UserRole).ThenBy(t => t.Months).Select(t => new UserTitleViewModel(this, t)));
        }
    }
}
