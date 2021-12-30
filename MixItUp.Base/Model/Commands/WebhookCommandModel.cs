using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class WebhookJSONParameter
    {
        [DataMember]
        public string JSONParameterName { get; set; }

        [DataMember]
        public string SpecialIdentifierName { get; set; }
    }

    [DataContract]
    public class WebhookCommandModel : CommandModelBase
    {
        [DataMember]
        public List<WebhookJSONParameter> JSONParameters { get; set; } = new List<WebhookJSONParameter>();

        public WebhookCommandModel(string name) : base(name, CommandTypeEnum.Webhook) { }

        [Obsolete]
        public WebhookCommandModel() : base() { }

        public override Dictionary<string, string> GetTestSpecialIdentifiers()
        {
            return JSONParameters.ToDictionary(j => j.SpecialIdentifierName, j => "Test Value");
        }
    }
}
