using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayItemV3ModelBase
    {
        [DataMember]
        public string HTML { get; set; } = string.Empty;
        [DataMember]
        public string CSS { get; set; } = string.Empty;
        [DataMember]
        public string Javascript { get; set; } = string.Empty;

        public OverlayItemV3ModelBase() { }

        public async Task<OverlayItemV3ModelBase> GetProcessedItem(CommandParametersModel parameters)
        {
            OverlayItemV3ModelBase result = new OverlayItemV3ModelBase();
            result.HTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.HTML, parameters);
            result.CSS = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.CSS, parameters);
            result.Javascript = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Javascript, parameters);
            return await this.GetProcessedItem(result, parameters);
        }

        protected virtual Task<OverlayItemV3ModelBase> GetProcessedItem(OverlayItemV3ModelBase item, CommandParametersModel parameters)
        {
            return Task.FromResult(item);
        }

        protected string ReplaceProperty(string text, string name, string value)
        {
            return text.Replace($"{{{name}}}", value);
        }
    }
}
