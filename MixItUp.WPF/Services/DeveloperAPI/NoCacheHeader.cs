using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services.DeveloperAPI
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

            HttpResponseMessage response = null;
            if (!request.Method.Method.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            else
            {
                response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }

            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Access-Control-Allow-Origin");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS, PUT, PATCH, DELETE");
            return response;
        }
    }
}
