using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlaySongRequestsListItemModel : OverlayListItemModelBase
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
            <img src=""{SONG_IMAGE}"" width=""{SONG_IMAGE_SIZE}"" height=""{SONG_IMAGE_SIZE}"" style=""position: absolute; top: 50%; transform: translate(0%, -50%); margin-left: 10px;"">
            <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; position: absolute; top: 50%; left: 28%; transform: translate(0%, -50%);"">{SONG_NAME}</span>
        </div>";

        private List<SongRequestModel> lastSongRequests { get; set; } = new List<SongRequestModel>();

        public OverlaySongRequestsListItemModel() : base() { }

        public OverlaySongRequestsListItemModel(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.SongRequests, htmlText, totalToShow, textFont, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation)
        { }

        public override async Task LoadTestData()
        {
            List<SongRequestModel> songs = new List<SongRequestModel>();
            for (int i = 0; i < this.TotalToShow; i++)
            {
                songs.Add(new SongRequestModel()
                {
                    ID = Guid.NewGuid().ToString(),
                    Type = SongRequestServiceTypeEnum.YouTube,
                    Name = "TEST SONG",
                    AlbumImage = "https://www.youtube.com/yt/about/media/images/brand-resources/icons/YouTube_icon_full-color.svg"
                });
            }
            await this.AddSongRequests(songs);
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnSongRequestsChangedOccurred += GlobalEvents_OnSongRequestsChangedOccurred;

            await base.Initialize();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnSongRequestsChangedOccurred -= GlobalEvents_OnSongRequestsChangedOccurred;

            await base.Disable();
        }

        private async void GlobalEvents_OnSongRequestsChangedOccurred(object sender, System.EventArgs e)
        {
            await this.AddSongRequests(ChannelSession.Services.SongRequestService.RequestSongs);
        }

        private async Task AddSongRequests(IEnumerable<SongRequestModel> songs)
        {
            await this.listSemaphore.WaitAndRelease(() =>
            {
                foreach (SongRequestModel song in this.lastSongRequests)
                {
                    if (!songs.Contains(song))
                    {
                        this.Items.Add(OverlayListIndividualItemModel.CreateRemoveItem(song.ID.ToString()));
                    }
                }

                for (int i = 0; i < songs.Count(); i++)
                {
                    SongRequestModel song = songs.ElementAt(i);

                    OverlayListIndividualItemModel item = null;

                    int lastIndex = this.lastSongRequests.IndexOf(song);
                    if (lastIndex < 0 || lastIndex != i)
                    {
                        item = OverlayListIndividualItemModel.CreateAddItem(song.ID.ToString(), song.User, i + 1, this.HTML);

                        item.TemplateReplacements.Add("SONG_NAME", song.Name);
                        item.TemplateReplacements.Add("SONG_IMAGE", song.AlbumImage);
                        item.TemplateReplacements.Add("SONG_IMAGE_SIZE", ((int)(0.8 * ((double)this.Height))).ToString());

                        this.Items.Add(item);
                    }
                }

                this.lastSongRequests = new List<SongRequestModel>(songs);

                this.SendUpdateRequired();
                return Task.FromResult(0);
            });
        }
    }
}
