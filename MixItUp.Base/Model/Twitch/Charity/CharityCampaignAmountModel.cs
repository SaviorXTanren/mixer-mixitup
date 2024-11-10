namespace MixItUp.Base.Model.Twitch.Charity
{
    /// <summary>
    /// Information about a charity campaign amount
    /// </summary>
    public class CharityCampaignAmountModel
    {
        /// <summary>
        /// The monetary amount. The amount is specified in the currency’s minor unit. For example, the minor units for USD is cents, so if the amount is $5.50 USD, value is set to 550.
        /// </summary>
        public int current_amount { get; set; }
        /// <summary>
        /// The number of decimal places used by the currency. For example, USD uses two decimal places. Use this number to translate value from minor units to major units by using the formula:
        /// 
        /// value / 10^decimal_places
        /// </summary>
        public int decimal_places { get; set; }
        /// <summary>
        /// The ISO-4217 three-letter currency code that identifies the type of currency in value.
        /// </summary>
        public string currency { get; set; }
    }
}
