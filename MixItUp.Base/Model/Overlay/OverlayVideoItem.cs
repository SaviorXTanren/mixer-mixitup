using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayVideoItem : OverlayItemBase
    {
        public const int DefaultHeight = 315;
        public const int DefaultWidth = 560;

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}", this.FileID);
                }
                return this.FilePath;
            }
            set { }
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayVideoItem() { this.Volume = 100; }

        public OverlayVideoItem(string filepath, int width, int height, int volume)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.FileID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayVideoItem item = this.Copy<OverlayVideoItem>();
            item.FilePath = await this.ReplaceStringWithSpecialModifiers(item.FilePath, user, arguments, extraSpecialIdentifiers);
            if (!Uri.IsWellFormedUriString(item.FilePath, UriKind.RelativeOrAbsolute))
            {
                item.FilePath = item.FilePath.ToFilePathString();
            }
            return item;
        }
    }
}
