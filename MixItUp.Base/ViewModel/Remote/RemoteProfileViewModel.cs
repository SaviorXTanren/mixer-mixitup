using MixItUp.Base.Remote.Models;
using MixItUp.Base.ViewModels;
using System;

namespace MixItUp.Base.ViewModel.Remote
{
    public class RemoteProfileViewModel : ModelViewModelBase<RemoteProfileModel>
    {
        public RemoteProfileViewModel(RemoteProfileModel model) : base(model) { }

        public RemoteProfileViewModel(string name)
            : this(new RemoteProfileModel())
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
        }

        public Guid ID
        {
            get { return this.model.ID; }
            private set
            {
                this.model.ID = value;
                this.NotifyPropertyChanged();
            }
        }

        public string Name
        {
            get { return this.model.Name; }
            set
            {
                this.model.Name = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool IsStreamer
        {
            get { return this.model.IsStreamer; }
            set
            {
                this.model.IsStreamer = value;
                this.NotifyPropertyChanged();
            }
        }
    }

    public class RemoteProfileBoardsViewModel : ModelViewModelBase<RemoteProfileBoardsModel>
    {
        public Guid ProfileID
        {
            get { return this.model.ProfileID; }
            set
            {
                this.model.ProfileID = value;
                this.NotifyPropertyChanged();
            }
        }

        public RemoteProfileBoardsViewModel(RemoteProfileBoardsModel model) : base(model) { }

        public RemoteBoardViewModel GetBoard(Guid boardID)
        {
            if (this.model.Boards.ContainsKey(boardID))
            {
                return new RemoteBoardViewModel(this.model.Boards[boardID]);
            }
            return null;
        }
    }
}
