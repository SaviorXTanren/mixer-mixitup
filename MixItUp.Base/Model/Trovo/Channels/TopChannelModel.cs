using MixItUp.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Trovo.Channels
{
    /// <summary>
    /// Information about a top channel.
    /// </summary>
    public class TopChannelModel
    {
        /// <summary>
        /// The ID of the channel.
        /// </summary>
        public string channel_id { get; set; }
        /// <summary>
        /// The ID of the associated streamer user.
        /// </summary>
        public string streamer_user_id { get; set; }
        /// <summary>
        /// The URL of the channel.
        /// </summary>
        public string channel_url { get; set; }
        /// <summary>
        /// Whether the channel is currently live.
        /// </summary>
        public bool is_live { get; set; }
        /// <summary>
        /// The title of the channel.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The audience type of the channel.
        /// </summary>
        public string audi_type { get; set; }
        /// <summary>
        /// The language code of the channel.
        /// </summary>
        public string language_code { get; set; }
        /// <summary>
        /// The thumbnail URL for the channel.
        /// </summary>
        public string thumbnail { get; set; }
        /// <summary>
        /// The current number of viewers for the channel.
        /// </summary>
        public long current_viewers { get; set; }
        /// <summary>
        /// The current number of followers for the channel.
        /// </summary>
        public long num_followers { get; set; }
        /// <summary>
        /// The profile picture of the channel.
        /// </summary>
        public string profile_pic { get; set; }
        /// <summary>
        /// The nick name of the channel.
        /// </summary>
        public string nick_name { get; set; }
        /// <summary>
        /// The username of the channel.
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// The ID of the category for the channel.
        /// </summary>
        public string category_id { get; set; }
        /// <summary>
        /// The name of the category for the channel.
        /// </summary>
        public string category_name { get; set; }
        /// <summary>
        /// The date &amp; time the stream started at.
        /// </summary>
        public string stream_started_at { get; set; }
        /// <summary>
        /// The video resolution of the stream.
        /// </summary>
        public string video_resolution { get; set; }
        /// <summary>
        /// The country of the channel.
        /// </summary>
        public string channel_country { get; set; }
        /// <summary>
        /// The number of subscribers of the channel.
        /// </summary>
        public int subscriber_num { get; set; }
        /// <summary>
        /// The social media links of the channel.
        /// </summary>
        public List<ChannelSocialLinkModel> social_links { get; set; } = new List<ChannelSocialLinkModel>();

        /// <summary>
        /// The audience enum for the channel.
        /// </summary>
        public ChannelAudienceTypeEnum Audience { get { return EnumHelper.GetEnumValueFromString<ChannelAudienceTypeEnum>(this.audi_type); } }
    }

    public class TopChannelsModel : PageDataResponseModel
    {
        public List<TopChannelModel> top_channels_lists { get; set; } = new List<TopChannelModel>();

        public override int GetItemCount() { return this.top_channels_lists.Count; }
    }
}
