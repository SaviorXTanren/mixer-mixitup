using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Controls.Settings.Generic
{
    public class GenericCombBoxSettingsOptionControlViewModel<T> : GenericSettingsOptionControlViewModelBase
    {
        public IEnumerable<T> Values { get; set; }

        public T Value
        {
            get { return this.value; }
            set
            {
                if (!object.Equals(this.value, value))
                {
                    this.value = value;
                    this.valueSetter(value);
                    this.NotifyPropertyChanged();
                }
            }
        }
        private T value;
        private Action<T> valueSetter;

        public int Width { get; set; } = 200;

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

        public GenericCombBoxSettingsOptionControlViewModel(string name, IEnumerable<T> values, T initialValue, Action<T> valueSetter, string tooltip = null)
            : base(name, tooltip)
        {
            this.Values = values;
            this.value = initialValue;
            this.valueSetter = valueSetter;
        }
    }
}
