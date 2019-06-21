using System;
using System.IO;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public abstract class OverlayFileItemModelBase : OverlayItemModelBase
    {
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public abstract string FileType { get; set; }
        [DataMember]
        public string FileID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}/{1}?nonce={2}", this.FileType, this.FileID, Guid.NewGuid());
                }
                return this.FilePath;
            }
            set { }
        }

        [DataMember]
        public string FileExtension { get { return Path.GetExtension(this.FilePath).Replace(".", ""); } set { } }

        [DataMember]
        public bool DefaultWidthHeight { get { return this.Width == 0 && this.Height == 0; } set { } }

        public OverlayFileItemModelBase() : base() { }

        public OverlayFileItemModelBase(OverlayItemModelTypeEnum type, string filepath, int width, int height)
            : base(type)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.FileID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
