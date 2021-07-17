using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
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
            }
        }
        private UserSearchFilterTypeEnum selectedSearchFilterType = UserSearchFilterTypeEnum.None;

        public bool IsRoleSearchFilterType { get { return this.SelectedSearchFilterType == UserSearchFilterTypeEnum.Role; } }

        public IEnumerable<UserRoleEnum> UserRoleSearchFilters { get { return UserDataModel.GetSelectableUserRoles(); } }

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

        public ThreadSafeObservableCollection<ConsumableSearchFilterViewModel> ConsumablesSearchFilters { get; set; } = new ThreadSafeObservableCollection<ConsumableSearchFilterViewModel>();

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

        public ThreadSafeObservableCollection<InventoryItemModel> ConsumablesItemsSearchFilters { get; set; } = new ThreadSafeObservableCollection<InventoryItemModel>();

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

        public bool IsDescendingSort { get { return this.SortDirection == ListSortDirection.Descending; } }

        public ICommand ExportDataCommand { get; private set; }

        private bool firstVisibleOccurred = false;

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

                    await ChannelSession.Settings.LoadAllUserData();
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
                    await ChannelSession.Settings.LoadAllUserData();

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

                    if (this.SelectedSearchFilterType != UserSearchFilterTypeEnum.None)
                    {
                        if (this.IsRoleSearchFilterType)
                        {
                            data = data.Where(u => u.UserRoles.Contains(this.SelectedUserRoleSearchFilter));
                        }
                        else if (this.IsWatchTimeSearchFilterType && this.WatchTimeAmountSearchFilter > 0)
                        {
                            if (this.SelectedWatchTimeComparisonSearchFilter.Equals(GreaterThanAmountFilter))
                            {
                                data = data.Where(u => u.ViewingMinutes > this.WatchTimeAmountSearchFilter);
                            }
                            else if (this.SelectedWatchTimeComparisonSearchFilter.Equals(LessThanAmountFilter))
                            {
                                data = data.Where(u => u.ViewingMinutes < this.WatchTimeAmountSearchFilter);
                            }
                            else if (this.SelectedWatchTimeComparisonSearchFilter.Equals(EqualToAmountFilter))
                            {
                                data = data.Where(u => u.ViewingMinutes == this.WatchTimeAmountSearchFilter);
                            }
                        }
                        else if (this.IsConsumablesSearchFilterType && this.SelectedConsumablesSearchFilter != null && this.ConsumablesAmountSearchFilter > 0)
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
                            data = data.Where(u => u.IsCurrencyRankExempt || u.CustomTitle != null || u.CustomCommandIDs.Count > 0 || u.EntranceCommandID != Guid.Empty || !string.IsNullOrEmpty(u.Notes));
                        }
                    }

                    if (this.SortColumnIndex == 0) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.Username) : data.OrderBy(u => u.Username); }
                    else if (this.SortColumnIndex == 1) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.Platform) : data.OrderBy(u => u.Platform); }
                    else if (this.SortColumnIndex == 2) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryRole) : data.OrderBy(u => u.PrimaryRole); }
                    else if (this.SortColumnIndex == 3) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.ViewingMinutes) : data.OrderBy(u => u.ViewingMinutes); }
                    else if (this.SortColumnIndex == 4) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryCurrency) : data.OrderBy(u => u.PrimaryCurrency); }
                    else if (this.SortColumnIndex == 5) { data = this.IsDescendingSort ? data.OrderByDescending(u => u.PrimaryRankPoints) : data.OrderBy(u => u.PrimaryRankPoints); }

                    this.Users.ClearAndAddRange(data);
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
