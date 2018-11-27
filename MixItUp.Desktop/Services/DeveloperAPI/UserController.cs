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
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        [Route]
        [HttpPost]
        public IEnumerable<User> BulkGet([FromBody] IEnumerable<string> usernamesOrIDs)
        {
            List<User> users = new List<User>();
            foreach (var usernameOrID in usernamesOrIDs)
            {
                UserDataViewModel user = null;
                if (uint.TryParse(usernameOrID, out uint userID))
                {
                    user = ChannelSession.Settings.UserData[userID];
                }

                if (user == null)
                {
                    user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.Equals(usernameOrID, StringComparison.InvariantCultureIgnoreCase));
                }

                if (user != null)
                {
                    users.Add(UserFromUserDataViewModel(user));
                }
            }

            return users;
        }

        [Route("{userID:int:min(0)}")]
        public User Get(uint userID)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData[userID];
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return UserFromUserDataViewModel(user);
        }

        [Route("{username}")]
        [HttpGet]
        public User Get(string username)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return UserFromUserDataViewModel(user);
        }

        [Route("{userID:int:min(0)}")]
        [HttpPut, HttpPatch]
        public User Update(uint userID, [FromBody] User updatedUserData)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData[userID];
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return UpdateUser(user, updatedUserData);
        }

        [Route("{username}")]
        [HttpPut, HttpPatch]
        public User Update(string username, [FromBody] User updatedUserData)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return UpdateUser(user, updatedUserData);
        }

        private User UpdateUser(UserDataViewModel user, User updatedUserData)
        {
            if (updatedUserData == null || !updatedUserData.ID.Equals(user.ID))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (updatedUserData.ViewingMinutes.HasValue)
            {
                user.ViewingMinutes = updatedUserData.ViewingMinutes.Value;
            }

            foreach (Currency currencyData in updatedUserData.CurrencyAmounts)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(currencyData.ID))
                {
                    user.SetCurrencyAmount(ChannelSession.Settings.Currencies[currencyData.ID], currencyData.Amount);
                }
            }

            return UserFromUserDataViewModel(user);
        }

        [Route("{userID:int:min(0)}/currency/{currencyID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public User AdjustCurrency(uint userID, Guid currencyID, [FromBody] UserCurrencyUpdateDeveloperAPIModel currencyUpdate)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData[userID];
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return AdjustCurrency(user, currencyID, currencyUpdate);
        }

        [Route("{username}/currency/{currencyID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public User AdjustCurrency(string username, Guid currencyID, [FromBody] UserCurrencyUpdateDeveloperAPIModel currencyUpdate)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return AdjustCurrency(user, currencyID, currencyUpdate);
        }

        [Route("top")]
        [HttpGet]
        public IEnumerable<User> Get(Guid currencyID, int count = 10)
        {
            if (count < 1)
            {
                // TODO: Consider checking or a max # too? (100?)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Dictionary<uint, UserDataViewModel> allUsersDictionary = ChannelSession.Settings.UserData.ToDictionary();
            allUsersDictionary.Remove(ChannelSession.Channel.user.id);

            IEnumerable<UserDataViewModel> allUsers = allUsersDictionary.Select(kvp => kvp.Value);
            allUsers = allUsers.Where(u => !u.IsCurrencyRankExempt);

            List<User> userList = new List<User>();
            foreach (UserDataViewModel user in allUsers.OrderByDescending(u => u.ViewingMinutes).Take(count))
            {
                userList.Add(UserFromUserDataViewModel(user));
            }
            return userList;
        }

        public static User UserFromUserDataViewModel(UserDataViewModel userData)
        {

            User user = new User
            {
                ID = userData.ID,
                UserName = userData.UserName,
                ViewingMinutes = userData.ViewingMinutes
            };

            foreach (UserCurrencyViewModel currencyData in ChannelSession.Settings.Currencies.Values)
            {
                user.CurrencyAmounts.Add(CurrencyController.CurrencyFromUserCurrencyViewModel(currencyData, userData.GetCurrencyAmount(currencyData)));
            }

            return user;
        }

        private User AdjustCurrency(UserDataViewModel user, Guid currencyID, [FromBody] UserCurrencyUpdateDeveloperAPIModel currencyUpdate)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (currencyUpdate == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[currencyID];

            if (currencyUpdate.Amount < 0)
            {
                int quantityToRemove = currencyUpdate.Amount * -1;
                if (!user.HasCurrencyAmount(currency, quantityToRemove))
                {
                    // If the request is to remove currency, but user doesn't have enough, fail
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
                }

                user.SubtractCurrencyAmount(currency, quantityToRemove);
            }
            else if (currencyUpdate.Amount > 0)
            {
                user.AddCurrencyAmount(currency, currencyUpdate.Amount);
            }

            return UserFromUserDataViewModel(user);
        }
    }
}
