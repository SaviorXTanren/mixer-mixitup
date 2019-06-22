using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlaySongRequestsListItemModel : OverlayListItemModelBase
    {
        [DataContract]
        public class OverlaySongRequestItemModel
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Image { get; set; }

            public OverlaySongRequestItemModel() { }

            public OverlaySongRequestItemModel(string name, string image)
            {
                this.Name = name;
                this.Image = image;
            }
        }

        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
            <img src=""{SONG_IMAGE}"" width=""{SONG_IMAGE_SIZE}"" height=""{SONG_IMAGE_SIZE}"" style=""position: absolute; top: 50%; transform: translate(0%, -50%); margin-left: 10px;"">
            <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; position: absolute; top: 50%; left: 28%; transform: translate(0%, -50%);"">{SONG_NAME}</span>
        </div>";

        [DataMember]
        public List<OverlaySongRequestItemModel> SongRequestItems { get; set; } = new List<OverlaySongRequestItemModel>();

        public OverlaySongRequestsListItemModel() : base() { }

        public OverlaySongRequestsListItemModel(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.SongRequests, htmlText, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        { }

        public override Task LoadTestData()
        {
            this.SongRequestItems.Clear();
            for (int i = 0; i < this.TotalToShow; i++)
            {
                this.SongRequestItems.Add(new OverlaySongRequestItemModel("TEST SONG", "https://www.youtube.com/yt/about/media/images/brand-resources/icons/YouTube_icon_full-color.svg"));
            }
            return Task.FromResult(0);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnSongRequestsChangedOccurred += GlobalEvents_OnSongRequestsChangedOccurred;

            this.SongRequestItems.Clear();

            await base.Initialize();
        }

        private void GlobalEvents_OnSongRequestsChangedOccurred(object sender, System.EventArgs e)
        {
            IEnumerable<SongRequestModel> songRequests = ChannelSession.Services.SongRequestService.RequestSongs;

            this.SongRequestItems.Clear();
            for (int i = 0; i < this.TotalToShow && i < songRequests.Count(); i++)
            {
                this.SongRequestItems.Add(new OverlaySongRequestItemModel(songRequests.ElementAt(i).Name, songRequests.ElementAt(i).AlbumImage));
            }

            this.SendUpdateRequired();
        }
    }
}
