using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayVideoV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayVideoDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayVideoDefaultCSS;
        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public double Volume { get; set; }

        [DataMember]
        public bool Loop { get; set; }

        public OverlayVideoV3Model() : base(OverlayItemV3Type.Video) { }

        public override Dictionary<string, string> GetGenerationProperties()
        {
            Dictionary<string, string> properties = base.GetGenerationProperties();
            properties[nameof(this.FilePath)] = RandomHelper.PickRandomFileFromDelimitedString(this.FilePath);
            properties[nameof(this.Volume)] = this.Volume.ToString();
            properties[nameof(this.Loop)] = this.Loop ? "loop" : string.Empty;
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, string> properties, CommandParametersModel parameters)
        {
            properties[nameof(this.FilePath)] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(properties[nameof(this.FilePath)], parameters);
            properties[nameof(this.FilePath)] = ServiceManager.Get<OverlayV3Service>().GetURLForFile(properties[nameof(this.FilePath)], "video");
        }
    }
}