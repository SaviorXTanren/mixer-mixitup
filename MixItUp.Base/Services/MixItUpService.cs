using MixItUp.Base.Model;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.API;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Model.Webhooks;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using MixItUp.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Services
{
    public interface IMixItUpService : IDisposable
    {
        bool IsWebhookHubConnected { get; }
        bool IsWebhookHubAllowed { get; }
        void BackgroundConnect();
        Task<Result> Connect();
        Task Disconnect();

        Task Authenticate(CommunityCommandLoginModel login);

        Task<GetWebhooksResponseModel> GetWebhooks();
        Task<Webhook> CreateWebhook();
        Task DeleteWebhook(Guid id);
    }

    public interface IWebhookService
    {
        bool IsWebhookHubConnected { get; }
        bool IsWebhookHubAllowed { get; }
        void BackgroundConnect();
        Task<Result> Connect();
        Task Disconnect();

        Task Authenticate(CommunityCommandLoginModel login);

        Task<GetWebhooksResponseModel> GetWebhooks();
        Task<Webhook> CreateWebhook();
        Task DeleteWebhook(Guid id);
    }

    public interface ICommunityCommandsService
    {
        Task<IEnumerable<CommunityCommandCategoryModel>> GetHomeCategories();
        Task<CommunityCommandsSearchResult> SearchCommands(string query, int skip, int top);
        Task<CommunityCommandDetailsModel> GetCommandDetails(Guid id);
        Task<CommunityCommandDetailsModel> AddOrUpdateCommand(CommunityCommandUploadModel command);
        Task DeleteCommand(Guid id);
        Task ReportCommand(CommunityCommandReportModel report);
        Task<CommunityCommandsSearchResult> GetCommandsByUser(Guid userID, int skip, int top);
        Task<CommunityCommandsSearchResult> GetMyCommands(int skip, int top);
        Task<CommunityCommandReviewModel> AddReview(CommunityCommandReviewModel review);
        Task DownloadCommand(Guid id);
    }

    public class CommunityCommandsSearchResult
    {
        public const string PageNumberHeader = "Page-Number";
        public const string PageSizeHeader = "Page-Size";
        public const string TotalElementsHeader = "Total-Elements";
        public const string TotalPagesHeader = "Total-Pages";

        public static async Task<CommunityCommandsSearchResult> Create(HttpResponseMessage response)
        {
            CommunityCommandsSearchResult result = new CommunityCommandsSearchResult();
            if (response.IsSuccessStatusCode)
            {
                result.Results.AddRange(await response.ProcessResponse<IEnumerable<CommunityCommandModel>>());

                if (int.TryParse(response.GetHeaderValue(PageNumberHeader), out int pageNumber))
                {
                    result.PageNumber = pageNumber;
                }
                if (int.TryParse(response.GetHeaderValue(PageSizeHeader), out int pageSize))
                {
                    result.PageSize = pageSize;
                }
                if (int.TryParse(response.GetHeaderValue(TotalElementsHeader), out int totalElements))
                {
                    result.TotalElements = totalElements;
                }
                if (int.TryParse(response.GetHeaderValue(TotalPagesHeader), out int totalPages))
                {
                    result.TotalPages = totalPages;
                }
            }
            return result;
        }

        public List<CommunityCommandModel> Results { get; set; } = new List<CommunityCommandModel>();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalElements { get; set; }
        public int TotalPages { get; set; }

        public CommunityCommandsSearchResult() { }

        public bool HasPreviousResults { get { return this.PageNumber > 1; } }

        public bool HasNextResults { get { return this.PageNumber < this.TotalPages; } }
    }

    public class MixItUpService : OAuthRestServiceBase, ICommunityCommandsService, IMixItUpService, IWebhookService, IDisposable
    {
        public const string MixItUpAPIEndpoint = "https://api.mixitupapp.com/api/";
        public const string MixItUpSignalRHubEndpoint = "https://api.mixitupapp.com/webhookhub";

        public const string DevMixItUpAPIEndpoint = "https://localhost:44309/api/";                // Dev Endpoint
        public const string DevMixItUpSignalRHubEndpoint = "https://localhost:44309/webhookhub";   // Dev Endpoint

        private string accessToken = null;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // IMixItUpService
        public async Task<MixItUpUpdateModel> GetLatestUpdate()
        {
            try
            {
                MixItUpUpdateModel update = await ServiceManager.Get<MixItUpService>().GetLatestPublicUpdate();
                if (update != null)
                {
                    if (ChannelSession.AppSettings.PreviewProgram)
                    {
                        MixItUpUpdateModel previewUpdate = await ServiceManager.Get<MixItUpService>().GetLatestPreviewUpdate();
                        if (previewUpdate != null && previewUpdate.SystemVersion >= update.SystemVersion)
                        {
                            update = previewUpdate;
                        }
                    }

                    // Remove this when we wish to re-enable Test Builds
                    ChannelSession.AppSettings.TestBuild = false;

                    if (ChannelSession.AppSettings.TestBuild)
                    {
                        MixItUpUpdateModel testUpdate = await ServiceManager.Get<MixItUpService>().GetLatestTestUpdate();
                        if (testUpdate != null && testUpdate.SystemVersion >= update.SystemVersion)
                        {
                            update = testUpdate;
                        }
                    }
                }
                return update;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<MixItUpUpdateModel> GetLatestPublicUpdate()
        {
            MixItUpUpdateModel update = await this.GetLatestPublicUpdateV2();
            if (update == null)
            {
                update = await this.GetAsync<MixItUpUpdateModel>("updates");
            }
            return update;
        }
        public async Task<MixItUpUpdateModel> GetLatestPreviewUpdate()
        {
            MixItUpUpdateModel update = await this.GetLatestPreviewUpdateV2();
            if (update == null)
            {
                update = await this.GetAsync<MixItUpUpdateModel>("updates/preview");
            }
            return update;
        }
        public async Task<MixItUpUpdateModel> GetLatestTestUpdate()
        {
            MixItUpUpdateModel update = await this.GetLatestTestUpdateV2();
            if (update == null)
            {
                update = await this.GetAsync<MixItUpUpdateModel>("updates/test");
            }
            return update;
        }

        public async Task<MixItUpUpdateModel> GetLatestPublicUpdateV2() { return await this.GetUpdateV2("public"); }
        public async Task<MixItUpUpdateModel> GetLatestPreviewUpdateV2() { return await this.GetUpdateV2("preview"); }
        public async Task<MixItUpUpdateModel> GetLatestTestUpdateV2() { return await this.GetUpdateV2("test"); }

        private async Task<MixItUpUpdateModel> GetUpdateV2(string type)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    MixItUpUpdateV2Model update = await client.GetAsync<MixItUpUpdateV2Model>($"https://raw.githubusercontent.com/mixitupapp/mixitupdesktop-data/main/Updates/{type}.json");
                    if (update != null)
                    {
                        return new MixItUpUpdateModel(update);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task SendIssueReport(IssueReportModel report)
        {
            string content = JSONSerializerHelper.SerializeToString(report);
            var response = await this.PostAsync("issuereport", new StringContent(content, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                string resultContent = await response.Content.ReadAsStringAsync();
                Logger.Log(resultContent);
            }
        }

        // ICommunityCommandsService
        public async Task<IEnumerable<CommunityCommandCategoryModel>> GetHomeCategories()
        {
            await EnsureLogin();
            return await GetAsync<IEnumerable<CommunityCommandCategoryModel>>("community/commands/categories");
        }

        public async Task<CommunityCommandsSearchResult> SearchCommands(string query, int skip, int top)
        {
            await EnsureLogin();
            return await CommunityCommandsSearchResult.Create(await this.GetAsync($"community/commands/command/search?query={HttpUtility.UrlEncode(query)}&skip={skip}&top={top}"));
        }

        public async Task<CommunityCommandDetailsModel> GetCommandDetails(Guid id)
        {
            try
            {
                await EnsureLogin();
                return await GetAsync<CommunityCommandDetailsModel>($"community/commands/command/{id}");
            }
            catch (HttpRestRequestException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<CommunityCommandDetailsModel> AddOrUpdateCommand(CommunityCommandUploadModel command)
        {
            await EnsureLogin();
            return await PostAsync<CommunityCommandDetailsModel>("community/commands/command", AdvancedHttpClient.CreateContentFromObject(command));
        }

        public async Task DeleteCommand(Guid id)
        {
            await EnsureLogin();
            await DeleteAsync<CommunityCommandDetailsModel>($"community/commands/command/{id}/delete");
        }

        public async Task ReportCommand(CommunityCommandReportModel report)
        {
            await EnsureLogin();
            await PostAsync($"community/commands/command/{report.CommandID}/report", AdvancedHttpClient.CreateContentFromObject(report));
        }

        public async Task<CommunityCommandsSearchResult> GetCommandsByUser(Guid userID, int skip, int top)
        {
            await EnsureLogin();
            return await CommunityCommandsSearchResult.Create(await GetAsync($"community/commands/command/user/{userID}?skip={skip}&top={top}"));
        }

        public async Task<CommunityCommandsSearchResult> GetMyCommands(int skip, int top)
        {
            await EnsureLogin();
            return await CommunityCommandsSearchResult.Create(await GetAsync($"community/commands/command/mine?skip={skip}&top={top}"));
        }

        public async Task<CommunityCommandReviewModel> AddReview(CommunityCommandReviewModel review)
        {
            await EnsureLogin();
            return await PostAsync<CommunityCommandReviewModel>($"community/commands/command/{review.CommandID}/review", AdvancedHttpClient.CreateContentFromObject(review));
        }

        public async Task DownloadCommand(Guid id)
        {
            try
            {
                await EnsureLogin();
                await GetAsync<IEnumerable<CommunityCommandDetailsModel>>($"community/commands/command/{id}/download");
            }
            catch { }
        }

        protected override Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true)
        {
            return Task.FromResult(new OAuthTokenModel { accessToken = this.accessToken });
        }

        protected override string GetBaseAddress()
        {
            //if (ChannelSession.IsDebug())
            //{
            //    return MixItUpService.DevMixItUpAPIEndpoint;
            //}
            return MixItUpService.MixItUpAPIEndpoint;
        }

        protected string GetSingalRAddress()
        {
            //if (ChannelSession.IsDebug())
            //{
            //    return MixItUpService.DevMixItUpSignalRHubEndpoint;
            //}
            return MixItUpService.MixItUpSignalRHubEndpoint;
        }

        private async Task EnsureLogin()
        {
            if (accessToken == null)
            {
                var token = this.GetLoginToken();
                var loginResponse = await PostAsync<CommunityCommandLoginResponseModel>("user/login", AdvancedHttpClient.CreateContentFromObject(token));
                this.accessToken = loginResponse.AccessToken;
            }
        }

        // IWebhookService
        public const string AuthenticateMethodName = "AuthenticateMany";
        private SignalRConnection signalRConnection = null;
        public bool IsWebhookHubConnected { get { return this.signalRConnection?.IsConnected() ?? false; } }
        public bool IsWebhookHubAllowed { get; private set; } = false;

        public void BackgroundConnect()
        {
            AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                Result result = await this.Connect();
                if (!result.Success)
                {
                    SignalRConnection_Disconnected(this, new Exception());
                }
            }, new CancellationToken());
        }

        public async Task<Result> Connect()
        {
            if (!this.IsWebhookHubConnected)
            {
                if (this.signalRConnection == null)
                {
                    this.signalRConnection = new SignalRConnection(this.GetSingalRAddress());

                    this.signalRConnection.Listen("TriggerWebhook", (Guid id, string payload) =>
                    {
                        Logger.Log($"Webhook Event - Generic Webhook - {id} - {payload}");
                        var _ = this.TriggerGenericWebhook(id, payload);
                    });

                    this.signalRConnection.Listen("AuthenticationCompleteEvent", (bool approved) =>
                    {
                        Logger.Log($"Webhook Authentication - {approved}");

                        this.IsWebhookHubAllowed = approved;
                        if (!this.IsWebhookHubAllowed)
                        {
                            Logger.Log(LogLevel.Error, $"Webhook Authentication Failed");

                            // Force disconnect is it doesn't retry
                            var _ = this.Disconnect();
                        }
                    });
                }

                this.signalRConnection.Connected -= SignalRConnection_Connected;
                this.signalRConnection.Disconnected -= SignalRConnection_Disconnected;

                this.signalRConnection.Connected += SignalRConnection_Connected;
                this.signalRConnection.Disconnected += SignalRConnection_Disconnected;

                if (await this.signalRConnection.Connect())
                {
                    return new Result(this.IsWebhookHubConnected);
                }
                return new Result(MixItUp.Base.Resources.WebhooksServiceFailedConnection);
            }
            return new Result(MixItUp.Base.Resources.WebhookServiceAlreadyConnected);
        }

        public async Task Disconnect()
        {
            if (this.signalRConnection != null)
            {
                this.signalRConnection.Connected -= SignalRConnection_Connected;
                this.signalRConnection.Disconnected -= SignalRConnection_Disconnected;

                await this.signalRConnection.Disconnect();

                this.signalRConnection = null;
            }
        }

        private async void SignalRConnection_Connected(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.MixItUpServices);

            await this.Authenticate(this.GetLoginToken());
        }

        private async void SignalRConnection_Disconnected(object sender, Exception e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.MixItUpServices);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000 + RandomHelper.GenerateRandomNumber(5000));

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.MixItUpServices);
        }

        public async Task Authenticate(CommunityCommandLoginModel login)
        {
            Logger.Log($"Webhook - Sending Auth - {JSONSerializerHelper.SerializeToString(login)}");

            try
            {
                await this.AsyncWrapper(this.signalRConnection.Send(AuthenticateMethodName, login));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<GetWebhooksResponseModel> GetWebhooks()
        {
            await EnsureLogin();
            return await GetAsync<GetWebhooksResponseModel>($"webhook");
        }

        public async Task<Webhook> CreateWebhook()
        {
            await EnsureLogin();
            return await PostAsync<Webhook>($"webhook", AdvancedHttpClient.CreateContentFromObject(new {}));
        }

        public async Task DeleteWebhook(Guid id)
        {
            await EnsureLogin();
            await DeleteAsync($"webhook/{id}");
        }

        private async Task AsyncWrapper(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task TriggerGenericWebhook(Guid id, string payload)
        {
            try
            {
                var command = ServiceManager.Get<CommandService>().WebhookCommands.FirstOrDefault(c => c.ID == id);
                if (command != null && command.IsEnabled)
                {
                    if (string.IsNullOrEmpty(payload))
                    {
                        payload = "{}";
                    }

                    Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
                    eventCommandSpecialIdentifiers["webhookpayload"] = payload;

                    // Do JSON => Special Identifier logic
                    CommandParametersModel parameters = new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.All, eventCommandSpecialIdentifiers);
                    Dictionary<string, string> jsonParameters = command.JSONParameters.ToDictionary(param => param.JSONParameterName, param => param.SpecialIdentifierName);
                    await WebRequestActionModel.ProcessJSONToSpecialIdentifiers(payload, jsonParameters, parameters);

                    await ServiceManager.Get<CommandService>().Queue(command, parameters);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private CommunityCommandLoginModel GetLoginToken()
        {
            var login = new CommunityCommandLoginModel();

            if (ServiceManager.Get<TwitchSession>().IsConnected)
            {
                login.TwitchAccessToken = ServiceManager.Get<TwitchSession>()?.StreamerService?.GetOAuthTokenCopy()?.accessToken;
                login.BypassTwitchWebhooks = true;
            }
            if (ServiceManager.Get<YouTubeSession>().IsConnected)
            {
                OAuthTokenModel token = ServiceManager.Get<YouTubeSession>()?.StreamerService?.GetOAuthTokenCopy();

                login.YouTubeOAuthToken = new StreamingClient.Base.Model.OAuth.OAuthTokenModel()
                {
                    clientID = ServiceManager.Get<YouTubeSession>().StreamerOAuthService.ClientID,
                    clientSecret = ServiceManager.Get<YouTubeSession>().StreamerOAuthService.ClientSecret,

                    accessToken = token.accessToken,
                    refreshToken = token.refreshToken,

                    expiresIn = token.expiresIn,
                };
            }
            if (ServiceManager.Get<TrovoSession>().IsConnected)
            {
                login.TrovoAccessToken = ServiceManager.Get<TrovoSession>()?.StreamerService?.GetOAuthTokenCopy()?.accessToken;
            }

            return login;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.cancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
