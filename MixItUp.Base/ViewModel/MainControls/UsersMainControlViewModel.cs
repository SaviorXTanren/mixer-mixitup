using MixItUp.Base.Model;
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
        public const int MaxUsersToDisplay = 200;

        public IEnumerable<StreamingPlatformTypeEnum> Platforms { get { return StreamingPlatforms.SelectablePlatforms; } }

        public StreamingPlatformTypeEnum SelectedPlatform
        {
            get { return this.selectedPlatform; }
            set
            {
                this.selectedPlatform = value;
                this.NotifyPropertyChanged();

                this.RefreshUsers();
            }
        }
        private StreamingPlatformTypeEnum selectedPlatform = StreamingPlatformTypeEnum.All;

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

        public bool IsUsersListCapped { get { return this.Users.Count >= MaxUsersToDisplay; } }

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

        public bool IsDescendingSort { get { return this.SortDirection == ListSortDirection.Descending; } }

        public ICommand ExportDataCommand { get; private set; }

        private bool columnsSorted = false;
        private bool allUserDataLoaded = false;

        public UsersMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.ExportDataCommand = this.CreateCommand(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog("User Data.txt");
                if (!string.IsNullOrEmpty(filePath))
                {
                    List<List<string>> contents = new List<List<string>>();

                    List<string> columns = new List<string>() { "MixItUpID", "TwitchID", "Username", "PrimaryRole", "ViewingMinutes", "OfflineViewingMinutes", "CustomTitle" };
                    foreach (var kvp in ChannelSession.Settings.Currency)
                    {
                        columns.Add(kvp.Value.Name.Replace(" ", ""));
                    }
                    foreach (var kvp in ChannelSession.Settings.StreamPass)
                    {
                        columns.Add(kvp.Value.Name.Replace(" ", ""));
                    }
                    columns.AddRange(new List<string>() { "TotalStreamsWatched", "TotalAmountDonated", "TotalSubsGifted", "TotalSubsReceived", "TotalChatMessagesSent", "TotalTimesTagged",
                        "TotalCommandsRun", "TotalMonthsSubbed", "LastSeen" });
                    contents.Add(columns);

                    foreach (UserDataModel user in ChannelSession.Settings.UserData.Values.ToList())
                    {
                        List<string> data = new List<string>() { user.ID.ToString(), user.TwitchID, user.Username, user.PrimaryRole.ToString(),
                            user.ViewingMinutes.ToString(), user.OfflineViewingMinutes.ToString(), user.CustomTitle };
                        foreach (var kvp in ChannelSession.Settings.Currency)
                        {
                            data.Add(kvp.Value.GetAmount(user).ToString());
                        }
                        foreach (var kvp in ChannelSession.Settings.StreamPass)
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

            this.columnsSorted = true;

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
                    if (!string.IsNullOrEmpty(this.UsernameFilter) || this.SelectedPlatform != StreamingPlatformTypeEnum.All || this.columnsSorted)
                    {
                        if (!this.allUserDataLoaded)
                        {
                            await ChannelSession.Settings.LoadUserData();
                            this.allUserDataLoaded = true;
                        }
                    }

                    IEnumerable<UserDataModel> data = ChannelSession.Settings.UserData.Values.ToList();
                    if (!string.IsNullOrEmpty(this.UsernameFilter))
                    {
                        string filter = this.UsernameFilter.ToLower();
                        if (this.SelectedPlatform != StreamingPlatformTypeEnum.All)
                        {
                            data = data.Where(u => u.Platform.HasFlag(this.SelectedPlatform) && u.Username != null && u.Username.Contains(filter, StringComparison.OrdinalIgnoreCase));
                        }
                        else
                        {
                            data = data.Where(u => u.Username != null && u.Username.Contains(filter, StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    if (this.SortColumnIndex == 0) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.Username) : data.OrderBy(u => u.Username); }
                    else if (this.SortColumnIndex == 1) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.Platform) : data.OrderBy(u => u.Platform); }
                    else if (this.SortColumnIndex == 2) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryRole) : data.OrderBy(u => u.PrimaryRole); }
                    else if (this.SortColumnIndex == 3) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.ViewingMinutes) : data.OrderBy(u => u.ViewingMinutes); }
                    else if (this.SortColumnIndex == 4) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryCurrency) : data.OrderBy(u => u.PrimaryCurrency); }
                    else if (this.SortColumnIndex == 5) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryRankPoints) : data.OrderBy(u => u.PrimaryRankPoints); }

                    this.Users.ClearAndAddRange(data.Take(MaxUsersToDisplay));

                    this.NotifyPropertyChanged("IsUsersListCapped");
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
            if (await DialogHelper.ShowConfirmation(Resources.DeleteUserDataPrompt))
            {
                ChannelSession.Settings.UserData.Remove(user.ID);
                await ChannelSession.Services.User.RemoveActiveUserByID(user.ID);
            }
            this.RefreshUsers();
        }

        protected override async Task OnLoadedInternal()
        {
            await ChannelSession.Settings.LoadUserData(MaxUsersToDisplay);

            this.RefreshUsers();
            await base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.RefreshUsers();
            return base.OnVisibleInternal();
        }
    }
}
