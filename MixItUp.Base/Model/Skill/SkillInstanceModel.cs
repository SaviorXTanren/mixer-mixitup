using Mixer.Base.Model.Skills;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Skill
{
    [DataContract]
    public class SkillInstanceModel
    {
        private const string GifUrlKey = "gifUrl";

        [DataMember]
        public SkillModel Skill { get; set; }
        [DataMember]
        public JObject Manifest { get; set; }
        [DataMember]
        public JObject Parameters { get; set; }

        public SkillInstanceModel() { }

        public SkillInstanceModel(SkillModel skill, JObject manifest, JObject parameters)
        {
            this.Skill = skill;
            this.Manifest = manifest;
            this.Parameters = parameters;
        }

        [JsonIgnore]
        public bool IsGif { get { return this.Parameters.ContainsKey(SkillInstanceModel.GifUrlKey); } }

        public string GifUrl
        {
            get
            {
                if (this.IsGif)
                {
                    return this.Parameters[SkillInstanceModel.GifUrlKey].ToString();
                }
                return null;
            }
        }
    }
}
