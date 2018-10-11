using MixItUp.Base;
using MixItUp.Base.Model.DeveloperAPIs;
using MixItUp.Base.ViewModel.User;
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
        public IEnumerable<UserDeveloperAPIModel> BulkGet([FromBody] IEnumerable<string> usernamesOrIDs)
        {
            List<UserDeveloperAPIModel> users = new List<UserDeveloperAPIModel>();
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
                    users.Add(new UserDeveloperAPIModel(user));
                }
            }

            return users;
        }

        [Route("{userID:int:min(0)}")]
        public UserDeveloperAPIModel Get(uint userID)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData[userID];
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new UserDeveloperAPIModel(user);
        }

        [Route("{username}")]
        [HttpGet]
        public UserDeveloperAPIModel Get(string username)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new UserDeveloperAPIModel(user);
        }

        [Route("{userID:int:min(0)}")]
        [HttpPut, HttpPatch]
        public UserDeveloperAPIModel Update(uint userID, [FromBody] UserDeveloperAPIModel updatedUserData)
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
        public UserDeveloperAPIModel Update(string username, [FromBody] UserDeveloperAPIModel updatedUserData)
        {
            UserDataViewModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return UpdateUser(user, updatedUserData);
        }

        private UserDeveloperAPIModel UpdateUser(UserDataViewModel user, UserDeveloperAPIModel updatedUserData)
        {
            if (updatedUserData == null || !updatedUserData.ID.Equals(user.ID))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            user.ViewingMinutes = updatedUserData.ViewingMinutes;
            foreach (UserCurrencyDeveloperAPIModel currencyData in updatedUserData.CurrencyAmounts)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(currencyData.ID))
                {
                    user.SetCurrencyAmount(ChannelSession.Settings.Currencies[currencyData.ID], currencyData.Amount);
                }
            }

            return new UserDeveloperAPIModel(user);
        }

        [Route("{userID:int:min(0)}/currency/{currencyID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public UserDeveloperAPIModel AdjustCurrency(uint userID, Guid currencyID, [FromBody] UserCurrencyUpdateDeveloperAPIModel currencyUpdate)
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
        public UserDeveloperAPIModel AdjustCurrency(string username, Guid currencyID, [FromBody] UserCurrencyUpdateDeveloperAPIModel currencyUpdate)
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
        public IEnumerable<UserDeveloperAPIModel> Get(Guid currencyID, int count = 10)
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

            List<UserDeveloperAPIModel> userList = new List<UserDeveloperAPIModel>();
            foreach (UserDataViewModel user in allUsers.OrderByDescending(u => u.ViewingMinutes).Take(count))
            {
                userList.Add(new UserDeveloperAPIModel(user));
            }
            return userList;
        }

        private UserDeveloperAPIModel AdjustCurrency(UserDataViewModel user, Guid currencyID, [FromBody] UserCurrencyUpdateDeveloperAPIModel currencyUpdate)
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

            return new UserDeveloperAPIModel(user);
        }
    }
}
