using MixItUp.Base.Model.Chat;
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
        private static Dictionary<string, BitmapImage> emoticonBitmapImages = new Dictionary<string, BitmapImage>();

        public MixerChatEmoteModel Emoticon { get { return this.DataContext as MixerChatEmoteModel; } }
        public bool ShowText
        {
            get { return EmoticonText.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    EmoticonText.Visibility = Visibility.Visible;
                }
                else
                {
                    EmoticonText.Visibility = Visibility.Collapsed;
                }
            }
        }

        public ChatEmoteControl()
        {
            InitializeComponent();

            this.Loaded += ChatEmoteControl_Loaded;
            this.DataContextChanged += EmoticonControl_DataContextChanged;
        }

        public ChatEmoteControl(MixerChatEmoteModel emoticon)
            : this()
        {
            this.DataContext = emoticon;
        }

        private void ChatEmoteControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.EmoticonControl_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private async void EmoticonControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.Emoticon != null)
                {
                    string uri = Emoticon.Uri;
                    if (!ChatEmoteControl.emoticonBitmapImages.ContainsKey(uri))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        using (WebClient client = new WebClient())
                        {
                            var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(uri); });

                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = new MemoryStream(bytes);
                            bitmap.EndInit();
                        }
                        ChatEmoteControl.emoticonBitmapImages[uri] = bitmap;
                    }

                    CroppedBitmap croppedBitmap = new CroppedBitmap(ChatEmoteControl.emoticonBitmapImages[uri],
                        new Int32Rect((int)Emoticon.X, (int)Emoticon.Y, (int)Emoticon.Width, (int)Emoticon.Height));

                    this.EmoteImage.Source = croppedBitmap;
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
