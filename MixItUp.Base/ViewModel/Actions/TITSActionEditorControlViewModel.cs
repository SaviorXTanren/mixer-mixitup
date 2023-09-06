using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TITSActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.TITS; } }

        public bool TITSConnected { get { return ServiceManager.Get<TITSService>().IsConnected; } }
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

        public ThreadSafeObservableCollection<TITSItem> Items { get; set; } = new ThreadSafeObservableCollection<TITSItem>();

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

        public int ThrowAmount
        {
            get { return this.throwAmount; }
            set
            {
                this.throwAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int throwAmount = 1;

        public bool ShowActivateTriggerGrid { get { return this.SelectedActionType == TITSActionTypeEnum.ActivateTrigger; } }

        public ThreadSafeObservableCollection<TITSTrigger> Triggers { get; set; } = new ThreadSafeObservableCollection<TITSTrigger>();

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
                    if (this.SelectedItem == null || this.ThrowDelayTime <= 0.0 || this.ThrowAmount <= 0)
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
            if (ChannelSession.Settings.TITSOAuthToken != null && !this.TITSConnected)
            {
                Result result = await ServiceManager.Get<TITSService>().Connect(ChannelSession.Settings.TITSOAuthToken);
                if (!result.Success)
                {
                    return;
                }
            }

            if (this.TITSConnected)
            {
                foreach (TITSItem item in await ServiceManager.Get<TITSService>().GetAllItems())
                {
                    this.Items.Add(item);
                }
                this.SelectedItem = this.selectedItem = this.Items.FirstOrDefault(i => string.Equals(i.ID, this.throwItemID));

                if (this.Items.Count == 0)
                {
                    Logger.Log(LogLevel.Error, "TITS Action - No items loaded");
                }

                foreach (TITSTrigger trigger in await ServiceManager.Get<TITSService>().GetAllTriggers())
                {
                    this.Triggers.Add(trigger);
                }
                this.SelectedTrigger = this.selectedTrigger = this.Triggers.FirstOrDefault(i => string.Equals(i.ID, this.triggerID));

                if (this.Triggers.Count == 0)
                {
                    Logger.Log(LogLevel.Error, "TITS Action - No triggers loaded");
                }
            }

            await base.OnOpenInternal();
        }
    }
}