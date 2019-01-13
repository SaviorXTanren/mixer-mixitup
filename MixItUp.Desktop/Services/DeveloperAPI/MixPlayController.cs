using MixItUp.Base;
using MixItUp.API.Models;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/mixplay")]
    public class MixPlayController : ApiController
    {
        [Route("broadcast")]
        [HttpPost]
        public async Task Broadcast([FromBody] MixPlayBroadcast broadcastEvent)
        {
            if (broadcastEvent == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (!ChannelSession.Interactive.IsConnected())
            {
                throw new HttpResponseException(HttpStatusCode.ServiceUnavailable);
            }
            
            await ChannelSession.Interactive.BroadcastEvent(broadcastEvent.Scopes, broadcastEvent.Data);
        }
    }
}
