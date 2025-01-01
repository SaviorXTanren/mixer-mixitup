using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class CounterActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Counter; } }

        public ObservableCollection<string> Counters { get; set; } = new ObservableCollection<string>();

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

        public string CounterName
        {
            get { return this.counterName; }
            set
            {
                this.counterName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string counterName;

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
            this.CounterName = action.CounterName?.ToLower();
            this.SelectedActionType = action.ActionType;
            this.Amount = action.Amount;

            if (ChannelSession.Settings.Counters.ContainsKey(this.CounterName))
            {
                CounterModel counter = ChannelSession.Settings.Counters[this.CounterName];
                this.SaveToFile = counter.SaveToFile;
                this.ResetOnLoad = counter.ResetOnLoad;
            }
        }

        public CounterActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.CounterName))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.CounterActionMissingName));
            }

            this.CounterName = this.CounterName.Replace("$", "");
            if (!SpecialIdentifierStringBuilder.IsValidSpecialIdentifier(this.CounterName))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.CounterActionInvalidName));
            }

            if (this.CanSetAmount && string.IsNullOrEmpty(this.Amount))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.CounterActionMissingAmount));
            }

            return Task.FromResult(new Result());
        }

        protected override async Task OnOpenInternal()
        {
            foreach (var kvp in ChannelSession.Settings.Counters.ToList())
            {
                this.Counters.Add(kvp.Key);
            }
            await base.OnOpenInternal();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            string counterName = this.CounterName.ToLower();

            CounterModel.CreateCounter(counterName, this.SaveToFile, this.ResetOnLoad);

            return Task.FromResult<ActionModelBase>(new CounterActionModel(counterName, this.SelectedActionType, this.Amount));
        }
    }
}
