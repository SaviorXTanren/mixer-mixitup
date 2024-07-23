using MixItUp.API.V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/currency                      Currencies.GetAllCurrenciesAsync
    //  GET:    http://localhost:8911/api/currency/{currencyID}/top     Currencies.GetTopUsersByCurrencyAsync
    //  POST:   http://localhost:8911/api/currency/{currencyID}/give    Currencies.GiveUsersCurrencyAsync
    public static class Currencies
    {
        public static async Task<Currency[]> GetAllCurrenciesAsync()
        {
            return await RestClient.GetAsync<Currency[]>($"currency");
        }

        public static async Task<User[]> GetTopUsersByCurrencyAsync(Guid currencyID, uint? count)
        {
            string uri = $"currency/{currencyID}/top";
            if (count.HasValue)
            {
                uri += $"?count={count}";
            }

            return await RestClient.GetAsync<User[]>(uri);
        }

        public static async Task<User[]> GiveUsersCurrencyAsync(Guid currencyID, IEnumerable<GiveUserCurrency> giveList)
        {
            if (giveList.Count() == 0)
            {
                return new User[0];
            }

            return await RestClient.PostAsync<User[]>($"currency/{currencyID}/give", giveList);
        }
    }
}
