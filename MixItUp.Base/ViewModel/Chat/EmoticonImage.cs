using System;

namespace MixItUp.Base.ViewModel.Chat
{
    public class EmoticonImage : IEquatable<EmoticonImage>
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }

        public bool Equals(EmoticonImage other)
        {
            return this.FilePath.Equals(other.FilePath, StringComparison.InvariantCultureIgnoreCase) &&
                this.X == other.X &&
                this.Y == other.Y &&
                this.Width == other.Width &&
                this.Height == other.Height;
        }

        public override int GetHashCode()
        {
            int hashFilePath = FilePath == null ? 0 : FilePath.GetHashCode();
            int hashX = X.GetHashCode();
            int hashY = Y.GetHashCode();
            int hashWidth = Width.GetHashCode();
            int hashHeight = Height.GetHashCode();

            return hashFilePath ^ hashX ^ hashY ^ hashWidth ^ hashHeight;
        }
    }
}