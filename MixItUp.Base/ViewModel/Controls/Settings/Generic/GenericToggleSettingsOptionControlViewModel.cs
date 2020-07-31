using System;

namespace MixItUp.Base.ViewModel.Controls.Settings.Generic
{
    public class GenericToggleSettingsOptionControlViewModel : GenericSettingsOptionControlViewModelBase
    {
        public bool Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.valueSetter(value);
                this.NotifyPropertyChanged();
            }
        }
        private bool value;
        private Action<bool> valueSetter;

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

        public GenericToggleSettingsOptionControlViewModel(string name, bool initialValue, Action<bool> valueSetter, string tooltip = null)
            : base(name, tooltip)
        {
            this.value = initialValue;
            this.valueSetter = valueSetter;
        }
    }
}
