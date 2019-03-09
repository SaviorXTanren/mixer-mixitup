using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote;
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

        public override async Task<bool> InitializeConnection(RemoteConnectionAuthenticationTokenModel connection)
        {
            if (!this.IsConnected)
            {
                if (!await this.ValidateConnection(connection))
                {
                    return false;
                }

                this.AuthenticationToken = connection;

                this.ListenForRequestProfiles(async (clientID) =>
                {
                    try
                    {
                        RemoteConnectionModel clientConnection = ChannelSession.Settings.RemoteClientConnections.FirstOrDefault(c => c.ID.Equals(clientID));
                        if (clientConnection != null)
                        {
                            foreach (RemoteProfileBoardModel profileBoard in ChannelSession.Settings.RemoteProfiles.Values)
                            {
                                RemoteProfileBoardViewModel profileBoardViewModel = new RemoteProfileBoardViewModel(profileBoard);
                                profileBoardViewModel.BuildHashValidation();
                            }
                            await this.SendProfiles(ChannelSession.Settings.RemoteProfiles.Values.Where(p => !p.Profile.IsStreamer || clientConnection.IsStreamer).Select(p => p.Profile));
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                this.ListenForRequestProfileBoard(async (clientID, profileID) =>
                {
                    try
                    {
                        RemoteConnectionModel clientConnection = ChannelSession.Settings.RemoteClientConnections.FirstOrDefault(c => c.ID.Equals(clientID));
                        if (clientConnection != null)
                        {
                            if (ChannelSession.Settings.RemoteProfiles.ContainsKey(profileID) && (!ChannelSession.Settings.RemoteProfiles[profileID].Profile.IsStreamer || clientConnection.IsStreamer))
                            {
                                await this.SendProfileBoard(ChannelSession.Settings.RemoteProfiles[profileID]);
                            }
                            else
                            {
                                await this.SendProfileBoard(null);
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                this.ListenForSendCommand(async (clientID, commandID) =>
                {
                    try
                    {
                        RemoteConnectionModel clientConnection = ChannelSession.Settings.RemoteClientConnections.FirstOrDefault(c => c.ID.Equals(clientID));
                        if (clientConnection != null)
                        {
                            CommandBase command = ChannelSession.AllEnabledCommands.FirstOrDefault(c => c.ID.Equals(commandID));
                            if (command != null)
                            {
                                await command.Perform();
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                await this.Connect();
                await this.Authenticate(connection.ID, ChannelSession.SecretManager.GetSecret("RemoteHostSecret"), connection.AccessToken);
                await Task.Delay(3000);

                return this.IsConnected;
            }
            return true;
        }

        public override async Task<RemoteConnectionAuthenticationTokenModel> NewHost(string name)
        {
            return await this.AsyncWrapper<RemoteConnectionAuthenticationTokenModel>(async () =>
            {
                return await this.GetAsync<RemoteConnectionAuthenticationTokenModel>("authentication/newhost?name=" + name);
            });
        }

        public override Task<RemoteConnectionShortCodeModel> NewClient(string name) { throw new System.NotImplementedException(); }

        public override Task<RemoteConnectionAuthenticationTokenModel> ValidateClient(RemoteConnectionShortCodeModel shortCode) { throw new System.NotImplementedException(); }

        public override async Task<RemoteConnectionModel> ApproveClient(RemoteConnectionModel connection, string clientShortCode, bool rememberClient = false)
        {
            return await this.AsyncWrapper<RemoteConnectionAuthenticationTokenModel>(async () =>
            {
                return await this.GetAsync<RemoteConnectionAuthenticationTokenModel>(string.Format("authentication/approveclient?hostID={0}&shortCode={1}&rememberClient={2}", connection.ID, clientShortCode, rememberClient));
            });
        }

        public override async Task<bool> ValidateConnection(RemoteConnectionAuthenticationTokenModel authToken)
        {
            return await this.AsyncWrapper<bool>(async () =>
            {
                HttpResponseMessage response = await this.PostAsync("authentication/validateconnection", this.CreateContentFromObject(authToken));
                return response.IsSuccessStatusCode;
            });
        }

        public override async Task<bool> RemoveClient(RemoteConnectionModel hostConnection, RemoteConnectionModel clientConnection)
        {
            return await this.AsyncWrapper<bool>(async () =>
            {
                return await this.GetAsync<bool>(string.Format("authentication/removeclient?hostID={0}&clientID={1}", hostConnection.ID, clientConnection.ID));
            });
        }
    }
}
