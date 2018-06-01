using Mixer.Base.Model.OAuth;
using Mixer.Base.Util;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class StreamlabsEventPacket
    {
        [JsonProperty("for")]
        public string forType { get; set; }
        public string type { get; set; }
        public string event_id { get; set; }
        public JArray message { get; set; }
    }

    public class StreamlabsDonation
    {
        [JsonProperty("donation_id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

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
                Username = this.Name,
                Message = this.Message,

                Amount = this.Amount,
                AmountText = string.Format("{0:C}", this.Amount),

                DateTime = DateTimeHelper.UnixTimestampToDateTimeOffset(this.CreatedAt),
            };
        }
    }

    public interface IStreamlabsService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task<IEnumerable<StreamlabsDonation>> GetDonations();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
