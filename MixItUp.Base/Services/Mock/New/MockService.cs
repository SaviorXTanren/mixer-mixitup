using MixItUp.Base.Model;
using MixItUp.Base.Model.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mock.New
{
    public class MockService : StreamingPlatformServiceBaseNew
    {
        public override string ClientID { get { return string.Empty; } }

        public override string ClientSecret { get { return string.Empty; } }

        public override string Name { get { return Resources.Mock; } }

        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.Mock; } }

        public override bool IsConnected { get; protected set; }

        public MockService(bool isBotService = false) : base("https://mixitupapp.com", new List<string>(), isBotService) { }

        protected override Task<string> GetAuthorizationCodeURL(IEnumerable<string> scopes, string state, bool forceApprovalPrompt = false)
        {
            return Task.FromResult("https://mixitupapp.com");
        }

        protected override Task RefreshOAuthToken()
        {
            return Task.CompletedTask;
        }

        protected override Task<OAuthTokenModel> RequestOAuthToken(string authorizationCode, IEnumerable<string> scopes, string state)
        {
            return Task.FromResult(new OAuthTokenModel());
        }
    }
}
