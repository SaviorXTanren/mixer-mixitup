using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
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
            }
        }
        private string usernameFilter;

        public ThreadSafeObservableCollection<UserDataModel> Users { get; private set; } = new ThreadSafeObservableCollection<UserDataModel>();

        public int SortColumnIndex
        {
            get { return this.sortColumnIndex; }
        }
        private int sortColumnIndex = 0;

        public ListSortDirection SortDirection
        {
            get { return this.sortDirection; }
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

                    List<string> columns = new List<string>() { "MixItUpID", "TwitchID", "MixerID", "Username", "PrimaryRole", "ViewingMinutes", "OfflineViewingMinutes", "CustomTitle" };
                    foreach (var kvp in ChannelSession.Settings.Currency)
                    {
                        columns.Add(kvp.Value.Name.Replace(" ", ""));
                    }
                    columns.AddRange(new List<string>() { "TotalStreamsWatched", "TotalAmountDonated", "TotalSubsGifted", "TotalSubsReceived", "TotalChatMessagesSent", "TotalTimesTagged",
                        "TotalCommandsRun", "TotalMonthsSubbed", "LastSeen" });
                    contents.Add(columns);

                    foreach (UserDataModel user in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        List<string> data = new List<string>() { user.ID.ToString(), user.TwitchID, user.MixerID.ToString(), user.Username, user.UserRoles.Max().ToString(),
                            user.ViewingMinutes.ToString(), user.OfflineViewingMinutes.ToString(), user.CustomTitle };
                        foreach (var kvp in ChannelSession.Settings.Currency)
                        {
                            data.Add(kvp.Value.GetAmount(user).ToString());
                        }
                        data.AddRange(new List<string>() { user.TotalStreamsWatched.ToString(), user.TotalAmountDonated.ToString(), user.TotalSubsGifted.ToString(), user.TotalSubsReceived.ToString(),
                            user.TotalChatMessageSent.ToString(), user.TotalTimesTagged.ToString(), user.TotalCommandsRun.ToString(), user.TotalMonthsSubbed.ToString(), user.LastSeen.ToFriendlyDateTimeString() });
                        contents.Add(data);
                    }

                    await SpreadsheetFileHelper.ExportToCSV(filePath, contents);
                }
            });
        }

        public void SetSortColumnIndexAndDirection(int index, ListSortDirection direction)
        {
            this.sortColumnIndex = index;
            this.sortDirection = direction;

            this.NotifyPropertyChanged("SortColumnIndex");
            this.NotifyPropertyChanged("SortDirection");

            this.RefreshUsers();
        }

        public void RefreshUsers()
        {
            _ = RefreshUsersAsync();
        }

        public async Task RefreshUsersAsync()
        {
            await Task.Run(async () =>
            {
                await DispatcherHelper.Dispatcher.InvokeAsync(() =>
                {
                    this.StartLoadingOperation();
                    return Task.CompletedTask;
                });

                try
                {
                    string filter = null;

                    if (!string.IsNullOrEmpty(this.UsernameFilter))
                    {
                        filter = this.UsernameFilter.ToLower();
                    }

                    IEnumerable<UserDataModel> data = ChannelSession.Settings.UserData.Values.ToList();

                    if (this.SortColumnIndex == 0) { data = data.OrderBy(u => u.Username); }
                    if (this.SortColumnIndex == 1) { data = data.OrderBy(u => u.ViewingMinutes); }
                    if (this.SortColumnIndex == 2) { data = data.OrderBy(u => u.PrimaryCurrency); }
                    if (this.SortColumnIndex == 3) { data = data.OrderBy(u => u.PrimaryRankPoints); }

                    if (this.SortDirection == ListSortDirection.Descending)
                    {
                        data = data.Reverse();
                    }

                    this.Users.ClearAndAddRange(data.Where(u => string.IsNullOrEmpty(filter) || (u.Username != null && u.Username.Contains(filter, StringComparison.OrdinalIgnoreCase))));
                }
                catch (Exception ex) { Logger.Log(ex); }

                await DispatcherHelper.Dispatcher.InvokeAsync(() =>
                {
                    this.EndLoadingOperation();
                    return Task.CompletedTask;
                });
            });
        }

        public async Task DeleteUser(UserDataModel user)
        {
            if (await DialogHelper.ShowConfirmation("This will delete this user's data, which includes their Hours, Currency, Rank, & Custom User Commands. Are you sure you want to do this?"))
            {
                ChannelSession.Settings.UserData.Remove(user.ID);
                await ChannelSession.Services.User.RemoveUserByID(user.ID);
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
