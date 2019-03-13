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
    public class RemoteService : LocalRemoteServiceBase
    {
        public RemoteService(string apiAddress, string signalRAddress) : base(apiAddress, new SignalRConnection(signalRAddress)) { }

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
                            await this.SendProfiles(ChannelSession.Settings.RemoteProfiles.Where(p => !p.IsStreamer || clientConnection.IsStreamer));
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });

                this.ListenForRequestBoard(async (clientID, profileID, boardID) =>
                {
                    try
                    {
                        RemoteConnectionModel clientConnection = ChannelSession.Settings.RemoteClientConnections.FirstOrDefault(c => c.ID.Equals(clientID));
                        if (clientConnection != null)
                        {
                            if (ChannelSession.Settings.RemoteProfileBoards.ContainsKey(profileID) && ChannelSession.Settings.RemoteProfileBoards[profileID].Boards.ContainsKey(boardID))
                            {
                                await this.SendBoard(ChannelSession.Settings.RemoteProfileBoards[profileID].Boards[boardID]);
                            }
                            else
                            {
                                await this.SendBoard(null);
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
