using Mixer.Base.Clients;
using MixItUp.Base.Commands;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for EventsControl.xaml
    /// </summary>
    public partial class EventsControl : MainControlBase
    {
        public EventsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.MixerFollowEventCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__followed);
            this.MixerHostEventCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__hosted);
            this.MixerSubscribeEventCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__subscribed);
            this.MixerResubscribeEventCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__resubscribed);

            this.StreamlabsDonationEventCommandControl.Initialize(this, OtherEventTypeEnum.StreamlabsDonation);
            this.GawkBoxDonationEventCommandControl.Initialize(this, OtherEventTypeEnum.GawkBoxDonation);

            this.GameWispSubscribeEventCommandControl.Initialize(this, OtherEventTypeEnum.GameWispSubscribed);
            this.GameWispResubscribeEventCommandControl.Initialize(this, OtherEventTypeEnum.GameWispResubscribed);

            return Task.FromResult(0);
        }
    }
}
