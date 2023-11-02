using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
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

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            string filepath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.FilePath, parameters);
            filepath = RandomHelper.PickRandomFileFromDelimitedString(filepath);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.FilePath), ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "video"));
            item.CSS = ReplaceProperty(item.CSS, nameof(this.FilePath), ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "video"));
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.FilePath), ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "video"));

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Volume), this.Volume.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Volume), this.Volume.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Volume), this.Volume.ToString());

            if (this.Loop)
            {
                item.HTML = ReplaceProperty(item.HTML, nameof(this.Loop), "loop");
            }
            else
            {
                item.HTML = ReplaceProperty(item.HTML, nameof(this.Loop), string.Empty);
            }

            string extension = Path.GetExtension(filepath);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1);
            }
            else
            {
                extension = "mp4";
            }
            item.HTML = ReplaceProperty(item.HTML, "VideoExtension", extension);
            item.CSS = ReplaceProperty(item.CSS, "VideoExtension", extension);
            item.Javascript = ReplaceProperty(item.Javascript, "VideoExtension", extension);

            return item;
        }
    }
}