using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using MixItUp.Base.Util;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for EmoticonControl.xaml
    /// </summary>
    public partial class EmoticonControl : UserControl
    {
        private static Dictionary<string, BitmapImage> emoticonBitmapImages = new Dictionary<string, BitmapImage>();

        public EmoticonImage Emoticon { get { return this.DataContext as EmoticonImage; } }
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

        public EmoticonControl()
        {
            this.DataContextChanged += EmoticonControl_DataContextChanged;
            InitializeComponent();
        }

        public EmoticonControl(EmoticonImage emoticon) : this()
        {
            InitializeComponent();
            this.DataContext = emoticon;
        }

        private async void EmoticonControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.Emoticon != null)
                {
                    string uri = Emoticon.Uri;
                    if (!EmoticonControl.emoticonBitmapImages.ContainsKey(uri))
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
                        EmoticonControl.emoticonBitmapImages[uri] = bitmap;
                    }

                    CroppedBitmap croppedBitmap = new CroppedBitmap(
                        EmoticonControl.emoticonBitmapImages[uri],
                        new Int32Rect((int)Emoticon.X, (int)Emoticon.Y, (int)Emoticon.Width, (int)Emoticon.Height));

                    this.EmoticonImage.Source = croppedBitmap;
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
