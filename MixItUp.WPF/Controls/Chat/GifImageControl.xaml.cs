using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for GifImageControl.xaml
    /// </summary>
    public partial class GifImageControl : UserControl
    {
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(int), typeof(GifImageControl), new PropertyMetadata(0));
        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set
            {
                SetValue(SizeProperty, value);
                this.SetSize(value);
            }
        }

        public GifImageControl()
        {
            InitializeComponent();

            this.Loaded += GifImageControl_Loaded;
            this.DataContextChanged += GifImageControl_DataContextChanged;
        }

        private void GifImageControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GifImageControl_DataContextChanged(this, new DependencyPropertyChangedEventArgs());
        }

        private void GifImageControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.Size > 0)
                {
                    this.SetSize(this.Size);
                }
            }
        }

        public void SetSize(int size)
        {
            this.GifImage.Width = size;
            this.GifImage.Height = size;
        }
    }
}
