namespace MixItUp.Base.Model.Twitch.Teams
{
    /// <summary>
    /// Information about a team member.
    /// </summary>
    public class TeamMemberModel
    {
        /// <summary>
        /// User ID of a Team member.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// Login of a Team member.
        /// </summary>
        public string user_login { get; set; }
        /// <summary>
        /// Display name of a Team member.
        /// </summary>
        public string user_name { get; set; }
    }
}
