using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;
using System.Windows;
using MixItUp.WPF.Controls.MainControls;
using System.Windows.Media;
using System.Threading.Tasks;
using System;
using MixItUp.Base.Util;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        public UserViewModel User { get; private set; }

        public ChatUserControl()
        {
            this.DataContextChanged += ChatUserControl_DataContextChanged;

            InitializeComponent();
        }

        public ChatUserControl(UserViewModel user)
        {
            this.DataContext = this.User = user;

            this.Loaded += ChatUserControl_Loaded;
            this.DataContextChanged += ChatUserControl_DataContextChanged;

            InitializeComponent();
        }

        public bool MatchesUser(UserViewModel other)
        {
            UserViewModel user = this.User;
            return user != null && user.Equals(other);
        }

        private void ChatUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeControls();
        }

        private void ChatUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.User = (UserViewModel)this.DataContext;
            this.InitializeControls();
        }

        private void InitializeControls()
        {
            try
            {
                if (this.IsLoaded && this.User != null)
                {
                    this.UserNameTextBlock.Foreground = Application.Current.FindResource(this.User.PrimaryRoleColorName) as SolidColorBrush;

                    if (!string.IsNullOrEmpty(this.User.AvatarLink))
                    {
                        Task.Run(() => this.Dispatcher.Invoke(() => this.UserAvatar.SetUserAvatarUrl(this.User)));
                    }

                    if (ChatControl.SubscriberBadgeBitmap != null && this.User.IsSubscriber)
                    {
                        this.SubscriberImage.Visibility = Visibility.Visible;
                        this.SubscriberImage.Source = ChatControl.SubscriberBadgeBitmap;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
