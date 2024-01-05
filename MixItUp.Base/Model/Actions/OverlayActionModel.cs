using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
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

                    Dictionary<string, string> properties = this.OverlayItemV3.GetGenerationProperties();

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
                        exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID, preTimeoutSeconds: duration, postAnimation: removeJavascript);
                    }
                    else
                    {
                        if (this.OverlayItemV3.Type == OverlayItemV3Type.Video)
                        {
                            exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID, postAnimation: removeJavascript);
                            exitJavascript = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayVideoNoDurationJavascript, OverlayActionModel.PostEventReplacementText, exitJavascript);
                        }
                        else if (this.OverlayItemV3.Type == OverlayItemV3Type.YouTube)
                        {
                            exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID, postAnimation: OverlayResources.OverlayYouTubeIFrameDestroyJavascript + "\n" + removeJavascript);
                            javascript = OverlayV3Service.ReplaceProperty(javascript, OverlayActionModel.PostEventReplacementText, exitJavascript);
                            exitJavascript = string.Empty;
                        }
                        else if (this.OverlayItemV3.Type == OverlayItemV3Type.TwitchClip)
                        {
                            exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID, postAnimation: removeJavascript);
                            exitJavascript = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayVideoNoDurationJavascript, OverlayActionModel.PostEventReplacementText, exitJavascript);

                            // For use with Embed-based clip displaying
                            //OverlayTwitchClipV3Model overlayTwitchClipItemV3 = (OverlayTwitchClipV3Model)this.OverlayItemV3;
                            //exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(MainDivElementID, preTimeoutSeconds: overlayTwitchClipItemV3.ClipDuration, postAnimation: removeJavascript);
                            //javascript = javascript + "\n\n" + exitJavascript + "\n\n";
                            //exitJavascript = string.Empty;
                        }
                    }

                    string entranceAndExitJavascript = this.EntranceAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID, postAnimation: exitJavascript);
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

                    // Replace any lingering {PostEvent} properties
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, OverlayActionModel.PostEventReplacementText, string.Empty);

                    iframeHTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(iframeHTML, parameters);

                    await overlay.Add(properties[nameof(this.OverlayItemV3.ID)], iframeHTML);
                }
            }
        }
    }
}
