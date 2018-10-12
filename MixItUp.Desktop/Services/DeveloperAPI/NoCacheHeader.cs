using Mixer.Base.Util;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    public class NoCacheHeader : DelegatingHandler
    {
        private const string MethodOverrideHeader = "X-HTTP-Method-Override";

        async protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains(MethodOverrideHeader))
            {
                request.Method = new HttpMethod(request.Headers.GetValues(MethodOverrideHeader).FirstOrDefault());
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("Cache-Control", "no-cache");
            return response;
        }
    }
}
