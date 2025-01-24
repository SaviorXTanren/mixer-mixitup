using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public enum OverlayListItemAlignmentTypeEnum
    {
        Top,
        Center,
        Bottom,


        None = 100,
    }

    [Obsolete]
    [DataContract]
    public class OverlayListIndividualItemModel
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public Guid UserID { get; set; }
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

        [JsonIgnore]
        private UserV2ViewModel cachedUser = null;

        public OverlayListIndividualItemModel() { }

        public Task<UserV2ViewModel> GetUser()
        {
            if (this.cachedUser == null && this.UserID != Guid.Empty)
            {
                //this.cachedUser = ServiceManager.Get<UserService>().GetActiveUserByID(this.UserID);
                //if (this.cachedUser == null)
                //{
                    //UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByID(this.UserID);
                    //if (user != null)
                    //{
                    //    this.cachedUser = user;
                    //}
                //}
            }
            return Task.FromResult(this.cachedUser);
        }

        public static OverlayListIndividualItemModel CreateAddItem(string id, UserV2ViewModel user, int position, string html)
        {
            return new OverlayListIndividualItemModel()
            {
                ID = id,
                UserID = (user != null) ? user.ID : Guid.Empty,
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

    [Obsolete]
    [DataContract]
    public class OverlayListItemModelBase : OverlayHTMLTemplateItemModelBase
    {
        [DataMember]
        public int TotalToShow { get; set; }
        [DataMember]
        public int FadeOut { get; set; }

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
        public OverlayListItemAlignmentTypeEnum Alignment { get; set; }

        [DataMember]
        public virtual bool ForceTopAlign { get { return this.Alignment == OverlayListItemAlignmentTypeEnum.Top; } }
        [DataMember]
        public virtual bool ForceCenterAlign { get { return this.Alignment == OverlayListItemAlignmentTypeEnum.Center; } }
        [DataMember]
        public virtual bool ForceBottomAlign { get { return this.Alignment == OverlayListItemAlignmentTypeEnum.Bottom; } }

        [DataMember]
        public List<OverlayListIndividualItemModel> Items = new List<OverlayListIndividualItemModel>();
        [DataMember]
        private List<OverlayListIndividualItemModel> cachedItems = new List<OverlayListIndividualItemModel>();

        protected SemaphoreSlim listSemaphore = new SemaphoreSlim(1);

        public OverlayListItemModelBase()
            : base()
        {
            this.Alignment = OverlayListItemAlignmentTypeEnum.Top;
        }

        public OverlayListItemModelBase(OverlayItemModelTypeEnum type, string htmlText, int totalToShow, int fadeOut, string textFont, int width, int height, string borderColor,
            string backgroundColor, string textColor, OverlayListItemAlignmentTypeEnum alignment, OverlayItemEffectEntranceAnimationTypeEnum addEventAnimation, OverlayItemEffectExitAnimationTypeEnum removeEventAnimation)
            : base(type, htmlText)
        {
            this.TotalToShow = totalToShow;
            this.FadeOut = fadeOut;
            this.TextFont = textFont;
            this.Width = width;
            this.Height = height;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.TextColor = textColor;
            this.Alignment = alignment;
            this.Effects = new OverlayItemEffectsModel(addEventAnimation, OverlayItemEffectVisibleAnimationTypeEnum.None, removeEventAnimation, 0);
        }

        [JsonIgnore]
        public override bool SupportsTestData { get { return true; } }

        public override async Task Disable()
        {
            this.Items.Clear();
            this.cachedItems.Clear();
            await base.Disable();
        }

        public override async Task<JObject> GetProcessedItem(CommandParametersModel parameters)
        {
            JObject jobj = await base.GetProcessedItem(parameters);
            this.cachedItems.AddRange(this.Items);
            while (this.cachedItems.Count > this.TotalToShow)
            {
                this.cachedItems.RemoveAt(0);
            }
            this.Items.Clear();
            return jobj;
        }

        public override Task LoadCachedData()
        {
            this.Items.Clear();
            this.Items.AddRange(this.cachedItems);
            this.cachedItems.Clear();
            return Task.CompletedTask;
        }

        protected override async Task PerformReplacements(JObject jobj, CommandParametersModel parameters)
        {
            if (jobj != null)
            {
                if (jobj.ContainsKey("Items"))
                {
                    JArray jarray = (JArray)jobj["Items"];
                    for (int i = 0; i < jarray.Count && i < this.Items.Count; i++)
                    {
                        JObject itemJObj = (JObject)jarray[i];

                        itemJObj["HTML"] = jobj["HTML"];
                        itemJObj["HTML"] = this.PerformTemplateReplacements(itemJObj["HTML"].ToString(), this.Items[i].TemplateReplacements);

                        await base.PerformReplacements(itemJObj, parameters);
                    }
                }
                await base.PerformReplacements(jobj, parameters);
            }
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(CommandParametersModel parameters)
        {
            Dictionary<string, string> replacementSets = await base.GetTemplateReplacements(parameters);

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
