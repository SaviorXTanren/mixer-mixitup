using Microsoft.Owin.Hosting;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.Desktop.Services
{
    [DataContract]
    public class UserCurrencyUpdateDeveloperAPIModel
    {
        [DataMember]
        public int Amount { get; set; }
    }

    [DataContract]
    public class UserCurrencyDeveloperAPIModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public UserCurrencyDeveloperAPIModel() { }

        public UserCurrencyDeveloperAPIModel(UserCurrencyDataViewModel currencyData)
        {
            this.ID = currencyData.Currency.ID;
            this.Name = currencyData.Currency.Name;
            this.Amount = currencyData.Amount;
        }
    }

    [DataContract]
    public class UserDeveloperAPIModel
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int ViewingMinutes { get; set; }

        [DataMember]
        public List<UserCurrencyDeveloperAPIModel> CurrencyAmounts { get; set; }

        public UserDeveloperAPIModel()
        {
            this.CurrencyAmounts = new List<UserCurrencyDeveloperAPIModel>();
        }

        public UserDeveloperAPIModel(UserDataViewModel userData)
            : this()
        {
            this.ID = userData.ID;
            this.UserName = userData.UserName;
            this.ViewingMinutes = userData.ViewingMinutes;
            foreach (UserCurrencyDataViewModel currencyData in userData.CurrencyAmounts.Values)
            {
                this.CurrencyAmounts.Add(new UserCurrencyDeveloperAPIModel(currencyData));
            }
        }
    }

    public class NoCacheHeader : DelegatingHandler
    {
        async protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("Cache-Control", "no-cache");
            return response;
        }
    }

    public class WindowsDeveloperAPIServiceStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new NoCacheHeader());

            appBuilder.UseWebApi(config);
        }
    }

    public class WindowsDeveloperAPIService : IDeveloperAPIService
    {
        private IDisposable webApp;
        public const string DeveloperAPIHttpListenerServerAddress = "http://localhost:8911/";

        public bool Start()
        {
            // Ensure it is cleaned up first
            End();

            this.webApp = WebApp.Start<WindowsDeveloperAPIServiceStartup>(DeveloperAPIHttpListenerServerAddress);
            return true;
        }

        public void End()
        {
            if (this.webApp != null)
            {
                this.webApp.Dispose();
                this.webApp = null;
            }
        }
    }

    [RoutePrefix("api/mixer/users")]
    public class MixerUserController : ApiController
    {
        [Route("{userID:int:min(0)}")]
        public UserModel Get(uint userID)
        {
            UserModel user = ChannelSession.Connection.GetUser(userID).Result;
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return user;
        }

        [Route("{username}")]
        [HttpGet]
        public UserModel Get(string username)
        {
            UserModel user = ChannelSession.Connection.GetUser(username).Result;
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return user;
        }
    }

    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
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

        // TODO: Add GiveAll
    }

    [RoutePrefix("api/currency")]
    public class CurrencyController : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<UserCurrencyViewModel> Get()
        {
            return ChannelSession.Settings.Currencies.Values;
        }

        [Route("{currencyID:guid}")]
        [HttpGet]
        public UserCurrencyViewModel Get(Guid currencyID)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return ChannelSession.Settings.Currencies[currencyID];
        }

        [Route("{currencyID:guid}/top")]
        [HttpGet]
        public IEnumerable<UserDeveloperAPIModel> Get(Guid currencyID, int count = 10)
        {
            if (!ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (count < 1)
            {
                // TODO: Consider checking or a max # too? (100?)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            UserCurrencyViewModel currency = ChannelSession.Settings.Currencies[currencyID];

            Dictionary<uint, UserDataViewModel> allUsersDictionary = ChannelSession.Settings.UserData.ToDictionary();
            allUsersDictionary.Remove(ChannelSession.Channel.user.id);

            IEnumerable<UserDataViewModel> allUsers = allUsersDictionary.Select(kvp => kvp.Value);
            allUsers = allUsers.Where(u => !u.IsCurrencyRankExempt);

            List<UserDeveloperAPIModel> currencyUserList = new List<UserDeveloperAPIModel>();
            foreach (UserDataViewModel currencyUser in allUsers.OrderByDescending(u => u.GetCurrencyAmount(currency)).Take(count))
            {
                currencyUserList.Add(new UserDeveloperAPIModel(currencyUser));
            }
            return currencyUserList;
        }
    }

    [RoutePrefix("api/commands")]
    public class CommandController : ApiController
    {
        [Route]
        [HttpGet]
        public IEnumerable<CommandBase> Get()
        {
            return GetAllCommands();
        }

        [Route("{commandID:guid}")]
        [HttpGet]
        public CommandBase Get(Guid commandID)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return selectedCommand;
        }

        [Route("{commandID:guid}")]
        [HttpPost]
        public CommandBase Run(Guid commandID)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            selectedCommand.Perform();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return selectedCommand;
        }

        [Route("{commandID:guid}")]
        [HttpPut, HttpPatch]
        public CommandBase Update(Guid commandID, [FromBody] CommandBase commandData)
        {
            CommandBase selectedCommand = GetAllCommands().SingleOrDefault(c => c.ID == commandID);
            if (selectedCommand == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            selectedCommand.IsEnabled = commandData.IsEnabled;
            return selectedCommand;
        }

        private List<CommandBase> GetAllCommands()
        {
            List<CommandBase> allCommands = new List<CommandBase>();
            allCommands.AddRange(ChannelSession.Settings.ChatCommands);
            allCommands.AddRange(ChannelSession.Settings.InteractiveCommands);
            allCommands.AddRange(ChannelSession.Settings.EventCommands);
            allCommands.AddRange(ChannelSession.Settings.TimerCommands);
            allCommands.AddRange(ChannelSession.Settings.ActionGroupCommands);
            allCommands.AddRange(ChannelSession.Settings.GameCommands);
            return allCommands;
        }
    }
}
