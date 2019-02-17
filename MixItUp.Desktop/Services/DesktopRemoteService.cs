using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.SignalR.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopRemoteService : LocalRemoteServiceBase
    {
        public DesktopRemoteService(string apiAddress, string signalRAddress) : base(apiAddress, new SignalRConnection(signalRAddress)) { }

        public override async Task InitializeConnection(RemoteConnectionAuthenticationTokenModel connection)
        {
            if (!this.IsConnected)
            {
                this.ListenForRequestProfiles(async () =>
                {
                    try
                    {
                        await this.SendProfiles(ChannelSession.Settings.RemoteProfiles.Values.Select(p => p.Profile));
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                this.ListenForRequestProfileBoard(async (profileID) =>
                {
                    try
                    {
                        if (ChannelSession.Settings.RemoteProfiles.ContainsKey(profileID))
                        {
                            await this.SendProfileBoard(ChannelSession.Settings.RemoteProfiles[profileID]);
                        }
                        else
                        {
                            await this.SendProfileBoard(null);
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                this.ListenForSendCommand(async (commandID) =>
                {
                    try
                    {
                        CommandBase command = ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(commandID));
                        if (command != null)
                        {
                            await command.Perform();
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                await this.Connect();
                await this.Authenticate(connection.ID, ChannelSession.SecretManager.GetSecret("RemoteHostSecret"), connection.AccessToken);
                await Task.Delay(2000);
            }
        }

        public override async Task<RemoteConnectionAuthenticationTokenModel> NewHost(string name)
        {
            return await this.AsyncWrapper<RemoteConnectionAuthenticationTokenModel>(async () =>
            {
                using (HttpClient httpClient = this.GetHttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync("authentication/newhost?name=" + name);
                    return SerializerHelper.DeserializeFromString<RemoteConnectionAuthenticationTokenModel>(await response.Content.ReadAsStringAsync());
                }
            });
        }

        public override Task<RemoteConnectionShortCodeModel> NewClient(string name) { throw new System.NotImplementedException(); }

        public override Task<RemoteConnectionAuthenticationTokenModel> ValidateClient(RemoteConnectionShortCodeModel shortCode) { throw new System.NotImplementedException(); }

        public override async Task<RemoteConnectionModel> ApproveClient(RemoteConnectionModel connection, string clientShortCode)
        {
            return await this.AsyncWrapper<RemoteConnectionAuthenticationTokenModel>(async () =>
            {
                using (HttpClient httpClient = this.GetHttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(string.Format("authentication/approveclient?hostID={0}&shortCode={1}", connection.ID, clientShortCode));
                    return SerializerHelper.DeserializeFromString<RemoteConnectionAuthenticationTokenModel>(await response.Content.ReadAsStringAsync());
                }
            });
        }

        public override async Task<bool> RemoveClient(RemoteConnectionModel hostConnection, RemoteConnectionModel clientConnection)
        {
            return await this.AsyncWrapper<bool>(async () =>
            {
                using (HttpClient httpClient = this.GetHttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(string.Format("authentication/removeclient?hostID={0}&clientID={1}", hostConnection.ID, clientConnection.ID));
                    return SerializerHelper.DeserializeFromString<bool>(await response.Content.ReadAsStringAsync());
                }
            });
        }
    }
}
