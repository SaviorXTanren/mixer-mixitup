using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayImageItemV3Model : OverlayItemV3ModelBase
    {
        public const string DefaultHTML = "<img id=\"image-{ID}\" src=\"{FilePath}\" />";

        public static readonly string DefaultCSS = "#image-{ID} {" + Environment.NewLine +
            "    width: {Width};" + Environment.NewLine +
            "    height: {Height};" + Environment.NewLine +
            "}";

        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public string FilePath { get; set; }

        public OverlayImageItemV3Model() : base(OverlayItemV3Type.Image) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            string filepath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.FilePath, parameters);
            filepath = RandomHelper.PickRandomFileFromDelimitedString(filepath);

            item.HTML = ReplaceProperty(item.HTML, "FilePath", overlayEndpointService.GetURLForLocalFile(filepath, "image"));
            item.CSS = ReplaceProperty(item.CSS, "FilePath", overlayEndpointService.GetURLForLocalFile(filepath, "image"));
            item.Javascript = ReplaceProperty(item.Javascript, "FilePath", overlayEndpointService.GetURLForLocalFile(filepath, "image"));

            return item;
        }
    }
}
