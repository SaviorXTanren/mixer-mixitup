using Mixer.Base.Model.User;
using MixItUp.Base.Model.Favorites;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Favorites
{
    public class FavoriteGroupViewModel
    {
        public FavoriteGroupModel Group { get; private set; }

        public string Name { get { return (this.IsTeam) ? this.Group.Team.name : this.Group.GroupName; } }

        public bool IsTeam { get { return (this.Group.Team != null); } }

        public List<UserWithChannelModel> LastCheckUsers { get; private set; }

        public int TotalUsers { get { return this.LastCheckUsers.Count; } }

        public int LiveUsers { get { return this.LastCheckUsers.Where(u => u.channel.online).Count(); } }

        public int LiveViewers { get { return this.LastCheckUsers.Select(u => (int)u.channel.viewersCurrent).Sum(); } }

        public FavoriteGroupViewModel(FavoriteGroupModel group)
        {
            this.Group = group;
            this.LastCheckUsers = new List<UserWithChannelModel>();
        }

        public async Task RefreshGroup()
        {
            if (this.IsTeam)
            {
                this.Group.Team = await ChannelSession.Connection.GetTeam(this.Group.Team.id);
            }
            this.LastCheckUsers = new List<UserWithChannelModel>(await this.GetUsers());
        }

        public void AddUser(UserModel user)
        {
            if (!this.IsTeam)
            {
                this.Group.GroupUserIDs.Add(user.id);
            }
        }

        public void RemoteUser(UserModel user)
        {
            if (!this.IsTeam)
            {
                this.Group.GroupUserIDs.Remove(user.id);
            }
        }

        public async Task<IEnumerable<UserWithChannelModel>> GetUsers()
        {
            if (this.IsTeam)
            {
                return await ChannelSession.Connection.GetTeamUsers(this.Group.Team, 10000);
            }
            else
            {
                List<UserWithChannelModel> users = new List<UserWithChannelModel>();
                foreach (uint userID in this.Group.GroupUserIDs)
                {
                    UserWithChannelModel user = await ChannelSession.Connection.GetUser(userID);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }
                return users;
            }
        }
    }
}
