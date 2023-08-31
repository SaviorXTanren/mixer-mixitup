using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class StreamlootsCardCommandModel : CommandModelBase
    {
        public static Dictionary<string, string> GetCardTestSpecialIdentifiers()
        {
            return new Dictionary<string, string>()
            {
                { "streamlootscardname", "Test Card" },
                { "streamlootscarddescription", "Test Description" },
                { "streamlootscardimage", "https://res.cloudinary.com/streamloots/image/upload/f_auto,c_scale,w_250,q_90/static/e19c7bf6-ca3e-49a8-807e-b2e9a1a47524/en_dl_character.png" },
                { "streamlootscardvideo", "https://cdn.streamloots.com/uploads/5c645b78666f31002f2979d1/3a6bf1dc-7d61-4f93-be0a-f5dc1d0d33b6.webm" },
                { "streamlootscardsound", "https://static.streamloots.com/b355d1ef-d931-4c16-a48f-8bed0076401b/alerts/default.mp3" },
                { "streamlootscardalertmessage", "This is an alert message" },
                { "streamlootsmessage", "Test Message" }
            };
        }

        public StreamlootsCardCommandModel(string name) : base(name, CommandTypeEnum.StreamlootsCard) { }

        [Obsolete]
        public StreamlootsCardCommandModel() : base() { }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return StreamlootsCardCommandModel.GetCardTestSpecialIdentifiers(); }
    }
}
