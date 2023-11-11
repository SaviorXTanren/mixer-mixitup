using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayLabelV3Model : OverlayVisualTextV3ModelBase
    {
        public static readonly string DefaultAmountHTML = OverlayResources.OverlayLabelAmountDefaultHTML;
        public static readonly string DefaultUsernameHTML = OverlayResources.OverlayLabelUsernameDefaultHTML;
        public static readonly string DefaultUsernameAmountHTML = OverlayResources.OverlayLabelUsernameAmountDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayLabelDefaultJavascript;

        [DataMember]
        public string Username { get;set; }
        [DataMember]
        public double Amount { get; set; }

        public OverlayLabelV3Model() : base(OverlayItemV3Type.Label) { }

        public override Dictionary<string, string> GetGenerationProperties()
        {
            Dictionary<string, string> properties = base.GetGenerationProperties();
            properties[nameof(this.Username)] = this.Username;
            properties[nameof(this.Amount)] = this.Amount.ToString();
            return properties;
        }
    }
}
