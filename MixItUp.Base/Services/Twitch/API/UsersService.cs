using MixItUp.Base.Model.Twitch;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for User-based services.
    /// </summary>
    public class UsersService : NewTwitchAPIServiceBase
    {
        /// <summary>
        /// Creates an instance of the UsersService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public UsersService(TwitchConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>The resulting user</returns>
        public async Task<UserModel> GetCurrentUser()
        {
            IEnumerable<UserModel> users = await this.GetDataResultAsync<UserModel>("users");
            return (users != null) ? users.FirstOrDefault() : null;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="user">The user to get</param>
        /// <returns>The resulting user</returns>
        public async Task<UserModel> GetUser(UserModel user) { return await this.GetUserByID(user.id); }

        /// <summary>
        /// Gets a user by their user ID.
        /// </summary>
        /// <param name="userID">The ID of the user</param>
        /// <returns>The user associated with the ID</returns>
        public async Task<UserModel> GetUserByID(string userID)
        {
            IEnumerable<UserModel> users = await this.GetUsersByID(new List<string>() { userID });
            return users.FirstOrDefault();
        }

        /// <summary>
        /// Gets a user by their login.
        /// </summary>
        /// <param name="login">The login of the user</param>
        /// <returns>The user associated with the login</returns>
        public async Task<UserModel> GetUserByLogin(string login)
        {
            IEnumerable<UserModel> users = await this.GetUsersByLogin(new List<string>() { login });
            return users.FirstOrDefault();
        }

        /// <summary>
        /// Gets the users by their user IDs.
        /// </summary>
        /// <param name="userIDs">The IDs of the users</param>
        /// <returns>The users associated with the IDs</returns>
        public async Task<IEnumerable<UserModel>> GetUsersByID(IEnumerable<string> userIDs) { return await this.GetUsers(userIDs, new List<string>()); }

        /// <summary>
        /// Gets the users by their logins.
        /// </summary>
        /// <param name="logins">The logins of the users</param>
        /// <returns>The users associated with the logins</returns>
        public async Task<IEnumerable<UserModel>> GetUsersByLogin(IEnumerable<string> logins) { return await this.GetUsers(new List<string>(), logins); }

        /// <summary>
        /// Gets the users by their user IDs &amp; logins.
        /// </summary>
        /// <param name="userIDs">The IDs of the users</param>
        /// <param name="logins">The logins of the users</param>
        /// <returns>The users associated with the IDs &amp; logins</returns>
        public async Task<IEnumerable<UserModel>> GetUsers(IEnumerable<string> userIDs, IEnumerable<string> logins)
        {
            Validator.ValidateVariable(userIDs, "userIDs");
            Validator.ValidateVariable(logins, "logins");
            Validator.Validate((userIDs.Count() > 0 || logins.Count() > 0), "At least one userID or login must be specified");

            List<string> parameters = new List<string>();
            foreach (string userID in userIDs)
            {
                parameters.Add("id=" + userID);
            }
            foreach (string login in logins)
            {
                parameters.Add("login=" + login);
            }

            return await this.GetDataResultAsync<UserModel>("users?" + string.Join("&", parameters));
        }

        /// <summary>
        /// Updates the description of the current user.
        /// </summary>
        /// <param name="description">The description to set</param>
        /// <returns>The updated current user</returns>
        public async Task<UserModel> UpdateCurrentUserDescription(string description)
        {
            NewTwitchAPIDataRestResult<UserModel> result = await this.PutAsync<NewTwitchAPIDataRestResult<UserModel>>("users?description=" + AdvancedHttpClient.URLEncodeString(description));
            if (result != null && result.data != null)
            {
                return result.data.FirstOrDefault();
            }
            return null;
        }
    }
}
