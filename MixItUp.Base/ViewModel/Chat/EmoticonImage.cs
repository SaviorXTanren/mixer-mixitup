using System;

namespace MixItUp.Base.ViewModel.Chat
{
    public class EmoticonImage : IEquatable<EmoticonImage>
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }

        public bool Equals(EmoticonImage other)
        {
            return this.Uri.Equals(other.Uri, StringComparison.InvariantCultureIgnoreCase) &&
                this.X == other.X &&
                this.Y == other.Y &&
                this.Width == other.Width &&
                this.Height == other.Height;
        }

        public override int GetHashCode()
        {
            int hashFilePath = Uri == null ? 0 : Uri.GetHashCode();
            int hashX = X.GetHashCode();
            int hashY = Y.GetHashCode();
            int hashWidth = Width.GetHashCode();
            int hashHeight = Height.GetHashCode();

            return hashFilePath ^ hashX ^ hashY ^ hashWidth ^ hashHeight;
        }
    }
}