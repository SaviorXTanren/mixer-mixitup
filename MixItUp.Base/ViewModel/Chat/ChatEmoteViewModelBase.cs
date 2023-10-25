using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Chat
{
    public abstract class ChatEmoteViewModelBase
    {
        private static Dictionary<string, string> EmoteLocalPaths = new Dictionary<string, string>();

        public abstract string ID { get; protected set; }
        public abstract string Name { get; protected set; }
        public abstract string ImageURL { get; protected set; }

        public virtual bool IsGIFImage { get { return this.ImageURL.Contains(".gif"); } }
        public virtual bool IsSVGImage { get { return this.ImageURL.Contains(".svg"); } }

        public string LocalFilePath { get { return EmoteLocalPaths.ContainsKey(this.Name) ? EmoteLocalPaths[this.Name] : null; } }

        public async Task<bool> SaveToTempFolder()
        {
            string folderPath = Path.Combine(ServiceManager.Get<IFileService>().GetTempFolder(), "MixItUp");
            string filePath = Path.Combine(folderPath, this.Name.ToFilePathString() + ".gif");

            EmoteLocalPaths[this.Name] = filePath;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            if (!ServiceManager.Get<IFileService>().FileExists(filePath))
            {
                try
                {
                    using (AdvancedHttpClient client = new AdvancedHttpClient())
                    {
                        byte[] bytes = await client.GetByteArrayAsync(this.ImageURL);
                        await ServiceManager.Get<IFileService>().SaveFileAsBytes(filePath, bytes);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    EmoteLocalPaths.Remove(this.Name);
                }
            }

            return EmoteLocalPaths.ContainsKey(this.Name);
        }
    }
}
