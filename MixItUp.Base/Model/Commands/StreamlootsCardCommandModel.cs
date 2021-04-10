using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class StreamlootsCardCommandModel : CommandModelBase
    {
        public StreamlootsCardCommandModel(string name) : base(name, CommandTypeEnum.StreamlootsCard) { }

        protected StreamlootsCardCommandModel() : base() { }
    }
}
