using Mixer.Base.Util;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    public enum UserDonationSourceEnum
    {
        GawkBox,
        Streamlabs,
        Tiltify,
    }

    [DataContract]
    public class UserDonationModel
    {
        [DataMember]
        public UserDonationSourceEnum Source { get; set; }

        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string ImageLink { get; set; }

        [DataMember]
        public double Amount { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [JsonIgnore]
        public string AmountText { get { return string.Format("{0:C}", Math.Round(this.Amount, 2)); } }

        public Dictionary<string, string> GetSpecialIdentifiers()
        {
            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationSourceSpecialIdentifier] = EnumHelper.GetEnumName(this.Source);
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationAmountNumberDigitsSpecialIdentifier] = this.Amount.ToString().Replace(".", "");
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationAmountNumberSpecialIdentifier] = this.Amount.ToString();
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationAmountSpecialIdentifier] = this.AmountText;
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationMessageSpecialIdentifier] = this.Message;
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationImageSpecialIdentifier] = this.ImageLink;
            return specialIdentifiers;
        }
    }
}
