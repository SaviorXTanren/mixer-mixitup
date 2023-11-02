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

        protected override async Task<Dictionary<string, string>> GetCustomProperties(CommandParametersModel parameters)
        {
            Dictionary<string, string> properties = await base.GetCustomProperties(parameters);

            string filepath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.FilePath, parameters);
            filepath = RandomHelper.PickRandomFileFromDelimitedString(filepath);

            properties[nameof(this.FilePath)] = ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "image");

            return properties;
        }
    }
}
