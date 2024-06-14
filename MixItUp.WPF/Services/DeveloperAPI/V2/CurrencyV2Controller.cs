using MixItUp.API.V2.Models;
using MixItUp.Base;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    [RoutePrefix("api/v2/currency")]
    public class CurrencyV2Controller : ApiController
    {
        [Route]
        [HttpGet]
        public IHttpActionResult GetCurrencies()
        {
            var currencies = new List<GetCurrencyResponse>();

            foreach (var currency in ChannelSession.Settings.Currency)
            {
                currencies.Add(new GetCurrencyResponse
                {
                    ID = currency.Value.ID,
                    Name = currency.Value.Name
                });
            }

            return Ok(currencies);
        }

        [Route("{currencyId:guid}/{userId:guid}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetCurrencyAmountForUser(Guid currencyId, Guid userId)
        {
            if (!ChannelSession.Settings.Currency.TryGetValue(currencyId, out var currency) || currency == null)
            {
                return NotFound();
            }

            await ServiceManager.Get<UserService>().LoadAllUserData();

            if (!ChannelSession.Settings.Users.TryGetValue(userId, out var user) || user == null)
            {
                return NotFound();
            }

            return Ok(currency.GetAmount(user));
        }

        [Route("{currencyId:guid}/{userId:guid}")]
        [HttpPatch]
        public async Task<IHttpActionResult> UpdateCurrencyAmountForUser(Guid currencyId, Guid userId, [FromBody] UpdateCurrencyAmount updateAmount)
        {
            if (!ChannelSession.Settings.Currency.TryGetValue(currencyId, out var currency) || currency == null)
            {
                return NotFound();
            }

            await ServiceManager.Get<UserService>().LoadAllUserData();

            if (!ChannelSession.Settings.Users.TryGetValue(userId, out var user) || user == null)
            {
                return NotFound();
            }

            if (updateAmount.Amount > 0)
            {
                currency.AddAmount(user, updateAmount.Amount);
            }
            else if (updateAmount.Amount < 0)
            {
                currency.SubtractAmount(user, -1 * updateAmount.Amount);
            }

            return Ok(currency.GetAmount(user));
        }

        [Route("{currencyId:guid}/{userId:guid}")]
        [HttpPut]
        public async Task<IHttpActionResult> SetCurrencyAmountForUser(Guid currencyId, Guid userId, [FromBody] UpdateCurrencyAmount updateAmount)
        {
            if (!ChannelSession.Settings.Currency.TryGetValue(currencyId, out var currency) || currency == null)
            {
                return NotFound();
            }

            await ServiceManager.Get<UserService>().LoadAllUserData();

            if (!ChannelSession.Settings.Users.TryGetValue(userId, out var user) || user == null)
            {
                return NotFound();
            }

            currency.SetAmount(user, updateAmount.Amount);

            return Ok(currency.GetAmount(user));
        }
    }
}