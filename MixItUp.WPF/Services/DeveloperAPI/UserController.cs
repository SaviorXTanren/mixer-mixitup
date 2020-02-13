using MixItUp.API.Models;
using MixItUp.Base;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI
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
                UserDataModel user = null;
                if (uint.TryParse(usernameOrID, out uint userID))
                {
                    user = ChannelSession.Settings.GetUserDataByMixerID(userID);
                }

                if (user == null)
                {
                    user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(usernameOrID, StringComparison.InvariantCultureIgnoreCase));
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
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {userID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return UserFromUserDataViewModel(user);
        }

        [Route("{username}")]
        [HttpGet]
        public User Get(string username)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {username}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Username not found"
                };
                throw new HttpResponseException(resp);
            }

            return UserFromUserDataViewModel(user);
        }

        [Route("{userID:int:min(0)}")]
        [HttpPut, HttpPatch]
        public User Update(uint userID, [FromBody] User updatedUserData)
        {
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {userID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return UpdateUser(user, updatedUserData);
        }

        [Route("{username}")]
        [HttpPut, HttpPatch]
        public User Update(string username, [FromBody] User updatedUserData)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {username}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Username not found"
                };
                throw new HttpResponseException(resp);
            }

            return UpdateUser(user, updatedUserData);
        }

        private User UpdateUser(UserDataModel user, User updatedUserData)
        {
            if (updatedUserData == null || !updatedUserData.ID.Equals(user.MixerID))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to parse update data from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            if (updatedUserData.ViewingMinutes.HasValue)
            {
                user.ViewingMinutes = updatedUserData.ViewingMinutes.Value;
            }

            foreach (CurrencyAmount currencyData in updatedUserData.CurrencyAmounts)
            {
                if (ChannelSession.Settings.Currencies.ContainsKey(currencyData.ID))
                {
                    ChannelSession.Settings.Currencies[currencyData.ID].SetAmount(user, currencyData.Amount);
                }
            }

            return UserFromUserDataViewModel(user);
        }

        [Route("{userID:int:min(0)}/currency/{currencyID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public User AdjustUserCurrency(uint userID, Guid currencyID, [FromBody] AdjustCurrency currencyUpdate)
        {
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {userID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return AdjustCurrency(user, currencyID, currencyUpdate);
        }

        [Route("{username}/currency/{currencyID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public User AdjustUserCurrency(string username, Guid currencyID, [FromBody] AdjustCurrency currencyUpdate)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {username}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Username not found"
                };
                throw new HttpResponseException(resp);
            }

            return AdjustCurrency(user, currencyID, currencyUpdate);
        }

        [Route("{userID:int:min(0)}/inventory/{inventoryID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public User AdjustUserInventory(uint userID, Guid inventoryID, [FromBody]AdjustInventory inventoryUpdate)
        {
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {userID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "User ID not found"
                };
                throw new HttpResponseException(resp);
            }
            return AdjustInventory(user, inventoryID, inventoryUpdate);
        }

        [Route("{username}/inventory/{inventoryID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public User AdjustUserInventory(string username, Guid inventoryID, [FromBody]AdjustInventory inventoryUpdate)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find user: {username}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Username not found"
                };
                throw new HttpResponseException(resp);
            }
            return AdjustInventory(user, inventoryID, inventoryUpdate);
        }

        [Route("top")]
        [HttpGet]
        public IEnumerable<User> Get(int count = 10)
        {
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

            Dictionary<Guid, UserDataModel> allUsersDictionary = ChannelSession.Settings.UserData.ToDictionary();
            UserDataModel streamer = ChannelSession.Settings.GetUserDataByMixerID(ChannelSession.MixerChannel.user.id);
            if (streamer != null)
            {
                allUsersDictionary.Remove(streamer.ID);
            }

            IEnumerable<UserDataModel> allUsers = allUsersDictionary.Select(kvp => kvp.Value);
            allUsers = allUsers.Where(u => !u.IsCurrencyRankExempt);

            List<User> userList = new List<User>();
            foreach (UserDataModel user in allUsers.OrderByDescending(u => u.ViewingMinutes).Take(count))
            {
                userList.Add(UserFromUserDataViewModel(user));
            }
            return userList;
        }

        public static User UserFromUserDataViewModel(UserDataModel userData)
        {
            User user = new User
            {
                ID = userData.MixerID,
                UserName = userData.Username,
                ViewingMinutes = userData.ViewingMinutes
            };

            foreach (UserCurrencyModel currency in ChannelSession.Settings.Currencies.Values)
            {
                user.CurrencyAmounts.Add(CurrencyController.CurrencyAmountFromUserCurrencyViewModel(currency, currency.GetAmount(userData)));
            }

            foreach (UserInventoryModel inventory in ChannelSession.Settings.Inventories.Values)
            {
                user.InventoryAmounts.Add(InventoryController.InventoryAmountFromUserInventoryViewModel(inventory, new UserInventoryDataViewModel(userData, inventory)));
            }

            return user;
        }

        private User AdjustCurrency(UserDataModel user, Guid currencyID, [FromBody] AdjustCurrency currencyUpdate)
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

            if (currencyUpdate == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to parse currency adjustment from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            UserCurrencyModel currency = ChannelSession.Settings.Currencies[currencyID];

            if (currencyUpdate.Amount < 0)
            {
                int quantityToRemove = currencyUpdate.Amount * -1;
                if (!currency.HasAmount(user, quantityToRemove))
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        Content = new ObjectContent<Error>(new Error { Message = "User does not have enough currency to remove" }, new JsonMediaTypeFormatter(), "application/json"),
                        ReasonPhrase = "Not Enough Currency"
                    };
                    throw new HttpResponseException(resp);
                }

                currency.SubtractAmount(user, quantityToRemove);
            }
            else if (currencyUpdate.Amount > 0)
            {
                currency.AddAmount(user, currencyUpdate.Amount);
            }

            return UserFromUserDataViewModel(user);
        }

        private User AdjustInventory(UserDataModel user, Guid inventoryID, [FromBody]AdjustInventory inventoryUpdate)
        {
            if (!ChannelSession.Settings.Inventories.ContainsKey(inventoryID))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find inventory: {inventoryID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Inventory ID not found"
                };
                throw new HttpResponseException(resp);
            }

            if (inventoryUpdate == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to parse inventory adjustment from POST body." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid POST Body"
                };
                throw new HttpResponseException(resp);
            }

            UserInventoryModel inventory = ChannelSession.Settings.Inventories[inventoryID];

            if (string.IsNullOrEmpty(inventoryUpdate.Name) || !inventory.Items.ContainsKey(inventoryUpdate.Name))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new ObjectContent<Error>(new Error { Message = "Unable to find requested inventory item." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Invalid Inventory Item"
                };
                throw new HttpResponseException(resp);
            }

            if (inventoryUpdate.Amount < 0)
            {
                int quantityToRemove = inventoryUpdate.Amount * -1;
                if (!inventory.HasAmount(user, inventoryUpdate.Name, quantityToRemove))
                {
                    // If the request is to remove inventory, but user doesn't have enough, fail
                    var resp = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        Content = new ObjectContent<Error>(new Error { Message = "User does not have enough inventory to remove" }, new JsonMediaTypeFormatter(), "application/json"),
                        ReasonPhrase = "Not Enough Inventory"
                    };
                    throw new HttpResponseException(resp);
                }

                inventory.SubtractAmount(user, inventoryUpdate.Name, quantityToRemove);
            }
            else if (inventoryUpdate.Amount > 0)
            {
                inventory.AddAmount(user, inventoryUpdate.Name, inventoryUpdate.Amount);
            }

            return UserFromUserDataViewModel(user);
        }
    }
}
