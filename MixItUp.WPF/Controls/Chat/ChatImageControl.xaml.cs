using MixItUp.Base;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatImageControl.xaml
    /// </summary>
    public partial class ChatImageControl : UserControl
    {
        private ChatEmoteViewModelBase emote;
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

            this.Loaded += ChatImageControl_Loaded;
            this.DataContextChanged += ChatImageControl_DataContextChanged;

            this.Image.Loaded += Image_Loaded;
            this.Image.DataContextChanged += Image_DataContextChanged;

            this.GifImage.Loaded += Image_Loaded;
            this.GifImage.DataContextChanged += Image_DataContextChanged;

            this.SVGImage.Loaded += Image_Loaded;
            this.SVGImage.DataContextChanged += Image_DataContextChanged;
        }

        public ChatImageControl(ChatEmoteViewModelBase emote) : this() { this.DataContext = emote; }

        private void ChatImageControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ChatImageControl_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private void ChatImageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.DataContext != null && this.DataContext is ChatEmoteViewModelBase)
                {
                    this.emote = (ChatEmoteViewModelBase)this.DataContext;
                    this.ProcessEmote(emote);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async void Image_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (emote != null)
                {
                    await this.ProcessEmote(emote);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async void Image_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (emote != null)
                {
                    await this.ProcessEmote(emote);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task ProcessEmote(ChatEmoteViewModelBase emote)
        {
            if (!loaded)
            {
                Image image = null;
                MediaElement mediaElement = null;
                if (emote.IsGIFImage && !ChannelSession.Settings.DisableAnimatedEmotes)
                {
                    mediaElement = this.GifImage;
                }
                else if (emote.IsSVGImage)
                {
                    image = this.SVGImage;
                }
                else
                {
                    image = this.Image;
                }

                if (image != null && image.IsLoaded)
                {
                    loaded = true;
                    this.ResizeImage(image);
                    image.DataContext = emote;
                    image.Visibility = Visibility.Visible;
                }
                else if (mediaElement != null && mediaElement.IsLoaded)
                {
                    if (emote.LocalFilePath == null)
                    {
                        await emote.SaveToTempFolder();
                    }

                    if (emote.LocalFilePath != null)
                    {
                        loaded = true;
                        mediaElement.Source = new Uri(emote.LocalFilePath, UriKind.Absolute);
                        mediaElement.MaxWidth = mediaElement.MaxHeight = mediaElement.Width = mediaElement.Height = ChannelSession.Settings.ChatFontSize * 2;
                        mediaElement.DataContext = emote;
                        mediaElement.Visibility = Visibility.Visible;
                        mediaElement.Play();
                    }
                }

                if (loaded)
                {
                    this.AltText.Text = emote.Name;
                    if (emote is TwitchBitsCheerViewModel)
                    {
                        TwitchBitsCheerViewModel bitsCheer = (TwitchBitsCheerViewModel)emote;
                        this.Text.Visibility = Visibility.Visible;
                        this.Text.Text = bitsCheer.Amount.ToString();
                    }
                }
            }
        }

        private void ResizeImage(Image image) { image.MaxWidth = image.MaxHeight = image.Width = image.Height = ChannelSession.Settings.ChatFontSize * 2; }
    }
}
