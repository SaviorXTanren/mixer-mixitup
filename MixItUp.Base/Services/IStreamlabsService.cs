using Mixer.Base.Model.OAuth;
using Mixer.Base.Util;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StreamlabsDonation
    {
        [JsonProperty("donation_id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonIgnore]
        public double AmountValue { get { return Math.Round(double.Parse(this.Amount), 2); } }

        public StreamlabsDonation()
        {
            this.CreatedAt = DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now);
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Streamlabs,

                ID = this.ID.ToString(),
                UserName = this.UserName,
                Message = this.Message,

                Amount = this.AmountValue,
                AmountText = string.Format("{0:C}", this.Amount),

                DateTime = DateTimeHelper.UnixTimestampToDateTimeOffset(this.CreatedAt),
            };
        }
    }

    public interface IStreamlabsService
    {
        Task<bool> Connect();

        Task Disconnect();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
