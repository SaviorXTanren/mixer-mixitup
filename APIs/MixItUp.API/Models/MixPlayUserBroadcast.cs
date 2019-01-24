using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.API.Models
{
    [DataContract]
    public class MixPlayUserBroadcast
    {
        [Required]
        [DataMember]
        public MixPlayBroadcastUser[] Users{ get; set; }

        [Required]
        [DataMember]
        public JObject Data { get; set; }
    }
}