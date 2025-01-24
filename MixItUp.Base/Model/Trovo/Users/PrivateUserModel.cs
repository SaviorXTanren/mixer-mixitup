namespace MixItUp.Base.Model.Trovo.Users
{
    /// <summary>
    /// Private information about a user.
    /// </summary>
    public class PrivateUserModel
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// The name of the user.
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// The display name of the user.
        /// </summary>
        public string nickName { get; set; }
        /// <summary>
        /// The email of the user.
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// The profile picture of the user.
        /// </summary>
        public string profilePic { get; set; }
        /// <summary>
        /// The display information for the user.
        /// </summary>
        public string info { get; set; }
        /// <summary>
        /// The ID of the channel for the user.
        /// </summary>
        public string channelId { get; set; }
    }
}
