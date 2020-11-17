using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlaySparkCrystalItemModel : OverlayHTMLTemplateItemModelBase
    {
        public OverlaySparkCrystalItemModel() : base() { }

        public override async Task Enable()
        {
            await base.Enable();
        }

        public override async Task Disable()
        {
            await base.Disable();
        }

        protected override Task<Dictionary<string, string>> GetTemplateReplacements(CommandParametersModel parameters)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();
            return Task.FromResult(replacementSets);
        }
    }
}
