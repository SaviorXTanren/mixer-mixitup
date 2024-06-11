using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayHeaderV3ModelBase : OverlayVisualTextV3ModelBase
    {
        public OverlayHeaderV3ModelBase() : base(OverlayItemV3Type.Text) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            foreach (var kvp in base.GetGenerationProperties())
            {
                properties["Header" + kvp.Key] = kvp.Value;
            }

            return properties;
        }
    }
}
