using Mixer.Base;
using MixItUp.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            this.Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string clientID = ConfigurationManager.AppSettings["ClientID"];
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException("ClientID value isn't set in application configuration");
            }

            bool mixerInitialized = await MixerAPIHandler.InitializeMixerClient(clientID,
                new List<ClientScopeEnum>()
                {
                    ClientScopeEnum.chat__bypass_links,
                    ClientScopeEnum.chat__bypass_slowchat,
                    ClientScopeEnum.chat__change_ban,
                    ClientScopeEnum.chat__change_role,
                    ClientScopeEnum.chat__chat,
                    ClientScopeEnum.chat__connect,
                    ClientScopeEnum.chat__clear_messages,
                    ClientScopeEnum.chat__edit_options,
                    ClientScopeEnum.chat__giveaway_start,
                    ClientScopeEnum.chat__poll_start,
                    ClientScopeEnum.chat__poll_vote,
                    ClientScopeEnum.chat__purge,
                    ClientScopeEnum.chat__remove_message,
                    ClientScopeEnum.chat__timeout,
                    ClientScopeEnum.chat__view_deleted,
                    ClientScopeEnum.chat__whisper,

                    ClientScopeEnum.channel__details__self,
                    ClientScopeEnum.channel__update__self,

                    ClientScopeEnum.user__details__self,
                    ClientScopeEnum.user__log__self,
                    ClientScopeEnum.user__notification__self,
                    ClientScopeEnum.user__update__self,
                });

            if (mixerInitialized)
            {
                await this.Chat.Initialize(await MixerAPIHandler.MixerConnection.Channels.GetChannel("SaviorXTanren"));
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MixerAPIHandler.Close();
            Application.Current.Shutdown();
        }
    }
}
