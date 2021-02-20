using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class SerialActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Serial; } }

        public IEnumerable<SerialDeviceModel> Devices { get { return ChannelSession.Settings.SerialDevices.ToList(); } }

        public SerialDeviceModel SelectedDevice
        {
            get { return this.selectedDevice; }
            set
            {
                this.selectedDevice = value;
                this.NotifyPropertyChanged();
            }
        }
        private SerialDeviceModel selectedDevice;

        public string Message
        {
            get { return this.message; }
            set
            {
                this.message = value;
                this.NotifyPropertyChanged();
            }
        }
        private string message;

        public SerialActionEditorControlViewModel(SerialActionModel action)
            : base(action)
        {
            this.SelectedDevice = this.Devices.FirstOrDefault(d => d.PortName.Equals(action.PortName));
            this.Message = action.Message;
        }

        public SerialActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.SelectedDevice == null)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.SerialActionMissingDevice));
            }

            if (string.IsNullOrEmpty(this.Message))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.SerialActionMissingMessage));
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal() { return Task.FromResult<ActionModelBase>(new SerialActionModel(this.SelectedDevice.PortName, this.Message)); }
    }
}
