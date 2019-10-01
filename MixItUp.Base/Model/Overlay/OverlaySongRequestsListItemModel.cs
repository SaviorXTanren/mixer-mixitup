using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
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

        public bool IncludeCurrentSong { get; set; }

        private List<OverlayListIndividualItemModel> lastItems { get; set; } = new List<OverlayListIndividualItemModel>();

        public OverlaySongRequestsListItemModel()
            : base()
        {
            this.IncludeCurrentSong = true;
        }

        public OverlaySongRequestsListItemModel(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            bool includeCurrentSong, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(OverlayItemModelTypeEnum.SongRequests, htmlText, totalToShow, 0, textFont, width, height, borderColor, backgroundColor, textColor, alignment, addEventAnimation, removeEventAnimation)
        {
            this.IncludeCurrentSong = includeCurrentSong;
        }

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
            this.lastItems.Clear();

            GlobalEvents.OnSongRequestsChangedOccurred -= GlobalEvents_OnSongRequestsChangedOccurred;

            await base.Disable();
        }

        private async void GlobalEvents_OnSongRequestsChangedOccurred(object sender, System.EventArgs e)
        {
            List<SongRequestModel> songs = ChannelSession.Services.SongRequestService.RequestSongs.ToList();
            if (this.IncludeCurrentSong)
            {
                SongRequestModel current = await ChannelSession.Services.SongRequestService.GetCurrent();
                if (current != null)
                {
                    songs.Insert(0, current);
                }
            }
            await this.AddSongRequests(songs.Take(this.TotalToShow));
        }

        private async Task AddSongRequests(IEnumerable<SongRequestModel> songs)
        {
            await this.listSemaphore.WaitAndRelease(() =>
            {
                List<OverlayListIndividualItemModel> items = new List<OverlayListIndividualItemModel>();
                for (int i = 0; i < songs.Count() && i < this.TotalToShow; i++)
                {
                    SongRequestModel song = songs.ElementAt(i);
                    OverlayListIndividualItemModel item = OverlayListIndividualItemModel.CreateAddItem(song.ID.ToString(), song.User, i + 1, this.HTML);
                    item.TemplateReplacements.Add("SONG_NAME", song.Name);
                    item.TemplateReplacements.Add("SONG_IMAGE", song.AlbumImage);
                    item.TemplateReplacements.Add("SONG_IMAGE_SIZE", ((int)(0.8 * ((double)this.Height))).ToString());
                    items.Add(item);
                }

                foreach (OverlayListIndividualItemModel item in this.lastItems)
                {
                    if (!items.Any(i => i.ID.Equals(item.ID)))
                    {
                        this.Items.Add(OverlayListIndividualItemModel.CreateRemoveItem(item.ID));
                    }
                }

                for (int i = 0; i < items.Count() && i < this.TotalToShow; i++)
                {
                    OverlayListIndividualItemModel item = items.ElementAt(i);

                    OverlayListIndividualItemModel foundItem = this.lastItems.FirstOrDefault(oi => oi.ID.Equals(item.ID));
                    if (foundItem == null || foundItem.Position != item.Position)
                    {
                        this.Items.Add(item);
                    }
                }

                this.lastItems = new List<OverlayListIndividualItemModel>(items);

                this.SendUpdateRequired();
                return Task.FromResult(0);
            });
        }
    }
}
