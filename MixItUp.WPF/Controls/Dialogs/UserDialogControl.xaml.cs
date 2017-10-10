using MixItUp.Base.ViewModel;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for UserDialogControl.xaml
    /// </summary>
    public partial class UserDialogControl : UserControl
    {
        private UserViewModel user;

        public UserDialogControl(UserViewModel user)
        {
            this.DataContext = this.user = user;

            InitializeComponent();

            this.Loaded += UserDialogControl_Loaded;
        }

        private void UserDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UserAvatar.SetSize(100);
            this.UserAvatar.SetImageUrl(this.user.AvatarLink);
        }
    }
}
