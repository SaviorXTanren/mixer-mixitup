using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for UsersSettingsControl.xaml
    /// </summary>
    public partial class UsersSettingsControl : SettingsControlBase
    {
        private ObservableCollection<UserTitleModel> titles = new ObservableCollection<UserTitleModel>();

        public UsersSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.UserTitlesDataGrid.ItemsSource = this.titles;
            this.RefreshTitleList();

            if (ChannelSession.Settings.RegularUserMinimumHours > 0)
            {
                this.RegularUserMinimumHoursTextBox.Text = ChannelSession.Settings.RegularUserMinimumHours.ToString();
            }

            this.TitleRoleComboBox.ItemsSource = EnumHelper.GetEnumNames(UserViewModel.SelectableBasicUserRoles());

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void RegularUserMinimumHoursTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.RegularUserMinimumHoursTextBox.Text) && int.TryParse(this.RegularUserMinimumHoursTextBox.Text, out int time) && time > 0)
            {
                ChannelSession.Settings.RegularUserMinimumHours = time;
            }
            else
            {
                this.RegularUserMinimumHoursTextBox.Text = string.Empty;
            }
        }

        private void TitleRoleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.TitleMinimumMonthsTextBox.IsEnabled = false;
            this.TitleMinimumMonthsTextBox.Text = string.Empty;
            if (this.TitleRoleComboBox.SelectedIndex >= 0)
            {
                UserRoleEnum role = EnumHelper.GetEnumValueFromString<UserRoleEnum>((string)this.TitleRoleComboBox.SelectedItem);
                if (role == UserRoleEnum.Follower || role == UserRoleEnum.Subscriber)
                {
                    this.TitleMinimumMonthsTextBox.IsEnabled = true;
                    this.TitleMinimumMonthsTextBox.Text = "0";
                }
            }
        }

        private async void AddTitleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.TitleNameTextBox.Text))
                {
                    await DialogHelper.ShowMessage("A name for the title must be specified");
                    return;
                }

                if (this.TitleRoleComboBox.SelectedIndex < 0)
                {
                    await DialogHelper.ShowMessage("A role for the title must be specified");
                    return;
                }

                int months = 0;

                UserRoleEnum role = EnumHelper.GetEnumValueFromString<UserRoleEnum>((string)this.TitleRoleComboBox.SelectedItem);
                if (role == UserRoleEnum.Follower || role == UserRoleEnum.Subscriber)
                {
                    if (!int.TryParse(this.TitleMinimumMonthsTextBox.Text, out months) || months < 0)
                    {
                        await DialogHelper.ShowMessage("A valid amount of months for the title must be specified");
                        return;
                    }
                }

                if (this.titles.Any(t => t.Name.Equals(this.TitleNameTextBox.Text)))
                {
                    await DialogHelper.ShowMessage("A title with the same name already exists");
                    return;
                }

                if (this.titles.Any(t => t.Role.Equals(role)))
                {
                    UserTitleModel existingTitle = this.titles.FirstOrDefault(t => t.Role.Equals(role));
                    if (existingTitle.Role == UserRoleEnum.Follower || existingTitle.Role == UserRoleEnum.Subscriber)
                    {
                        if (existingTitle.Months == months)
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

                ChannelSession.Settings.UserTitles.Add(new UserTitleModel(this.TitleNameTextBox.Text, role, months));

                this.TitleNameTextBox.Text = string.Empty;
                this.TitleRoleComboBox.SelectedIndex = -1;
                this.TitleMinimumMonthsTextBox.Text = string.Empty;

                this.RefreshTitleList();
            });
        }

        private void DeleteTitleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserTitleModel title = (UserTitleModel)button.DataContext;
            ChannelSession.Settings.UserTitles.Remove(title);
            this.RefreshTitleList();
        }

        private void RefreshTitleList()
        {
            this.titles.Clear();
            foreach (UserTitleModel title in ChannelSession.Settings.UserTitles.OrderBy(t => t.Role).ThenBy(t => t.Months))
            {
                this.titles.Add(title);
            }
        }
    }
}