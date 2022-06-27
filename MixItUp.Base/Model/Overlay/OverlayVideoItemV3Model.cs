using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayVideoItemV3Model : OverlayItemV3ModelBase
    {
        public const string DefaultHTML = "<video onloadstart=\"this.volume={Volume}\" allow=\"autoplay; encrypted-media\" autoplay {Loop} style=\"{Width} {Height}\"><source src=\"{FilePath}\" type=\"video/{VideoExtension}\" /></video>";

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

            item.HTML = ReplaceProperty(item.HTML, "FilePath", overlayEndpointService.GetURLForLocalFile(this.FilePath, "video"));
            item.HTML = ReplaceProperty(item.HTML, "Volume", this.Volume.ToString());

            if (this.Loop)
            {
                item.HTML = ReplaceProperty(item.HTML, "Loop", "loop");
            }
            else
            {
                item.HTML = ReplaceProperty(item.HTML, "Loop", string.Empty);
            }

            string extension = Path.GetExtension(this.FilePath);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1);
            }
            else
            {
                extension = "mp4";
            }
            item.HTML = ReplaceProperty(item.HTML, "VideoExtension", extension);

            return item;
        }
    }
}