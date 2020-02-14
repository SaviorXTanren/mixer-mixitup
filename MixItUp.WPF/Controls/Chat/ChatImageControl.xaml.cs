using MixItUp.Base;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.WPF.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
                        CroppedBitmap croppedBitmap = new CroppedBitmap(ChatImageControl.bitmapImages[emote.Uri], new Int32Rect((int)emote.X, (int)emote.Y, (int)emote.Width, (int)emote.Height));
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
                            await this.DownloadImageUrl(emote.Url);
                            this.Image.Source = ChatImageControl.bitmapImages[emote.Url];
                            this.Image.ToolTip = this.AltText.Text = emote.code;
                        }
                    }
                    else if (this.DataContext is MixerSkillModel)
                    {
                        MixerSkillModel skill = (MixerSkillModel)this.DataContext;
                        await this.DownloadImageUrl(skill.Image);
                        this.Image.Source = ChatImageControl.bitmapImages[skill.Image];
                        this.Image.ToolTip = this.AltText.Text = skill.Name;
                    }
                    else if (this.DataContext is string)
                    {
                        string imageUrl = (string)this.DataContext;
                        await this.DownloadImageUrl(imageUrl);
                        this.Image.Source = ChatImageControl.bitmapImages[imageUrl];
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

        private async Task DownloadImageUrl(string url)
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
        }
    }
}
