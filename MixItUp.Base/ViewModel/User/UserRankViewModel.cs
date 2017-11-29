using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserRankViewModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int MinimumPoints { get; set; }

        public UserRankViewModel() { }

        public UserRankViewModel(string name, int minimumPoints)
        {
            this.Name = name;
            this.MinimumPoints = minimumPoints;
        }
    }
}
