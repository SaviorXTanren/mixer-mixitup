using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class IFTTTActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.IFTTT; } }

        public string EventName
        {
            get { return this.eventName; }
            set
            {
                this.eventName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string eventName;

        public bool IFTTTNotEnabled { get { return !ServiceManager.Get<IFTTTService>().IsConnected; } }

        public string Value1
        {
            get { return this.value1; }
            set
            {
                this.value1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value1;

        public string Value2
        {
            get { return this.value2; }
            set
            {
                this.value2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value2;

        public string Value3
        {
            get { return this.value3; }
            set
            {
                this.value3 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string value3;

        public IFTTTActionEditorControlViewModel(IFTTTActionModel action)
            : base(action)
        {
            this.EventName = action.EventName;
            this.Value1 = action.EventValue1;
            this.Value2 = action.EventValue2;
            this.Value3 = action.EventValue3;
        }

        public IFTTTActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.EventName))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.IFTTTActionMissingEventName));
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal() { return Task.FromResult<ActionModelBase>(new IFTTTActionModel(this.EventName, this.Value1, this.Value2, this.Value3)); }
    }
}
