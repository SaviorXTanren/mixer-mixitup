using MixItUp.Base.Services.External;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class CrowdControlEffectCommandModel : CommandModelBase
    {
        public static Dictionary<string, string> GetEffectTestSpecialIdentifiers()
        {
            return new Dictionary<string, string>()
            {
                { "crowdcontroleffectid", "Test Effect ID" },
                { "crowdcontroleffectname", "Test Effect" },
                { "crowdcontroleffectdescription", "Test Effect Description" },
                { "crowdcontroleffectimage", "https://resources.crowdcontrol.live/images/MegaMan2/MegaMan2/icons/etank.png" },
                { "crowdcontrolgamename", "Test Game" }
            };
        }

        [DataMember]
        public string GameID { get; set; }
        [DataMember]
        public string GameName { get; set; }

        [DataMember]
        public string EffectID { get; set; }
        [DataMember]
        public string EffectName { get; set; }

        public CrowdControlEffectCommandModel(CrowdControlGame game, CrowdControlGamePackEffect effect)
            : base(string.Join(" - ", game.name, effect.name), CommandTypeEnum.CrowdControlEffect)
        {
            this.GameID = game.gameID;
            this.GameName = game.Name;
            this.EffectID = effect.id;
            this.EffectName = effect.name;
        }

        [Obsolete]
        public CrowdControlEffectCommandModel() : base() { }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return CrowdControlEffectCommandModel.GetEffectTestSpecialIdentifiers(); }
    }
}
