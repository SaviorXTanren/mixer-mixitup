using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Clips;
using static System.Net.Mime.MediaTypeNames;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class OverlayActionModel : ActionModelBase
    {
        private const string PostEventReplacementText = "PostEvent";

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
        public Guid EndCreditsID { get; set; }
        [DataMember]
        public Guid EndCreditsSectionID { get; set; }
        [DataMember]
        public string EndCreditsItemText { get; set; }

        [Obsolete]
        public OverlayActionModel(OverlayItemModelBase overlayItem)
            : base(ActionTypeEnum.Overlay)
        {
            //var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
            //if (overlays.Contains(overlayName))
            //{
            //    this.OverlayName = overlayName;
            //}
            //else
            //{
            //    this.OverlayName = ServiceManager.Get<OverlayService>().DefaultOverlayName;
            //}

            this.OverlayItem = overlayItem;
        }

        public OverlayActionModel(OverlayItemV3ModelBase overlayItem, string duration, OverlayAnimationV3Model entranceAnimation, OverlayAnimationV3Model exitAnimation)
            : base(ActionTypeEnum.Overlay)
        {
            //var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
            //if (overlays.Contains(overlayName))
            //{
            //    this.OverlayName = overlayName;
            //}
            //else
            //{
            //    this.OverlayName = ServiceManager.Get<OverlayService>().DefaultOverlayName;
            //}

            this.OverlayItemV3 = overlayItem;
            this.Duration = duration;
            this.EntranceAnimation = entranceAnimation;
            this.ExitAnimation = exitAnimation;
        }

        public OverlayActionModel(Guid widgetID, bool showWidget)
            : base(ActionTypeEnum.Overlay)
        {
            this.WidgetID = widgetID;
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

        [Obsolete]
        public OverlayActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.WidgetID != Guid.Empty)
            {
#pragma warning disable CS0612 // Type or member is obsolete
                OverlayWidgetModel widget = ChannelSession.Settings.OverlayWidgets.FirstOrDefault(w => w.Item.ID.Equals(this.WidgetID));
                if (widget != null)
                {
                    if (this.ShowWidget)
                    {
                        await widget.Enable(parameters);
                    }
                    else
                    {
                        await widget.Disable();
                    }
                }
#pragma warning restore CS0612 // Type or member is obsolete
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
                    if (double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.TimeAmount, parameters), out double amount))
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
            else
            {
                OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayItemV3.OverlayEndpointID);
                if (overlay != null)
                {
                    if (this.OverlayItemV3 is OverlayTwitchClipV3Model)
                    {
                        OverlayTwitchClipV3Model overlayTwitchClipItemV3 = (OverlayTwitchClipV3Model)this.OverlayItemV3;

                        if (ServiceManager.Get<TwitchSessionService>().IsConnected)
                        {
                            string clipReferenceID = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(overlayTwitchClipItemV3.ClipReferenceID, parameters);

                            Twitch.Base.Models.NewAPI.Users.UserModel twitchUser = ServiceManager.Get<TwitchSessionService>().User;
                            if (!string.IsNullOrEmpty(clipReferenceID))
                            {
                                if (overlayTwitchClipItemV3.ClipType == OverlayTwitchClipV3ClipType.RandomClip || overlayTwitchClipItemV3.ClipType == OverlayTwitchClipV3ClipType.LatestClip)
                                {
                                    twitchUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByLogin(clipReferenceID);
                                    if (twitchUser == null)
                                    {
                                        // No valid user found, fail out and send an error message
                                        await ServiceManager.Get<ChatService>().SendMessage(Resources.OverlayTwitchClipErrorUnableToFindValidClip, StreamingPlatformTypeEnum.Twitch);
                                        return;
                                    }
                                }
                            }

                            ClipModel clip = null;
                            if (overlayTwitchClipItemV3.ClipType == OverlayTwitchClipV3ClipType.RandomClip)
                            {
                                DateTimeOffset startDate = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(30 + RandomHelper.GenerateRandomNumber(365)));
                                IEnumerable<ClipModel> clips = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClips(twitchUser, startDate: startDate, endDate: DateTimeOffset.Now, maxResults: 100);
                                if (clips != null && clips.Count() > 0)
                                {
                                    clip = clips.Random();
                                }
                                else
                                {
                                    clips = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClips(twitchUser, maxResults: 100);
                                    if (clips != null && clips.Count() > 0)
                                    {
                                        clip = clips.Random();
                                    }
                                }
                            }
                            else if (overlayTwitchClipItemV3.ClipType == OverlayTwitchClipV3ClipType.LatestClip)
                            {
                                DateTimeOffset startDate = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(30));
                                IEnumerable<ClipModel> clips = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClips(twitchUser, startDate: startDate, endDate: DateTimeOffset.Now, maxResults: int.MaxValue);
                                if (clips != null && clips.Count() > 0)
                                {
                                    clip = clips.OrderByDescending(c => c.created_at).First();
                                }
                            }
                            else if (overlayTwitchClipItemV3.ClipType == OverlayTwitchClipV3ClipType.SpecificClip && !string.IsNullOrEmpty(clipReferenceID))
                            {
                                clip = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClip(clipReferenceID);
                            }

                            if (clip == null)
                            {
                                // No valid clip found, fail out and send an error message
                                await ServiceManager.Get<ChatService>().SendMessage(Resources.OverlayTwitchClipErrorUnableToFindValidClip, StreamingPlatformTypeEnum.Twitch);
                                return;
                            }

                            overlayTwitchClipItemV3.ClipID = clip.id;
                            overlayTwitchClipItemV3.ClipDuration = clip.duration;
                            overlayTwitchClipItemV3.SetDirectLinkFromThumbnailURL(clip.thumbnail_url);
                        }
                    }

                    Dictionary<string, object> properties = this.OverlayItemV3.GetGenerationProperties();

                    double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Duration, parameters), out double duration);
                    if (duration <= 0.0)
                    {
                        if (this.OverlayItemV3.Type != OverlayItemV3Type.Video && this.OverlayItemV3.Type != OverlayItemV3Type.YouTube &&
                            this.OverlayItemV3.Type != OverlayItemV3Type.TwitchClip)
                        {
                            // Invalid item to have 0 duration on
                            return;
                        }
                    }

                    string javascript = this.OverlayItemV3.Javascript;
                    string removeJavascript = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayItemHideAndSendParentMessageRemoveJavascript, nameof(this.OverlayItemV3.ID), properties[nameof(this.OverlayItemV3.ID)]);

                    string exitJavascript = string.Empty;
                    if (duration > 0.0)
                    {
                        exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement, preTimeoutSeconds: duration, postAnimation: removeJavascript);
                    }
                    else
                    {
                        if (this.OverlayItemV3.Type == OverlayItemV3Type.Video)
                        {
                            exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement, postAnimation: removeJavascript);
                            exitJavascript = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayVideoNoDurationJavascript, OverlayActionModel.PostEventReplacementText, exitJavascript);
                        }
                        else if (this.OverlayItemV3.Type == OverlayItemV3Type.YouTube)
                        {
                            exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement, postAnimation: OverlayResources.OverlayYouTubeIFrameDestroyJavascript + "\n" + removeJavascript);
                            javascript = OverlayV3Service.ReplaceProperty(javascript, OverlayActionModel.PostEventReplacementText, exitJavascript);
                            exitJavascript = string.Empty;
                        }
                        else if (this.OverlayItemV3.Type == OverlayItemV3Type.TwitchClip)
                        {
                            exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement, postAnimation: removeJavascript);
                            exitJavascript = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayVideoNoDurationJavascript, OverlayActionModel.PostEventReplacementText, exitJavascript);

                            // For use with Embed-based clip displaying
                            //OverlayTwitchClipV3Model overlayTwitchClipItemV3 = (OverlayTwitchClipV3Model)this.OverlayItemV3;
                            //exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(MainDivElementID, preTimeoutSeconds: overlayTwitchClipItemV3.ClipDuration, postAnimation: removeJavascript);
                            //javascript = javascript + "\n\n" + exitJavascript + "\n\n";
                            //exitJavascript = string.Empty;
                        }
                    }

                    string entranceAndExitJavascript = this.EntranceAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElement, postAnimation: exitJavascript);
                    javascript = javascript + "\n\n" + entranceAndExitJavascript;

                    string iframeHTML = overlay.GetItemIFrameHTML();
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.HTML), this.OverlayItemV3.HTML);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.CSS), this.OverlayItemV3.CSS);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.Javascript), javascript);

                    await this.OverlayItemV3.ProcessGenerationProperties(properties, parameters);
                    foreach (var property in properties)
                    {
                        iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, property.Key, property.Value);
                    }
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.Duration), duration);

                    // Replace any lingering {PostEvent} properties
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, OverlayActionModel.PostEventReplacementText, string.Empty);

                    iframeHTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(iframeHTML, parameters);

                    await overlay.Add(properties[nameof(this.OverlayItemV3.ID)].ToString(), iframeHTML);
                }
            }
        }
    }
}
