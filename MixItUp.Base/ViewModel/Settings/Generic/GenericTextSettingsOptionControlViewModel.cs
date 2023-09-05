using System;

namespace MixItUp.Base.ViewModel.Settings.Generic
{
    public class GenericTextSettingsOptionControlViewModel : GenericSettingsOptionControlViewModelBase
    {
        public string Value
        {
            get { return this.value; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.value = value;
                    this.valueSetter(value);
                }
                this.NotifyPropertyChanged();
            }
        }
        private string value;
        private Action<string> valueSetter;

        public GenericTextSettingsOptionControlViewModel(string name, string initialValue, Action<string> valueSetter, string tooltip = null)
            : base(name, tooltip)
        {
            this.value = initialValue;
            this.valueSetter = valueSetter;
        }
    }
}
