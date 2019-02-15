using MixItUp.Base.Remote.Models.Items;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public abstract class RemoteButtonItemViewModelBase : RemoteItemViewModelBase
    {
        private new RemoteButtonItemModelBase model;

        public RemoteButtonItemViewModelBase(RemoteButtonItemModelBase model) : base(model) { this.model = model; }

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
            }
        }

        public bool HasBackgroundImage { get { return !string.IsNullOrEmpty(this.BackgroundImage); } }
    }
}
