using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class UsersMainControlViewModel : WindowControlViewModelBase
    {
        public string UsernameFilter
        {
            get { return this.usernameFilter; }
            set
            {
                this.usernameFilter = value;
                this.NotifyPropertyChanged();

                this.RefreshUsers();
            }
        }
        private string usernameFilter;

        public bool LimitedResults
        {
            get { return this.limitedResults; }
            set
            {
                this.limitedResults = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool limitedResults = false;

        public ObservableCollection<UserDataModel> Users { get; private set; } = new ObservableCollection<UserDataModel>();

        public int SortColumnIndex
        {
            get { return this.sortColumnIndex; }
            set
            {
                this.sortColumnIndex = value;
                this.NotifyPropertyChanged();

                this.RefreshUsers();
            }
        }
        private int sortColumnIndex = 0;

        public ListSortDirection SortDirection
        {
            get { return this.sortDirection; }
            set
            {
                this.sortDirection = value;
                this.NotifyPropertyChanged();

                this.RefreshUsers();
            }
        }
        private ListSortDirection sortDirection = ListSortDirection.Ascending;

        public ICommand ExportDataCommand { get; private set; }

        public UsersMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.ExportDataCommand = this.CreateCommand(async (parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog("User Data.txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    List<List<string>> contents = new List<List<string>>();

                    List<string> columns = new List<string>() { "MixItUpID", "MixerID", "Username", "ViewingMinutes", "OfflineViewingMinutes" };
                    foreach (var kvp in ChannelSession.Settings.Currency)
                    {
                        columns.Add(kvp.Value.Name.Replace(" ", ""));
                    }
                    contents.Add(columns);

                    foreach (UserDataModel user in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        List<string> data = new List<string>();
                        data.Add(user.ID.ToString());
                        data.Add(user.MixerID.ToString());
                        data.Add(user.Username);
                        data.Add(user.ViewingMinutes.ToString());
                        data.Add(user.OfflineViewingMinutes.ToString());
                        foreach (var kvp in ChannelSession.Settings.Currency)
                        {
                            data.Add(kvp.Value.GetAmount(user).ToString());
                        }
                        contents.Add(data);
                    }

                    await SpreadsheetFileHelper.ExportToCSV(filePath, contents);
                }
            });
        }

        public void RefreshUsers()
        {
            try
            {
                string filter = null;

                if (!string.IsNullOrEmpty(this.UsernameFilter))
                {
                    filter = this.UsernameFilter.ToLower();
                }

                this.LimitedResults = false;

                this.Users.Clear();

                IEnumerable<UserDataModel> data = ChannelSession.Settings.UserData.Values.ToList();

                if (this.SortColumnIndex == 0) { data = data.OrderBy(u => u.Username); }
                if (this.SortColumnIndex == 1) { data = data.OrderBy(u => u.ViewingMinutes); }
                if (this.SortColumnIndex == 2) { data = data.OrderBy(u => u.PrimaryCurrency); }
                if (this.SortColumnIndex == 3) { data = data.OrderBy(u => u.PrimaryRankPoints); }

                if (this.SortDirection == ListSortDirection.Descending)
                {
                    data = data.Reverse();
                }

                foreach (var userData in data)
                {
                    if (string.IsNullOrEmpty(filter))
                    {
                        this.Users.Add(userData);
                    }
                    else if (!string.IsNullOrEmpty(userData.Username) && userData.Username.ToLower().Contains(filter))
                    {
                        this.Users.Add(userData);
                    }

                    if (this.Users.Count >= 200)
                    {
                        this.LimitedResults = true;
                        break;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task DeleteUser(UserDataModel user)
        {
            if (await DialogHelper.ShowConfirmation("This will delete this user's data, which includes their Hours, Currency, Rank, & Custom User Commands. Are you sure you want to do this?"))
            {
                ChannelSession.Settings.UserData.Remove(user.ID);
            }
            this.RefreshUsers();
        }

        protected override Task OnLoadedInternal()
        {
            this.RefreshUsers();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.RefreshUsers();
            return base.OnVisibleInternal();
        }
    }
}
