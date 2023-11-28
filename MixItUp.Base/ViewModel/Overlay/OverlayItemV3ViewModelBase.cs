using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayItemV3ViewModelBase : UIViewModelBase
    {
        public OverlayItemV3Type Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemV3Type type;

        public abstract string DefaultHTML { get; }
        public abstract string DefaultCSS { get; }
        public abstract string DefaultJavascript { get; }

        public OverlayItemV3ViewModelBase(OverlayItemV3Type type)
        {
            this.Type = type;
        }

        public OverlayItemV3ViewModelBase(OverlayItemV3ModelBase item)
        {
            this.Type = item.Type;
        }

        public virtual Result Validate()
        {
            return new Result();
        }

        public OverlayItemV3ModelBase GetItem() { return this.GetItemInternal(); }

        protected abstract OverlayItemV3ModelBase GetItemInternal();
    }
}
