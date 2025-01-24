using System.Collections.Generic;

namespace MixItUp.Base.Model.Trovo.Users
{
    /// <summary>
    /// Information about a user.
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// The ID of the user
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The name of the user.
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// The display name of the user.
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// The ID of the channel for the user.
        /// </summary>
        public string channel_id { get; set; }
    }

    public class UsersModel
    {
        public long total { get; set; }
        public List<UserModel> users { get; set; } = new List<UserModel>();
    }
}
