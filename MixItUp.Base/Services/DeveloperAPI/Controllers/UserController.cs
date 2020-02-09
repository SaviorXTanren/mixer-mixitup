using Microsoft.AspNetCore.Mvc;
using MixItUp.API.Models;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace MixItUp.Base.Services.DeveloperAPI.Controllers
{
    [Route("api/users")]
    public class UserController : BaseController
    {
        [Route("")]
        [HttpPost]
        public IActionResult BulkGet([FromBody] IEnumerable<string> usernamesOrIDs)
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

            return Ok(users);
        }

        [Route("{userID:int:min(0)}")]
        public IActionResult Get(uint userID)
        {
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {userID.ToString()}." });
            }

            return Ok(UserFromUserDataViewModel(user));
        }

        [Route("{username}")]
        [HttpGet]
        public IActionResult Get(string username)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {username.ToString()}." });
            }

            return Ok(UserFromUserDataViewModel(user));
        }

        [Route("{userID:int:min(0)}")]
        [HttpPut, HttpPatch]
        public IActionResult Update(uint userID, [FromBody] User updatedUserData)
        {
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {userID.ToString()}." });
            }

            return UpdateUser(user, updatedUserData);
        }

        [Route("{username}")]
        [HttpPut, HttpPatch]
        public IActionResult Update(string username, [FromBody] User updatedUserData)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {username.ToString()}." });
            }

            return UpdateUser(user, updatedUserData);
        }

        private IActionResult UpdateUser(UserDataModel user, User updatedUserData)
        {
            if (updatedUserData == null || !updatedUserData.ID.Equals(user.MixerID))
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = "Unable to parse update data from POST body." });
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

            return Ok(UserFromUserDataViewModel(user));
        }

        [Route("{userID:int:min(0)}/currency/{currencyID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public IActionResult AdjustUserCurrency(uint userID, Guid currencyID, [FromBody] AdjustCurrency currencyUpdate)
        {
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {userID.ToString()}." });
            }

            return AdjustCurrency(user, currencyID, currencyUpdate);
        }

        [Route("{username}/currency/{currencyID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public IActionResult AdjustUserCurrency(string username, Guid currencyID, [FromBody] AdjustCurrency currencyUpdate)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {username.ToString()}." });
            }

            return AdjustCurrency(user, currencyID, currencyUpdate);
        }

        [Route("{userID:int:min(0)}/inventory/{inventoryID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public IActionResult AdjustUserInventory(uint userID, Guid inventoryID, [FromBody]AdjustInventory inventoryUpdate)
        {
            UserDataModel user = ChannelSession.Settings.GetUserDataByMixerID(userID);
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {userID.ToString()}." });
            }
            return AdjustInventory(user, inventoryID, inventoryUpdate);
        }

        [Route("{username}/inventory/{inventoryID:guid}/adjust")]
        [HttpPut, HttpPatch]
        public IActionResult AdjustUserInventory(string username, Guid inventoryID, [FromBody]AdjustInventory inventoryUpdate)
        {
            UserDataModel user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find user: {username.ToString()}." });
            }
            return AdjustInventory(user, inventoryID, inventoryUpdate);
        }

        [Route("top")]
        [HttpGet]
        public IActionResult Get(int count = 10)
        {
            if (count < 1)
            {
                // TODO: Consider checking or a max # too? (100?)
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = "Count must be greater than 0." });
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
            return Ok(userList);
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

        private IActionResult AdjustCurrency(UserDataModel user, Guid currencyID, [FromBody] AdjustCurrency currencyUpdate)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find currency: {currencyID.ToString()}." });
            }

            if (currencyUpdate == null)
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = "Unable to parse currency adjustment from POST body." });
            }

            UserCurrencyModel currency = ChannelSession.Settings.Currencies[currencyID];

            if (currencyUpdate.Amount < 0)
            {
                int quantityToRemove = currencyUpdate.Amount * -1;
                if (!currency.HasAmount(user, quantityToRemove))
                {
                    return GetErrorResponse(HttpStatusCode.Forbidden, new Error { Message = "User does not have enough currency to remove" });
                }

                currency.SubtractAmount(user, quantityToRemove);
            }
            else if (currencyUpdate.Amount > 0)
            {
                currency.AddAmount(user, currencyUpdate.Amount);
            }

            return Ok(UserFromUserDataViewModel(user));
        }

        private IActionResult AdjustInventory(UserDataModel user, Guid inventoryID, [FromBody]AdjustInventory inventoryUpdate)
        {
            if (!ChannelSession.Settings.Inventories.ContainsKey(inventoryID))
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find inventory: {inventoryID.ToString()}." });
            }

            if (inventoryUpdate == null)
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = "Unable to parse inventory adjustment from POST body." });
            }

            UserInventoryModel inventory = ChannelSession.Settings.Inventories[inventoryID];

            if (string.IsNullOrEmpty(inventoryUpdate.Name) || !inventory.Items.ContainsKey(inventoryUpdate.Name))
            {
                return GetErrorResponse(HttpStatusCode.BadRequest, new Error { Message = "Unable to find requested inventory item." });
            }

            if (inventoryUpdate.Amount < 0)
            {
                int quantityToRemove = inventoryUpdate.Amount * -1;
                if (!inventory.HasAmount(user, inventoryUpdate.Name, quantityToRemove))
                {
                    // If the request is to remove inventory, but user doesn't have enough, fail
                    return GetErrorResponse(HttpStatusCode.Forbidden, new Error { Message = "User does not have enough inventory to remove" });
                }

                inventory.SubtractAmount(user, inventoryUpdate.Name, quantityToRemove);
            }
            else if (inventoryUpdate.Amount > 0)
            {
                inventory.AddAmount(user, inventoryUpdate.Name, inventoryUpdate.Amount);
            }

            return Ok(UserFromUserDataViewModel(user));
        }
    }
}
