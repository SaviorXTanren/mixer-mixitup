using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel
{
    [DataContract]
    public class UserDataViewModel : UserViewModel, IEquatable<UserDataViewModel>
    {
        [DataMember]
        public int CurrencyAmount { get; set; }

        public UserDataViewModel() { }

        public UserDataViewModel(uint id, string username)
            : base(id, username)
        {
            this.CurrencyAmount = 0;
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
