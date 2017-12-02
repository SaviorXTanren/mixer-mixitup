using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
{
    [DataContract]
    public class SoundwaveSettings
    {
        [DataMember]
        public bool StaticCooldown { get; set; }
        [DataMember]
        public int StaticCooldownAmount { get; set; }
        
        [DataMember]
        public bool DynamicCooldown { get; set; }

        [DataMember]
        public Dictionary<string, List<SoundwaveButton>> Profiles { get; set; }

        public SoundwaveSettings()
        {
            this.Profiles = new Dictionary<string, List<SoundwaveButton>>();
        }

        public SoundwaveSettings(JObject settings, JObject profiles, JObject sounds)
            : base()
        {
            this.StaticCooldown = settings["cooldownOption"].Equals("static");
            this.StaticCooldownAmount = (int)settings["staticCooldown"];

            this.DynamicCooldown = settings["cooldownOption"].Equals("dynamic");

            JArray profileArray = (JArray)profiles["profiles"];
            foreach (JToken profile in profileArray)
            {
                this.Profiles.Add(profile["name"].ToString(), new List<SoundwaveButton>());
                JArray profileSoundArray = (JArray)profile["sounds"];
                foreach (JToken sound in profileSoundArray)
                {
                    this.Profiles[profile["name"].ToString()].Add(new SoundwaveButton() { id = sound.ToString() });
                }
            }

            int defaultCooldown = (int)sounds["default_cooldown"];
            int defaultSparks = (int)sounds["default_sparks"];
            JArray soundArray = (JArray)sounds["sounds"];
            foreach (JToken sound in soundArray)
            {
                SoundwaveButton button = sound.ToObject<SoundwaveButton>();
                foreach (var kvp in this.Profiles)
                {
                    foreach (var profileButton in kvp.Value)
                    {

                    }
                }
            }
        }
    }
}
