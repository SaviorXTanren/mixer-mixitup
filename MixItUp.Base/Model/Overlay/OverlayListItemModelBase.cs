using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayListIndividualItemModel
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public UserViewModel User { get; set; }
        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public bool Add { get; set; }
        [DataMember]
        public bool Remove { get; set; }

        [DataMember]
        public Dictionary<string, string> TemplateReplacements { get; set; } = new Dictionary<string, string>();
        [DataMember]
        public string HTML { get; set; }

        [DataMember]
        public string Hash { get; set; } = string.Empty;

        [DataMember]
        public int FadeOut { get; set; }

        public OverlayListIndividualItemModel() { }

        public static OverlayListIndividualItemModel CreateAddItem(string id, UserViewModel user, int position, string html)
        {
            return new OverlayListIndividualItemModel()
            {
                ID = id,
                User = user,
                Position = position,
                HTML = html,
                Add = true
            };
        }

        public static OverlayListIndividualItemModel CreateRemoveItem(string id)
        {
            return new OverlayListIndividualItemModel()
            {
                ID = id,
                Remove = true
            };
        }
    }

    [DataContract]
    public class OverlayListItemModelBase : OverlayHTMLTemplateItemModelBase
    {
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
        public List<OverlayListIndividualItemModel> Items = new List<OverlayListIndividualItemModel>();

        protected SemaphoreSlim listSemaphore = new SemaphoreSlim(1);

        public OverlayListItemModelBase() : base() { }

        public OverlayListItemModelBase(OverlayItemModelTypeEnum type, string htmlText, int totalToShow, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(type, htmlText)
        {
            this.TotalToShow = totalToShow;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.Effects = new OverlayItemEffectsModel(addEventAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, removeEventAnimation, 0);
        }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override async Task Disable()
        {
            this.Items.Clear();
            await base.Disable();
        }

        public override async Task<JObject> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            JObject jobj = await base.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
            this.Items.Clear();
            return jobj;
        }

        protected override async Task PerformReplacements(JObject jobj, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (jobj != null)
            {
                if (jobj.ContainsKey("Items"))
                {
                    JArray jarray = (JArray)jobj["Items"];
                    for (int i = 0; i < jarray.Count; i++)
                    {
                        JObject itemJObj = (JObject)jarray[i];

                        itemJObj["HTML"] = jobj["HTML"];

                        itemJObj["HTML"] = this.PerformTemplateReplacements(itemJObj["HTML"].ToString(), this.Items[i].TemplateReplacements);

                        await base.PerformReplacements(itemJObj, user, arguments, extraSpecialIdentifiers);
                    }
                }
                await base.PerformReplacements(jobj, user, arguments, extraSpecialIdentifiers);
            }
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            Dictionary<string, string> replacementSets = await base.GetTemplateReplacements(user, arguments, extraSpecialIdentifiers);

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_HEIGHT"] = ((int)(0.4 * ((double)this.Height))).ToString();

            return replacementSets;
        }
    }
}
