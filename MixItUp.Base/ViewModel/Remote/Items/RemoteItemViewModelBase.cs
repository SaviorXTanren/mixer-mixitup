using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public abstract class RemoteItemViewModelBase : ModelViewModelBase<RemoteItemModelBase>
    {
        public RemoteItemViewModelBase(RemoteItemModelBase model) : base(model) { }

        public string Name { get { return this.model.Name; } }

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

        public virtual bool IsCommand { get { return false; } }

        public virtual bool IsFolder { get { return false; } }
    }
}
