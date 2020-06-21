using MixItUp.Base;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.Base.Services.Twitch;
using MixItUp.WPF.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TwitchV5API = Twitch.Base.Models.V5.Emotes;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatImageControl.xaml
    /// </summary>
    public partial class ChatImageControl : UserControl
    {
        public bool ShowText
        {
            get { return this.AltText.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    this.AltText.Visibility = Visibility.Visible;
                }
                else
                {
                    this.AltText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static Dictionary<string, BitmapImage> bitmapImages = new Dictionary<string, BitmapImage>();

        public ChatImageControl()
        {
            InitializeComponent();

            this.Loaded += ChatEmoteControl_Loaded;
            this.DataContextChanged += EmoticonControl_DataContextChanged;
        }

        public ChatImageControl(MixerChatEmoteModel emoticon) : this() { this.DataContext = emoticon; }

        public ChatImageControl(MixrElixrEmoteModel emoticon) : this() { this.DataContext = emoticon; }

        public ChatImageControl(MixerSkillModel skill) : this() { this.DataContext = skill; }

        public ChatImageControl(TwitchV5API.EmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(BetterTTVEmoteModel emote) : this() { this.DataContext = emote; }

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
                        CroppedBitmap croppedBitmap = new CroppedBitmap(await this.DownloadImageUrl(emote.Uri), new Int32Rect((int)emote.X, (int)emote.Y, (int)emote.Width, (int)emote.Height));
                        this.Image.Source = croppedBitmap;
                        this.Image.ToolTip = this.AltText.Text = emote.Name;
                    }
                    else if (this.DataContext is MixrElixrEmoteModel)
                    {
                        MixrElixrEmoteModel emote = (MixrElixrEmoteModel)this.DataContext;
                        if (emote.animated)
                        {
                            this.GifImage.DataContext = emote.Url;
                            this.GifImage.ToolTip = this.AltText.Text = emote.code;
                        }
                        else
                        {
                            this.Image.Source = await this.DownloadImageUrl(emote.Url);
                            this.Image.ToolTip = this.AltText.Text = emote.code;
                        }
                    }
                    else if (this.DataContext is MixerSkillModel)
                    {
                        MixerSkillModel skill = (MixerSkillModel)this.DataContext;
                        this.Image.Source = await this.DownloadImageUrl(skill.Image);
                        this.Image.ToolTip = this.AltText.Text = skill.Name;
                    }
                    else if (this.DataContext is TwitchV5API.EmoteModel)
                    {
                        TwitchV5API.EmoteModel emote = (TwitchV5API.EmoteModel)this.DataContext;
                        this.Image.Source = await this.DownloadImageUrl(emote.URL);
                        this.Image.ToolTip = this.AltText.Text = emote.code;
                    }
                    else if (this.DataContext is BetterTTVEmoteModel)
                    {
                        BetterTTVEmoteModel emote = (BetterTTVEmoteModel)this.DataContext;
                        this.Image.Source = await this.DownloadImageUrl(emote.url);
                        this.Image.ToolTip = this.AltText.Text = emote.code;
                    }
                    else if (this.DataContext is string)
                    {
                        string imageUrl = (string)this.DataContext;
                        this.Image.Source = await this.DownloadImageUrl(imageUrl);
                    }

                    if (this.Image.Source != null)
                    {
                        this.Image.MaxWidth = this.Image.MaxHeight = this.Image.Width = this.Image.Height = ChannelSession.Settings.ChatFontSize * 2;
                    }
                    else if (this.GifImage.DataContext != null)
                    {
                        this.GifImage.MaxWidth = this.GifImage.MaxHeight = this.GifImage.Width = this.GifImage.Height = ChannelSession.Settings.ChatFontSize * 2;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task<BitmapImage> DownloadImageUrl(string url)
        {
            if (!ChatImageControl.bitmapImages.ContainsKey(url))
            {
                BitmapImage bitmap = new BitmapImage();
                using (WebClient client = new WebClient())
                {
                    var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(url); });
                    bitmap = WindowsImageService.Load(bytes);
                }
                ChatImageControl.bitmapImages[url] = bitmap;
            }
            return ChatImageControl.bitmapImages[url];
        }
    }
}
