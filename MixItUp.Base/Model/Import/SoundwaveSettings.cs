using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
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

        public void Initialize(JObject interactive, JObject profiles, JObject sounds)
        {
            try
            {
                if (interactive["cooldownOption"] != null)
                {
                    string cooldownOption = interactive["cooldownOption"].ToString();
                    this.StaticCooldown = cooldownOption.Equals("static");
                    this.DynamicCooldown = cooldownOption.Equals("dynamic");
                }

                if (interactive["staticCooldown"] != null && int.TryParse(interactive["staticCooldown"].ToString(), out int staticCooldownAmount))
                {
                    this.StaticCooldownAmount = staticCooldownAmount;
                }

                int defaultCooldown = 0;
                if (interactive["default_cooldown"] != null)
                {
                    int.TryParse(interactive["default_cooldown"].ToString(), out defaultCooldown);
                }

                int defaultSparks = 0;
                if (interactive["default_sparks"] != null)
                {
                    int.TryParse(interactive["default_sparks"].ToString(), out defaultSparks);
                }

                Dictionary<string, SoundwaveButton> buttons = new Dictionary<string, SoundwaveButton>();
                if (sounds["sounds"] != null)
                {
                    JArray soundArray = (JArray)sounds["sounds"];
                    foreach (JToken sound in soundArray)
                    {
                        SoundwaveButton button = sound.ToObject<SoundwaveButton>();
                        buttons[button.id] = button;
                    }
                }

                if (profiles["profiles"] != null)
                {
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
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
