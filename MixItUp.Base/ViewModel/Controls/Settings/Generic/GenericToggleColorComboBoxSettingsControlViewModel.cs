using System;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Settings.Generic
{
    public class GenericToggleColorComboBoxSettingsControlViewModel : GenericColorComboBoxSettingsOptionControlViewModel
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
                    this.Value = this.Values.FirstOrDefault(c => c.Equals(new ColorOptionViewModel("Black")));
                }
                else
                {
                    this.Value = null;
                }
            }
        }
        private bool enabled = true;

        public bool ShowEnabledOption { get; private set; } = true;

        public GenericToggleColorComboBoxSettingsControlViewModel(string name, string initialValue, Action<string> valueSetter, string tooltip = null)
            : base(name, initialValue, valueSetter, tooltip)
        {
            this.enabled = (this.Value != null);
        }
    }
}
