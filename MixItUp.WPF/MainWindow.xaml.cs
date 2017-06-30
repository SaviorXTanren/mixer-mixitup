using Mixer.Base;
using MixItUp.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Windows;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string clientID = ConfigurationManager.AppSettings["ClientID"];
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException("ClientID value isn't set in application configuration");
            }

            await MixerAPIHandler.InitializeMixerClient(clientID,
                new List<ClientScopeEnum>()
                {
                    ClientScopeEnum.chat__chat,
                    ClientScopeEnum.chat__connect,
                    ClientScopeEnum.channel__details__self,
                    ClientScopeEnum.channel__update__self,
                    ClientScopeEnum.user__details__self,
                    ClientScopeEnum.user__log__self,
                    ClientScopeEnum.user__notification__self,
                    ClientScopeEnum.user__update__self,
                },
                (string code) =>
                {
                    Process.Start("https://mixer.com/oauth/shortcode?code=" + code);
                }
            );
        }
    }
}
