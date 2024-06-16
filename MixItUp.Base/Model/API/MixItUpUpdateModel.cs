using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.API
{
    [DataContract]
    public class MixItUpUpdateModel
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
}
