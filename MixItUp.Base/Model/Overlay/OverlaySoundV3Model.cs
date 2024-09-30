using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlaySoundV3Model : OverlayItemV3ModelBase
    {
        public const string URLPathPropertyName = "URLPath";
        public const string SourceTypePropertyName = "SourceType";

        public const string SoundFinishedPacketType = "SoundFinished";

        public static readonly string DefaultHTML = OverlayResources.OverlaySoundDefaultHTML;
        public static readonly string DefaultCSS = string.Empty;
        public static readonly string DefaultJavascript = OverlayResources.OverlaySoundDefaultJavascript;

        public static string GetAudioFileSourceType(string filePath)
        {
            int fileExtensionIndex = filePath.LastIndexOf(".");
            if (fileExtensionIndex > 0)
            {
                return "audio/" + filePath.Substring(fileExtensionIndex + 1);
            }
            return string.Empty;
        }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public double Volume { get; set; }

        public OverlaySoundV3Model(string filePath, double volume) : base(OverlayItemV3Type.Sound)
        {
            this.FilePath = filePath;
            this.Volume = ((double)volume) / 100.0;

            this.HTML = OverlaySoundV3Model.DefaultHTML;
            this.CSS = OverlaySoundV3Model.DefaultCSS;
            this.Javascript = OverlaySoundV3Model.DefaultJavascript;
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.FilePath)] = this.FilePath;
            properties[nameof(this.Volume)] = this.Volume.ToInvariantNumberString();
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            if (!string.IsNullOrEmpty(this.FilePath))
            {
                properties[nameof(this.FilePath)] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(properties[nameof(this.FilePath)].ToString(), parameters);
                properties[URLPathPropertyName] = ServiceManager.Get<OverlayV3Service>().GetURLForFile(properties[nameof(this.FilePath)].ToString(), "sound");
                properties[SourceTypePropertyName] = OverlaySoundV3Model.GetAudioFileSourceType(properties[nameof(this.FilePath)].ToString());
            }
        }
    }
}
