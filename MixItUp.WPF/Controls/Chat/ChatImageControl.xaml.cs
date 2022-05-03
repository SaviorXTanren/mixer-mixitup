using MixItUp.Base;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Glimesh;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.Chat.YouTube;
using StreamingClient.Base.Util;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatImageControl.xaml
    /// </summary>
    public partial class ChatImageControl : UserControl
    {
        private IChatEmoteViewModel emote;
        private bool loaded = false;

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

        public ChatImageControl()
        {
            InitializeComponent();

            this.Loaded += ChatEmoteControl_Loaded;
            this.DataContextChanged += EmoticonControl_DataContextChanged;

            this.Image.Loaded += Image_Loaded;
            this.Image.DataContextChanged += Image_DataContextChanged;

            this.GifImage.Loaded += Image_Loaded;
            this.GifImage.DataContextChanged += Image_DataContextChanged;

            this.SVGImage.Loaded += Image_Loaded;
            this.SVGImage.DataContextChanged += Image_DataContextChanged;
        }

        public ChatImageControl(TwitchChatEmoteViewModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(BetterTTVEmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(FrankerFaceZEmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(TwitchBitsCheerViewModel bitsCheer) : this() { this.DataContext = bitsCheer; }

        public ChatImageControl(GlimeshChatEmoteViewModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(TrovoChatEmoteViewModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(YouTubeChatEmoteViewModel emote) : this() { this.DataContext = emote; }

        private void ChatEmoteControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.EmoticonControl_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private void EmoticonControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.DataContext != null && this.DataContext is IChatEmoteViewModel)
                {
                    this.emote = (IChatEmoteViewModel)this.DataContext;
                    this.ProcessEmote(emote);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void ProcessEmote(IChatEmoteViewModel emote)
        {
            bool forceGIF = false;
            if (emote is TwitchChatEmoteViewModel)
            {
                forceGIF = ((TwitchChatEmoteViewModel)emote).IsAnimated;
            }
            else if (emote is BetterTTVEmoteModel)
            {
                forceGIF = ((BetterTTVEmoteModel)emote).IsGIF;
            }

            Image image = this.Image;
            if (forceGIF || this.IsGIFImage(emote.ImageURL))
            {
                image = this.GifImage;
            }
            else if (this.IsSVGImage(emote.ImageURL))
            {
                image = this.SVGImage;
            }

            if (image.IsLoaded && !loaded)
            {
                loaded = true;
                this.ResizeImage(image);
                image.DataContext = emote;
                image.Visibility = Visibility.Visible;
                this.AltText.Text = emote.Name;

                if (emote is TwitchBitsCheerViewModel)
                {
                    TwitchBitsCheerViewModel bitsCheer = (TwitchBitsCheerViewModel)emote;
                    this.Text.Visibility = Visibility.Visible;
                    this.Text.Text = bitsCheer.Amount.ToString();
                }
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private void Image_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (emote != null)
                {
                    this.ProcessEmote(emote);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private bool IsGIFImage(string url) { return url.Contains(".gif"); }
        private bool IsSVGImage(string url) { return url.Contains(".svg"); }

        private void ResizeImage(Image image) { image.MaxWidth = image.MaxHeight = image.Width = image.Height = ChannelSession.Settings.ChatFontSize * 2; }
    }
}
