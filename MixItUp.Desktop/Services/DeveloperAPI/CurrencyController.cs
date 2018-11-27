using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace MixItUp.Desktop.Services.DeveloperAPI
{
    [RoutePrefix("api/currency")]
    public class CurrencyController : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<UserCurrencyViewModel> Get()
        {
            return ChannelSession.Settings.Currencies.Values;
        }

        [Route("{currencyID:guid}")]
        [HttpGet]
        public UserCurrencyViewModel Get(Guid currencyID)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return ChannelSession.Settings.Currencies[currencyID];
        }

        [Route("{currencyID:guid}/top")]
        [HttpGet]
        public IEnumerable<User> Get(Guid currencyID, int count = 10)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (count < 1)
            {
                // TODO: Consider checking or a max # too? (100?)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[currencyID];

            Dictionary<uint, UserDataViewModel> allUsersDictionary = ChannelSession.Settings.UserData.ToDictionary();
            allUsersDictionary.Remove(ChannelSession.Channel.user.id);

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
        public IEnumerable<User> BulkGive(Guid currencyID, [FromBody] IEnumerable<UserCurrencyGiveDeveloperAPIModel> giveDatas)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (giveDatas == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
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
                    user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.Equals(giveData.UsernameOrID, StringComparison.InvariantCultureIgnoreCase));
                }

                if (user != null && giveData.Amount > 0)
                {
                    user.AddCurrencyAmount(currency, giveData.Amount);
                    users.Add(UserController.UserFromUserDataViewModel(user));
                }
            }

            return users;
        }

        public static Currency CurrencyFromUserCurrencyViewModel(UserCurrencyViewModel currency, int amount)
        {
            return new Currency
            {
                ID = currency.ID,
                Name = currency.Name,
                Amount = amount
            };
        }
    }
}
