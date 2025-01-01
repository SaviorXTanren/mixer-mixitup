using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TITSActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.TITS; } }

        public bool TITSConnected { get { return ChannelSession.Settings.TITSOAuthToken != null || ServiceManager.Get<TITSService>().IsConnected; } }
        public bool TITSNotConnected { get { return !this.TITSConnected; } }

        public IEnumerable<TITSActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<TITSActionTypeEnum>(); } }

        public TITSActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowThrowItemGrid));
                this.NotifyPropertyChanged(nameof(this.ShowActivateTriggerGrid));
            }
        }
        private TITSActionTypeEnum selectedActionType;

        public bool ShowThrowItemGrid { get { return this.SelectedActionType == TITSActionTypeEnum.ThrowItem; } }

        public ObservableCollection<TITSItem> Items { get; set; } = new ObservableCollection<TITSItem>();

        public TITSItem SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                this.selectedItem = value;
                this.NotifyPropertyChanged();
            }
        }
        private TITSItem selectedItem;

        public double ThrowDelayTime
        {
            get { return this.throwDelayTime; }
            set
            {
                this.throwDelayTime = value;
                this.NotifyPropertyChanged();
            }
        }
        private double throwDelayTime = 0.05;

        public string ThrowAmount
        {
            get { return this.throwAmount; }
            set
            {
                this.throwAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string throwAmount = "1";

        public bool ShowActivateTriggerGrid { get { return this.SelectedActionType == TITSActionTypeEnum.ActivateTrigger; } }

        public ObservableCollection<TITSTrigger> Triggers { get; set; } = new ObservableCollection<TITSTrigger>();

        public TITSTrigger SelectedTrigger
        {
            get { return this.selectedTrigger; }
            set
            {
                this.selectedTrigger = value;
                this.NotifyPropertyChanged();
            }
        }
        private TITSTrigger selectedTrigger;

        public bool RefreshItemsCommandEnabled
        {
            get { return this.refreshItemsCommandEnabled; }
            set
            {
                this.refreshItemsCommandEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool refreshItemsCommandEnabled = true;
        public ICommand RefreshItemsCommand { get; set; }

        public bool RefreshTriggersCommandEnabled
        {
            get { return this.refreshTriggersCommandEnabled; }
            set
            {
                this.refreshTriggersCommandEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool refreshTriggersCommandEnabled = true;
        public ICommand RefreshTriggersCommand { get; set; }

        private string throwItemID;
        private string triggerID;

        public TITSActionEditorControlViewModel(TITSActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowThrowItemGrid)
            {
                this.throwItemID = action.ThrowItemID;
                this.throwDelayTime = action.ThrowDelayTime;
                this.ThrowAmount = action.ThrowAmount;
            }
            else if (this.ShowActivateTriggerGrid)
            {
                this.triggerID = action.TriggerID;
            }
        }

        public TITSActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.ShowThrowItemGrid)
            {
                if (this.TITSConnected)
                {
                    if (this.SelectedItem == null || this.ThrowDelayTime <= 0.0 || string.IsNullOrWhiteSpace(this.ThrowAmount))
                    {
                        return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.TITSActionMissingItem));
                    }
                }
            }
            else if (this.ShowActivateTriggerGrid)
            {
                if (this.TITSConnected)
                {
                    if (this.SelectedTrigger == null)
                    {
                        return Task.FromResult<Result>(new Result(MixItUp.Base.Resources.TITSActionMissingTrigger));
                    }
                }
            }
            return Task.FromResult<Result>(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowThrowItemGrid)
            {
                return Task.FromResult<ActionModelBase>(new TITSActionModel(this.SelectedActionType, (this.SelectedItem != null) ? this.SelectedItem.ID : this.throwItemID, this.ThrowDelayTime, this.ThrowAmount));
            }
            else if (this.ShowActivateTriggerGrid)
            {
                return Task.FromResult<ActionModelBase>(new TITSActionModel(this.SelectedActionType, (this.SelectedTrigger != null) ? this.SelectedTrigger.ID : this.triggerID));
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        protected override async Task OnOpenInternal()
        {
            this.RefreshItemsCommand = this.CreateCommand(async () =>
            {
                this.RefreshItemsCommandEnabled = false;

                await this.TryConnectToTITS();

                if (this.TITSConnected)
                {
                    DateTimeOffset lastUpdated = ServiceManager.Get<TITSService>().ItemsLastUpdated;

                    if (await ServiceManager.Get<TITSService>().RequestAllItems())
                    {
                        for (int i = 0; i < 60 && lastUpdated == ServiceManager.Get<TITSService>().ItemsLastUpdated; i++)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }

                this.LoadItems();

                this.RefreshItemsCommandEnabled = true;
            });

            this.RefreshTriggersCommand = this.CreateCommand(async () =>
            {
                this.RefreshTriggersCommandEnabled = false;

                await this.TryConnectToTITS();

                if (this.TITSConnected)
                {
                    DateTimeOffset lastUpdated = ServiceManager.Get<TITSService>().TriggersLastUpdated;

                    if (await ServiceManager.Get<TITSService>().RequestAllTriggers())
                    {
                        for (int i = 0; i < 60 && lastUpdated == ServiceManager.Get<TITSService>().TriggersLastUpdated; i++)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }

                this.LoadTriggers();

                this.RefreshTriggersCommandEnabled = true;
            });

            await this.TryConnectToTITS();

            this.LoadItems();
            this.LoadTriggers();

            await base.OnOpenInternal();
        }

        private async Task TryConnectToTITS()
        {
            if (this.TITSConnected && !ServiceManager.Get<TITSService>().IsWebSocketConnected)
            {
                await ServiceManager.Get<TITSService>().Connect(ChannelSession.Settings.TITSOAuthToken);
            }
        }

        private void LoadItems()
        {
            if (this.TITSConnected)
            {
                this.Items.ClearAndAddRange(ServiceManager.Get<TITSService>().GetAllItems());
                this.SelectedItem = this.selectedItem = this.Items.FirstOrDefault(i => string.Equals(i.ID, this.throwItemID));

                if (this.Items.Count == 0)
                {
                    Logger.Log(LogLevel.Error, "TITS Action - No items loaded");
                }
            }
        }

        private void LoadTriggers()
        {
            if (this.TITSConnected)
            {
                this.Triggers.ClearAndAddRange(ServiceManager.Get<TITSService>().GetAllTriggers());
                this.SelectedTrigger = this.selectedTrigger = this.Triggers.FirstOrDefault(i => string.Equals(i.ID, this.triggerID));

                if (this.Triggers.Count == 0)
                {
                    Logger.Log(LogLevel.Error, "TITS Action - No triggers loaded");
                }
            }
        }
    }
}