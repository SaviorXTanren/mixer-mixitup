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
        private const string MainDivElementID = "maindiv";

        [DataMember]
        [Obsolete]
        public string OverlayName { get; set; }

        [DataMember]
        [Obsolete]
        public OverlayItemModelBase OverlayItem { get; set; }

        [DataMember]
        public Guid OverlayEndpointID { get; set; }
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
        public OverlayActionModel(Guid overlayEndpointID, OverlayItemModelBase overlayItem)
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

            this.OverlayEndpointID = overlayEndpointID;
            this.OverlayItem = overlayItem;
        }

        public OverlayActionModel(Guid overlayEndpointID, OverlayItemV3ModelBase overlayItem, string duration, OverlayAnimationV3Model entranceAnimation, OverlayAnimationV3Model exitAnimation)
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

            this.OverlayEndpointID = overlayEndpointID;
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
                OverlayEndpointV3Service overlay = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.OverlayEndpointID);
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
                        }
                    }

                    Dictionary<string, string> properties = this.OverlayItemV3.GetGenerationProperties();

                    string javascript = this.OverlayItemV3.Javascript;
                    double.TryParse(await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.Duration, parameters), out double duration);
                    if (duration > 0.0)
                    {
                        string removeJavascript = OverlayV3Service.ReplaceProperty(OverlayResources.OverlayItemHideAndSendParentMessageRemoveJavascript, nameof(this.OverlayItemV3.ID), properties[nameof(this.OverlayItemV3.ID)]);
                        string exitJavascript = this.ExitAnimation.GenerateAnimationJavascript(MainDivElementID, preTimeout: duration, postAnimation: removeJavascript);
                        string entranceAndExitJavascript = this.EntranceAnimation.GenerateAnimationJavascript(MainDivElementID, postAnimation: exitJavascript);
                        javascript = javascript + "\n\n" + entranceAndExitJavascript;
                    }
                    else
                    {
                        if (this.OverlayItemV3 is OverlayVideoV3Model)
                        {
                            // TODO
                            return;
                        }
                        else
                        {
                            // Invalid item to have 0 duration on
                            return;
                        }
                    }

                    string iframeHTML = overlay.GetItemIFrameHTML();
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.HTML), this.OverlayItemV3.HTML);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.CSS), this.OverlayItemV3.CSS);
                    iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(this.OverlayItemV3.Javascript), javascript);

                    await this.OverlayItemV3.ProcessGenerationProperties(properties, parameters);
                    foreach (var property in properties)
                    {
                        iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, property.Key, property.Value);
                    }

                    iframeHTML = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(iframeHTML, parameters);

                    await overlay.Add(properties[nameof(this.OverlayItemV3.ID)], iframeHTML);
                }
            }
        }
    }
}
