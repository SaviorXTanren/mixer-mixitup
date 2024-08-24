using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayVideoV3Model : OverlayItemV3ModelBase
    {
        public const string FilePathIDPropertyName = "FilePathID";
        public const string URLPathPropertyName = "URLPath";

        public static readonly string DefaultHTML = OverlayResources.OverlayVideoDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayVideoDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayVideoActionDefaultJavascript;

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public double Volume { get; set; }

        [DataMember]
        public string StartTime { get; set; }

        [DataMember]
        public bool Loop { get; set; }

        public OverlayVideoV3Model() : base(OverlayItemV3Type.Video) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.FilePath)] = RandomHelper.PickRandomFileFromDelimitedString(this.FilePath);
            properties[nameof(this.Volume)] = this.Volume.ToInvariantNumberString();
            properties[nameof(this.StartTime)] = this.StartTime;
            properties[nameof(this.Loop)] = this.Loop ? "loop" : string.Empty;
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            if (!string.IsNullOrEmpty(this.FilePath))
            {
                properties[nameof(this.FilePath)] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(properties[nameof(this.FilePath)].ToString(), parameters);
                properties[FilePathIDPropertyName] = properties[nameof(this.FilePath)].ToString().GetHashCode().ToString();
                properties[URLPathPropertyName] = ServiceManager.Get<OverlayV3Service>().GetURLForFile(properties[nameof(this.FilePath)].ToString(), "video");

                properties[nameof(this.StartTime)] = "0";
                if (!string.IsNullOrEmpty(this.StartTime))
                {
                    string startTime = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.StartTime, parameters);
                    if (int.TryParse(startTime, out int time) && time >= 0)
                    {
                        properties[URLPathPropertyName] = properties[URLPathPropertyName].ToString() + "#t=" + time;
                    }
                }
            }
        }
    }
}