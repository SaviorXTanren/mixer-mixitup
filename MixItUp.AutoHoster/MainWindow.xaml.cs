using MaterialDesignThemes.Wpf;
using Mixer.Base.Model.User;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.AutoHoster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async Task Login()
        {
            await this.RunAsyncOperation(async () =>
            {
                this.DataContext = this.viewModel = new MainWindowViewModel();

                if (!await this.viewModel.Initialize())
                {
                    this.Close();
                    return;
                }

                PrivatePopulatedUserModel currentUser = await this.viewModel.GetCurrentUser();
                if (currentUser != null)
                {
                    this.Title += " - " + currentUser.username;
                }

                Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            await this.viewModel.Run();
                        }
                        catch (Exception ex) { Logger.Log(ex); }

                        await Task.Delay(1000 * 60);
                    }
                }).Wait(1);
            });
        }

        private async void EnableDisableToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                ToggleButton button = (ToggleButton)sender;
                ChannelHostModel channel = (ChannelHostModel)button.DataContext;
                channel.IsEnabled = button.IsChecked.GetValueOrDefault();
                await this.viewModel.SaveData();
            });
        }

        private async void UpButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                ChannelHostModel channel = (ChannelHostModel)button.DataContext;
                await this.viewModel.MoveChannelUp(channel);
            });
        }

        private async void DownButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                ChannelHostModel channel = (ChannelHostModel)button.DataContext;
                await this.viewModel.MoveChannelDown(channel);
            });
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                ChannelHostModel channel = (ChannelHostModel)button.DataContext;
                await this.viewModel.DeleteChannel(channel);
            });
        }

        private async void AddChannelButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                BasicTextEntryDialogControl control = new BasicTextEntryDialogControl("Channel Name:");
                await this.ShowDialog(control);
                if (!await this.viewModel.AddChannel(control.TextEntry))
                {
                    await this.ShowDialog(new BasicDialogControl("The channel does not exist"));
                }
            });
        }

        private async Task ShowDialog(UserControl control)
        {
            object dialogObj = this.FindName("MDDialogHost");
            if (dialogObj != null && dialogObj is DialogHost)
            {
                this.IsEnabled = false;
                await ((DialogHost)dialogObj).ShowDialog(control);
                this.IsEnabled = true;
            }
        }

        private async Task RunAsyncOperation(Func<Task> func)
        {
            this.StatusBar.Visibility = Visibility.Visible;
            this.IsEnabled = false;
            await func();
            this.IsEnabled = true;
            this.StatusBar.Visibility = Visibility.Hidden;
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await this.viewModel.Logout();
            await Login();
        }
    }
}
