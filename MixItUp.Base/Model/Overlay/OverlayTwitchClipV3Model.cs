using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Clips;
using MixItUp.Base.Model.Commands;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayTwitchClipV3ClipType
    {
        LatestClip,
        RandomClip,
        SpecificClip,
        LatestFeaturedClip,
        RandomFeaturedClip,
    }

    [DataContract]
    public class OverlayTwitchClipV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayTwitchClipVideoDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTwitchClipVideoDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript + "\n\n" + OverlayResources.OverlayVideoActionDefaultJavascript;

        public const string TwitchClipURLPrefix = "https://clips.twitch.tv/";

        [DataMember]
        public OverlayTwitchClipV3ClipType ClipType { get; set; }

        [DataMember]
        public string ClipReferenceID { get; set; }

        [DataMember]
        public double Volume { get; set; }

        [JsonIgnore]
        public string ClipID { get; set; }
        [JsonIgnore]
        public float ClipDuration { get; set; }
        [JsonIgnore]
        public string ClipDirectLink { get; set; }

        [JsonIgnore]
        public string ClipHeight { get { return (this.Height > 0) ? $"{this.Height}px" : "100%"; } }
        [JsonIgnore]
        public string ClipWidth { get { return (this.Width > 0) ? $"{this.Width}px" : "100%"; } }

        public OverlayTwitchClipV3Model() : base(OverlayItemV3Type.TwitchClip) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.ClipID)] = this.ClipID;
            properties[nameof(this.ClipDirectLink)] = this.ClipDirectLink;
            properties[nameof(this.Volume)] = this.Volume.ToInvariantNumberString();
            properties[nameof(this.ClipHeight)] = this.ClipHeight;
            properties[nameof(this.ClipWidth)] = this.ClipWidth;
            return properties;
        }

        public async Task<bool> ProcessClip(CommandParametersModel parameters)
        {
            this.ClipID = null;
            this.ClipDuration = 0;
            this.ClipDirectLink = null;

            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                string clipReferenceID = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.ClipReferenceID, parameters);

                Twitch.Base.Models.NewAPI.Users.UserModel twitchUser = ServiceManager.Get<TwitchSessionService>().User;
                if (!string.IsNullOrEmpty(clipReferenceID))
                {
                    if (this.ClipType == OverlayTwitchClipV3ClipType.RandomClip || this.ClipType == OverlayTwitchClipV3ClipType.LatestClip ||
                        this.ClipType == OverlayTwitchClipV3ClipType.RandomFeaturedClip || this.ClipType == OverlayTwitchClipV3ClipType.LatestFeaturedClip)
                    {
                        twitchUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByLogin(clipReferenceID);
                        if (twitchUser == null)
                        {
                            // No valid user found, fail out and send an error message
                            await ServiceManager.Get<ChatService>().SendMessage(Resources.OverlayTwitchClipErrorUnableToFindValidClip, StreamingPlatformTypeEnum.Twitch);
                            return false;
                        }
                    }
                }

                ClipModel clip = null;
                if (this.ClipType == OverlayTwitchClipV3ClipType.RandomClip || this.ClipType == OverlayTwitchClipV3ClipType.RandomFeaturedClip)
                {
                    DateTimeOffset startDate = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(30 + RandomHelper.GenerateRandomNumber(365)));
                    bool featured = this.ClipType == OverlayTwitchClipV3ClipType.RandomFeaturedClip;
                    IEnumerable<ClipModel> clips = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClips(twitchUser, startDate: startDate, endDate: DateTimeOffset.Now, featured: featured, maxResults: 100);
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
                else if (this.ClipType == OverlayTwitchClipV3ClipType.LatestClip || this.ClipType == OverlayTwitchClipV3ClipType.LatestFeaturedClip)
                {
                    DateTimeOffset startDate = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(30));
                    bool featured = this.ClipType == OverlayTwitchClipV3ClipType.LatestFeaturedClip;
                    IEnumerable<ClipModel> clips = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClips(twitchUser, startDate: startDate, endDate: DateTimeOffset.Now, featured: featured, maxResults: int.MaxValue);
                    if (clips != null && clips.Count() > 0)
                    {
                        clip = clips.OrderByDescending(c => c.created_at).First();
                    }
                }
                else if (this.ClipType == OverlayTwitchClipV3ClipType.SpecificClip && !string.IsNullOrEmpty(clipReferenceID))
                {
                    if (clipReferenceID.StartsWith(OverlayTwitchClipV3Model.TwitchClipURLPrefix))
                    {
                        clipReferenceID = clipReferenceID.Replace(OverlayTwitchClipV3Model.TwitchClipURLPrefix, "");
                    }
                    clip = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetClip(clipReferenceID);
                }

                if (clip == null)
                {
                    // No valid clip found, fail out and send an error message
                    await ServiceManager.Get<ChatService>().SendMessage(Resources.OverlayTwitchClipErrorUnableToFindValidClip, StreamingPlatformTypeEnum.Twitch);
                    return false;
                }

                this.ClipID = clip.id;
                this.ClipDuration = clip.duration;

                int index = clip.thumbnail_url.IndexOf("-preview-");
                if (index >= 0)
                {
                    this.ClipDirectLink = clip.thumbnail_url.Substring(0, index) + ".mp4";
                }

                return true;
            }

            return false;
        }
    }
}