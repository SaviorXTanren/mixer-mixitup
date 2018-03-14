using System;

namespace MixItUp.Base.Model.Remote
{
    public abstract class RemoteBoardItemModelBase
    {
        public Guid ID { get; set; }
        public string Name { get; set; }

        public RemoteBoardItemSizeEnum Size { get; set; }

        public int XPosition { get; set; }
        public int YPosition { get; set; }
    }

    public class RemoteBoardButtonModel : RemoteBoardItemModelBase
    {
        public string ImageName { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
    }
}
