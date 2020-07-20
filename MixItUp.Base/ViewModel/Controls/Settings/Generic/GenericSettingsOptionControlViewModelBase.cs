using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Controls.Settings.Generic
{
    public abstract class GenericSettingsOptionControlViewModelBase : UIViewModelBase
    {
        public string Name { get; set; }

        public string Tooltip { get; set; }

        public GenericSettingsOptionControlViewModelBase(string name, string tooltip = null)
        {
            this.Name = name;
            this.Tooltip = tooltip;
        }
    }
}
