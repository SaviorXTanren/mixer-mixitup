using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
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
        TipeeeStream,
        TreatStream,
        Rainmaker,
        JustGiving,
        StreamElements,
        Twitch,
        DonorDrive,
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
        [DataMember]
        public bool IsAnonymous { get; set; }

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

        public string Username
        {
            get
            {
                if (this.IsAnonymous)
                {
                    return Resources.Anonymous;
                }

                if (this.User != null)
                {
                    return this.User.DisplayName;
                }

                if (!string.IsNullOrWhiteSpace(this.username))
                {
                    return this.username;
                }

                return Resources.Anonymous;
            }
            set { this.username = value; }
        }
        [DataMember]
        private string username { get; set; }

        [JsonIgnore]
        public UserV2ViewModel User { get; set; }

        [JsonIgnore]
        public string AmountText { get { return CurrencyHelper.ToCurrencyString(this.Amount); } }

        public void AssignUser()
        {
            if (this.User == null)
            {
                if (this.IsAnonymous)
                {
                    this.User = UserV2ViewModel.CreateUnassociated(Resources.Anonymous);
                }

                if (!string.IsNullOrEmpty(this.username))
                {
                    this.User = ServiceManager.Get<UserService>().GetActiveUserByPlatform(this.Platform, platformUsername: this.username);
                    if (this.User == null)
                    {
                        this.User = UserV2ViewModel.CreateUnassociated(this.username);
                    }
                }
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
