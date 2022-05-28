using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayItemV3Model
    {
        [DataMember]
        public string HTML { get; set; }
        [DataMember]
        public string CSS { get; set; }
        [DataMember]
        public string Javascript { get; set; }

        public async Task<OverlayItemV3Model> GetProcessedItem(CommandParametersModel parameters)
        {
            OverlayItemV3Model result = new OverlayItemV3Model();
            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Javascript, parameters);
            return await this.GetProcessedItem(result, parameters);
        }

        protected virtual Task<OverlayItemV3Model> GetProcessedItem(OverlayItemV3Model item, CommandParametersModel parameters)
        {
            return Task.FromResult(item);
        }

        protected string ReplaceProperty(string text, string name, string value)
        {
            return text.Replace($"{{{name}}}", value);
        }
    }
}
