namespace MixItUp.Base.Model.Twitch.Charity
{
    /// <summary>
    /// Information about a campaign charity donation.
    /// </summary>
    public class CharityCampaignDonationModel
    {
        /// <summary>
        /// An ID that identifies the charity campaign that the donation applies to.
        /// </summary>
        public string campaign_id { get; set; }
        /// <summary>
        /// An ID that identifies a user that donated money to the campaign.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The user’s login name.
        /// </summary>
        public string user_login { get; set; }
        /// <summary>
        /// The user’s display name.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// An object that contains the amount of money that the user donated.
        /// </summary>
        public CharityCampaignAmountModel amount { get; set; }
    }
}
