using System;

namespace MixItUp.Base.Model.Remote
{
    public abstract class RemoteBoardItemModelBase
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }

    public class RemoteBoardButtonModel : RemoteBoardItemModelBase
    {
        public string Image { get; set; }
    }
}
