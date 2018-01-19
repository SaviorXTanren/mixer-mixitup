using Mixer.Base.Model.User;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

namespace MixItUp.Desktop.Services
{
    [DataContract]
    public class UserCurrencyDeveloperAPIModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public UserCurrencyDeveloperAPIModel() { }

        public UserCurrencyDeveloperAPIModel(UserCurrencyDataViewModel currencyData)
        {
            this.ID = currencyData.Currency.ID;
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
        public const string DeveloperAPIHttpListenerServerAddress = "http://localhost:8911/api/";

        public WindowsDeveloperAPIService() : base(WindowsDeveloperAPIService.DeveloperAPIHttpListenerServerAddress) { }

        protected override HttpStatusCode RequestReceived(HttpListenerRequest request, string data, out string result)
        {
            if (!string.IsNullOrEmpty(request.RawUrl) && request.RawUrl.ToLower().StartsWith("/api"))
            {
                List<string> urlSegments = new List<string>(request.RawUrl.ToLower().Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
                urlSegments.RemoveAt(0);
                if (urlSegments.Count() == 0)
                {
                    result = "Welcome to the Mix It Up Developer API! More detailed documentation about this service, please visit https://github.com/SaviorXTanren/mixer-mixitup/wiki/Developer-API";
                    return HttpStatusCode.OK;
                }
                else if (urlSegments[0].Equals("mixer"))
                {
                    if (urlSegments.Count() == 3 && urlSegments[1].Equals("users"))
                    {
                        if (request.HttpMethod.Equals("GET"))
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
                                result = SerializerHelper.SerializeToString(user);
                                return HttpStatusCode.OK;
                            }
                            else
                            {
                                result = "Could not find the user specified";
                                return HttpStatusCode.NotFound;
                            }
                        }
                    }
                }
                else if (urlSegments[0].Equals("users") && urlSegments.Count >= 2)
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

                    if (request.HttpMethod.Equals("GET"))
                    {
                        if (user != null)
                        {
                            result = SerializerHelper.SerializeToString(new UserDeveloperAPIModel(user));
                            return HttpStatusCode.OK;
                        }
                        else
                        {
                            result = "Could not find the user specified";
                            return HttpStatusCode.NotFound;
                        }
                    }
                    else if (request.HttpMethod.Equals("PUT") || request.HttpMethod.Equals("PATCH"))
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

                            result = SerializerHelper.SerializeToString(new UserDeveloperAPIModel(user));
                            return HttpStatusCode.OK;
                        }
                        else
                        {
                            result = "Invalid data/could not find matching user";
                            return HttpStatusCode.NotFound;
                        }
                    }
                }
                else if (urlSegments[0].Equals("currency") && urlSegments.Count() == 2)
                {
                    if (request.HttpMethod.Equals("GET"))
                    {
                        string identifier = urlSegments[1];
                        if (Guid.TryParse(identifier, out Guid currencyID) && ChannelSession.Settings.Currencies.ContainsKey(currencyID))
                        {
                            result = SerializerHelper.SerializeToString(ChannelSession.Settings.Currencies[currencyID]);
                            return HttpStatusCode.OK;
                        }
                        else
                        {
                            result = "Could not find the currency specified";
                            return HttpStatusCode.NotFound;
                        }
                    }
                }
                else if (urlSegments[0].Equals("commands"))
                {
                    if (request.HttpMethod.Equals("GET"))
                    {

                    }
                    else if (request.HttpMethod.Equals("POST"))
                    {

                    }
                }

                result = "This is not a valid API";
                return HttpStatusCode.BadRequest;
            }
            else
            {
                result = "This is not a valid API";
                return HttpStatusCode.BadRequest;
            }
        }
    }
}
