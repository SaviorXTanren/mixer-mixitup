using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Import
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

        public SoundwaveSettings(JObject interactive, JObject profiles, JObject sounds)
            : this()
        {
            string cooldownOption = interactive["cooldownOption"].ToString();
            this.StaticCooldown = cooldownOption.Equals("static");
            this.StaticCooldownAmount = (int)interactive["staticCooldown"];

            this.DynamicCooldown = cooldownOption.Equals("dynamic");

            Dictionary<string, SoundwaveButton> buttons = new Dictionary<string, SoundwaveButton>();

            int defaultCooldown = (int)sounds["default_cooldown"];
            int defaultSparks = (int)sounds["default_sparks"];
            JArray soundArray = (JArray)sounds["sounds"];
            foreach (JToken sound in soundArray)
            {
                SoundwaveButton button = sound.ToObject<SoundwaveButton>();
                buttons[button.id] = button;
            }

            JArray profileArray = (JArray)profiles["profiles"];
            foreach (JToken profile in profileArray)
            {
                this.Profiles[profile["name"].ToString()] = new List<SoundwaveButton>();
                JArray profileSoundArray = (JArray)profile["sounds"];
                foreach (JToken sound in profileSoundArray)
                {
                    if (buttons.ContainsKey(sound.ToString()))
                    {
                        this.Profiles[profile["name"].ToString()].Add(buttons[sound.ToString()]);
                    }
                }
            }
        }
    }
}
