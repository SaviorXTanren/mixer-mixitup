using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

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

        private void EmoticonControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!EmoticonControl.emoticonBitmapImages.ContainsKey(Emoticon.FilePath))
            {
                EmoticonControl.emoticonBitmapImages[Emoticon.FilePath] = new BitmapImage(new Uri(Emoticon.FilePath));
            }

            CroppedBitmap bitmap = new CroppedBitmap(
                EmoticonControl.emoticonBitmapImages[Emoticon.FilePath],
                new Int32Rect((int)Emoticon.X, (int)Emoticon.Y, (int)Emoticon.Width, (int)Emoticon.Height));

            this.EmoticonImage.Source = bitmap;
        }
    }
}
