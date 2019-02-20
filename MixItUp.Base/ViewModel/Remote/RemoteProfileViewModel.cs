using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Security.Cryptography;
using System.Text;

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

        public string HashValidation { get { return this.model.HashValidation; } set { this.model.HashValidation = value; } }
    }

    public class RemoteProfileBoardViewModel : ViewModelBase
    {
        public RemoteProfileViewModel Profile { get; private set; }
        public RemoteBoardViewModel Board { get; private set; }

        private RemoteProfileBoardModel model;

        public RemoteProfileBoardViewModel(RemoteProfileBoardModel model)
        {
            this.model = model;

            this.Profile = new RemoteProfileViewModel(this.model.Profile);
            this.Board = new RemoteBoardViewModel(this.model.Board);
        }

        public void BuildHashValidation()
        {
            this.Profile.HashValidation = HashHelper.ComputeMD5Hash(SerializerHelper.SerializeToString(this.Profile.GetModel()) + SerializerHelper.SerializeToString(this.Board.GetModel()));
        }
    }
}
