using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Currency;
using MixItUp.WPF.Windows.Users;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for UsersControl.xaml
    /// </summary>
    public partial class UsersControl : MainControlBase
    {
        private ObservableCollection<UserDataViewModel> userData = new ObservableCollection<UserDataViewModel>();

        private DataGridColumn lastSortedColumn = null;

        public UsersControl()
        {
            InitializeComponent();

            this.UserDataGridView.ItemsSource = this.userData;
            this.UserDataGridView.Sorted += UserDataGridView_Sorted;
        }

        protected override async Task InitializeInternal()
        {
            this.RefreshList();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            this.RefreshList();
            await base.OnVisibilityChanged();
        }

        private void RefreshList(DataGridColumn sortColumn = null)
        {
            string filter = this.UsernameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.LimitingResultsMessage.Visibility = Visibility.Collapsed;
            this.userData.Clear();

            IEnumerable<UserDataViewModel> data = ChannelSession.Settings.UserData.Values.ToList();
            if (sortColumn != null)
            {
                int columnIndex = this.UserDataGridView.Columns.IndexOf(sortColumn);
                if (columnIndex == 0) { data = data.OrderBy(u => u.UserName); }
                if (columnIndex == 1) { data = data.OrderBy(u => u.ViewingMinutes); }
                if (columnIndex == 2) { data = data.OrderBy(u => u.PrimaryCurrency); }
                if (columnIndex == 3) { data = data.OrderBy(u => u.PrimaryRankPoints); }

                if (sortColumn.SortDirection.GetValueOrDefault() == ListSortDirection.Descending)
                {
                    data = data.Reverse();
                }
                lastSortedColumn = sortColumn;
            }

            foreach (var userData in data)
            {
                if (string.IsNullOrEmpty(filter) || userData.UserName.ToLower().Contains(filter))
                {
                    this.userData.Add(userData);
                }

                if (this.userData.Count >= 200)
                {
                    this.LimitingResultsMessage.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        private void UsernameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RefreshList();
        }

        private void UserEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataViewModel userData = (UserDataViewModel)button.DataContext;
            UserDataEditorWindow window = new UserDataEditorWindow(userData);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void UserDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserDataViewModel userData = (UserDataViewModel)button.DataContext;
            if (await MessageBoxHelper.ShowConfirmationDialog("This will delete this user's data, which includes their Hours, Currency, Rank, & Custom User Commands. Are you sure you want to do this?"))
            {
                ChannelSession.Settings.UserData.Remove(userData.ID);
            }
            this.RefreshList();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }

        private void UserDataGridView_Sorted(object sender, DataGridColumn column)
        {
            this.RefreshList(column);
        }

        private void ImportUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            UserDataImportWindow window = new UserDataImportWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void ExportUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog("User Data.txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    StringBuilder fileContents = new StringBuilder();
                    fileContents.Append("User ID\tUsername\tViewing Minutes\tOffline Viewing Minutes");
                    foreach (var kvp in ChannelSession.Settings.Currencies.ToDictionary())
                    {
                        fileContents.Append("\t" + kvp.Value.Name);
                    }
                    fileContents.AppendLine();

                    foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        fileContents.Append(string.Format("{0}\t{1}\t{2}\t{3}", userData.ID, userData.UserName, userData.ViewingMinutes, userData.OfflineViewingMinutes));
                        foreach (var kvp in ChannelSession.Settings.Currencies.ToDictionary())
                        {
                            fileContents.Append("\t" + userData.GetCurrencyAmount(kvp.Value));
                        }
                        fileContents.AppendLine();
                    }

                    await ChannelSession.Services.FileService.SaveFile(filePath, fileContents.ToString());
                }
            });
        }
    }
}
