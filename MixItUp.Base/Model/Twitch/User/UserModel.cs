using System;

namespace MixItUp.Base.Model.Twitch.User
{
    /// <summary>
    /// Information about a user.
    /// </summary>
    public class UserModel
    {
        private const string ThumbnailPreviewURLFormat = "https://static-cdn.jtvnw.net/previews-ttv/live_user_{0}{1}.jpg";

        /// <summary>
        /// The user ID.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The user's account name.
        /// </summary>
        public string login { get; set; }
        /// <summary>
        /// The user's display name.
        /// </summary>
        public string display_name { get; set; }
        /// <summary>
        /// The type of the user.
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// The broadcast type of the user.
        /// </summary>
        public string broadcaster_type { get; set; }
        /// <summary>
        /// The user's description.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// The user's profile image URL.
        /// </summary>
        public string profile_image_url { get; set; }
        /// <summary>
        /// The user's offline image URL.
        /// </summary>
        public string offline_image_url { get; set; }
        /// <summary>
        /// The user's total view count.
        /// </summary>
        public long view_count { get; set; }
        /// <summary>
        /// The user's email account.
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Date when the user was created.
        /// </summary>
        public string created_at { get; set; }

        /// <summary>
        /// Gets the current thumbnail preview image for the user's channel in a large size.
        /// </summary>
        public string ThumbnailPreviewLarge { get { return string.Format(ThumbnailPreviewURLFormat, this.login, ""); } }
        /// <summary>
        /// Gets the current thumbnail preview image for the user's channel in a small size.
        /// </summary>
        public string ThumbnailPreviewSmall { get { return string.Format(ThumbnailPreviewURLFormat, this.login, "-640x360"); } }

        public bool IsAffiliate()
        {
            return string.Equals(this.broadcaster_type, "affiliate", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsPartner()
        {
            return string.Equals(this.broadcaster_type, "partner", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsStaff()
        {
            return string.Equals(this.type, "staff", StringComparison.OrdinalIgnoreCase) || string.Equals(this.type, "admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsGlobalMod()
        {
            return string.Equals(this.type, "global_mod", StringComparison.OrdinalIgnoreCase);
        }
    }
}
