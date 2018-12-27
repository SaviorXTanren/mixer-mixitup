using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayEffectPositionType
    {
        Percentage,
        Pixel,
    }

    public class OverlayItemPosition
    {
        [DataMember]
        public OverlayEffectPositionType PositionType;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        [DataMember]
        public bool IsPercentagePosition { get { return this.PositionType == OverlayEffectPositionType.Percentage; } }
        [DataMember]
        public bool IsPixelPosition { get { return this.PositionType == OverlayEffectPositionType.Pixel; } }

        public OverlayItemPosition() { }

        public OverlayItemPosition(OverlayEffectPositionType positionType, int horizontal, int vertical)
        {
            this.PositionType = positionType;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }
    }
}
