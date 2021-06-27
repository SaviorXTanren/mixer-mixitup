using MixItUp.Base;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.WPF.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;

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

        public ChatImageControl(TwitchChatEmoteViewModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(BetterTTVEmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(FrankerFaceZEmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(TwitchBitsCheerViewModel bitsCheer) : this() { this.DataContext = bitsCheer; }

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
                    if (this.DataContext is TwitchChatEmoteViewModel)
                    {
                        TwitchChatEmoteViewModel emote = (TwitchChatEmoteViewModel)this.DataContext;
                        this.ProcessGifImage(emote.ImageURL, emote.Name);
                        this.Image.ToolTip = this.AltText.Text = emote.Name;
                    }
                    else if (this.DataContext is BetterTTVEmoteModel)
                    {
                        BetterTTVEmoteModel emote = (BetterTTVEmoteModel)this.DataContext;
                        if (emote.imageType.Equals("gif"))
                        {
                            this.ProcessGifImage(emote.url, emote.code);
                        }
                        else
                        {
                            this.Image.Source = await this.DownloadImageUrl(emote.url);
                        }
                        this.Image.ToolTip = this.AltText.Text = emote.code;
                    }
                    else if (this.DataContext is FrankerFaceZEmoteModel)
                    {
                        FrankerFaceZEmoteModel emote = (FrankerFaceZEmoteModel)this.DataContext;
                        this.Image.Source = await this.DownloadImageUrl(emote.url);
                        this.Image.ToolTip = this.AltText.Text = emote.name;
                    }
                    else if (this.DataContext is TwitchBitsCheerViewModel)
                    {
                        TwitchBitsCheerViewModel bitsCheer = (TwitchBitsCheerViewModel)this.DataContext;
                        this.Image.Source = await this.DownloadImageUrl((ChannelSession.AppSettings.IsDarkBackground) ? bitsCheer.Tier.DarkImage : bitsCheer.Tier.LightImage);
                        this.Image.ToolTip = this.AltText.Text = bitsCheer.Text;
                        this.Text.Visibility = Visibility.Visible;
                        this.Text.Text = bitsCheer.Amount.ToString();
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
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task<BitmapImage> DownloadImageUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (!ChatImageControl.bitmapImages.ContainsKey(url))
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (WebClient client = new WebClient())
                    {
                        var bytes = await Task.Run<byte[]>(async () =>
                        {
                            try
                            {
                                return await client.DownloadDataTaskAsync(url);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Failed to download image: " + url);
                                throw ex;
                            }
                        });
                        bitmap = WindowsImageService.Load(bytes);
                    }
                    ChatImageControl.bitmapImages[url] = bitmap;
                }
                return ChatImageControl.bitmapImages[url];
            }
            return null;
        }

        private void ProcessGifImage(string url, string code)
        {
            this.GifImage.SetSize(ChannelSession.Settings.ChatFontSize * 2);
            this.GifImage.DataContext = url;
            this.GifImage.ToolTip = code;
            this.GifImage.Visibility = Visibility.Visible;
        }
    }
}
