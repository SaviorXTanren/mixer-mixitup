using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayImageItem : OverlayItemBase
    {
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}?nonce={1}", this.FileID, Guid.NewGuid());
                }
                return this.FilePath;
            }
            set { }
        }

        public OverlayImageItem() { }

        public OverlayImageItem(string filepath, int width, int height)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.FileID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayImageItem item = this.Copy<OverlayImageItem>();
            item.FilePath = await this.ReplaceStringWithSpecialModifiers(item.FilePath, user, arguments, extraSpecialIdentifiers);
            if (!Uri.IsWellFormedUriString(item.FilePath, UriKind.RelativeOrAbsolute))
            {
                item.FilePath = item.FilePath.ToFilePathString();
            }
            return item;
        }
    }
}
