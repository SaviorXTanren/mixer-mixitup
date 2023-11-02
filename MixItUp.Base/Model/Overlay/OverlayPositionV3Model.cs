using MixItUp.Base.Services;
using MixItUp.Base.Util;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayPositionV3Type
    {
        Simple,
        Percentage,
        Pixel,
        Random
    }

    public class OverlayPositionV3Model
    {
        public static int zIndexCounter = 0;

        public OverlayPositionV3Type Type { get; set; }
        
        public int XPosition { get; set; }
        public int YPosition { get; set; }

        public int Layer { get; set; }

        public virtual void SetPosition(OverlayOutputV3Model result) { this.SetPositionInternal(result, this.XPosition, this.YPosition); }

        protected void SetPositionInternal(OverlayOutputV3Model result, int x, int y)
        {
            result.HTML = OverlayV3Service.ReplaceProperty(result.HTML, nameof(this.XPosition), x.ToString());
            result.CSS = OverlayV3Service.ReplaceProperty(result.CSS, nameof(this.XPosition), x.ToString());
            result.Javascript = OverlayV3Service.ReplaceProperty(result.Javascript, nameof(this.XPosition), x.ToString());

            result.HTML = OverlayV3Service.ReplaceProperty(result.HTML, nameof(this.YPosition), y.ToString());
            result.CSS = OverlayV3Service.ReplaceProperty(result.CSS, nameof(this.YPosition), y.ToString());
            result.Javascript = OverlayV3Service.ReplaceProperty(result.Javascript, nameof(this.YPosition), y.ToString());

            bool positionTypeIsPercentage = this.Type == OverlayPositionV3Type.Simple || this.Type == OverlayPositionV3Type.Percentage;
            result.HTML = OverlayV3Service.ReplaceProperty(result.HTML, "PositionType", positionTypeIsPercentage ? "%" : "px");
            result.CSS = OverlayV3Service.ReplaceProperty(result.CSS, "PositionType", positionTypeIsPercentage ? "%" : "px");
            result.Javascript = OverlayV3Service.ReplaceProperty(result.Javascript, "PositionType", positionTypeIsPercentage ? "%" : "px");

            if (this.Layer == 0)
            {
                zIndexCounter++;
                result.HTML = OverlayV3Service.ReplaceProperty(result.HTML, nameof(this.Layer), zIndexCounter.ToString());
                result.CSS = OverlayV3Service.ReplaceProperty(result.CSS, nameof(this.Layer), zIndexCounter.ToString());
                result.Javascript = OverlayV3Service.ReplaceProperty(result.Javascript, nameof(this.Layer), zIndexCounter.ToString());
            }
            else
            {
                result.HTML = OverlayV3Service.ReplaceProperty(result.HTML, nameof(this.Layer), this.Layer.ToString());
                result.CSS = OverlayV3Service.ReplaceProperty(result.CSS, nameof(this.Layer), this.Layer.ToString());
                result.Javascript = OverlayV3Service.ReplaceProperty(result.Javascript, nameof(this.Layer), this.Layer.ToString());
            }
        }
    }

    public class OverlayRandomPositionV3Model : OverlayPositionV3Model
    {
        public int XMinimum
        {
            get { return this.XPosition; }
            set { this.XPosition = value; }
        }

        public int YMinimum
        {
            get { return this.YPosition; }
            set { this.YPosition = value; }
        }

        public int XMaximum { get; set; }
        public int YMaximum { get; set; }

        public override void SetPosition(OverlayOutputV3Model result)
        {
            int x = RandomHelper.GenerateRandomNumber(this.XMinimum, this.XMaximum);
            int y = RandomHelper.GenerateRandomNumber(this.YMinimum, this.YMaximum);
            this.SetPositionInternal(result, x, y);
        }
    }
}
