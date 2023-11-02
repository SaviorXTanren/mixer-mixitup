using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
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

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            string filepath = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.FilePath, parameters);
            filepath = RandomHelper.PickRandomFileFromDelimitedString(filepath);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.FilePath), ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "image"));
            item.CSS = ReplaceProperty(item.CSS, nameof(this.FilePath), ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "image"));
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.FilePath), ServiceManager.Get<OverlayV3Service>().GetURLForFile(filepath, "image"));

            return item;
        }
    }
}
