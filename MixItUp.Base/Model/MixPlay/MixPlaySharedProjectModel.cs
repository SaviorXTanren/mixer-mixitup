using System.Collections.Generic;

namespace MixItUp.Base.Model.MixPlay
{
    public class MixPlaySharedProjectModel
    {
        public static readonly MixPlaySharedProjectModel FortniteDropMap = new MixPlaySharedProjectModel(271086, 277002, "dxr3qllr");
        public static readonly MixPlaySharedProjectModel PUBGDropMap = new MixPlaySharedProjectModel(271188, 277104, "58virtn9");
        public static readonly MixPlaySharedProjectModel RealmRoyaleDropMap = new MixPlaySharedProjectModel(271221, 277137, "4h0qt5ub");
        public static readonly MixPlaySharedProjectModel BlackOps4DropMap = new MixPlaySharedProjectModel(286365, 292278, "svdiqaq5");
        public static readonly MixPlaySharedProjectModel ApexLegendsDropMap = new MixPlaySharedProjectModel(320709, 326616, "yar067ds");
        public static readonly MixPlaySharedProjectModel SuperAnimalRoyaleDropMap = new MixPlaySharedProjectModel(332619, 338526, "nbb7xrcu");

        public static readonly MixPlaySharedProjectModel MixerPaint = new MixPlaySharedProjectModel(271176, 277092, "zu52jzv2");

        public static readonly MixPlaySharedProjectModel FlySwatter = new MixPlaySharedProjectModel(295410, 301323, "5b11a82j");

        public static readonly List<MixPlaySharedProjectModel> AllMixPlayProjects = new List<MixPlaySharedProjectModel>() { FortniteDropMap, PUBGDropMap, RealmRoyaleDropMap, BlackOps4DropMap, ApexLegendsDropMap, SuperAnimalRoyaleDropMap, MixerPaint, FlySwatter };

        public uint GameID { get; set; }
        public uint VersionID { get; set; }
        public string ShareCode { get; set; }

        public MixPlaySharedProjectModel() { }

        public MixPlaySharedProjectModel(uint versionID, string shareCode) : this(0, versionID, shareCode) { }

        public MixPlaySharedProjectModel(uint gameID, uint versionID, string shareCode)
        {
            this.GameID = gameID;
            this.VersionID = versionID;
            this.ShareCode = shareCode;
        }
    }
}
