using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User
{
    public enum UserDonationSourceEnum
    {
        GawkBox,
        Streamlabs,
        Tiltify,
        [Name("Extra Life")]
        ExtraLife,
        TipeeeStream,
        TreatStream,
        StreamJar,
        JustGiving,
    }

    [DataContract]
    public class UserDonationModel : JSONObjectBase<UserDonationModel>
    {
        [DataMember]
        public UserDonationSourceEnum Source { get; set; }

        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Type { get; set; }
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

        [JsonIgnore]
        public UserViewModel User
        {
            get
            {
                UserViewModel user = ChannelSession.Services.User.GetUserByUsername(this.Username);
                if (user != null)
                {
                    return user;
                }
                return new UserViewModel() { MixerUsername = this.Username };
            }
        }

        public Dictionary<string, string> GetSpecialIdentifiers()
        {
            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationSourceSpecialIdentifier] = EnumHelper.GetEnumName(this.Source);
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationTypeSpecialIdentifier] = this.Type;
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationAmountNumberDigitsSpecialIdentifier] = (this.Amount * 100).ToString();
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationAmountNumberSpecialIdentifier] = this.Amount.ToString();
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationAmountSpecialIdentifier] = this.AmountText;
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationMessageSpecialIdentifier] = this.Message;
            specialIdentifiers[SpecialIdentifierStringBuilder.DonationImageSpecialIdentifier] = this.ImageLink;
            return specialIdentifiers;
        }
    }
}
