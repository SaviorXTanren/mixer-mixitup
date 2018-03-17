using Mixer.Base.Model.Teams;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Favorites
{
    [DataContract]
    public class FavoriteGroupModel
    {
        [DataMember]
        public TeamModel Team { get; set; }

        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public HashSet<uint> GroupUserIDs { get; set; }

        public FavoriteGroupModel()
        {
            this.GroupUserIDs = new HashSet<uint>();
        }

        public FavoriteGroupModel(TeamModel team)
            : this()
        {
            this.Team = team;
        }

        public FavoriteGroupModel(string groupName)
            : this()
        {
            this.GroupName = groupName;
        }
    }
}
