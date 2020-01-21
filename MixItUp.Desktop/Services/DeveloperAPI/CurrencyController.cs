using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/currency")]
    public class CurrencyController : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<Currency> Get()
        {
            List<Currency> list = new List<Currency>();
            foreach (var currency in ChannelSession.Settings.Currencies.Values)
            {
                list.Add(CurrencyFromUserCurrencyViewModel(currency));
            }

            return list;
        }

        [Route("{currencyID:guid}")]
        [HttpGet]
        public Currency Get(Guid currencyID)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find currency: {currencyID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Currency ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return CurrencyFromUserCurrencyViewModel(ChannelSession.Settings.Currencies[currencyID]);
        }

        [Route("{currencyID:guid}/top")]
        [HttpGet]
        public IEnumerable<User> Get(Guid currencyID, int count = 10)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find currency: {currencyID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Currency ID not found"
                };
                throw new HttpResponseException(resp);
            }

            if (count < 1)
            {
                // TODO: Consider checking or a max # too? (100?)
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Count must be greater than 0." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Count too low"
                };
                throw new HttpResponseException(resp);
            }

            UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[currencyID];

            Dictionary<uint, UserDataViewModel> allUsersDictionary = ChannelSession.Settings.UserData.ToDictionary();
            allUsersDictionary.Remove(ChannelSession.MixerChannel.user.id);

            IEnumerable<UserDataViewModel> allUsers = allUsersDictionary.Select(kvp => kvp.Value);
            allUsers = allUsers.Where(u => !u.IsCurrencyRankExempt);

            List<User> currencyUserList = new List<User>();
            foreach (UserDataViewModel currencyUser in allUsers.OrderByDescending(u => u.GetCurrencyAmount(currency)).Take(count))
            {
                currencyUserList.Add(UserController.UserFromUserDataViewModel(currencyUser));
            }
            return currencyUserList;
        }

        [Route("{currencyID:guid}/give")]
        [HttpPost]
        public IEnumerable<User> BulkGive(Guid currencyID, [FromBody] IEnumerable<GiveUserCurrency> giveDatas)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find currency: {currencyID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Currency ID not found"
                };
                throw new HttpResponseException(resp);
            }

            if (giveDatas == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to parse array of give data from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[currencyID];

            List<User> users = new List<User>();
            foreach (var giveData in giveDatas)
            {
                UserDataViewModel user = null;
                if (uint.TryParse(giveData.UsernameOrID, out uint userID))
                {
                    user = ChannelSession.Settings.UserData[userID];
                }

                if (user == null)
                {
                    user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.MixerUsername.Equals(giveData.UsernameOrID, StringComparison.InvariantCultureIgnoreCase));
                }

                if (user != null && giveData.Amount > 0)
                {
                    user.AddCurrencyAmount(currency, giveData.Amount);
                    users.Add(UserController.UserFromUserDataViewModel(user));
                }
            }

            return users;
        }

        public static CurrencyAmount CurrencyAmountFromUserCurrencyViewModel(UserCurrencyViewModel currency, int amount)
        {
            return new CurrencyAmount
            {
                ID = currency.ID,
                Name = currency.Name,
                Amount = amount
            };
        }

        public static Currency CurrencyFromUserCurrencyViewModel(UserCurrencyViewModel currency)
        {
            return new Currency
            {
                ID = currency.ID,
                Name = currency.Name
            };
        }
    }
}
