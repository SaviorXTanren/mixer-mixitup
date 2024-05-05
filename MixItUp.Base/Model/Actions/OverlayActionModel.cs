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

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class OverlayActionModel : ActionModelBase
    {
        public const string EntranceAnimationFrameworkPropertyName = "EntranceAnimationFramework";
        public const string EntranceAnimationNamePropertyName = "EntranceAnimationName";
        public const string ExitAnimationFrameworkPropertyName = "ExitAnimationFramework";
        public const string ExitAnimationNamePropertyName = "ExitAnimationName";

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

        [DataMember]
        public Guid EventListID { get; set; }
        [DataMember]
        public string EventListDetails { get; set; }

        [DataMember]
        public Guid WheelID { get; set; }

        public OverlayActionModel(OverlayItemV3ModelBase overlayItem, string duration, OverlayAnimationV3Model entranceAnimation, OverlayAnimationV3Model exitAnimation)
            : base(ActionTypeEnum.Overlay)
        {
            this.OverlayItemV3 = overlayItem;
            this.Duration = duration;
            this.EntranceAnimation = entranceAnimation;
            this.ExitAnimation = exitAnimation;
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

        [Obsolete]
        public OverlayActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
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

                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, EntranceAnimationFrameworkPropertyName, this.EntranceAnimation.AnimationFramework);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, EntranceAnimationNamePropertyName, this.EntranceAnimation.AnimationName);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, ExitAnimationFrameworkPropertyName, this.ExitAnimation.AnimationFramework);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, ExitAnimationNamePropertyName, this.ExitAnimation.AnimationName);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.Duration), duration);

                    iframeHTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(iframeHTML, parameters);

                    await overlay.Add(properties[nameof(this.OverlayItemV3.ID)].ToString(), iframeHTML, this.OverlayItemV3.LayerProcessed);
                }
            }
        }
    }
}
