using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Chat.Mixer
{
    public class MixerChatEmoteModel : IEquatable<MixerChatEmoteModel>
    {
        private const string DefaultEmoticonsManifest = "https://mixer.com/_latest/assets/emoticons/manifest.json";
        private const string DefaultEmoticonsLinkFormat = "https://mixer.com/_latest/assets/emoticons/{0}.png";

        private static readonly LockedDictionary<string, MixerChatEmoteModel> cachedEmotes = new LockedDictionary<string, MixerChatEmoteModel>();

        private class BuiltinEmoticonPack
        {
            public string[] authors = null;
            public bool @default = false;
            public string name = null;
            public Dictionary<string, EmoticonGroupModel> emoticons = new Dictionary<string, EmoticonGroupModel>();
        }

        public static async Task InitializeEmoteCache()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(MixerChatEmoteModel.DefaultEmoticonsManifest))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Dictionary<string, BuiltinEmoticonPack> manifest = JsonConvert.DeserializeObject<Dictionary<string, BuiltinEmoticonPack>>(await response.Content.ReadAsStringAsync());
                        foreach (KeyValuePair<string, BuiltinEmoticonPack> pack in manifest)
                        {
                            MixerChatEmoteModel.AddEmotePack(pack.Value.emoticons, string.Format(MixerChatEmoteModel.DefaultEmoticonsLinkFormat, pack.Key));
                        }
                    }
                }
            }

            List<EmoticonPackModel> userPacks = (await ChannelSession.MixerStreamerConnection.GetEmoticons(ChannelSession.MixerChannel, ChannelSession.MixerStreamerUser)).ToList();
            foreach (EmoticonPackModel userPack in userPacks)
            {
                MixerChatEmoteModel.AddEmotePack(userPack.emoticons, userPack.url);
            }
        }

        public static MixerChatEmoteModel GetEmoteForMessageData(ChatMessageDataModel message)
        {
            if (!string.IsNullOrEmpty(message.type) && message.type.Equals("emoticon", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!cachedEmotes.ContainsKey(message.text) && message.coords != null && System.Uri.IsWellFormedUriString(message.pack, UriKind.Absolute))
                {
                    cachedEmotes[message.text] = new MixerChatEmoteModel
                    {
                        Name = message.text,
                        Uri = message.pack,
                        X = message.coords.x,
                        Y = message.coords.y,
                        Width = message.coords.width,
                        Height = message.coords.height,
                    };
                }

                if (cachedEmotes.ContainsKey(message.text))
                {
                    return cachedEmotes[message.text];
                }
            }
            return null;
        }

        public static IEnumerable<MixerChatEmoteModel> FindMatchingEmoticons(string text)
        {
            if (text.Length == 1 && char.IsLetterOrDigit(text[0]))
            {
                // Short circuit for very short searches that start with letters or digits
                return new List<MixerChatEmoteModel>();
            }
            return cachedEmotes.ToDictionary().Where(v => v.Key.StartsWith(text, StringComparison.InvariantCultureIgnoreCase)).Select(v => v.Value).Distinct();
        }

        private static void AddEmotePack(Dictionary<string, EmoticonGroupModel> emoticons, string imageLink)
        {
            foreach (KeyValuePair<string, EmoticonGroupModel> emoticon in emoticons)
            {
                cachedEmotes[emoticon.Key] = new MixerChatEmoteModel
                {
                    Name = emoticon.Key,
                    Uri = imageLink,
                    X = emoticon.Value.x,
                    Y = emoticon.Value.y,
                    Width = emoticon.Value.width,
                    Height = emoticon.Value.height,
                };
            }
        }

        public string Name { get; set; }
        public string Uri { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }

        public bool Equals(MixerChatEmoteModel other)
        {
            return this.Uri.Equals(other.Uri, StringComparison.InvariantCultureIgnoreCase) && this.X == other.X && this.Y == other.Y &&
                this.Width == other.Width && this.Height == other.Height;
        }

        public override int GetHashCode()
        {
            int hashFilePath = Uri == null ? 0 : Uri.GetHashCode();
            int hashX = X.GetHashCode();
            int hashY = Y.GetHashCode();
            int hashWidth = Width.GetHashCode();
            int hashHeight = Height.GetHashCode();

            return hashFilePath ^ hashX ^ hashY ^ hashWidth ^ hashHeight;
        }
    }
}
