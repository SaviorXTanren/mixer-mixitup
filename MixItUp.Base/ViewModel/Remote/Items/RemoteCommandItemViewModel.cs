using MixItUp.Base.Commands;
using MixItUp.Base.Remote.Models.Items;
using System;
using System.Linq;

namespace MixItUp.Base.ViewModel.Remote.Items
{
    public class RemoteCommandItemViewModel : RemoteButtonItemViewModelBase
    {
        private new RemoteCommandItemModel model;

        public RemoteCommandItemViewModel(string name, int xPosition, int yPosition)
            : this(new RemoteCommandItemModel(xPosition, yPosition))
        {
            this.Name = name;
        }

        public RemoteCommandItemViewModel(RemoteCommandItemModel model)
            : base(model)
        {
            this.model = model;
        }

        public Guid CommandID { get { return this.model.CommandID; } set { this.model.CommandID = value; } }

        public override bool IsCommand { get { return true; } }
    }
}
