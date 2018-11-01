using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class OverlayAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return OverlayAction.asyncSemaphore; } }

        [DataMember]
        public string OverlayName { get; set; }

        [DataMember]
        public OverlayItemBase Item { get; set; }

        [DataMember]
        public OverlayItemPosition Position { get; set; }

        [DataMember]
        public OverlayItemEffect Effect { get; set; }

        public OverlayAction() : base(ActionTypeEnum.Overlay) { }

        public OverlayAction(string overlayName, OverlayItemBase item, OverlayItemPosition position, OverlayItemEffect effect)
            : this()
        {
            this.OverlayName = overlayName;
            this.Item = item;
            this.Position = position;
            this.Effect = effect;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            string overlayName = (string.IsNullOrEmpty(this.OverlayName)) ? ChannelSession.Services.OverlayServers.DefaultOverlayName : this.OverlayName;
            IOverlayService overlay = ChannelSession.Services.OverlayServers.GetOverlay(overlayName);
            if (overlay != null)
            {
                if (this.Item is OverlayImageItem)
                {
                    OverlayImageItem imageItem = (OverlayImageItem)this.Item;
                    string imageFilePath = await this.ReplaceStringWithSpecialModifiers(imageItem.FilePath, user, arguments);
                    if (!Uri.IsWellFormedUriString(imageFilePath, UriKind.RelativeOrAbsolute))
                    {
                        imageFilePath = imageFilePath.ToFilePathString();
                    }

                    if (!string.IsNullOrEmpty(imageFilePath))
                    {
                        OverlayImageItem copy = imageItem.Copy<OverlayImageItem>();
                        copy.FilePath = imageFilePath;
                        await overlay.SendImage(copy, this.Position, this.Effect);
                    }
                }
                else if (this.Item is OverlayTextItem)
                {
                    OverlayTextItem textEffect = (OverlayTextItem)this.Item;
                    string text = await this.ReplaceStringWithSpecialModifiers(textEffect.Text, user, arguments);
                    OverlayTextItem copy = textEffect.Copy<OverlayTextItem>();
                    copy.Text = text;
                    await overlay.SendText(copy, this.Position, this.Effect);
                }
                else if (this.Item is OverlayYouTubeItem)
                {
                    await overlay.SendYouTubeVideo((OverlayYouTubeItem)this.Item, this.Position, this.Effect);
                }
                else if (this.Item is OverlayVideoItem)
                {
                    OverlayVideoItem videoEffect = (OverlayVideoItem)this.Item;
                    string videoFilePath = await this.ReplaceStringWithSpecialModifiers(videoEffect.FilePath, user, arguments);
                    if (!Uri.IsWellFormedUriString(videoFilePath, UriKind.RelativeOrAbsolute))
                    {
                        videoFilePath = videoFilePath.ToFilePathString();
                    }

                    if (!string.IsNullOrEmpty(videoFilePath))
                    {
                        OverlayVideoItem copy = videoEffect.Copy<OverlayVideoItem>();
                        copy.FilePath = videoFilePath;
                        await overlay.SendLocalVideo(copy, this.Position, this.Effect);
                    }
                }
                else if (this.Item is OverlayHTMLItem)
                {
                    OverlayHTMLItem htmlEffect = (OverlayHTMLItem)this.Item;
                    string htmlText = await this.ReplaceStringWithSpecialModifiers(htmlEffect.HTMLText, user, arguments);
                    OverlayHTMLItem copy = htmlEffect.Copy<OverlayHTMLItem>();
                    copy.HTMLText = htmlText;
                    await overlay.SendHTML(copy, this.Position, this.Effect);
                }
                else if (this.Item is OverlayWebPageItem)
                {
                    OverlayWebPageItem webPageEffect = (OverlayWebPageItem)this.Item;
                    string url = await this.ReplaceStringWithSpecialModifiers(webPageEffect.URL, user, arguments);
                    OverlayWebPageItem copy = webPageEffect.Copy<OverlayWebPageItem>();
                    copy.URL = url;
                    await overlay.SendWebPage(copy, this.Position, this.Effect);
                }
            }
        }
    }
}
