using MixItUp.API.V1.Models;
using MixItUp.Base;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V1
{
    [RoutePrefix("api/currency")]
    public class CurrencyV1Controller : ApiController
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
        public async Task<IEnumerable<User>> Get(Guid currencyID, int count = 10)
        {
            await ServiceManager.Get<UserService>().LoadAllUserData();

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

            Dictionary<Guid, UserV2Model> allUsersDictionary = ChannelSession.Settings.Users.ToDictionary();

            IEnumerable<UserV2Model> allUsers = allUsersDictionary.Select(kvp => kvp.Value);
            allUsers = allUsers.Where(u => !u.IsSpecialtyExcluded);

            List<User> currencyUserList = new List<User>();
            foreach (UserV2Model currencyUser in allUsers.OrderByDescending(u => currency.GetAmount(u)).Take(count))
            {
                currencyUserList.Add(UserV1Controller.UserFromUserDataViewModel(new UserV2ViewModel(currencyUser)));
            }
            return currencyUserList;
        }

        [Route("{currencyID:guid}/give")]
        [HttpPost]
        public async Task<IEnumerable<User>> BulkGive(Guid currencyID, [FromBody] IEnumerable<GiveUserCurrency> giveDatas)
        {
            await ServiceManager.Get<UserService>().LoadAllUserData();

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
                UserV2ViewModel user = await UserV1Controller.GetUserData(ChannelSession.Settings.DefaultStreamingPlatform, giveData.UsernameOrID);
                if (user != null && giveData.Amount > 0)
                {
                    currency.AddAmount(user, giveData.Amount);
                    users.Add(UserV1Controller.UserFromUserDataViewModel(user));
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
