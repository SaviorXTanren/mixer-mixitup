using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Desktop.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.AutoHoster
{
    public class Program
    {
        private const string SettingsFileName = "Settings.txt";

        private static MixerConnectionWrapper Connection = null;
        private static ExpandedChannelModel Channel = null;

        private static ExpandedChannelModel CurrentlyHostedChannel = null;

        public static void Main(string[] args)
        {
            try
            {
                DesktopServicesHandler desktopServicesHandler = new DesktopServicesHandler();
                desktopServicesHandler.Initialize();

                Logger.Initialize(desktopServicesHandler.FileService);
                SerializerHelper.Initialize(desktopServicesHandler.FileService);

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                ChannelSession.Initialize(desktopServicesHandler);

                AsyncRunner.RunAsync(async () =>
                {
                    if (await Program.LogInBotAccount())
                    {
                        Program.Channel = await Program.Connection.GetChannel("MixItUpBot");
                        while (true)
                        {
                            if (CurrentlyHostedChannel != null)
                            {
                                CurrentlyHostedChannel = await Program.Connection.GetChannel(CurrentlyHostedChannel);
                                if (CurrentlyHostedChannel != null && !CurrentlyHostedChannel.online)
                                {
                                    Console.WriteLine("Channel has gone offline: " + CurrentlyHostedChannel.id + " - " + CurrentlyHostedChannel.user.username);
                                    CurrentlyHostedChannel = null;
                                }
                            }
                            else
                            {
                                CurrentlyHostedChannel = await Program.FindChannelToHost();
                                if (CurrentlyHostedChannel != null)
                                {
                                    if (!await Program.HostChannel(CurrentlyHostedChannel))
                                    {
                                        Console.WriteLine("Failed to host: " + CurrentlyHostedChannel.id + " - " + CurrentlyHostedChannel.user.username);
                                        CurrentlyHostedChannel = null;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Successfully hosted: " + CurrentlyHostedChannel.id + " - " + CurrentlyHostedChannel.user.username);
                                        await Program.SendHelloMessage(CurrentlyHostedChannel);
                                    }
                                }
                            }

                            await SerializerHelper.SerializeToFile(SettingsFileName, Program.Connection.Connection.GetOAuthTokenCopy());

                            await Task.Delay(60000);
                        }
                    }
                }).Wait();
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private static async Task<bool> LogInBotAccount()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    OAuthTokenModel token = await SerializerHelper.DeserializeFromFile<OAuthTokenModel>(SettingsFileName);
                    if (token != null)
                    {
                        MixerConnection connection = await MixerConnection.ConnectViaOAuthToken(token);
                        if (connection != null)
                        {
                            Program.Connection = new MixerConnectionWrapper(connection);
                        }
                    }
                }

                if (Program.Connection == null)
                {
                    MixerConnection connection = await MixerConnection.ConnectViaLocalhostOAuthBrowser(ChannelSession.ClientID, ChannelSession.StreamerScopes, false, loginSuccessHtmlPageFilePath: "LoginRedirectPage.html");
                    if (connection != null)
                    {
                        Program.Connection = new MixerConnectionWrapper(connection);
                        return true;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        private static async Task<ExpandedChannelModel> FindChannelToHost()
        {
            try
            {
                return await Program.Connection.GetChannel("ThisGuyTom");
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        private static async Task<bool> HostChannel(ExpandedChannelModel channelToHost)
        {
            try
            {
                ChannelModel hosterChannel = await Program.Connection.SetHostChannel(Program.Channel, channelToHost);
                return (hosterChannel.hosteeId == channelToHost.id);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        private static async Task SendHelloMessage(ExpandedChannelModel channelToHost)
        {
            try
            {
                ChatClient client = await ChatClient.CreateFromChannel(Program.Connection.Connection, channelToHost);
                if (client != null)
                {
                    if (await client.Connect() && await client.Authenticate())
                    {
                        await client.SendMessage("Hello, you're being hosted by Mix It Up! As a thank you for using us, we're now showcasing you on our website! http://mixitupapp.com");
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log((Exception)e.ExceptionObject);
        }
    }
}
