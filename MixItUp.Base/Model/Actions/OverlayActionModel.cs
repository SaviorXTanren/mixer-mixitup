using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class OverlayActionModel : ActionModelBase
    {
        [DataMember]
        [Obsolete]
        public string OverlayName { get; set; }

        [DataMember]
        [Obsolete]
        public OverlayItemModelBase OverlayItem { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase OverlayItemV3 { get; set; }
        [DataMember]
        public string Duration { get; set; }
        [DataMember]
        public OverlayAnimationV3Model EntranceAnimation { get; set; }
        [DataMember]
        public OverlayAnimationV3Model ExitAnimation { get; set; }
        [DataMember]
        public List<OverlayAnimationV3Model> CustomAnimations { get; set; } = new List<OverlayAnimationV3Model>();

        [DataMember]
        public Guid WidgetID { get; set; }
        [DataMember]
        public bool ShowWidget { get; set; }

        [DataMember]
        public Guid StreamBossID { get; set; }
        [DataMember]
        public string StreamBossDamageAmount { get; set; }
        [DataMember]
        public bool StreamBossForceDamage { get; set; }

        [DataMember]
        public Guid GoalID { get; set; }
        [DataMember]
        public string GoalAmount { get; set; }

        [DataMember]
        public Guid PersistentTimerID { get; set; }
        [DataMember]
        public string TimeAmount { get; set; }
        [DataMember]
        public bool? PauseUnpausePersistentTimer { get; set; }

        [DataMember]
        public Guid EndCreditsID { get; set; }
        [DataMember]
        public Guid EndCreditsSectionID { get; set; }
        [DataMember]
        public string EndCreditsItemText { get; set; }

        [DataMember]
        public Guid EventListID { get; set; }
        [DataMember]
        public string EventListDetails { get; set; }

        [DataMember]
        public Guid WheelID { get; set; }

        [DataMember]
        public Guid RunWidgetFunctionID { get; set; }
        [DataMember]
        public string RunWidgetFunctionName { get; set; }
        [DataMember]
        public Dictionary<string, object> RunWidgetFunctionParameters { get; set; } = new Dictionary<string, object>();

        public OverlayActionModel(OverlayItemV3ModelBase overlayItem, string duration, OverlayAnimationV3Model entranceAnimation, OverlayAnimationV3Model exitAnimation, IEnumerable<OverlayAnimationV3Model> customAnimations)
            : base(ActionTypeEnum.Overlay)
        {
            this.OverlayItemV3 = overlayItem;
            this.Duration = duration;
            this.EntranceAnimation = entranceAnimation;
            this.ExitAnimation = exitAnimation;
            this.CustomAnimations = new List<OverlayAnimationV3Model>(customAnimations);
        }

        public OverlayActionModel(OverlayWidgetV3Model widget, bool showWidget)
            : base(ActionTypeEnum.Overlay)
        {
            this.WidgetID = widget.ID;
            this.ShowWidget = showWidget;
        }

        public OverlayActionModel(OverlayStreamBossV3Model streamBoss, string damageAmount, bool streamBossForceDamage)
            : base(ActionTypeEnum.Overlay)
        {
            this.StreamBossID = streamBoss.ID;
            this.StreamBossDamageAmount = damageAmount;
            this.StreamBossForceDamage = streamBossForceDamage;
        }

        public OverlayActionModel(OverlayGoalV3Model goal, string goalAmount)
            : base(ActionTypeEnum.Overlay)
        {
            this.GoalID = goal.ID;
            this.GoalAmount = goalAmount;
        }

        public OverlayActionModel(OverlayPersistentTimerV3Model timer, string timeAmount)
            : base(ActionTypeEnum.Overlay)
        {
            this.PersistentTimerID = timer.ID;
            this.TimeAmount = timeAmount;
        }

        public OverlayActionModel(OverlayPersistentTimerV3Model timer, bool pauseUnpausePersistentTimer)
            : base(ActionTypeEnum.Overlay)
        {
            this.PersistentTimerID = timer.ID;
            this.PauseUnpausePersistentTimer = pauseUnpausePersistentTimer;
        }

        public OverlayActionModel(OverlayEndCreditsV3Model endCredits)
            : base(ActionTypeEnum.Overlay)
        {
            this.EndCreditsID = endCredits.ID;
        }

        public OverlayActionModel(OverlayEndCreditsV3Model endCredits, OverlayEndCreditsSectionV3Model endCreditsSection, string itemText)
            : base(ActionTypeEnum.Overlay)
        {
            this.EndCreditsID = endCredits.ID;
            this.EndCreditsSectionID = endCreditsSection.ID;
            this.EndCreditsItemText = itemText;
        }

        public OverlayActionModel(OverlayEventListV3Model eventList, string details)
            : base(ActionTypeEnum.Overlay)
        {
            this.EventListID = eventList.ID;
            this.EventListDetails = details;
        }

        public OverlayActionModel(OverlayWheelV3Model wheel)
            : base(ActionTypeEnum.Overlay)
        {
            this.WheelID = wheel.ID;
        }

        public OverlayActionModel(OverlayWidgetV3Model widget, string functionName, Dictionary<string, object> functionParameters)
            : base(ActionTypeEnum.Overlay)
        {
            this.RunWidgetFunctionID = widget.ID;
            this.RunWidgetFunctionName = functionName;
            this.RunWidgetFunctionParameters = functionParameters;
        }

        [Obsolete]
        public OverlayActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (this.OverlayItem != null)
            {
                SettingsV3Upgrader.UpdateActionsV7(ChannelSession.Settings, new List<ActionModelBase>() { this });
            }
#pragma warning restore CS0612 // Type or member is obsolete

            if (this.WidgetID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.Item.ID.Equals(this.WidgetID));
                if (widget != null)
                {
                    if (this.ShowWidget)
                    {
                        await widget.Enable();
                    }
                    else
                    {
                        await widget.Disable();
                    }
                }
            }
            else if (this.StreamBossID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID.Equals(this.StreamBossID));
                if (widget != null && widget.Type == OverlayItemV3Type.StreamBoss)
                {
                    if (double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.StreamBossDamageAmount, parameters), out double damage))
                    {
                        await ((OverlayStreamBossV3Model)widget.Item).ProcessEvent(parameters.User, damage, this.StreamBossForceDamage);
                    }
                }
            }
            else if (this.GoalID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID.Equals(this.GoalID));
                if (widget != null && widget.Type == OverlayItemV3Type.Goal)
                {
                    if (double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.GoalAmount, parameters), out double amount))
                    {
                        await ((OverlayGoalV3Model)widget.Item).ProcessEvent(parameters.User, amount);
                    }
                }
            }
            else if (this.PersistentTimerID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID.Equals(this.PersistentTimerID));
                if (widget != null && widget.Type == OverlayItemV3Type.PersistentTimer)
                {
                    if (this.PauseUnpausePersistentTimer != null)
                    {
                        if (this.PauseUnpausePersistentTimer.GetValueOrDefault())
                        {
                            await ((OverlayPersistentTimerV3Model)widget.Item).Pause();
                        }
                        else
                        {
                            await ((OverlayPersistentTimerV3Model)widget.Item).Unpause();
                        }
                    }
                    else if (double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TimeAmount, parameters), out double amount))
                    {
                        await ((OverlayPersistentTimerV3Model)widget.Item).ProcessEvent(parameters.User, amount);
                    }
                }
            }
            else if (this.EndCreditsID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID.Equals(this.EndCreditsID));
                if (widget != null && widget.Type == OverlayItemV3Type.EndCredits)
                {
                    if (this.EndCreditsSectionID != Guid.Empty)
                    {
                        string text = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.EndCreditsItemText, parameters);
                        if (!string.IsNullOrEmpty(text))
                        {
                            ((OverlayEndCreditsV3Model)widget.Item).CustomTrack(this.EndCreditsSectionID, parameters.User, text);
                        }
                    }
                    else
                    {
                        await ((OverlayEndCreditsV3Model)widget.Item).PlayCredits();
                    }
                }
            }
            else if (this.EventListID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID.Equals(this.EventListID));
                if (widget != null && widget.Type == OverlayItemV3Type.EventList)
                {
                    string details = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.EventListDetails, parameters);
                    await ((OverlayEventListV3Model)widget.Item).AddEvent(parameters.User, "Custom", details, new Dictionary<string, string>());
                }
            }
            else if (this.WheelID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID.Equals(this.WheelID));
                if (widget != null && widget.Type == OverlayItemV3Type.Wheel)
                {
                    await ((OverlayWheelV3Model)widget.Item).Spin(parameters);
                }
            }
            else if (this.RunWidgetFunctionID != Guid.Empty)
            {
                OverlayWidgetV3Model widget = ChannelSession.Settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID.Equals(this.RunWidgetFunctionID));
                if (widget != null)
                {
                    OverlayEndpointV3Service overlay = widget.GetOverlayEndpointService();
                    if (overlay != null)
                    {
                        Dictionary<string, object> functionParameters = new Dictionary<string, object>();
                        foreach (var kvp in this.RunWidgetFunctionParameters)
                        {
                            functionParameters[kvp.Key] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(kvp.Value.ToString(), parameters);
                        }

                        await overlay.Function(widget.ID.ToString(), this.RunWidgetFunctionName, functionParameters);
                    }
                }
            }
            else
            {
                OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayItemV3.OverlayEndpointID);
                if (overlay != null)
                {
                    if (this.OverlayItemV3 is OverlayTwitchClipV3Model)
                    {
                        OverlayTwitchClipV3Model overlayTwitchClipItemV3 = (OverlayTwitchClipV3Model)this.OverlayItemV3;
                        if (!await overlayTwitchClipItemV3.ProcessClip(parameters))
                        {
                            return;
                        }
                    }

                    double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Duration, parameters), NumberStyles.Any, CultureInfo.CurrentCulture, out double duration);
                    if (duration <= 0.0)
                    {
                        if (this.OverlayItemV3.Type != OverlayItemV3Type.Video && this.OverlayItemV3.Type != OverlayItemV3Type.YouTube &&
                            this.OverlayItemV3.Type != OverlayItemV3Type.TwitchClip)
                        {
                            // Invalid item to have 0 duration on
                            return;
                        }
                    }

                    string iframeHTML = overlay.GetItemIFrameHTML();
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.HTML), this.OverlayItemV3.HTML);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.CSS), this.OverlayItemV3.CSS);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.Javascript), this.OverlayItemV3.Javascript);

                    Dictionary<string, object> properties = this.OverlayItemV3.GetGenerationProperties();
                    await this.OverlayItemV3.ProcessGenerationProperties(properties, parameters);
                    foreach (var property in properties)
                    {
                        iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, property.Key, property.Value);
                    }

                    Dictionary<string, object> animationProperties = new Dictionary<string, object>();
                    this.EntranceAnimation.AddAnimationProperties(animationProperties, nameof(this.EntranceAnimation));
                    this.ExitAnimation.AddAnimationProperties(animationProperties, nameof(this.ExitAnimation));

                    foreach (var kvp in animationProperties)
                    {
                        iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, kvp.Key, kvp.Value);
                    }

                    List<string> customAnimations = new List<string>();
                    foreach (OverlayAnimationV3Model animation in this.CustomAnimations)
                    {
                        animationProperties = new Dictionary<string, object>();
                        animation.AddAnimationProperties(animationProperties, "Animation");

                        string animationJavascript = OverlayResources.OverlayCustomAnimationDefaultJavascript;
                        foreach (var kvp in animationProperties)
                        {
                            animationJavascript = OverlayV3Service.ReplaceProperty(animationJavascript, kvp.Key, kvp.Value);
                        }

                        customAnimations.Add(animationJavascript);
                    }
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.CustomAnimations), string.Join(Environment.NewLine + Environment.NewLine, customAnimations));

                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.Duration), duration.ToString(CultureInfo.InvariantCulture));

                    iframeHTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(iframeHTML, parameters);

                    await overlay.Add(properties[nameof(this.OverlayItemV3.ID)].ToString(), iframeHTML, this.OverlayItemV3.Layer);
                }
            }
        }
    }
}
