using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public enum UserSearchFilterTypeEnum
    {
        None,
        Role,
        WatchTime,
        Consumables,
        CustomSettings,
        LastSeen,
    }

    public class ConsumableSearchFilterViewModel : UIViewModelBase
    {
        public string Name
        {
            get
            {
                if (this.Currency != null)
                {
                    return this.Currency.Name;
                }
                else if (this.Inventory != null)
                {
                    return this.Inventory.Name;
                }
                else if (this.StreamPass != null)
                {
                    return this.StreamPass.Name;
                }
                return string.Empty;
            }
        }

        public bool IsInventory { get { return this.Inventory != null; } }

        public CurrencyModel Currency { get; private set; }

        public InventoryModel Inventory { get; private set; }

        public StreamPassModel StreamPass { get; private set; }

        public ConsumableSearchFilterViewModel(CurrencyModel currency)
        {
            this.Currency = currency;
        }

        public ConsumableSearchFilterViewModel(InventoryModel inventory)
        {
            this.Inventory = inventory;
        }

        public ConsumableSearchFilterViewModel(StreamPassModel streamPass)
        {
            this.StreamPass = streamPass;
        }
    }

    public class UsersMainControlViewModel : WindowControlViewModelBase
    {
        private const string GreaterThanAmountFilter = ">";
        private const string EqualToAmountFilter = "=";
        private const string LessThanAmountFilter = "<";

        public IEnumerable<StreamingPlatformTypeEnum> Platforms { get { return StreamingPlatforms.SelectablePlatforms; } }

        public StreamingPlatformTypeEnum SelectedPlatform
        {
            get { return this.selectedPlatform; }
            set
            {
                this.selectedPlatform = value;
                this.NotifyPropertyChanged();
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

        public IEnumerable<UserSearchFilterTypeEnum> SearchFilterTypes { get { return EnumHelper.GetEnumList<UserSearchFilterTypeEnum>(); } }

        public UserSearchFilterTypeEnum SelectedSearchFilterType
        {
            get { return this.selectedSearchFilterType; }
            set
            {
                this.selectedSearchFilterType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsRoleSearchFilterType");
                this.NotifyPropertyChanged("IsWatchTimeSearchFilterType");
                this.NotifyPropertyChanged("IsConsumablesSearchFilterType");
                this.NotifyPropertyChanged("IsCustomSettingsSearchFilterType");
                this.NotifyPropertyChanged("IsLastSeenSearchFilterType");
            }
        }
        private UserSearchFilterTypeEnum selectedSearchFilterType = UserSearchFilterTypeEnum.None;

        public bool IsRoleSearchFilterType { get { return this.SelectedSearchFilterType == UserSearchFilterTypeEnum.Role; } }

        public IEnumerable<UserRoleEnum> UserRoleSearchFilters { get { return UserRoles.All; } }

        public UserRoleEnum SelectedUserRoleSearchFilter
        {
            get { return this.selectedUserRoleSearchFilter; }
            set
            {
                this.selectedUserRoleSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum selectedUserRoleSearchFilter = UserRoleEnum.User;

        public bool IsWatchTimeSearchFilterType { get { return this.SelectedSearchFilterType == UserSearchFilterTypeEnum.WatchTime; } }

        public IEnumerable<string> WatchTimeComparisonSearchFilters { get { return new List<string>() { GreaterThanAmountFilter, EqualToAmountFilter, LessThanAmountFilter }; } }

        public string SelectedWatchTimeComparisonSearchFilter
        {
            get { return this.selectedWatchTimeComparisonSearchFilter; }
            set
            {
                this.selectedWatchTimeComparisonSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedWatchTimeComparisonSearchFilter = GreaterThanAmountFilter;

        public int WatchTimeAmountSearchFilter
        {
            get { return this.watchTimeAmountSearchFilter; }
            set
            {
                this.watchTimeAmountSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private int watchTimeAmountSearchFilter = 0;

        public bool IsConsumablesSearchFilterType { get { return this.SelectedSearchFilterType == UserSearchFilterTypeEnum.Consumables; } }

        public ObservableCollection<ConsumableSearchFilterViewModel> ConsumablesSearchFilters { get; set; } = new ObservableCollection<ConsumableSearchFilterViewModel>();

        public ConsumableSearchFilterViewModel SelectedConsumablesSearchFilter
        {
            get { return this.selectedConsumablesSearchFilter; }
            set
            {
                this.selectedConsumablesSearchFilter = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsConsumablesSearchFilterInventory");

                this.ConsumablesItemsSearchFilters.Clear();
                this.SelectedConsumablesItemsSearchFilter = null;
                if (this.IsConsumablesSearchFilterInventory)
                {
                    foreach (InventoryItemModel item in this.SelectedConsumablesSearchFilter.Inventory.Items.Values.ToList())
                    {
                        this.ConsumablesItemsSearchFilters.Add(item);
                    }
                }
            }
        }
        private ConsumableSearchFilterViewModel selectedConsumablesSearchFilter;

        public bool IsConsumablesSearchFilterInventory { get { return this.SelectedConsumablesSearchFilter != null && this.SelectedConsumablesSearchFilter.IsInventory; } }

        public ObservableCollection<InventoryItemModel> ConsumablesItemsSearchFilters { get; set; } = new ObservableCollection<InventoryItemModel>();

        public InventoryItemModel SelectedConsumablesItemsSearchFilter
        {
            get { return this.selectedConsumablesItemsSearchFilter; }
            set
            {
                this.selectedConsumablesItemsSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private InventoryItemModel selectedConsumablesItemsSearchFilter;

        public IEnumerable<string> ConsumablesComparisonSearchFilters { get { return new List<string>() { GreaterThanAmountFilter, EqualToAmountFilter, LessThanAmountFilter }; } }

        public string SelectedConsumablesComparisonSearchFilter
        {
            get { return this.selectedConsumablesComparisonSearchFilter; }
            set
            {
                this.selectedConsumablesComparisonSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedConsumablesComparisonSearchFilter = GreaterThanAmountFilter;

        public int ConsumablesAmountSearchFilter
        {
            get { return this.consumablesAmountSearchFilter; }
            set
            {
                this.consumablesAmountSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private int consumablesAmountSearchFilter = 0;

        public bool IsCustomSettingsSearchFilterType { get { return this.SelectedSearchFilterType == UserSearchFilterTypeEnum.CustomSettings; } }

        public bool IsLastSeenSearchFilterType { get { return this.SelectedSearchFilterType == UserSearchFilterTypeEnum.LastSeen; } }

        public IEnumerable<string> LastSeenComparisonSearchFilters { get { return new List<string>() { GreaterThanAmountFilter, EqualToAmountFilter, LessThanAmountFilter }; } }

        public string SelectedLastSeenComparisonSearchFilter
        {
            get { return this.selectedLastSeenComparisonSearchFilter; }
            set
            {
                this.selectedLastSeenComparisonSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedLastSeenComparisonSearchFilter = GreaterThanAmountFilter;

        public int LastSeenAmountSearchFilter
        {
            get { return this.lastSeenAmountSearchFilter; }
            set
            {
                this.lastSeenAmountSearchFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private int lastSeenAmountSearchFilter = 0;

        public ThreadSafeObservableCollection<UserV2ViewModel> Users { get; private set; } = new ThreadSafeObservableCollection<UserV2ViewModel>();

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

        private bool firstVisibleOccurred = false;

        public UsersMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.ExportDataCommand = this.CreateCommand(async () =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowSaveFileDialog("User Data.txt", MixItUp.Base.Resources.TextFileFormatFilter);
                if (!string.IsNullOrEmpty(filePath))
                {
                    List<List<string>> contents = new List<List<string>>();

                    List<string> columns = new List<string>() { "MixItUpID" };
                    StreamingPlatforms.ForEachPlatform(p =>
                    {
                        columns.AddRange(new List<string>() { p.ToString() + "ID", p.ToString() + "Username" });
                    });
                    foreach (var kvp in ChannelSession.Settings.Currency)
                    {
                        columns.Add(kvp.Value.Name.Replace(" ", ""));
                    }
                    foreach (var kvp in ChannelSession.Settings.StreamPass)
                    {
                        columns.Add(kvp.Value.Name.Replace(" ", ""));
                    }
                    columns.AddRange(new List<string>() { "Minutes", "CustomTitle", "TotalStreamsWatched", "TotalAmountDonated", "TotalSubsGifted", "TotalSubsReceived", "TotalChatMessagesSent", "TotalTimesTagged",
                        "TotalCommandsRun", "TotalMonthsSubbed", "LastSeen" });
                    contents.Add(columns);

                    await ServiceManager.Get<UserService>().LoadAllUserData();
                    foreach (UserV2Model u in ChannelSession.Settings.Users.Values.ToList())
                    {
                        UserV2ViewModel user = new UserV2ViewModel(u);

                        List<string> data = new List<string>() { user.ID.ToString() };
                        StreamingPlatforms.ForEachPlatform(p =>
                        {
                            UserPlatformV2ModelBase platformUser = user.GetPlatformData<UserPlatformV2ModelBase>(p);
                            if (platformUser != null)
                            {
                                data.AddRange(new List<string>() { platformUser.ID, platformUser.Username });
                            }
                            else
                            {
                                data.AddRange(new List<string>() { "", "" });
                            }
                        });
                        foreach (var kvp in ChannelSession.Settings.Currency)
                        {
                            data.Add(kvp.Value.GetAmount(user).ToString());
                        }
                        foreach (var kvp in ChannelSession.Settings.StreamPass)
                        {
                            data.Add(kvp.Value.GetAmount(user).ToString());
                        }
                        data.AddRange(new List<string>() { user.OnlineViewingMinutes.ToString(), user.CustomTitle, user.TotalStreamsWatched.ToString(), user.TotalAmountDonated.ToString(), user.TotalSubsGifted.ToString(), user.TotalSubsReceived.ToString(),
                            user.TotalChatMessageSent.ToString(), user.TotalTimesTagged.ToString(), user.TotalCommandsRun.ToString(), user.TotalMonthsSubbed.ToString(), user.LastActivity.ToFriendlyDateTimeString() });
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
                    await ServiceManager.Get<UserService>().LoadAllUserData();

                    IEnumerable<UserV2Model> data = ChannelSession.Settings.Users.Values.ToList();
                    if (data.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(this.UsernameFilter))
                        {
                            string filter = this.UsernameFilter.ToLower();
                            if (this.SelectedPlatform != StreamingPlatformTypeEnum.All)
                            {
                                data = data.Where(u => u.GetPlatformUsername(this.SelectedPlatform) != null &&
                                    (u.GetPlatformUsername(this.SelectedPlatform).Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                    u.GetPlatformDisplayName(this.SelectedPlatform).Contains(filter, StringComparison.OrdinalIgnoreCase)));
                            }
                            else
                            {
                                data = data.Where(u => u.GetAllPlatformUsernames().Any(username => (username != null) ? username.Contains(filter, StringComparison.OrdinalIgnoreCase) : false) ||
                                    u.GetAllPlatformDisplayNames().Any(username => (username != null) ? username.Contains(filter, StringComparison.OrdinalIgnoreCase) : false));
                            }
                        }

                        if (this.SelectedSearchFilterType != UserSearchFilterTypeEnum.None)
                        {
                            if (this.IsRoleSearchFilterType)
                            {
                                data = data.Where(u => u.GetAllPlatformData().Any(p => p.Roles.Contains(this.SelectedUserRoleSearchFilter)));
                            }
                            else if (this.IsWatchTimeSearchFilterType && this.WatchTimeAmountSearchFilter >= 0)
                            {
                                if (this.SelectedWatchTimeComparisonSearchFilter.Equals(GreaterThanAmountFilter))
                                {
                                    data = data.Where(u => u.OnlineViewingMinutes > this.WatchTimeAmountSearchFilter);
                                }
                                else if (this.SelectedWatchTimeComparisonSearchFilter.Equals(LessThanAmountFilter))
                                {
                                    data = data.Where(u => u.OnlineViewingMinutes < this.WatchTimeAmountSearchFilter);
                                }
                                else if (this.SelectedWatchTimeComparisonSearchFilter.Equals(EqualToAmountFilter))
                                {
                                    data = data.Where(u => u.OnlineViewingMinutes == this.WatchTimeAmountSearchFilter);
                                }
                            }
                            else if (this.IsConsumablesSearchFilterType && this.SelectedConsumablesSearchFilter != null && this.ConsumablesAmountSearchFilter >= 0)
                            {
                                if (this.SelectedConsumablesSearchFilter.Currency != null)
                                {
                                    if (this.SelectedConsumablesComparisonSearchFilter.Equals(GreaterThanAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.Currency.GetAmount(u) > this.ConsumablesAmountSearchFilter);
                                    }
                                    else if (this.SelectedConsumablesComparisonSearchFilter.Equals(LessThanAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.Currency.GetAmount(u) < this.ConsumablesAmountSearchFilter);
                                    }
                                    else if (this.SelectedConsumablesComparisonSearchFilter.Equals(EqualToAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.Currency.GetAmount(u) == this.ConsumablesAmountSearchFilter);
                                    }
                                }
                                else if (this.SelectedConsumablesSearchFilter.Inventory != null && this.SelectedConsumablesItemsSearchFilter != null)
                                {
                                    if (this.SelectedConsumablesComparisonSearchFilter.Equals(GreaterThanAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.Inventory.GetAmount(u, this.SelectedConsumablesItemsSearchFilter) > this.ConsumablesAmountSearchFilter);
                                    }
                                    else if (this.SelectedConsumablesComparisonSearchFilter.Equals(LessThanAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.Inventory.GetAmount(u, this.SelectedConsumablesItemsSearchFilter) < this.ConsumablesAmountSearchFilter);
                                    }
                                    else if (this.SelectedConsumablesComparisonSearchFilter.Equals(EqualToAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.Inventory.GetAmount(u, this.SelectedConsumablesItemsSearchFilter) == this.ConsumablesAmountSearchFilter);
                                    }
                                }
                                else if (this.SelectedConsumablesSearchFilter.StreamPass != null)
                                {
                                    if (this.SelectedConsumablesComparisonSearchFilter.Equals(GreaterThanAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.StreamPass.GetAmount(u) > this.ConsumablesAmountSearchFilter);
                                    }
                                    else if (this.SelectedConsumablesComparisonSearchFilter.Equals(LessThanAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.StreamPass.GetAmount(u) < this.ConsumablesAmountSearchFilter);
                                    }
                                    else if (this.SelectedConsumablesComparisonSearchFilter.Equals(EqualToAmountFilter))
                                    {
                                        data = data.Where(u => this.SelectedConsumablesSearchFilter.StreamPass.GetAmount(u) == this.ConsumablesAmountSearchFilter);
                                    }
                                }
                            }
                            else if (this.IsCustomSettingsSearchFilterType)
                            {
                                data = data.Where(u => u.IsSpecialtyExcluded || u.CustomTitle != null || u.CustomCommandIDs.Count > 0 || u.EntranceCommandID != Guid.Empty || !string.IsNullOrEmpty(u.Notes));
                            }
                            else if (this.IsLastSeenSearchFilterType && this.LastSeenAmountSearchFilter >= 0)
                            {
                                DateTime lastSeenDate = DateTimeOffset.Now.Date.Subtract(TimeSpan.FromDays(this.LastSeenAmountSearchFilter));
                                if (this.SelectedLastSeenComparisonSearchFilter.Equals(GreaterThanAmountFilter))
                                {
                                    data = data.Where(u => u.LastActivity.Date < lastSeenDate);
                                }
                                else if (this.SelectedLastSeenComparisonSearchFilter.Equals(LessThanAmountFilter))
                                {
                                    data = data.Where(u => u.LastActivity.Date > lastSeenDate);
                                }
                                else if (this.SelectedLastSeenComparisonSearchFilter.Equals(EqualToAmountFilter))
                                {
                                    data = data.Where(u => u.LastActivity.Date == lastSeenDate);

                                }
                            }
                        }

                        //if (this.SortColumnIndex == 0) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.Username) : data.OrderBy(u => u.Username); }
                        //else if (this.SortColumnIndex == 1) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.Platforms) : data.OrderBy(u => u.Platforms); }
                        //else if (this.SortColumnIndex == 2) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryRole) : data.OrderBy(u => u.PrimaryRole); }
                        //else if (this.SortColumnIndex == 3) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.OnlineViewingMinutes) : data.OrderBy(u => u.OnlineViewingMinutes); }
                        //else if (this.SortColumnIndex == 4) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryCurrency) : data.OrderBy(u => u.PrimaryCurrency); }
                        //else if (this.SortColumnIndex == 5) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryRankPoints) : data.OrderBy(u => u.PrimaryRankPoints); }
                    }

                    List<UserV2ViewModel> users = new List<UserV2ViewModel>();
                    foreach (UserV2Model u in data)
                    {
                        users.Add(new UserV2ViewModel(u));
                    }
                    this.Users.ClearAndAddRange(users);
                }
                catch (Exception ex) { Logger.Log(ex); }

                await DispatcherHelper.Dispatcher.InvokeAsync(() =>
                {
                    this.EndLoadingOperation();
                    return Task.CompletedTask;
                });
            });
        }

        public async Task DeleteUser(UserV2Model user)
        {
            if (await DialogHelper.ShowConfirmation(Resources.DeleteUserDataPrompt))
            {
                ServiceManager.Get<UserService>().DeleteUserData(user.ID);
            }
            this.RefreshUsers();
        }

        public async Task FindAndAddUser(StreamingPlatformTypeEnum platform, string username)
        {
            UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(platform, platformUsername: username, performPlatformSearch: true);
            if (user != null)
            {
                await DialogHelper.ShowMessage(string.Format(MixItUp.Base.Resources.UsersSuccessfullyFoundUser, user.DisplayName));
                await this.RefreshUsersAsync();
            }
            else
            {
                await DialogHelper.ShowMessage(string.Format(MixItUp.Base.Resources.UsersUnableToFindUser, username));
            }
        }

        protected override Task OnVisibleInternal()
        {
            if (this.firstVisibleOccurred)
            {
                this.RefreshUsers();
            }
            this.firstVisibleOccurred = true;

            this.ConsumablesSearchFilters.Clear();
            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values.ToList())
            {
                this.ConsumablesSearchFilters.Add(new ConsumableSearchFilterViewModel(currency));
            }

            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values.ToList())
            {
                this.ConsumablesSearchFilters.Add(new ConsumableSearchFilterViewModel(streamPass));
            }

            foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values.ToList())
            {
                this.ConsumablesSearchFilters.Add(new ConsumableSearchFilterViewModel(inventory));
            }

            this.SelectedConsumablesSearchFilter = null;

            return base.OnVisibleInternal();
        }
    }
}
