using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.IO;
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

        protected override async Task<Dictionary<string, string>> GetCustomProperties(CommandParametersModel parameters)
        {
            Dictionary<string, string> properties = await base.GetCustomProperties(parameters);

            string filepath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.FilePath, parameters);
            filepath = RandomHelper.PickRandomFileFromDelimitedString(filepath);

            string extension = Path.GetExtension(filepath);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1);
            }
            else
            {
                extension = "mp4";
            }

            properties[nameof(this.FilePath)] = ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "video");
            properties[nameof(this.Volume)] = this.Volume.ToString();
            properties[nameof(this.Loop)] = this.Loop ? "loop" : string.Empty;
            properties["VideoExtension"] = extension;

            return properties;
        }
    }
}