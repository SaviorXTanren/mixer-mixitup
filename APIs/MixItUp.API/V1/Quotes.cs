using MixItUp.API.V1.Models;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/quotes              Quotes.GetAllQuotesAsync
    //  GET:    http://localhost:8911/api/quotes/{quoteId}    Quotes.GetQuoteAsync
    //  PUT:    http://localhost:8911/api/quotes              Quotes.AddQuoteAsync
    public static class Quotes
    {
        public static Task<Quote[]> GetAllQuotesAsync()
        {
            return RestClient.GetAsync<Quote[]>($"quotes");
        }

        public static Task<Quote> GetQuoteAsync(int quoteId)
        {
            return RestClient.GetAsync<Quote>($"quotes/{quoteId}");
        }

        public static Task<Quote> AddQuoteAsync(Quote quote)
        {
            return RestClient.PutAsync<Quote>($"quotes/{quote.ID}", quote);
        }
    }
}
