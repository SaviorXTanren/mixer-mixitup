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
                return "Black";
            }
            set { this.model.BackgroundColor = value; }
        }

        public string TextColor
        {
            get
            {
                if (!string.IsNullOrEmpty(this.model.TextColor))
                {
                    return this.model.TextColor;
                }
                return "White";
            }
            set { this.model.TextColor = value; }
        }

        public string BackgroundImage { get { return this.model.ImagePath; } }
    }
}
