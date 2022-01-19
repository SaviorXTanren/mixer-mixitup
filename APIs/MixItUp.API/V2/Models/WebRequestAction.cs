using System.Collections.Generic;

namespace MixItUp.API.V2.Models
{
    public class WebRequestAction : ActionBase
    {
        public string Url { get; set; }
        public string ResponseType { get; set; }
        public Dictionary<string, string> JSONToSpecialIdentifiers { get; set; } = new Dictionary<string, string>();
    }
}
