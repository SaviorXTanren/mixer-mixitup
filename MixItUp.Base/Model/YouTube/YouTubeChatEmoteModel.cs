using System.Collections.Generic;

namespace MixItUp.Base.Model.YouTube
{
    // https://stackoverflow.com/questions/64726611/how-to-get-a-list-of-youtube-channel-emojis
    public class YouTubeChatEmoteModel
    {
        public string emojiId { get; set; }
        public List<string> searchTerms { get; set; } = new List<string>();
        public List<string> shortcuts { get; set; } = new List<string>();

        public YouTubeChatEmoteImageModel image { get; set; } = null;
    }

    public class YouTubeChatEmoteImageModel
    {
        public class YouTubeChatEmoteImageURLModel
        {
            public string url { get; set; }
        }

        public List<YouTubeChatEmoteImageURLModel> thumbnails { get; set; } = new List<YouTubeChatEmoteImageURLModel>();
    }
}
