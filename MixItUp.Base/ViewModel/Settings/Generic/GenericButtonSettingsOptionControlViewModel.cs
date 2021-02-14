using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings.Generic
{
    public class GenericButtonSettingsOptionControlViewModel : GenericSettingsOptionControlViewModelBase
    {
        public string ButtonName { get; set; }

        public ICommand Command { get; set; }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                this.enabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool enabled = true;

        public GenericButtonSettingsOptionControlViewModel(string name, string buttonName, ICommand command, string tooltip = null)
            : base(name, tooltip)
        {
            this.ButtonName = buttonName;
            this.Command = command;
        }
    }
}
