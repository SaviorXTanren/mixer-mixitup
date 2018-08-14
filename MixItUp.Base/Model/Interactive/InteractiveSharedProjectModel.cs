namespace MixItUp.Base.Model.Interactive
{
    public class InteractiveSharedProjectModel
    {
        public static readonly InteractiveSharedProjectModel FortniteDropMap = new InteractiveSharedProjectModel(271086, 277002, "dxr3qllr");

        public static readonly InteractiveSharedProjectModel MixerPaint = new InteractiveSharedProjectModel(271176, 277092, "zu52jzv2");

        public uint GameID { get; set; }
        public uint VersionID { get; set; }
        public string ShareCode { get; set; }

        public InteractiveSharedProjectModel() { }

        public InteractiveSharedProjectModel(uint versionID, string shareCode) : this(0, versionID, shareCode) { }

        public InteractiveSharedProjectModel(uint gameID, uint versionID, string shareCode)
        {
            this.GameID = gameID;
            this.VersionID = versionID;
            this.ShareCode = shareCode;
        }
    }
}
