using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public abstract class RemoteButtonItemViewModelBase : RemoteItemViewModelBase
    {
        public const int BackgroundImageResizeValue = 120;

        private new RemoteButtonItemModelBase model;

        public RemoteButtonItemViewModelBase(RemoteButtonItemModelBase model)
            : base(model)
        {
            this.model = model;
        }

        public IEnumerable<string> PreDefinedColors { get { return ColorSchemes.WPFColorSchemeDictionary; } }

        public string BackgroundColor
        {
            get
            {
                if (!string.IsNullOrEmpty(this.model.BackgroundColor))
                {
                    return this.model.BackgroundColor;
                }
                return "Transparent";
            }
            set
            {
                this.model.BackgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }

        public string TextColor
        {
            get
            {
                if (!string.IsNullOrEmpty(this.model.TextColor))
                {
                    return this.model.TextColor;
                }
                return "Black";
            }
            set
            {
                this.model.TextColor = value;
                this.NotifyPropertyChanged();
            }
        }

        public string BackgroundImage
        {
            get { return this.model.ImagePath; }
            set
            {
                this.model.ImagePath = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("HasBackgroundImage");
                this.NotifyPropertyChanged("DoesNotHaveBackgroundImage");
            }
        }

        public bool HasBackgroundImage { get { return !string.IsNullOrEmpty(this.BackgroundImage); } }
        public bool DoesNotHaveBackgroundImage { get { return !this.HasBackgroundImage; } }
    }
}
