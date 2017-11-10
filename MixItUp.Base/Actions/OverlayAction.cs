using MixItUp.Base.Services;
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

        protected override SemaphoreSlim AsyncSempahore { get { return OverlayAction.asyncSemaphore; } }

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
        public double Duration;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        private string imageData { get; set; }

        public OverlayAction() : base(ActionTypeEnum.Overlay) { }

        public OverlayAction(string imagePath, int width, int height, double duration, int horizontal, int vertical)
            : this(duration, horizontal, vertical)
        {
            this.ImagePath = imagePath;
            this.ImageWidth = width;
            this.ImageHeight = height;
        }

        public OverlayAction(string text, string color, int fontSize, double duration, int horizontal, int vertical)
            : this(duration, horizontal, vertical)
        {
            this.Text = text;
            this.Color = color;
            this.FontSize = fontSize;
        }

        public OverlayAction(double duration, int horizontal, int vertical)
            : this()
        {
            this.Duration = duration;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
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
                        if (Uri.IsWellFormedUriString(imageFilePath, UriKind.RelativeOrAbsolute))
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
                    catch (Exception) { }

                    if (this.imageData != null)
                    {
                        ChannelSession.Services.OverlayServer.SetImage(new OverlayImage()
                        {
                            imagePath = imageFilePath,
                            width = this.ImageWidth,
                            height = this.ImageHeight,
                            duration = this.Duration,
                            horizontal = this.Horizontal,
                            vertical = this.Vertical,
                            imageData = this.imageData
                        });
                    }
                }
                else if (!string.IsNullOrEmpty(this.Text))
                {
                    string text = await this.ReplaceStringWithSpecialModifiers(this.Text, user, arguments);
                    ChannelSession.Services.OverlayServer.SetText(new OverlayText()
                    {
                        text = text, color = this.Color, fontSize = this.FontSize, duration = this.Duration, horizontal = this.Horizontal, vertical = this.Vertical
                    });
                }
            }
        }
    }
}
