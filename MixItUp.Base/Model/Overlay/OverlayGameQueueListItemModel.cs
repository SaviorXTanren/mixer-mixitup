using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayGameQueueListItemModel : OverlayListItemModelBase
    {
        [DataContract]
        public class OverlayGameQueueItemModel
        {
            [DataMember]
            public int Position { get; set; }

            [DataMember]
            public UserViewModel User { get; set; }

            public OverlayGameQueueItemModel() { }

            public OverlayGameQueueItemModel(int position, UserViewModel user)
            {
                this.Position = position;
                this.User = user;
            }
        }

        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
          <p style=""position: absolute; top: 50%; left: 5%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">#{POSITION} {USERNAME}</p>
        </div>";

        [DataMember]
        public List<OverlayGameQueueItemModel> GameQueueItems { get; set; } = new List<OverlayGameQueueItemModel>();

        public OverlayGameQueueListItemModel() : base() { }

        public OverlayGameQueueListItemModel(string htmlText, int totalToShow, string textFont, int width, int height,
            string borderColor, string backgroundColor, string textColor, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.GameQueue, htmlText, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        { }

        public override async Task LoadTestData()
        {
            UserViewModel user = await ChannelSession.GetCurrentUser();

            this.GameQueueItems.Clear();
            for (int i = 0; i < this.TotalToShow; i++)
            {
                this.GameQueueItems.Add(new OverlayGameQueueItemModel(i + 1, user));
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnGameQueueUpdated += GlobalEvents_OnGameQueueUpdated;

            this.GameQueueItems.Clear();

            await base.Initialize();
        }

        private void GlobalEvents_OnGameQueueUpdated(object sender, System.EventArgs e)
        {
            IEnumerable<UserViewModel> gameQueue = ChannelSession.Services.GameQueueService.Queue;

            this.GameQueueItems.Clear();
            for (int i = 0; i < this.TotalToShow && i < gameQueue.Count(); i++)
            {
                this.GameQueueItems.Add(new OverlayGameQueueItemModel(i + 1, gameQueue.ElementAt(i)));
            }

            this.SendUpdateRequired();
        }
    }
}
