using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public abstract class RemoteItemViewModelBase : ModelViewModelBase<RemoteItemModelBase>
    {
        public RemoteItemViewModelBase(RemoteItemModelBase model) : base(model) { }

        public string Name
        {
            get { return this.model.Name; }
            set
            {
                this.model.Name = value;
                this.NotifyPropertyChanged();
            }
        }

        public int XPosition { get { return this.model.XPosition; } }

        public int YPosition { get { return this.model.YPosition; } }

        public int ItemWidth
        {
            get
            {
                switch (this.model.Size)
                {
                    case RemoteItemSizeEnum.TwoByOne:
                    case RemoteItemSizeEnum.TwoByTwo:
                        return 2;
                    default:
                        return 1;
                }
            }
        }

        public int ItemHeight
        {
            get
            {
                switch (this.model.Size)
                {
                    case RemoteItemSizeEnum.OneByTwo:
                    case RemoteItemSizeEnum.TwoByTwo:
                        return 2;
                    default:
                        return 1;
                }
            }
        }

        public bool IsSet { get { return true; } }

        public virtual bool IsEmpty { get { return false; } }

        public virtual bool IsCommand { get { return false; } }

        public virtual bool IsFolder { get { return false; } }
    }
}
