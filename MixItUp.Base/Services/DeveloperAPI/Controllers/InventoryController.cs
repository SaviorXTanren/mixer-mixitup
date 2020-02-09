using MixItUp.Base.ViewModel.User;
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
    [Route("api/inventory")]
    public class InventoryController : BaseController
    {
        [Route("")]
        [HttpGet]
        public IActionResult Get()
        {
            List<Inventory> list = new List<Inventory>();
            foreach (var inventory in ChannelSession.Settings.Inventories.Values)
            {
                list.Add(InventoryFromUserInventoryViewModel(inventory));
            }
            return Ok(list);
        }

        [Route("{inventoryID:guid}")]
        [HttpGet]
        public IActionResult Get(Guid inventoryID)
        {
            if (!ChannelSession.Settings.Inventories.ContainsKey(inventoryID))
            {
                return GetErrorResponse(HttpStatusCode.NotFound, new Error { Message = $"Unable to find inventory: {inventoryID.ToString()}." });
            }

            return Ok(InventoryFromUserInventoryViewModel(ChannelSession.Settings.Inventories[inventoryID]));
        }

        public static InventoryAmount InventoryAmountFromUserInventoryViewModel(UserInventoryModel inventory, UserInventoryDataViewModel inventoryData)
        {
            return new InventoryAmount
            {
                ID = inventory.ID,
                Name = inventory.Name,
                Items = inventoryData.GetAmounts().Select(kvp => new InventoryItemAmount() { Name = kvp.Key, Amount = kvp.Value }).ToList(),
            };
        }

        public static Inventory InventoryFromUserInventoryViewModel(UserInventoryModel inventory)
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
