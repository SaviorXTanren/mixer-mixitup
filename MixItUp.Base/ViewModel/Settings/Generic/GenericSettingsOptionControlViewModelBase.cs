using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Settings.Generic
{
    public abstract class GenericSettingsOptionControlViewModelBase : UIViewModelBase
    {
        public string Name { get; set; }

        public string Tooltip { get; set; }

        public bool ShowEnabledOption { get; protected set; }

        public GenericSettingsOptionControlViewModelBase(string name, string tooltip = null)
        {
            this.Name = name;
            if (!string.IsNullOrEmpty(tooltip))
            {
                this.Tooltip = tooltip.AddNewLineEveryXCharacters(75);
            }
        }
    }
}
