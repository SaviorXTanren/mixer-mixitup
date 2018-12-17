using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class ScoutUser
    {
        [DataMember]
        public string PlayerID { get; set; }
        [DataMember]
        public string PlayerHandle { get; set; }

        [DataMember]
        public string PersonaID { get; set; }
        [DataMember]
        public string PersonaHandle { get; set; }

        public ScoutUser() { }

        public ScoutUser(JObject jobj)
        {
            if (jobj.ContainsKey("player") && jobj["player"] != null)
            {
                JObject player = (JObject)jobj["player"];
                if (player.ContainsKey("playerId"))
                {
                    this.PlayerID = player["playerId"].ToString();
                }
                if (player.ContainsKey("handle"))
                {
                    this.PlayerHandle = player["handle"].ToString();
                }
            }

            if (jobj.ContainsKey("persona") && jobj["persona"] != null)
            {
                JObject persona = (JObject)jobj["persona"];
                if (persona.ContainsKey("id"))
                {
                    this.PersonaID = persona["id"].ToString();
                }
                if (persona.ContainsKey("handle"))
                {
                    this.PersonaHandle = persona["handle"].ToString();
                }
            }
        }
    }

    [DataContract]
    public class ScoutStat
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }

        [JsonIgnore]
        public int ValueInt
        {
            get
            {
                int.TryParse(this.Value, out int result);
                return result;
            }
        }

        [JsonIgnore]
        public double ValueDecimal
        {
            get
            {
                double.TryParse(this.Value, out double result);
                return result;
            }
        }

        public ScoutStat() { }

        public ScoutStat(JObject jobj)
        {
            if (jobj.ContainsKey("key"))
            {
                this.Name = jobj["key"].ToString();
            }
            else if (jobj.ContainsKey("metadata"))
            {
                JObject metadata = (JObject)jobj["metadata"];
                if (metadata.ContainsKey("key"))
                {
                    this.Name = metadata["key"].ToString();
                }
            }

            if (jobj.ContainsKey("value") && jobj["value"] != null)
            {
                this.Value = jobj["value"].ToString();
            }
        }
    }

    public interface IScoutService
    {
        Task<ScoutUser> GetUser(string title, string identifier, Dictionary<string, string> parameters = null);

        Task<Dictionary<string, ScoutStat>> GetStats(string title, ScoutUser user, Dictionary<string, string> parameters = null);
    }
}
