namespace MixItUp.Base.Model.Serial
{
    public class SerialDeviceModel
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public bool DTREnabled { get; set; }
        public bool RTSEnabled { get; set; }

        public SerialDeviceModel() { }

        public SerialDeviceModel(string portName, int baudRate, bool dtrEnabled, bool rtsEnabled)
        {
            this.PortName = portName;
            this.BaudRate = baudRate;
            this.DTREnabled = dtrEnabled;
            this.RTSEnabled = rtsEnabled;
        }
    }
}
