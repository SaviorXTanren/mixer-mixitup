using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.API
{
    /// <summary>
    /// The APIs for user-based services.
    /// </summary>
    public class UsersService : TrovoServiceBase
    {
        private class GetUsersResult
        {
            public long total { get; set; }
            public List<UserModel> users { get; set; } = new List<UserModel>();
        }

        /// <summary>
        /// Creates an instance of the UsersService.
        /// </summary>
        /// <param name="connection">The Trovo connection to use</param>
        public UsersService(TrovoConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the currently authenticated user.
        /// </summary>
        /// <returns>The currently authenticated user</returns>
        public async Task<PrivateUserModel> GetCurrentUser()
        {
            return await this.GetAsync<PrivateUserModel>("getuserinfo");
        }

        /// <summary>
        /// Gets the user matching the specified username.
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>The matching user</returns>
        public async Task<UserModel> GetUser(string username)
        {
            Validator.ValidateString(username, "username");
            IEnumerable<UserModel> users = await this.GetUsers(new List<string>() { username });
            return (users != null) ? users.FirstOrDefault() : null;
        }

        /// <summary>
        /// Gets the set of users matching the specified usernames.
        /// </summary>
        /// <param name="usernames">The usernames to search for</param>
        /// <returns>The matching users.</returns>
        public async Task<IEnumerable<UserModel>> GetUsers(IEnumerable<string> usernames)
        {
            Validator.ValidateList(usernames, "usernames");

            JObject jobj = new JObject();
            JArray jarr = new JArray();
            foreach (string username in usernames)
            {
                jarr.Add(username);
            }
            jobj["user"] = jarr;

            GetUsersResult result = await this.PostAsync<GetUsersResult>("getusers", AdvancedHttpClient.CreateContentFromObject(jobj));
            if (result != null)
            {
                return result.users;
            }
            return null;
        }
    }
}
