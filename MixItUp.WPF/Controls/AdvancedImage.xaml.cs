using MixItUp.Base;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.WPF.Util;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for AdvancedImage.xaml
    /// </summary>
    public partial class AdvancedImage : Image
    {
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(AdvancedImage), new PropertyMetadata(null));

        public bool UseChatFontSize
        {
            get { return (bool)GetValue(UseChatFontSizeProperty); }
            set { SetValue(UseChatFontSizeProperty, value); }
        }
        public static readonly DependencyProperty UseChatFontSizeProperty = DependencyProperty.Register("UseChatFontSize", typeof(bool), typeof(AdvancedImage), new PropertyMetadata(false));

        public int ChatFontSizeScale
        {
            get { return (int)GetValue(ChatFontSizeScaleProperty); }
            set { SetValue(ChatFontSizeScaleProperty, value); }
        }
        public static readonly DependencyProperty ChatFontSizeScaleProperty = DependencyProperty.Register("ChatFontSizeScale", typeof(int), typeof(AdvancedImage), new PropertyMetadata(1));

        private bool initialized;

        public AdvancedImage()
        {
            InitializeComponent();

            this.Loaded += AdvancedImage_Loaded;
            this.DataContextChanged += AdvancedImage_DataContextChanged;
        }

        private void AdvancedImage_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) { this.Load(); }

        private void AdvancedImage_Loaded(object sender, System.Windows.RoutedEventArgs e) { this.Load(); }

        private void Load()
        {
            if (!this.initialized && this.DataContext != null && this.IsLoaded)
            {
                this.initialized = true;

                double width = this.Width;
                double height = this.Height;

                if (this.UseChatFontSize)
                {
                    width = height = ChannelSession.Settings.ChatFontSize * this.ChatFontSizeScale;
                }

                if (!string.IsNullOrEmpty(this.Path) || this.DataContext is string)
                {
                    ImageHelper.SetImageSource(this, this.Path ?? this.DataContext as string, width, height);
                }
                else if (this.DataContext is ChatEmoteViewModelBase)
                {
                    ChatEmoteViewModelBase emote = this.DataContext as ChatEmoteViewModelBase;
                    ImageHelper.SetImageSource(this, emote.ImageURL, width, height, emote.Name);
                }
            }
        }
    }
}
