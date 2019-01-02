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
    [RoutePrefix("api/inventory")]
    public class InventoryController : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<Inventory> Get()
        {
            List<Inventory> list = new List<Inventory>();
            foreach (var inventory in ChannelSession.Settings.Inventories.Values)
            {
                list.Add(InventoryFromUserInventoryViewModel(inventory));
            }
            return list;
        }

        [Route("{inventoryID:guid}")]
        [HttpGet]
        public Inventory Get(Guid inventoryID)
        {
            if (!ChannelSession.Settings.Inventories.ContainsKey(inventoryID))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return InventoryFromUserInventoryViewModel(ChannelSession.Settings.Inventories[inventoryID]);
        }

        public static InventoryAmount InventoryAmountFromUserInventoryViewModel(UserInventoryViewModel inventory, UserInventoryDataViewModel inventoryData)
        {
            return new InventoryAmount
            {
                ID = inventory.ID,
                Name = inventory.Name,
                Items = inventoryData.Amounts.Select(kvp => new InventoryItemAmount() { Name = kvp.Key, Amount = kvp.Value }).ToList(),
            };
        }

        public static Inventory InventoryFromUserInventoryViewModel(UserInventoryViewModel inventory)
        {
            return new Inventory
            {
                ID = inventory.ID,
                Name = inventory.Name,
                Items = inventory.Items.Keys.Select(i => new InventoryItem() { Name = i }).ToList(),
            };
        }
    }
}
