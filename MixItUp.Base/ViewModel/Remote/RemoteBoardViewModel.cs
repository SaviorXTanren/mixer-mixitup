using MixItUp.Base.Remote.Models;
using MixItUp.Base.Remote.Models.Items;
using MixItUp.Base.ViewModel.Remote.Items;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Remote
{
    public class RemoteBoardViewModel : ModelViewModelBase<RemoteBoardModel>
    {
        protected RemoteItemViewModelBase[,] items = new RemoteItemViewModelBase[RemoteBoardModel.BoardWidth, RemoteBoardModel.BoardHeight];

        public RemoteBoardViewModel(RemoteBoardModel model)
            : base(model)
        {
            this.BuildBoardItems();
        }

        public string BackgroundColor
        {
            get
            {
                if (!string.IsNullOrEmpty(this.model.BackgroundColor))
                {
                    return this.model.BackgroundColor;
                }
                return "White";
            }
            set { this.model.BackgroundColor = value; }
        }

        public string BackgroundImage { get { return this.model.ImagePath; } }

        public RemoteItemViewModelBase Item00 { get { return this.items[0, 0]; } }
        public RemoteItemViewModelBase Item10 { get { return this.items[1, 0]; } }
        public RemoteItemViewModelBase Item20 { get { return this.items[2, 0]; } }
        public RemoteItemViewModelBase Item30 { get { return this.items[3, 0]; } }
        public RemoteItemViewModelBase Item40 { get { return this.items[4, 0]; } }

        public RemoteItemViewModelBase Item01 { get { return this.items[0, 1]; } }
        public RemoteItemViewModelBase Item11 { get { return this.items[1, 1]; } }
        public RemoteItemViewModelBase Item21 { get { return this.items[2, 1]; } }
        public RemoteItemViewModelBase Item31 { get { return this.items[3, 1]; } }
        public RemoteItemViewModelBase Item41 { get { return this.items[4, 1]; } }

        public RemoteItemViewModelBase Item02 { get { return this.items[0, 2]; } }
        public RemoteItemViewModelBase Item12 { get { return this.items[1, 2]; } }
        public RemoteItemViewModelBase Item22 { get { return this.items[2, 2]; } }
        public RemoteItemViewModelBase Item32 { get { return this.items[3, 2]; } }
        public RemoteItemViewModelBase Item42 { get { return this.items[4, 2]; } }

        public void AddItem(RemoteItemViewModelBase item)
        {
            this.model.SetItem(item.GetModel(), item.XPosition, item.YPosition);
            this.BuildBoardItems();
        }

        public void RemoveItem(int xPosition, int yPosition)
        {
            this.model.SetItem(null, xPosition, yPosition);
            this.BuildBoardItems();
        }

        private void BuildBoardItems()
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    this.items[x, y] = null;

                    RemoteItemModelBase item = this.model.GetItem(x, y);
                    if (item != null)
                    {
                        if (item is RemoteCommandItemModel)
                        {
                            this.items[x, y] = new RemoteCommandItemViewModel((RemoteCommandItemModel)item);
                        }
                        else if (item is RemoteFolderItemModel)
                        {
                            this.items[x, y] = new RemoteFolderItemViewModel((RemoteFolderItemModel)item);
                        }
                    }
                    else
                    {
                        this.items[x, y] = new RemoteEmptyItemViewModel(x, y);
                    }
                }
            }

            if (this.model.IsSubBoard)
            {
                this.items[0, 0] = new RemoteBackItemViewModel();
            }

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    this.NotifyPropertyChanged("Item" + x + y);
                }
            }
        }
    }
}
