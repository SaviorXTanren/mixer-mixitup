using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    public class NoCacheHeader : DelegatingHandler
    {
        async protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("Cache-Control", "no-cache");
            return response;
        }
    }
}
