using Mixer.Base.Model.User;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Desktop.Services
{
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

    public class WindowsDeveloperAPIService : HttpListenerServerBase, IDeveloperAPIService
    {
        public const string GetHttpMethod = "GET";
        public const string PostHttpMethod = "POST";
        public const string PutHttpMethod = "PUT";
        public const string PatchHttpMethod = "PATCH";

        public const string DeveloperAPIHttpListenerServerAddress = "http://localhost:8911/api/";

        public WindowsDeveloperAPIService() : base(WindowsDeveloperAPIService.DeveloperAPIHttpListenerServerAddress) { }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            if (!string.IsNullOrEmpty(listenerContext.Request.RawUrl))
            {
                string url = listenerContext.Request.RawUrl.ToLower();
                string httpMethod = listenerContext.Request.HttpMethod;
                if (url.StartsWith("/api"))
                {
                    List<string> urlSegments = new List<string>(url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
                    urlSegments.RemoveAt(0);
                    if (urlSegments.Count() == 0)
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, "Welcome to the Mix It Up Developer API! More detailed documentation about this service, please visit https://github.com/SaviorXTanren/mixer-mixitup/wiki/Developer-API");
                        return;
                    }
                    else
                    {
                        string data = await this.GetRequestData(listenerContext);
                        await this.ProcessDeveloperAPIRequest(listenerContext, httpMethod, urlSegments, data);
                        return;
                    }
                }
            }
            await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "This is not a valid API");
        }

        private async Task ProcessDeveloperAPIRequest(HttpListenerContext listenerContext, string httpMethod, List<string> urlSegments, string data)
        {
            if (urlSegments[0].Equals("mixer"))
            {
                if (urlSegments.Count() == 3 && urlSegments[1].Equals("users"))
                {
                    if (httpMethod.Equals(GetHttpMethod))
                    {
                        string identifier = urlSegments[2];

                        UserModel user = null;
                        if (uint.TryParse(identifier, out uint userID))
                        {
                            user = ChannelSession.Connection.GetUser(userID).Result;
                        }
                        else
                        {
                            user = ChannelSession.Connection.GetUser(identifier).Result;
                        }

                        if (user != null)
                        {
                            await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(user));
                            return;
                        }
                        else
                        {
                            await this.CloseConnection(listenerContext, HttpStatusCode.NotFound, "Could not find the user specified");
                            return;
                        }
                    }
                }
            }
            else if (urlSegments[0].Equals("users") && urlSegments.Count() >= 2)
            {
                string identifier = urlSegments[1];

                UserDataViewModel user = null;
                if (uint.TryParse(identifier, out uint userID) && ChannelSession.Settings.UserData.ContainsKey(userID))
                {
                    user = ChannelSession.Settings.UserData[userID];
                }
                else
                {
                    user = ChannelSession.Settings.UserData.Values.FirstOrDefault(u => u.UserName.ToLower().Equals(identifier));
                }

                if (httpMethod.Equals(GetHttpMethod))
                {
                    if (user != null)
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(new UserDeveloperAPIModel(user)));
                        return;
                    }
                    else
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.NotFound, "Could not find the user specified");
                        return;
                    }
                }
                else if (httpMethod.Equals(PutHttpMethod) || httpMethod.Equals(PatchHttpMethod))
                {
                    UserDeveloperAPIModel updatedUserData = SerializerHelper.DeserializeFromString<UserDeveloperAPIModel>(data);
                    if (updatedUserData != null && updatedUserData.ID.Equals(user.ID))
                    {
                        user.ViewingMinutes = updatedUserData.ViewingMinutes;
                        foreach (UserCurrencyDeveloperAPIModel currencyData in updatedUserData.CurrencyAmounts)
                        {
                            if (ChannelSession.Settings.Currencies.ContainsKey(currencyData.ID))
                            {
                                user.SetCurrencyAmount(ChannelSession.Settings.Currencies[currencyData.ID], currencyData.Amount);
                            }
                        }

                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(new UserDeveloperAPIModel(user)));
                        return;
                    }
                    else
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.NotFound, "Invalid data/could not find matching user");
                        return;
                    }
                }
            }
            else if (urlSegments[0].Equals("currency") && urlSegments.Count() == 2)
            {
                if (httpMethod.Equals(GetHttpMethod))
                {
                    string identifier = urlSegments[1];
                    if (Guid.TryParse(identifier, out Guid currencyID) && ChannelSession.Settings.Currencies.ContainsKey(currencyID))
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(ChannelSession.Settings.Currencies[currencyID]));
                        return;
                    }
                    else
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.NotFound, "Could not find the currency specified");
                        return;
                    }
                }
            }
            else if (urlSegments[0].Equals("commands"))
            {
                List<CommandBase> allCommands = new List<CommandBase>();
                allCommands.AddRange(ChannelSession.Settings.ChatCommands);
                allCommands.AddRange(ChannelSession.Settings.InteractiveCommands);
                allCommands.AddRange(ChannelSession.Settings.EventCommands);
                allCommands.AddRange(ChannelSession.Settings.TimerCommands);
                allCommands.AddRange(ChannelSession.Settings.ActionGroupCommands);
                allCommands.AddRange(ChannelSession.Settings.GameCommands);

                if (httpMethod.Equals(GetHttpMethod))
                {
                    if (urlSegments.Count() == 1)
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(allCommands));
                        return;
                    }
                    else if (urlSegments.Count() == 2 && Guid.TryParse(urlSegments[1], out Guid ID))
                    {
                        CommandBase command = allCommands.FirstOrDefault(c => c.ID.Equals(ID));
                        if (command != null)
                        {
                            await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(command));
                            return;
                        }
                        else
                        {
                            await this.CloseConnection(listenerContext, HttpStatusCode.NotFound, "Could not find the command specified");
                            return;
                        }
                    }
                }
                else if (httpMethod.Equals(PostHttpMethod))
                {
                    if (urlSegments.Count() == 2 && Guid.TryParse(urlSegments[1], out Guid ID))
                    {
                        CommandBase command = allCommands.FirstOrDefault(c => c.ID.Equals(ID));
                        if (command != null)
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            command.Perform();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                            await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(command));
                            return;
                        }
                        else
                        {
                            await this.CloseConnection(listenerContext, HttpStatusCode.NotFound, "Could not find the command specified");
                            return;
                        }
                    }
                }
                else if (httpMethod.Equals(PutHttpMethod) || httpMethod.Equals(PatchHttpMethod))
                {
                    if (urlSegments.Count() == 2 && Guid.TryParse(urlSegments[1], out Guid ID))
                    {
                        CommandBase commandData = SerializerHelper.DeserializeAbstractFromString<CommandBase>(data);
                        CommandBase matchedCommand = allCommands.FirstOrDefault(c => c.ID.Equals(ID));
                        if (matchedCommand != null)
                        {
                            matchedCommand.IsEnabled = commandData.IsEnabled;

                            await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(matchedCommand));
                            return;
                        }
                        else
                        {
                            await this.CloseConnection(listenerContext, HttpStatusCode.NotFound, "Invalid data/could not find matching command");
                            return;
                        }
                    }
                }
            }
            else if (urlSegments[0].Equals("spotify") && urlSegments.Count() >= 2)
            {
                if (ChannelSession.Services.Spotify != null)
                {
                    if (httpMethod.Equals(GetHttpMethod))
                    {
                        if (urlSegments.Count() == 2)
                        {
                            if (urlSegments[1].Equals("current"))
                            {
                                await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(await ChannelSession.Services.Spotify.GetCurrentlyPlaying()));
                                return;
                            }
                            else if (urlSegments[1].StartsWith("search?query="))
                            {
                                string search = urlSegments[1].Replace("search?query=", "");
                                search = HttpUtility.UrlDecode(search);

                                await this.CloseConnection(listenerContext, HttpStatusCode.OK, SerializerHelper.SerializeToString(await ChannelSession.Services.Spotify.SearchSongs(search)));
                                return;
                            }
                        }
                    }
                    else if (httpMethod.Equals(PostHttpMethod))
                    {
                        if (urlSegments.Count() == 2)
                        {
                            if (urlSegments[1].Equals("play"))
                            {
                                if (string.IsNullOrEmpty(data))
                                {
                                    await ChannelSession.Services.Spotify.PlayCurrentlyPlaying();
                                    await this.CloseConnection(listenerContext, HttpStatusCode.OK, string.Empty);
                                    return;
                                }
                                else
                                {
                                    if (await ChannelSession.Services.Spotify.PlaySong(data))
                                    {
                                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, string.Empty);
                                    }
                                    else
                                    {
                                        await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "We were unable to play the uri you specified. If your uri is correct, please try again in a moment");
                                    }
                                    return;
                                }
                            }
                            else if (urlSegments[1].Equals("pause"))
                            {
                                await ChannelSession.Services.Spotify.PauseCurrentlyPlaying();
                                await this.CloseConnection(listenerContext, HttpStatusCode.OK, string.Empty);
                                return;
                            }
                            else if (urlSegments[1].Equals("next"))
                            {
                                await ChannelSession.Services.Spotify.NextCurrentlyPlaying();
                                await this.CloseConnection(listenerContext, HttpStatusCode.OK, string.Empty);
                                return;
                            }
                            else if (urlSegments[1].Equals("previous"))
                            {
                                await ChannelSession.Services.Spotify.PreviousCurrentlyPlaying();
                                await this.CloseConnection(listenerContext, HttpStatusCode.OK, string.Empty);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    await this.CloseConnection(listenerContext, HttpStatusCode.ServiceUnavailable, "The Spotify service is not currently connected in Mix It Up");
                }
            }

            await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "This is not a valid API");
        }
    }
}
