namespace MixItUp.Base.Model.Twitch.Charity
{
    /// <summary>
    /// Information about a charity campaign.
    /// </summary>
    public class CharityCampaignModel
    {
        /// <summary>
        /// An ID that uniquely identifies the charity campaign.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// An ID that uniquely identifies the broadcaster that’s running the campaign.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The broadcaster’s login name.
        /// </summary>
        public string broadcaster_login { get; set; }
        /// <summary>
        /// The broadcaster’s display name.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// The charity’s name.
        /// </summary>
        public string charity_name { get; set; }
        /// <summary>
        /// A description of the charity.
        /// </summary>
        public string charity_description { get; set; }
        /// <summary>
        /// A URL to an image of the charity’s logo. The image’s type is PNG and its size is 100px X 100px.
        /// </summary>
        public string charity_logo { get; set; }
        /// <summary>
        /// 	A URL to the charity’s website.
        /// </summary>
        public string charity_website { get; set; }
        /// <summary>
        /// The current amount of donations that the campaign has received.
        /// </summary>
        public CharityCampaignAmountModel current_amount { get; set; }
        /// <summary>
        /// The campaign’s fundraising goal. This field is null if the broadcaster has not defined a fundraising goal.
        /// </summary>
        public CharityCampaignAmountModel target_amount { get; set; }
    }
}
