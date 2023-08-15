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
        [DataMember]
        [Obsolete]
        public string OverlayName { get; set; }
        [DataMember]
        public Guid OverlayEndpointID { get; set; }

        [DataMember]
        [Obsolete]
        public OverlayItemModelBase OverlayItem { get; set; }

        [DataMember]
        public OverlayItemV3ModelBase OverlayItemV3 { get; set; }

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

        public OverlayActionModel(Guid overlayEndpointID, OverlayItemV3ModelBase overlayItem)
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

                            overlayTwitchClipItemV3.TempClipID = clip.id;
                            overlayTwitchClipItemV3.TempClipDuration = clip.duration;
                        }
                    }

                    await overlay.SendBasic(this.OverlayItemV3, parameters);
                }
            }
        }
    }
}
