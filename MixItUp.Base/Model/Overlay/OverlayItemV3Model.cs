using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayItemV3Model
    {
        public string HTML { get; set; }
        public string CSS { get; set; }
        public string Javascript { get; set; }

        public async Task<OverlayItemV3Model> GetProcessedItem(CommandParametersModel parameters)
        {
            OverlayItemV3Model result = new OverlayItemV3Model();
            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Javascript, parameters);
            return result;
        }
    }
}
