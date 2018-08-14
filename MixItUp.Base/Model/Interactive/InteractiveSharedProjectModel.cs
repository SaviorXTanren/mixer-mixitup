namespace MixItUp.Base.Model.Interactive
{
    public class InteractiveSharedProjectModel
    {
        public uint VersionID { get; set; }
        public string ShareCode { get; set; }

        public InteractiveSharedProjectModel() { }

        public InteractiveSharedProjectModel(uint versionID) : this(versionID, null) { }

        public InteractiveSharedProjectModel(uint versionID, string shareCode)
        {
            this.VersionID = versionID;
            this.ShareCode = shareCode;
        }
    }
}
