using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Teams
{
    /// <summary>
    /// Information about a team.
    /// </summary>
    public class TeamModel
    {
        /// <summary>
        /// Team ID.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Team name.
        /// </summary>
        public string team_name { get; set; }
        /// <summary>
        /// Team display name.
        /// </summary>
        public string team_display_name { get; set; }
        /// <summary>
        /// Team description.
        /// </summary>
        public string info { get; set; }
        /// <summary>
        /// Image URL for the Team logo.
        /// </summary>
        public string thumbnail_url { get; set; }
        /// <summary>
        /// URL of the Team background image.
        /// </summary>
        public string background_image_url { get; set; }
        /// <summary>
        /// URL for the Team banner.
        /// </summary>
        public string banner { get; set; }
        /// <summary>
        /// 	Date and time the Team was created.
        /// </summary>
        public string created_at { get; set; }
        /// <summary>
        /// Date and time the Team was last updated.
        /// </summary>
        public string updated_at { get; set; }
    }

    /// <summary>
    /// Information about a team's details.
    /// </summary>
    public class TeamDetailsModel : TeamModel
    {
        /// <summary>
        /// Users in the specified Team.
        /// </summary>
        public List<TeamMemberModel> users { get; set; } = new List<TeamMemberModel>();
    }
}
