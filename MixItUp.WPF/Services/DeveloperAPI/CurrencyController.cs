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
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.Currency;

namespace MixItUp.WPF.Services.DeveloperAPI
{
    [RoutePrefix("api/currency")]
    public class CurrencyController : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<Currency> Get()
        {
            List<Currency> list = new List<Currency>();
            foreach (var currency in ChannelSession.Settings.Currency.Values)
            {
                list.Add(CurrencyFromUserCurrencyViewModel(currency));
            }

            return list;
        }

        [Route("{currencyID:guid}")]
        [HttpGet]
        public Currency Get(Guid currencyID)
        {
            if (!ChannelSession.Settings.Currency.ContainsKey(currencyID))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find currency: {currencyID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Currency ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return CurrencyFromUserCurrencyViewModel(ChannelSession.Settings.Currency[currencyID]);
        }

        [Route("{currencyID:guid}/top")]
        [HttpGet]
        public IEnumerable<User> Get(Guid currencyID, int count = 10)
        {
            if (!ChannelSession.Settings.Currency.ContainsKey(currencyID))
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

            CurrencyModel currency = ChannelSession.Settings.Currency[currencyID];

            Dictionary<Guid, UserDataModel> allUsersDictionary = ChannelSession.Settings.UserData.ToDictionary();
            UserDataModel streamer = ChannelSession.Settings.GetUserDataByMixerID(ChannelSession.MixerChannel.user.id);
            if (streamer != null)
            {
                allUsersDictionary.Remove(streamer.ID);
            }

            IEnumerable<UserDataModel> allUsers = allUsersDictionary.Select(kvp => kvp.Value);
            allUsers = allUsers.Where(u => !u.IsCurrencyRankExempt);

            List<User> currencyUserList = new List<User>();
            foreach (UserDataModel currencyUser in allUsers.OrderByDescending(u => currency.GetAmount(u)).Take(count))
            {
                currencyUserList.Add(UserController.UserFromUserDataViewModel(currencyUser));
            }
            return currencyUserList;
        }

        [Route("{currencyID:guid}/give")]
        [HttpPost]
        public IEnumerable<User> BulkGive(Guid currencyID, [FromBody] IEnumerable<GiveUserCurrency> giveDatas)
        {
            if (!ChannelSession.Settings.Currency.ContainsKey(currencyID))
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

            CurrencyModel currency = ChannelSession.Settings.Currency[currencyID];

            List<User> users = new List<User>();
            foreach (var giveData in giveDatas)
            {
                UserDataModel user = null;
                if (uint.TryParse(giveData.UsernameOrID, out uint userID))
                {
                    user = ChannelSession.Settings.GetUserDataByMixerID(userID);
                }

                if (user == null)
                {
                    user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u != null && u.Username != null && u.Username.Equals(giveData.UsernameOrID, StringComparison.InvariantCultureIgnoreCase));
                }

                if (user == null && Guid.TryParse(giveData.UsernameOrID, out Guid userId))
                {
                    user = ChannelSession.Settings.GetUserData(userId);
                }

                if (user != null && giveData.Amount > 0)
                {
                    currency.AddAmount(user, giveData.Amount);
                    users.Add(UserController.UserFromUserDataViewModel(user));
                }
            }

            return users;
        }

        public static CurrencyAmount CurrencyAmountFromUserCurrencyViewModel(CurrencyModel currency, int amount)
        {
            return new CurrencyAmount
            {
                ID = currency.ID,
                Name = currency.Name,
                Amount = amount
            };
        }

        public static Currency CurrencyFromUserCurrencyViewModel(CurrencyModel currency)
        {
            return new Currency
            {
                ID = currency.ID,
                Name = currency.Name
            };
        }
    }
}
