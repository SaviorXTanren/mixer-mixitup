using Mixer.Base.Model.Game;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.ViewModel.Favorites;
using MixItUp.WPF.Util;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Favorites
{
    public class FavoriteUser
    {
        public UserWithChannelModel User { get; private set; }
        private GameTypeSimpleModel game;

        public string Name { get { return User.username; } }

        public bool Live { get { return User.channel.online; } }

        public int Viewers { get { return (int)User.channel.viewersCurrent; } }

        public string Game { get { return (game != null) ? game.name : "No Game"; } }

        public string Link { get { return string.Format("https://www.mixer.com/{0}", this.Name); } }

        public bool CanBeRemoved { get; private set; }

        public FavoriteUser(UserWithChannelModel user, GameTypeSimpleModel game, bool canBeRemoved = false)
        {
            this.User = user;
            this.game = game;
            this.CanBeRemoved = canBeRemoved;
        }
    }

    /// <summary>
    /// Interaction logic for FavoriteGroupWindow.xaml
    /// </summary>
    public partial class FavoriteGroupWindow : LoadingWindowBase
    {
        private FavoriteGroupViewModel favoriteGroup;

        private ObservableCollection<FavoriteUser> groupUsers = new ObservableCollection<FavoriteUser>();

        public FavoriteGroupWindow(FavoriteGroupViewModel favoriteGroup)
        {
            this.favoriteGroup = favoriteGroup;

            InitializeComponent();

            this.Initialize(this.StatusBar);

            this.Loaded += CurrencyUserEditorWindow_Loaded;
        }

        private async void CurrencyUserEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += favoriteGroup.Name;
            this.FavoriteGroupsDataGrid.ItemsSource = this.groupUsers;

            await this.RefreshView();
        }

        private void ViewStreamButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FavoriteUser user = (FavoriteUser)button.DataContext;
            Process.Start(user.Link);
        }

        private async void HostChannelButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FavoriteUser user = (FavoriteUser)button.DataContext;
            await this.RunAsyncOperation(async () =>
            {
                await ChannelSession.Connection.SetHostChannel(ChannelSession.Channel, user.User.channel);
            });
        }

        private async void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FavoriteUser user = (FavoriteUser)button.DataContext;
            await this.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you want to remove this user?"))
                {
                    this.favoriteGroup.RemoteUser(user.User);
                    await ChannelSession.SaveSettings();

                    await this.RefreshView();
                }
            });
        }

        private async Task RefreshView()
        {
            this.groupUsers.Clear();
            await this.RunAsyncOperation(async () =>
            {
                await this.favoriteGroup.RefreshGroup();

                foreach (UserWithChannelModel user in this.favoriteGroup.LastCheckUsers.OrderByDescending(u => u.channel.online).ThenByDescending(u => u.channel.viewersCurrent))
                {
                    GameTypeSimpleModel game = null;
                    if (user.channel.typeId != null)
                    {
                        game = await ChannelSession.Connection.GetGameType(user.channel.typeId.GetValueOrDefault());
                    }
                    this.groupUsers.Add(new FavoriteUser(user, game, !this.favoriteGroup.IsTeam));
                }
            });
        }
    }
}
