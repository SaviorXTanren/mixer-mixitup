using MixItUp.Base.ViewModel.ScorpBot;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserDataViewModel : UserViewModel, IEquatable<UserDataViewModel>
    {
        [DataMember]
        public long RankPoints { get; set; }

        [DataMember]
        public long CurrencyAmount { get; set; }

        public UserDataViewModel() { }

        public UserDataViewModel(uint id, string username)
            : base(id, username)
        {
            this.RankPoints = 0;
            this.CurrencyAmount = 0;
        }

        public UserDataViewModel(ScorpBotViewer viewer)
            : this(viewer.ID, viewer.UserName)
        {
            this.RankPoints = viewer.RankPoints;
            this.CurrencyAmount = viewer.Currency;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserDataViewModel)
            {
                return this.Equals((UserDataViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserDataViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }
    }
}
