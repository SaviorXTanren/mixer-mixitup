using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayGameQueueItem
    {
        [DataMember]
        public string HTMLText { get; set; }
    }

    [DataContract]
    public class OverlayGameQueue : OverlayCustomHTMLItem
    {
        public const string HTMLTemplate =
    @"<div style=""position: relative; border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px"">
  <p style=""position: absolute; top: 50%; left: 5%; float: left; text-align: left; font-family: '{TEXT_FONT}'; font-size: {TEXT_HEIGHT}px; color: {TEXT_COLOR}; white-space: nowrap; font-weight: bold; margin: auto; transform: translate(0, -50%);"">#{POSITION} {USERNAME}</p>
</div>";

        private const string GameQueueUserPositionSpecialIdentifier = "gamequeueuserposition";

        [DataMember]
        public override string ItemType { get { return "gamequeue"; } }

        [DataMember]
        public int TotalToShow { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }

        [DataMember]
        public string TextFont { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public OverlayEffectEntranceAnimationTypeEnum AddEventAnimation { get; set; }
        [DataMember]
        public string AddEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.AddEventAnimation); } set { } }
        [DataMember]
        public OverlayEffectExitAnimationTypeEnum RemoveEventAnimation { get; set; }
        [DataMember]
        public string RemoveEventAnimationName { get { return OverlayItemEffects.GetAnimationClassName(this.RemoveEventAnimation); } set { } }

        [DataMember]
        public List<OverlayGameQueueItem> GameQueueUpdates = new List<OverlayGameQueueItem>();

        [JsonIgnore]
        private bool gameQueueUpdated = true;

        public OverlayGameQueue() { }

        public OverlayGameQueue(string htmlText, int totalToShow, string textFont, int width, int height, string borderColor, string backgroundColor, string textColor,
            OverlayEffectEntranceAnimationTypeEnum addEventAnimation, OverlayEffectExitAnimationTypeEnum removeEventAnimation)
            : base(htmlText)
        {
            this.TotalToShow = totalToShow;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.AddEventAnimation = addEventAnimation;
            this.RemoveEventAnimation = removeEventAnimation;
        }

        public override async Task Initialize()
        {
            GlobalEvents.OnGameQueueUpdated += GlobalEvents_OnGameQueueUpdated;

            await base.Initialize();
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (this.gameQueueUpdated)
            {
                this.gameQueueUpdated = false;

                List<UserViewModel> users = ChannelSession.GameQueue.ToList();

                this.GameQueueUpdates.Clear();
                OverlayGameQueue copy = this.Copy<OverlayGameQueue>();
                for (int i = 0; i < users.Count && i < this.TotalToShow; i++)
                {
                    extraSpecialIdentifiers[GameQueueUserPositionSpecialIdentifier] = (i + 1).ToString();
                    OverlayCustomHTMLItem overlayItem = (OverlayCustomHTMLItem)await base.GetProcessedItem(users[i], arguments, extraSpecialIdentifiers);
                    copy.GameQueueUpdates.Add(new OverlayGameQueueItem() { HTMLText = overlayItem.HTMLText });
                }
                return copy;
            }
            return null;
        }

        public override OverlayCustomHTMLItem GetCopy() { return this.Copy<OverlayEventList>(); }

        protected override Task<Dictionary<string, string>> GetReplacementSets(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();

            replacementSets["USERNAME"] = user.UserName;
            replacementSets["POSITION"] = extraSpecialIdentifiers[GameQueueUserPositionSpecialIdentifier];

            return Task.FromResult(replacementSets);
        }

        private void GlobalEvents_OnGameQueueUpdated(object sender, System.EventArgs e) { this.gameQueueUpdated = true; }
    }
}
