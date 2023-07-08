using MixItUp.API.V1.Models;
using MixItUp.Base;
using MixItUp.Base.Model.Currency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V1
{
    [RoutePrefix("api/inventory")]
    public class InventoryV1Controller : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<Inventory> Get()
        {
            List<Inventory> list = new List<Inventory>();
            foreach (var inventory in ChannelSession.Settings.Inventory.Values)
            {
                list.Add(InventoryFromUserInventoryViewModel(inventory));
            }
            return list;
        }

        [Route("{inventoryID:guid}")]
        [HttpGet]
        public Inventory Get(Guid inventoryID)
        {
            if (!ChannelSession.Settings.Inventory.ContainsKey(inventoryID))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new ObjectContent<Error>(new Error { Message = $"Unable to find inventory: {inventoryID.ToString()}." }, new JsonMediaTypeFormatter(), "application/json"),
                    ReasonPhrase = "Inventory ID not found"
                };
                throw new HttpResponseException(resp);
            }

            return InventoryFromUserInventoryViewModel(ChannelSession.Settings.Inventory[inventoryID]);
        }

        public static InventoryAmount InventoryAmountFromUserInventoryViewModel(InventoryModel inventory, Dictionary<Guid, int> amounts)
        {
            return new InventoryAmount
            {
                ID = inventory.ID,
                Name = inventory.Name,
                Items = amounts.Select(kvp => new InventoryItemAmount() { Name = inventory.Items.Values.FirstOrDefault(i => i.ID.Equals(kvp.Key)).Name, Amount = kvp.Value }).ToList(),
            };
        }

        public static Inventory InventoryFromUserInventoryViewModel(InventoryModel inventory)
        {
            return new Inventory
            {
                ID = inventory.ID,
                Name = inventory.Name,
                ShopCurrencyID = inventory.ShopCurrencyID,
                Items = inventory.Items.Values.Select(
                    i =>
                    new InventoryItem()
                    {
                        Name = i.Name,
                        BuyAmount = i.BuyAmount,
                        SellAmount = i.SellAmount,
                        MaxAmount = i.MaxAmount,
                    }).ToList(),
            };
        }
    }
}
