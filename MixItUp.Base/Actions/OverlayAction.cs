using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        public string ImagePath;
        [DataMember]
        public int ImageWidth;
        [DataMember]
        public int ImageHeight;

        [DataMember]
        public string Text;
        [DataMember]
        public string Color;
        [DataMember]
        public int FontSize;

        [DataMember]
        public int VideoWidth;
        [DataMember]
        public int VideoHeight;

        [DataMember]
        public string youtubeVideoID;
        [DataMember]
        public int youtubeStartTime;

        [DataMember]
        public string localVideoFilePath;

        [DataMember]
        public string HTMLText;

        [DataMember]
        public double Duration;
        [DataMember]
        public int FadeDuration;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        private string imageData { get; set; }

        public static OverlayAction CreateForImage(string imagePath, int width, int height, double duration, int horizontal, int vertical, int fadeDuration)
        {
            OverlayAction action = new OverlayAction(duration, horizontal, vertical, fadeDuration);
            action.ImagePath = imagePath;
            action.ImageWidth = width;
            action.ImageHeight = height;
            return action;
        }

        public static OverlayAction CreateForText(string text, string color, int fontSize, double duration, int horizontal, int vertical, int fadeDuration)
        {
            OverlayAction action = new OverlayAction(duration, horizontal, vertical, fadeDuration);
            action.Text = text;
            action.Color = color;
            action.FontSize = fontSize;
            return action;
        }

        public static OverlayAction CreateForYoutube(string youtubeVideoID, int startTime, int width, int height, double duration, int horizontal, int vertical, int fadeDuration)
        {
            OverlayAction action = new OverlayAction(duration, horizontal, vertical, fadeDuration);
            action.youtubeVideoID = youtubeVideoID;
            action.youtubeStartTime = startTime;
            action.VideoWidth = width;
            action.VideoHeight = height;
            return action;
        }

        public static OverlayAction CreateForVideo(string localVideoFilePath, int width, int height, double duration, int horizontal, int vertical, int fadeDuration)
        {
            OverlayAction action = new OverlayAction(duration, horizontal, vertical, fadeDuration);
            action.localVideoFilePath = localVideoFilePath;
            action.VideoWidth = width;
            action.VideoHeight = height;
            return action;
        }

        public static OverlayAction CreateForHTML(string htmlText, double duration, int horizontal, int vertical, int fadeDuration)
        {
            OverlayAction action = new OverlayAction(duration, horizontal, vertical, fadeDuration);
            action.HTMLText = htmlText;
            return action;
        }

        public OverlayAction()
            : base(ActionTypeEnum.Overlay)
        {
            this.VideoHeight = 315;
            this.VideoWidth = 560;
        }

        private OverlayAction(double duration, int horizontal, int vertical, int fadeDuration)
            : this()
        {
            this.Duration = duration;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
            this.FadeDuration = fadeDuration;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                if (!string.IsNullOrEmpty(this.ImagePath))
                {
                    string imageFilePath = await this.ReplaceStringWithSpecialModifiers(this.ImagePath, user, arguments);
                    try
                    {
                        if (!string.IsNullOrEmpty(imageFilePath) && Uri.IsWellFormedUriString(imageFilePath, UriKind.RelativeOrAbsolute))
                        {
                            string tempFilePath = Path.GetTempFileName();
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFile(new Uri(imageFilePath), tempFilePath);
                            }
                            imageFilePath = tempFilePath;
                        }

                        if (File.Exists(imageFilePath))
                        {
                            byte[] byteData = File.ReadAllBytes(imageFilePath);
                            this.imageData = Convert.ToBase64String(byteData);
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }

                    if (this.imageData != null)
                    {
                        await ChannelSession.Services.OverlayServer.SetImage(new OverlayImage()
                        {
                            imagePath = imageFilePath, imageData = this.imageData, width = this.ImageWidth, height = this.ImageHeight,
                            duration = this.Duration, horizontal = this.Horizontal, vertical = this.Vertical, fadeDuration = this.FadeDuration,
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(this.Text))
                {
                    string text = await this.ReplaceStringWithSpecialModifiers(this.Text, user, arguments);
                    await ChannelSession.Services.OverlayServer.SetText(new OverlayText()
                    {
                        text = text, color = this.Color, fontSize = this.FontSize, duration = this.Duration, horizontal = this.Horizontal,
                        vertical = this.Vertical, fadeDuration = this.FadeDuration,
                    });
                }
                else if (!string.IsNullOrEmpty(this.youtubeVideoID))
                {
                    await ChannelSession.Services.OverlayServer.SetYoutubeVideo(new OverlayYoutubeVideo()
                    {
                        videoID = this.youtubeVideoID, startTime = this.youtubeStartTime, height = this.VideoHeight, width = this.VideoWidth,
                        duration = this.Duration, horizontal = this.Horizontal, vertical = this.Vertical, fadeDuration = this.FadeDuration,
                    });
                }
                else if (!string.IsNullOrEmpty(this.localVideoFilePath))
                {
                    await ChannelSession.Services.OverlayServer.SetLocalVideo(new OverlayLocalVideo()
                    {
                        filepath = this.localVideoFilePath, height = this.VideoHeight, width = this.VideoWidth,
                        duration = this.Duration, horizontal = this.Horizontal, vertical = this.Vertical, fadeDuration = this.FadeDuration
                    });
                }
                else if (!string.IsNullOrEmpty(this.HTMLText))
                {
                    string htmlText = await this.ReplaceStringWithSpecialModifiers(this.HTMLText, user, arguments);
                    await ChannelSession.Services.OverlayServer.SetHTMLText(new OverlayHTML()
                    {
                        htmlText = htmlText, duration = this.Duration, horizontal = this.Horizontal, vertical = this.Vertical, fadeDuration = this.FadeDuration,
                    });
                }
            }
        }
    }
}
