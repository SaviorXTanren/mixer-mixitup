using MixItUp.Base.Model.Commands;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayLabelItemV3Type
    {
        Viewers,
        Chatters,
        LastestFollower,
        TotalFollowers,
        LatestRaid,
        LatestSubscriber,
        TotalSubscribers,
        LatestDonation,
        LatestTwitchBits,
        LatestTrovoSpell,

        Counter = 100,
    }

    public class OverlayLabelItemV3Model : OverlayVisualTextItemV3ModelBase
    {
        private CommandParametersModel lastParameters;

        public OverlayLabelItemV3Model() : base(OverlayItemV3Type.Label) { }

        public override async Task Enable()
        {
            await this.Update(this.lastParameters ?? new CommandParametersModel());
        }

        public override async Task Update(CommandParametersModel parameters)
        {
            await this.ProcessLabel(parameters);
        }

        private Task ProcessLabel(CommandParametersModel parameters)
        {
            this.lastParameters = parameters;



            return Task.CompletedTask;
        }
    }
}
