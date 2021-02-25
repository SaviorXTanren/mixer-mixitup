using Glimesh.Base.Models.Clients.Chat;
using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.ViewModel.Chat.Glimesh
{
    public class GlimeshChatMessageViewModel : UserChatMessageViewModel
    {
        public GlimeshChatMessageViewModel(ChatMessagePacketModel message, UserViewModel user = null)
            : base(null, StreamingPlatformTypeEnum.Glimesh, (user != null) ? user : new UserViewModel(message))
        {
            //this.User.SetTwitchChatDetails(message);

            //foreach (var kvp in message.EmotesDictionary)
            //{
            //    if (!messageEmotesHashSet.Contains(kvp.Key) && kvp.Value.Count > 0)
            //    {
            //        long emoteID = kvp.Key;
            //        Tuple<int, int> instance = kvp.Value.FirstOrDefault();
            //        if (0 <= instance.Item1 && instance.Item1 < message.Message.Length && 0 <= instance.Item2 && instance.Item2 < message.Message.Length)
            //        {
            //            string emoteCode = message.Message.Substring(instance.Item1, instance.Item2 - instance.Item1 + 1);
            //            messageEmotesCache[emoteCode] = new EmoteModel()
            //            {
            //                id = emoteID,
            //                code = emoteCode
            //            };
            //            messageEmotesHashSet.Add(kvp.Key);
            //        }
            //    }
            //}

            //this.HasBits = (int.TryParse(message.Bits, out int bits) && bits > 0);
            //this.IsHighlightedMessage = message.RawPacket.Tags.ContainsKey(TagMessageID) && message.RawPacket.Tags[TagMessageID].Equals(MessageIDHighlightedMessage);

            //if (message.Message.StartsWith(SlashMeAction) && message.Message.Last() == SOHCharacter)
            //{
            //    this.IsSlashMe = true;
            //    string text = message.Message.Replace(SlashMeAction, string.Empty);
            //    this.ProcessMessageContents(text.Substring(0, text.Length - 1));
            //}
            //else
            //{
            //    this.ProcessMessageContents(message.Message);
            //}
        }
    }
}
