using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    [DataContract]
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
        public virtual string FullLink { get { return this.FilePath; } set { } }

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
