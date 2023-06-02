using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Users
{
    /// <summary>
    /// Interaction logic for UserProfileAvatarControl.xaml
    /// </summary>
    public partial class UserProfileAvatarControl : UserControl
    {
        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(int), typeof(UserProfileAvatarControl), new PropertyMetadata(0));
        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set
            {
                SetValue(SizeProperty, value);
                this.SetSize(value);
            }
        }

        public UserProfileAvatarControl()
        {
            InitializeComponent();

            this.Loaded += UserProfileAvatarControl_Loaded;
            this.DataContextChanged += UserProfileAvatarControl_DataContextChanged;
        }

        private void UserProfileAvatarControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UserProfileAvatarControl_DataContextChanged(sender, new System.Windows.DependencyPropertyChangedEventArgs());
        }

        private void UserProfileAvatarControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
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
            this.ProfileAvatarContainer.Width = size;
            this.ProfileAvatarContainer.Height = size;
        }
    }
}
