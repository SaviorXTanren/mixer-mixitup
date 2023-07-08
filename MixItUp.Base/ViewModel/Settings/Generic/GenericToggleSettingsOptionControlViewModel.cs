using System;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Settings.Generic
{
    public class GenericToggleSettingsOptionControlViewModel : GenericSettingsOptionControlViewModelBase
    {
        public bool Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.NotifyPropertyChanged();

                if (this.valueSetterAsync != null)
                {
                    this.valueSetterAsync(value);
                }
                else
                {
                    this.valueSetter(value);
                }
            }
        }
        private bool value;

        private Action<bool> valueSetter;
        private Func<bool, Task> valueSetterAsync;

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

        public GenericToggleSettingsOptionControlViewModel(string name, bool initialValue, Func<bool, Task> valueSetterAsync, string tooltip = null)
            : base(name, tooltip)
        {
            this.value = initialValue;
            this.valueSetterAsync = valueSetterAsync;
        }
    }
}
