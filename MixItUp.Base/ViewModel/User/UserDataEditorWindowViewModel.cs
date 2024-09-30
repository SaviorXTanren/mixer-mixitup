using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public class UserConsumableEditorViewModel : UIViewModelBase
    {
        public string Name
        {
            get
            {
                if (this.currency != null)
                {
                    return this.currency.Name;
                }
                else if (this.inventory != null && this.item != null)
                {
                    return this.item.Name;
                }
                else if (this.streamPass != null)
                {
                    return this.streamPass.Name;
                }
                return string.Empty;
            }
        }

        public int Amount
        {
            get
            {
                if (this.currency != null)
                {
                    return this.currency.GetAmount(this.user);
                }
                else if (this.inventory != null && this.item != null)
                {
                    return this.inventory.GetAmount(this.user, this.item);
                }
                else if (this.streamPass != null)
                {
                    return this.streamPass.GetAmount(this.user);
                }
                return 0;
            }
            set
            {
                if (value >= 0)
                {
                    if (this.currency != null)
                    {
                        this.currency.SetAmount(this.user, value);
                    }
                    else if (this.inventory != null && this.item != null)
                    {
                        this.inventory.SetAmount(this.user, this.item, value);
                    }
                    else if (this.streamPass != null)
                    {
                        this.streamPass.SetAmount(this.user, value);
                    }
                }
                this.NotifyPropertyChanged();
            }
        }

        private UserV2ViewModel user;

        private CurrencyModel currency;

        private InventoryModel inventory;
        private InventoryItemModel item;

        private StreamPassModel streamPass;

        public UserConsumableEditorViewModel(UserV2ViewModel user, CurrencyModel currency)
            : this(user)
        {
            this.currency = currency;
        }

        public UserConsumableEditorViewModel(UserV2ViewModel user, InventoryModel inventory, InventoryItemModel item)
            : this(user)
        {
            this.inventory = inventory;
            this.item = item;
        }

        public UserConsumableEditorViewModel(UserV2ViewModel user, StreamPassModel streamPass)
            : this(user)
        {
            this.streamPass = streamPass;
        }

        private UserConsumableEditorViewModel(UserV2ViewModel user)
        {
            this.user = user;
        }
    }

    public class UserInventoryEditorViewModel
    {
        public string Name { get { return this.inventory.Name; } }

        public ObservableCollection<UserConsumableEditorViewModel> Items { get; set; } = new ObservableCollection<UserConsumableEditorViewModel>();

        private UserV2ViewModel user;

        private InventoryModel inventory;

        public UserInventoryEditorViewModel(UserV2ViewModel user, InventoryModel inventory)
        {
            this.user = user;
            this.inventory = inventory;

            List<UserConsumableEditorViewModel> itemsToAdd = new List<UserConsumableEditorViewModel>();
            foreach (InventoryItemModel item in this.inventory.Items.Values)
            {
                itemsToAdd.Add(new UserConsumableEditorViewModel(this.user, this.inventory, item));
            }
            this.Items.ClearAndAddRange(itemsToAdd);
        }
    }

    public class UserMetricViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public UserMetricViewModel(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class UserDataEditorWindowViewModel : UIViewModelBase
    {
        public const string UserEntranceCommandName = "Entrance Command";

        public UserV2ViewModel User { get; private set; }

        public IEnumerable<string> PlatformImages
        {
            get
            {
                List<string> results = new List<string>();
                foreach (StreamingPlatformTypeEnum platform in this.User.AllPlatforms)
                {
                    results.Add(StreamingPlatforms.GetPlatformImage(platform));
                }
                return results;
            }
        }

        public int ViewingHours
        {
            get { return this.User.OnlineViewingHoursOnly; }
            set
            {
                this.User.OnlineViewingHoursOnly = value;
                this.NotifyPropertyChanged();
            }
        }

        public int ViewingMinutes
        {
            get { return this.User.OnlineViewingMinutesOnly; }
            set
            {
                this.User.OnlineViewingMinutesOnly = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ViewingHours");
            }
        }

        public ObservableCollection<UserConsumableEditorViewModel> Consumables { get; set; } = new ObservableCollection<UserConsumableEditorViewModel>();

        public ObservableCollection<UserInventoryEditorViewModel> Inventories { get; set; } = new ObservableCollection<UserInventoryEditorViewModel>();

        public bool HasInventorySelected
        {
            get { return this.selectedInventory != null; }
        }

        private UserInventoryEditorViewModel selectedInventory;
        public UserInventoryEditorViewModel SelectedInventory
        {
            get { return this.selectedInventory; }
            set
            {
                this.selectedInventory = value;
                this.SelectedItem = this.selectedInventory?.Items?.FirstOrDefault();

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("SelectedInventoryItems");
                this.NotifyPropertyChanged("HasInventorySelected");
            }
        }

        public IEnumerable<UserConsumableEditorViewModel> SelectedInventoryItems
        {
            get { return this.selectedInventory?.Items?.OrderBy(i => i.Name); }
        }

        public bool HasItemSelected
        {
            get { return this.selectedItem != null; }
        }

        private UserConsumableEditorViewModel selectedItem;
        public UserConsumableEditorViewModel SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                this.selectedItem = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasItemSelected");
            }
        }

        public bool HasConsumableSelected
        {
            get { return this.selectedConsumable != null; }
        }

        private UserConsumableEditorViewModel selectedConsumable;
        public UserConsumableEditorViewModel SelectedConsumable
        {
            get { return this.selectedConsumable; }
            set
            {
                this.selectedConsumable = value;

                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasConsumableSelected");
            }
        }

        public ObservableCollection<UserOnlyChatCommandModel> UserOnlyChatCommands { get; set; } = new ObservableCollection<UserOnlyChatCommandModel>();
        public bool HasUserOnlyChatCommands { get { return this.UserOnlyChatCommands.Count > 0; } }

        public CommandModelBase EntranceCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.User.EntranceCommandID); }
            set
            {
                if (value == null)
                {
                    ChannelSession.Settings.RemoveCommand(this.User.EntranceCommandID);
                    this.User.EntranceCommandID = Guid.Empty;
                }
                else
                {
                    this.User.EntranceCommandID = value.ID;
                }
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasEntranceCommand");
                this.NotifyPropertyChanged("DoesNotHaveEntranceCommand");
            }
        }
        public bool HasEntranceCommand { get { return this.EntranceCommand != null; } }
        public bool DoesNotHaveEntranceCommand { get { return !this.HasEntranceCommand; } }

        public bool IsPatreonConnected { get { return ServiceManager.Get<PatreonService>().IsConnected; } }
        public IEnumerable<PatreonCampaignMember> PatreonUsers { get { return ServiceManager.Get<PatreonService>().CampaignMembers.ToList(); } }
        public PatreonCampaignMember PatreonUser
        {
            get { return this.User.PatreonUser; }
            set
            {
                this.User.PatreonUser = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool IsSpecialtyExcluded
        {
            get { return this.User.IsSpecialtyExcluded; }
            set
            {
                this.User.IsSpecialtyExcluded = value;
                if (this.IsSpecialtyExcluded)
                {
                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.ResetAmount(this.User);
                    }
                    ChannelSession.Settings.Users.ManualValueChanged(this.User.ID);
                }
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<UserMetricViewModel> Metrics1 { get; private set; } = new ObservableCollection<UserMetricViewModel>();
        public ObservableCollection<UserMetricViewModel> Metrics2 { get; private set; } = new ObservableCollection<UserMetricViewModel>();

        public string Notes
        {
            get { return this.User.Notes; }
            set
            {
                this.User.Notes = value;
                this.NotifyPropertyChanged();
            }
        }

        public UserDataEditorWindowViewModel(UserV2Model user)
        {
            this.User = new UserV2ViewModel(user);
        }

        public async Task Load()
        {
            await this.User.Refresh(force: true);

            if (ServiceManager.Get<PatreonService>().IsConnected)
            {
                this.PatreonUser = this.User.PatreonUser;
            }

            List<UserConsumableEditorViewModel> consumablesToAdd = new List<UserConsumableEditorViewModel>();
            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values.ToList())
            {
                consumablesToAdd.Add(new UserConsumableEditorViewModel(this.User, currency));
            }
            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values.ToList())
            {
                consumablesToAdd.Add(new UserConsumableEditorViewModel(this.User, streamPass));
            }
            this.Consumables.ClearAndAddRange(consumablesToAdd);
            this.SelectedConsumable = this.Consumables.FirstOrDefault();

            List<UserInventoryEditorViewModel> inventoriesToAdd = new List<UserInventoryEditorViewModel>();
            foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values.ToList())
            {
                inventoriesToAdd.Add(new UserInventoryEditorViewModel(this.User, inventory));
            }
            this.Inventories.ClearAndAddRange(inventoriesToAdd);
            this.SelectedInventory = this.Inventories.FirstOrDefault();

            this.RefreshUserOnlyChatCommands();

            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.StreamsWatched, this.User.TotalStreamsWatched.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.CumulativeMonthsSubbed, this.User.TotalMonthsSubbed.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.SubsGifted, this.User.TotalSubsGifted.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.SubsReceived, this.User.TotalSubsReceived.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.ChatMessagesSent, this.User.TotalChatMessageSent.ToString()));
            this.Metrics1.Add(new UserMetricViewModel(MixItUp.Base.Resources.CommandsRun, this.User.TotalCommandsRun.ToString()));

            this.Metrics2.Add(new UserMetricViewModel(MixItUp.Base.Resources.TaggedInChat, this.User.TotalTimesTagged.ToString()));
            this.Metrics2.Add(new UserMetricViewModel(MixItUp.Base.Resources.AmountDonated, CurrencyHelper.ToCurrencyString(this.User.TotalAmountDonated)));

            if (this.User.HasPlatformData(StreamingPlatformTypeEnum.Twitch))
            {
                TwitchUserPlatformV2Model pUser = this.User.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);
                this.Metrics2.Add(new UserMetricViewModel(MixItUp.Base.Resources.BitsCheered, pUser.TotalBitsCheered.ToString()));
            }
        }

        public void AddUserOnlyChatCommand(UserOnlyChatCommandModel command)
        {
            this.User.CustomCommandIDs.Add(command.ID);
            this.RefreshUserOnlyChatCommands();
        }

        public void RemoveUserOnlyChatCommand(UserOnlyChatCommandModel command)
        {
            this.User.CustomCommandIDs.Remove(command.ID);
            ChannelSession.Settings.RemoveCommand(command.ID);
            this.RefreshUserOnlyChatCommands();
        }

        public void RefreshUserOnlyChatCommands()
        {
            List<UserOnlyChatCommandModel> commands = new List<UserOnlyChatCommandModel>();
            foreach (Guid commandID in this.User.CustomCommandIDs)
            {
                UserOnlyChatCommandModel command = ChannelSession.Settings.GetCommand<UserOnlyChatCommandModel>(commandID);
                if (command != null)
                {
                    commands.Add(command);
                }
            }
            this.UserOnlyChatCommands.ClearAndAddRange(commands);
            this.NotifyPropertyChanged("HasUserOnlyChatCommands");
        }
    }
}
