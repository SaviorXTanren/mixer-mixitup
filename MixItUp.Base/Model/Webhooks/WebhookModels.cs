using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Webhooks
{
    public class GetWebhooksResponseModel
    {
        public IEnumerable<Webhook> Webhooks { get; set; }
        public int MaxNumberOfWebhooks { get; set; }
    }

    public class Webhook
    {
        public Guid Id { get; set; }
        public string Secret { get; set; }
    }
}
