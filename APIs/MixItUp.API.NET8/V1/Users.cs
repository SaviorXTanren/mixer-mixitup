using MixItUp.API.V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/users/{usernameOrID}      Users.GetUserAsync
    //  PUT:    http://localhost:8911/api/users/{usernameOrID}      Users.UpdateUserAsync
    //  GET:    http://localhost:8911/api/users/top                 Users.GetTopUsersByViewingTimeAsync
    //  POST:   http://localhost:8911/api/users                     Users.GetUsersAsync
    //  PUT:    http://localhost:8911/api/users/{usernameOrID}/currency/{currencyID}/adjust Users.AdjustUserCurrencyAsync
    //  PUT:    http://localhost:8911/api/users/{usernameOrID}/inventory/{inventoryID}/adjust Users.AdjustUserInventoryAsync
    public static class Users
    {
        public static async Task<User> GetUserAsync(uint userId)
        {
            return await RestClient.GetAsync<User>($"users/{userId}");
        }

        public static async Task<User> GetUserAsync(string userName)
        {
            return await RestClient.GetAsync<User>($"users/{userName}");
        }

        public static async Task<User> UpdateUserAsync(User user)
        {
            return await RestClient.PutAsync<User>($"users/{user.ID}", user);
        }

        public static async Task<User[]> GetUsersAsync(IEnumerable<string> userNamesOrIds)
        {
            if (userNamesOrIds.Count() == 0)
            {
                return new User[0];
            }

            return await RestClient.PostAsync<User[]>("users", userNamesOrIds);
        }

        public static async Task<User[]> GetTopUsersByViewingTimeAsync(uint? count)
        {
            string uri = "users/top";
            if (count.HasValue)
            {
                uri += $"?count={count}";
            }

            return await RestClient.GetAsync<User[]>(uri);
        }

        public static async Task<User> AdjustUserCurrencyAsync(string userName, Guid currencyID, int offsetAmount)
        {
            AdjustCurrency adjustCurrency = new AdjustCurrency
            {
                Amount = offsetAmount
            };

            return await RestClient.PutAsync<User>($"users/{userName}/currency/{currencyID}/adjust", adjustCurrency);
        }

        public static async Task<User> AdjustUserCurrencyAsync(uint userId, Guid currencyID, int offsetAmount)
        {
            AdjustCurrency adjustCurrency = new AdjustCurrency
            {
                Amount = offsetAmount
            };

            return await RestClient.PutAsync<User>($"users/{userId}/currency/{currencyID}/adjust", adjustCurrency);
        }

        public static async Task<User> AdjustUserInventoryAsync(string userName, Guid inventoryID, string itemName, int offsetAmount)
        {
            AdjustInventory adjustInventory = new AdjustInventory
            {
                Name = itemName,
                Amount = offsetAmount
            };

            return await RestClient.PutAsync<User>($"users/{userName}/inventory/{inventoryID}/adjust", adjustInventory);
        }

        public static async Task<User> AdjustUserInventoryAsync(uint userId, Guid inventoryID, string itemName, int offsetAmount)
        {
            AdjustInventory adjustInventory = new AdjustInventory
            {
                Name = itemName,
                Amount = offsetAmount
            };

            return await RestClient.PutAsync<User>($"users/{userId}/inventory/{inventoryID}/adjust", adjustInventory);
        }
    }
}
