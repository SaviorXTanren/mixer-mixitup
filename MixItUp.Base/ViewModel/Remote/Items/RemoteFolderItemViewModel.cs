using MixItUp.Base.Remote.Models.Items;
using System;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteFolderItemViewModel : RemoteButtonItemViewModelBase
    {
        private new RemoteFolderItemModel model;

        public RemoteFolderItemViewModel(string name, int xPosition, int yPosition)
            : this(new RemoteFolderItemModel(xPosition, yPosition))
        {
            this.Name = name;
        }

        public RemoteFolderItemViewModel(RemoteFolderItemModel model)
            : base(model)
        {
            this.model = model;
        }

        public Guid BoardID { get { return this.model.BoardID; } set { this.model.BoardID = value; } }

        public override bool IsFolder { get { return true; } }
    }
}
