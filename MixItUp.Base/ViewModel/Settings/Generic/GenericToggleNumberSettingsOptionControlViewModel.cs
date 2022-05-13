using System;

namespace MixItUp.Base.ViewModel.Settings.Generic
{
    public class GenericToggleNumberSettingsOptionControlViewModel : GenericNumberSettingsOptionControlViewModel
    {
        public new bool Enabled
        {
            get { return this.enabled; }
            set
            {
                this.enabled = value;
                this.NotifyPropertyChanged();
                if (this.Enabled)
                {
                    this.Value = 1;
                }
                else
                {
                    this.Value = 0;
                }
            }
        }
        private bool enabled = true;

        public GenericToggleNumberSettingsOptionControlViewModel(string name, int initialValue, Action<int> valueSetter, string tooltip = null)
            : base(name, initialValue, valueSetter, tooltip)
        {
            this.enabled = (this.Value > 0);
            this.ShowEnabledOption = true;
        }
    }
}
