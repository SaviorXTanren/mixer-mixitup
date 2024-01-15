using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayImageV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayImageDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayImageDefaultCSS;
        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public string FilePath { get; set; }

        public OverlayImageV3Model() : base(OverlayItemV3Type.Image) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.FilePath)] = RandomHelper.PickRandomFileFromDelimitedString(this.FilePath);
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            properties[nameof(this.FilePath)] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(properties[nameof(this.FilePath)].ToString(), parameters);
            properties[nameof(this.FilePath)] = ServiceManager.Get<OverlayV3Service>().GetURLForFile(properties[nameof(this.FilePath)].ToString(), "image");
        }
    }
}
