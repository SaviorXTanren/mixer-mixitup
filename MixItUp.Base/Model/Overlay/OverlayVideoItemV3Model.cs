using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayVideoItemV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = "<video id=\"video-{ID}\" onloadstart=\"this.volume={Volume}\" allow=\"autoplay; encrypted-media\" autoplay {Loop}>" + Environment.NewLine + 
            "    <source src=\"{FilePath}\" type=\"video/{VideoExtension}\" />" + Environment.NewLine + 
            "</video>";

        public static readonly string DefaultCSS = "#video-{ID} {" + Environment.NewLine +
            "    width: {Width};" + Environment.NewLine +
            "    height: {Height};" + Environment.NewLine +
            "}";

        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public double Volume { get; set; }

        [DataMember]
        public bool Loop { get; set; }

        public OverlayVideoItemV3Model() : base(OverlayItemV3Type.Video) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            string filepath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.FilePath, parameters);
            filepath = RandomHelper.PickRandomFileFromDelimitedString(filepath);

            item.HTML = ReplaceProperty(item.HTML, "FilePath", overlayEndpointService.GetURLForLocalFile(filepath, "video"));
            item.CSS = ReplaceProperty(item.CSS, "FilePath", overlayEndpointService.GetURLForLocalFile(filepath, "video"));
            item.Javascript = ReplaceProperty(item.Javascript, "FilePath", overlayEndpointService.GetURLForLocalFile(filepath, "video"));

            item.HTML = ReplaceProperty(item.HTML, "Volume", this.Volume.ToString());
            item.CSS = ReplaceProperty(item.CSS, "Volume", this.Volume.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, "Volume", this.Volume.ToString());

            if (this.Loop)
            {
                item.HTML = ReplaceProperty(item.HTML, "Loop", "loop");
            }
            else
            {
                item.HTML = ReplaceProperty(item.HTML, "Loop", string.Empty);
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