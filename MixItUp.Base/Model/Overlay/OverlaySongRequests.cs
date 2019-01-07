using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlaySongRequestItem
    {
        [DataMember]
        public string HTMLText { get; set; }
    }

    [DataContract]
    public class OverlaySongRequests : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
        @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
            <img src=""{SONG_IMAGE}"" width=""{SONG_IMAGE_SIZE}"" height=""{SONG_IMAGE_SIZE}"" style=""position: absolute; top: 50%; transform: translate(0%, -50%); margin-left: 10px;"">
            <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; position: absolute; top: 50%; left: 28%; transform: translate(0%, -50%);"">{SONG_NAME}</span>
        </div>";

        public const string SongRequestsItemType = "songrequestsqueue";

        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        public List<OverlaySongRequestItem> SongRequestUpdates = new List<OverlaySongRequestItem>();

        [JsonIgnore]
        private bool songRequestsUpdated = true;
        [JsonIgnore]
        private List<SongRequestItem> currentSongRequests = new List<SongRequestItem>();

        [JsonIgnore]
        private List<SongRequestItem> testSongRequestsList = new List<SongRequestItem>();

        public OverlaySongRequests() : base(SongRequestsItemType, HTMLTemplate) { }

        public OverlaySongRequests(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(SongRequestsItemType, htmlText)
        {
            this.TotalToShow = totalToShow;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.AddEventAnimation = addEventAnimation;
            this.RemoveEventAnimation = removeEventAnimation;
        }

        [JsonIgnore]
        public override bool SupportsTestButton { get { return true; } }

        public override async Task LoadTestData()
        {
            for (int i = 0; i < 5; i++)
            {
                this.testSongRequestsList.Add(new SongRequestItem()
                {
                    ID = Guid.NewGuid().ToString(),
                    Type = SongRequestServiceTypeEnum.YouTube,
                    Name = "TEST SONG",
                    AlbumImage = "https://www.youtube.com/yt/about/media/images/brand-resources/icons/YouTube_icon_full-color.svg"
                });
                this.songRequestsUpdated = true;
                await Task.Delay(1500);
            }
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnSongRequestsChangedOccurred += GlobalEvents_OnSongRequestsChangedOccurred;

            this.testSongRequestsList.Clear();

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (ChannelSession.Services.SongRequestService != null && this.songRequestsUpdated)
            {
                this.songRequestsUpdated = false;

                List<SongRequestItem> songRequests = new List<SongRequestItem>();

                SongRequestItem currentlyPlaying = await ChannelSession.Services.SongRequestService.GetCurrentlyPlaying();
                if (currentlyPlaying != null)
                {
                    songRequests.Add(currentlyPlaying);
                }

                IEnumerable<SongRequestItem> allSongRequests = this.testSongRequestsList;
                if (this.testSongRequestsList.Count == 0)
                {
                    allSongRequests = await ChannelSession.Services.SongRequestService.GetAllRequests();
                }

                foreach (SongRequestItem songRequest in allSongRequests)
                {
                    if (!songRequests.Any(sr => sr.Equals(songRequest)))
                    {
                        songRequests.Add(songRequest);
                    }
                }

                this.SongRequestUpdates.Clear();
                this.currentSongRequests.Clear();

                OverlaySongRequests copy = this.Copy<OverlaySongRequests>();
                for (int i = 0; i < songRequests.Count() && i < this.TotalToShow; i++)
                {
                    this.currentSongRequests.Add(songRequests.ElementAt(i));
                }

                while (this.currentSongRequests.Count > 0)
                {
                    OverlayCustomHTMLItem overlayItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
                    copy.SongRequestUpdates.Add(new OverlaySongRequestItem() { HTMLText = overlayItem.HTMLText });
                    this.currentSongRequests.RemoveAt(0);
                }

                return copy;
            }
            return null;
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlaySongRequests>(); }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            SongRequestItem songRequest = this.currentSongRequests.ElementAt(0);

            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_SIZE"] = ((int)(0.2 * ((double)this.Height))).ToString();

            replacementSets["SONG_IMAGE"] = songRequest.AlbumImage;
            replacementSets["SONG_IMAGE_SIZE"] = ((int)(0.8 * ((double)this.Height))).ToString();
            replacementSets["SONG_NAME"] = songRequest.Name;

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnSongRequestsChangedOccurred(object sender, System.EventArgs e) { this.songRequestsUpdated = true; }
    }
}
