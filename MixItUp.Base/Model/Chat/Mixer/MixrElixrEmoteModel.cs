using System.Collections.Generic;

namespace MixItUp.Base.Model.Chat.Mixer
{
    public class MixrElixrEmotePackageModel
    {
        public uint channelId { get; set; }
        public List<MixrElixrEmoteModel> channelEmotes { get; set; } = new List<MixrElixrEmoteModel>();
        public List<MixrElixrEmoteModel> globalEmotes { get; set; } = new List<MixrElixrEmoteModel>();
        public string channelEmoteUrlTemplate { get; set; }
        public string globalEmoteUrlTemplate { get; set; }
    }

    public class MixrElixrEmoteModel
    {
        public string id { get; set; }
        public string code { get; set; }
        public bool animated { get; set; }
        public int maxSize { get; set; }

        public string Url { get; set; }
    }
}
