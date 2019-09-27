using MixItUp.Base;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatEmoteControl.xaml
    /// </summary>
    public partial class ChatEmoteControl : UserControl
    {
        public bool ShowText
        {
            get { return this.EmoticonText.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    this.EmoticonText.Visibility = Visibility.Visible;
                }
                else
                {
                    this.EmoticonText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static Dictionary<string, BitmapImage> emoticonBitmapImages = new Dictionary<string, BitmapImage>();

        public ChatEmoteControl()
        {
            InitializeComponent();

            this.Loaded += ChatEmoteControl_Loaded;
            this.DataContextChanged += EmoticonControl_DataContextChanged;
        }

        public ChatEmoteControl(MixerChatEmoteModel emoticon) : this() { this.DataContext = emoticon; }

        public ChatEmoteControl(MixrElixrEmoteModel emoticon) : this() { this.DataContext = emoticon; }

        private void ChatEmoteControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.EmoticonControl_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private async void EmoticonControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.DataContext != null)
                {
                    if (this.DataContext is MixerChatEmoteModel)
                    {
                        MixerChatEmoteModel emote = (MixerChatEmoteModel)this.DataContext;
                        await this.DownloadImageUrl(emote.Uri);
                        CroppedBitmap croppedBitmap = new CroppedBitmap(ChatEmoteControl.emoticonBitmapImages[emote.Uri], new Int32Rect((int)emote.X, (int)emote.Y, (int)emote.Width, (int)emote.Height));
                        this.EmoteImage.Source = croppedBitmap;
                        this.EmoteImage.ToolTip = this.EmoticonText.Text = emote.Name;
                    }
                    else if (this.DataContext is MixrElixrEmoteModel)
                    {
                        MixrElixrEmoteModel emote = (MixrElixrEmoteModel)this.DataContext;
                        if (emote.animated)
                        {
                            this.EmoteGifImage.DataContext = emote.Url;
                            this.EmoteGifImage.ToolTip = this.EmoticonText.Text = emote.code;
                        }
                        else
                        {
                            await this.DownloadImageUrl(emote.Url);
                            this.EmoteImage.Source = ChatEmoteControl.emoticonBitmapImages[emote.Url];
                            this.EmoteImage.ToolTip = this.EmoticonText.Text = emote.code;
                        }
                    }

                    if (this.EmoteImage.Source != null)
                    {
                        this.EmoteImage.Width = this.EmoteImage.Height = ChannelSession.Settings.ChatFontSize * 2;
                    }
                    else if (this.EmoteGifImage.DataContext != null)
                    {
                        this.EmoteGifImage.Width = this.EmoteGifImage.Height = ChannelSession.Settings.ChatFontSize * 2;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task DownloadImageUrl(string url)
        {
            if (!ChatEmoteControl.emoticonBitmapImages.ContainsKey(url))
            {
                BitmapImage bitmap = new BitmapImage();
                using (WebClient client = new WebClient())
                {
                    var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(url); });
                    bitmap = BitmapImageLoader.Load(bytes);
                }
                ChatEmoteControl.emoticonBitmapImages[url] = bitmap;
            }
        }
    }
}
