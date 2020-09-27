using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public class CounterActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Counter; } }

        public bool SaveToFile
        {
            get { return this.saveToFile; }
            set
            {
                this.saveToFile = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool saveToFile;

        public bool ResetOnLoad
        {
            get { return this.resetOnLoad; }
            set
            {
                this.resetOnLoad = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool resetOnLoad;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public IEnumerable<CounterActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<CounterActionTypeEnum>(); } }

        public CounterActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CanSetAmount");
            }
        }
        private CounterActionTypeEnum selectedActionType;

        public string Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string amount;

        public bool CanSetAmount { get { return this.SelectedActionType == CounterActionTypeEnum.Set || this.SelectedActionType == CounterActionTypeEnum.Update; } }

        public CounterActionEditorControlViewModel(CounterActionModel action)
            : base(action)
        {
            this.Name = action.Name;
            this.SelectedActionType = action.ActionType;
            this.Amount = action.Amount;

            if (ChannelSession.Settings.Counters.ContainsKey(this.Name))
            {
                CounterModel counter = ChannelSession.Settings.Counters[this.Name];
                this.SaveToFile = counter.SaveToFile;
                this.ResetOnLoad = counter.ResetOnLoad;
            }
        }

        public CounterActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.CounterActionMissingName));
            }

            if (this.CanSetAmount && string.IsNullOrEmpty(this.Amount))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.CounterActionMissingAmount));
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (!ChannelSession.Settings.Counters.ContainsKey(this.Name))
            {
                ChannelSession.Settings.Counters[this.Name] = new CounterModel(this.Name);
            }
            CounterModel counter = ChannelSession.Settings.Counters[this.Name];
            this.SaveToFile = counter.SaveToFile;
            this.ResetOnLoad = counter.ResetOnLoad;

            return Task.FromResult<ActionModelBase>(new CounterActionModel(this.Name, this.SelectedActionType, this.Amount));
        }
    }
}
