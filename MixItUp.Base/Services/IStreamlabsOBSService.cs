using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class StreamlabsOBSScene
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [DataContract]
    public class StreamlabsOBSRequest : StreamlabsOBSPacketBase
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public JObject Parameters { get; set; }

        public StreamlabsOBSRequest() { this.Parameters = new JObject(); }

        public StreamlabsOBSRequest(string method, string resource)
            : this()
        {
            this.Method = method;
            this.Parameters["resource"] = resource;
            this.Parameters["args"] = new JArray();
        }
    }

    [DataContract]
    public class StreamlabsOBSResponse : StreamlabsOBSPacketBase
    {
        [JsonProperty("result")]
        public JObject Result { get; set; }

        public StreamlabsOBSResponse() { }
    }

    [DataContract]
    public class StreamlabsOBSArrayResponse : StreamlabsOBSPacketBase
    {
        [JsonProperty("result")]
        public JArray Result { get; set; }

        public StreamlabsOBSArrayResponse() { }
    }

    [DataContract]
    public abstract class StreamlabsOBSPacketBase
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty]
        public string jsonrpc = "2.0";

        public StreamlabsOBSPacketBase() { }
    }

    public interface IStreamlabsOBSService
    {
        Task<IEnumerable<StreamlabsOBSScene>> GetScenes();
    }
}
