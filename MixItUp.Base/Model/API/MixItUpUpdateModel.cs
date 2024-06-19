using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class MixItUpUpdateV2Model
    {
        [DataMember]
        public string InstallerLink { get; set; }

        [DataMember]
        private string Version { get; set; }
        [DataMember]
        private string Iteration { get; set; }
        [DataMember]
        private string Path { get; set; }

        [JsonIgnore]
        public bool IsPreview { get { return this.SystemVersion.Revision > 1000; } }

        [JsonIgnore]
        public string FullVersion { get { return $"{this.Version}.{this.Iteration}"; } }

        [JsonIgnore]
        public Version SystemVersion { get { return new Version(this.FullVersion); } }

        [JsonIgnore]
        public string ChangelogLink { get { return $"https://github.com/SaviorXTanren/mixer-mixitup/releases/download/{this.Version}{this.Path}/Changelog.html"; } }

        [JsonIgnore]
        public string ZipArchiveLink { get { return $"https://github.com/SaviorXTanren/mixer-mixitup/releases/download/{this.Version}{this.Path}/MixItUp.zip"; } }
    }

    [DataContract]
    public class MixItUpUpdateModel
    {
        [JsonProperty]
        public string Version { get; set; }
        [JsonProperty]
        public string ChangelogLink { get; set; }
        [JsonProperty]
        public string ZipArchiveLink { get; set; }

        [JsonProperty]
        public string InstallerLink { get; set; }

        [JsonIgnore]
        public Version SystemVersion { get { return new Version(this.Version); } }

        [JsonIgnore]
        public bool IsPreview { get { return this.SystemVersion.Revision > 1000; } }

        public MixItUpUpdateModel() { }

        public MixItUpUpdateModel(MixItUpUpdateV2Model update)
        {
            this.Version = update.FullVersion;
            this.ChangelogLink = update.ChangelogLink;
            this.ZipArchiveLink = update.ZipArchiveLink;
            this.InstallerLink = update.InstallerLink;
        }
    }
}