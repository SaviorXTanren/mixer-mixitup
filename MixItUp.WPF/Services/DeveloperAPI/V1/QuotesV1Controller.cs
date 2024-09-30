using MixItUp.API.V1.Models;
using MixItUp.Base;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using WebSocketSharp;

namespace MixItUp.WPF.Services.DeveloperAPI.V1
{
    [RoutePrefix("api/quotes")]
    public class QuotesV1Controller : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<Quote> Get()
        {
            return ChannelSession.Settings.Quotes.Select(q => QuoteFromUserQuoteViewModel(q));
        }

        [Route("{quoteID:int}")]
        [HttpGet]
        public Quote Get(int quoteID)
        {
            var quote = ChannelSession.Settings.Quotes.FirstOrDefault(q => q.ID == quoteID);
            if (quote == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find quote: {quoteID}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Quote ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return QuoteFromUserQuoteViewModel(quote);
        }

        [Route]
        [HttpPut]
        public async Task<Quote> Add([FromBody]AddQuote quote)
        {
            if (quote == null || quote.QuoteText.IsNullOrEmpty())
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to create quote, no QuoteText was supplied." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "QuoteText not specified"
                };
                throw new HttpResponseException(resp);
            }

            var quoteText = quote.QuoteText.Trim(new char[] { ' ', '\'', '\"' });
            UserQuoteModel newQuote = new UserQuoteModel(UserQuoteViewModel.GetNextQuoteNumber(), quoteText, DateTimeOffset.Now, await GamePreMadeChatCommandModel.GetCurrentGameName(ChannelSession.Settings.DefaultStreamingPlatform));
            ChannelSession.Settings.Quotes.Add(newQuote);
            await ChannelSession.SaveSettings();

            UserQuoteModel.QuoteAdded(newQuote);

            return QuoteFromUserQuoteViewModel(newQuote);
        }

        public static Quote QuoteFromUserQuoteViewModel(UserQuoteModel quoteData)
        {
            return new Quote
            {
                ID = quoteData.ID,
                DateTime = quoteData.DateTime,
                GameName = quoteData.GameName,
                QuoteText = quoteData.Quote,
            };
        }
    }
}
