using MixItUp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using MixItUp.Base.Model.User;
using Microsoft.AspNetCore.Mvc;

namespace MixItUp.Base.Services.DeveloperAPI.Controllers
{
    [Route("api/currency")]
    public class CurrencyController : BaseController
    {
        [Route("")]
        [HttpGet]
        public IActionResult Get()
        {
            List<Currency> list = new List<Currency>();
            foreach (var currency in ChannelSession.Settings.Currencies.Values)
            {
                list.Add(CurrencyFromUserCurrencyViewModel(currency));
            }

            return Ok(list);
        }

        [Route("{currencyID:guid}")]
        [HttpGet]
        public IActionResult Get(Guid currencyID)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find currency: {currencyID.ToString()}." });
            }

            return Ok(CurrencyFromUserCurrencyViewModel(ChannelSession.Settings.Currencies[currencyID]));
        }

        [Route("{currencyID:guid}/top")]
        [HttpGet]
        public IActionResult Get(Guid currencyID, int count = 10)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find currency: {currencyID.ToString()}." });
            }

            if (count < 1)
            {
                // TODO: Consider checking or a max # too? (100?)
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = $"Count must be greater than 0." });
            }

            UserCurrencyModel currency = ChannelSession.Settings.Currencies[currencyID];

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
            return Ok(currencyUserList);
        }

        [Route("{currencyID:guid}/give")]
        [HttpPost]
        public IActionResult BulkGive(Guid currencyID, [FromBody] IEnumerable<GiveUserCurrency> giveDatas)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find currency: {currencyID.ToString()}." });
            }

            if (giveDatas == null)
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = $"Unable to parse array of give data from POST body." });
            }

            UserCurrencyModel currency = ChannelSession.Settings.Currencies[currencyID];

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
                    user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(giveData.UsernameOrID, StringComparison.InvariantCultureIgnoreCase));
                }

                if (user != null && giveData.Amount > 0)
                {
                    currency.AddAmount(user, giveData.Amount);
                    users.Add(UserController.UserFromUserDataViewModel(user));
                }
            }

            return Ok(users);
        }

        public static CurrencyAmount CurrencyAmountFromUserCurrencyViewModel(UserCurrencyModel currency, int amount)
        {
            return new CurrencyAmount
            {
                ID = currency.ID,
                Name = currency.Name,
                Amount = amount
            };
        }

        public static Currency CurrencyFromUserCurrencyViewModel(UserCurrencyModel currency)
        {
            return new Currency
            {
                ID = currency.ID,
                Name = currency.Name
            };
        }
    }
}
