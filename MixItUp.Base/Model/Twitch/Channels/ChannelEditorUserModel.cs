namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a channel editor.
    /// </summary>
    public class ChannelEditorUserModel
    {
        /// <summary>
        /// 	User ID of the editor.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// Display name of the editor.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// Date and time the editor was given editor permissions.
        /// </summary>
        public string created_at { get; set; }
    }
}
