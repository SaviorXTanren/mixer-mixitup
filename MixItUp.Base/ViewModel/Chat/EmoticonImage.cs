using System.Runtime.CompilerServices;

namespace MixItUp.Base.ViewModel.Chat
{
    public class EmoticonImage
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
    }
}