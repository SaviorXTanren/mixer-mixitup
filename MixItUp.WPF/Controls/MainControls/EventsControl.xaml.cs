using Mixer.Base.Clients;
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
            this.OnFollowCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__followed);
            this.OnHostCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__hosted);
            this.OnSubscribeCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__subscribed);
            this.OnResubscribeCommandControl.Initialize(this, ConstellationEventTypeEnum.channel__id__resubscribed);
            //this.OnDonationCommandControl.Initialize(this, OtherEventTypeEnum.Donation);

            return Task.FromResult(0);
        }
    }
}
