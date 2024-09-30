using MixItUp.API.V1.Models;
using System.Threading.Tasks;

namespace MixItUp.API
{
    //  GET:    http://localhost:8911/api/inventory                                 Currencies.GetAllinventoryAsync
    public static class Inventories
    {
        public static async Task<Inventory[]> GetAllInventoriesAsync()
        {
            return await RestClient.GetAsync<Inventory[]>($"inventory");
        }
    }
}
