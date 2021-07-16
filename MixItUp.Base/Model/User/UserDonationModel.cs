using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
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
        [Name("Extra Life")]
        ExtraLife,
        TipeeeStream,
        TreatStream,
        Rainmaker,
        JustGiving,
        StreamElements,
    }

    [DataContract]
    public class UserDonationModel : JSONObjectBase<UserDonationModel>
    {
        [DataMember]
        public UserDonationSourceEnum Source { get; set; }

        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.All;

        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                if (string.IsNullOrWhiteSpace(this.username))
                {
                    this.username = MixItUp.Base.Resources.Anonymous;
                }
            }
        }
        [DataMember]
        private string username { get; set; }

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
        public string AmountText { get { return this.Amount.ToCurrencyString(); } }

        [JsonIgnore]
        public UserViewModel User
        {
            get
            {
                lock (this)
                {
                    if (this.user == null && !string.IsNullOrEmpty(this.username))
                    {
                        this.user = ChannelSession.Services.User.GetActiveUserByUsername(this.username, this.Platform);
                        if (this.user == null)
                        {
                            this.user = UserViewModel.Create(this.username);
                        }
                    }
                }
                return this.user;
            }
        }
        private UserViewModel user;

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
