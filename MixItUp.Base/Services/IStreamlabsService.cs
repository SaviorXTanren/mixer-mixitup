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
        public int donation_id { get; set; }

        public string name { get; set; }
        public string message { get; set; }
        public string email { get; set; }

        public double amount { get; set; }
        public string currency { get; set; }

        public long created_at { get; set; }

        public StreamlabsDonation() { }

        public StreamlabsDonation(StreamlabsEventPacket packet)
        {
            this.donation_id = int.Parse(packet.message["id"].ToString());
            this.name = packet.message["name"].ToString();
            this.message = packet.message["message"].ToString();

            this.amount = double.Parse(packet.message["amount"].ToString());
            this.currency = packet.message["currency"].ToString();

            this.created_at = DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now);
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                ID = this.donation_id.ToString(),
                Username = this.name,
                Message = this.message,

                Amount = this.amount,
                AmountText = string.Format("{0:C}", this.amount),

                DateTime = DateTimeHelper.UnixTimestampToDateTimeOffset(this.created_at),
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
