using Mixer.Base.Model.Patronage;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
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

        protected override Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();
            return Task.FromResult(replacementSets);
        }
    }
}
