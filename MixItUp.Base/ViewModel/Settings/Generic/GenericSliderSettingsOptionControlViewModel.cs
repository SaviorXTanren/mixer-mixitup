using System;

namespace MixItUp.Base.ViewModel.Settings.Generic
{
    public class GenericSliderSettingsOptionControlViewModel : GenericSettingsOptionControlViewModelBase
    {
        public int Value
        {
            get { return this.value; }
            set
            {
                if (value >= 0)
                {
                    this.value = value;
                    this.valueSetter(value);
                }
                this.NotifyPropertyChanged();
            }
        }
        private int value;
        private Action<int> valueSetter;

        public int Minimum
        {
            get { return this.minimum; }
            set
            {
                this.minimum = value;
                this.NotifyPropertyChanged();
            }
        }
        private int minimum = int.MaxValue;

        public int Maximum
        {
            get { return this.maximum; }
            set
            {
                this.maximum = value;
                this.NotifyPropertyChanged();
            }
        }
        private int maximum = int.MinValue;

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

        public GenericSliderSettingsOptionControlViewModel(string name, int initialValue, int minimum, int maximum, Action<int> valueSetter, string tooltip = null)
            : base(name, tooltip)
        {
            this.value = initialValue;
            this.minimum = minimum;
            this.maximum = maximum;
            this.valueSetter = valueSetter;
        }
    }
}
