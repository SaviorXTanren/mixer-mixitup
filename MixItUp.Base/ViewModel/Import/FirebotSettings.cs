using Mixer.Base.Model.Interactive;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Import
{
    [DataContract]
    public class FirebotSettings
    {
        public List<FirebotButton> Interactive = new List<FirebotButton>();

        public FirebotSettings() { }

        public FirebotSettings(JObject settings, Dictionary<string, JObject> controls)
        {
            foreach (var kvp in controls)
            {
                List<InteractiveSceneModel> scenes = new List<InteractiveSceneModel>();
                JArray scenesArray = (JArray)kvp.Value["mixer"];
                foreach (JToken sceneObj in scenesArray)
                {
                    scenes.Add(sceneObj.ToObject<InteractiveSceneModel>());
                }

                JArray controlsArray = (JArray)kvp.Value["firebot"]["controls"];
                foreach (JObject controlObj in controlsArray)
                {
                    this.Interactive.Add(this.GetButton(controlObj, scenes));
                }
            }
        }

        private FirebotButton GetButton(JObject jobj, IEnumerable<InteractiveSceneModel> scenes)
        {
            FirebotButton button = new FirebotButton();
            foreach (JProperty prop in jobj.Properties())
            {
                string name = prop.Name;

            }
            return button;
        }
    }
}
