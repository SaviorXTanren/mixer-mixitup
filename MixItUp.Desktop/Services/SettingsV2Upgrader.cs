using MixItUp.Base;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public static class SettingsV2Upgrader
    {
        public static async Task<SettingsV2Model> UpgradeSettingsToLatest(string filePath)
        {
            int currentVersion = await GetSettingsVersion(filePath);
            if (currentVersion < 0)
            {
                // Settings file is invalid, we can't use this
                return null;
            }
            else if (currentVersion > SettingsV2Model.LatestVersion)
            {
                // Future build, like a preview build, we can't load this
                return null;
            }
            else if (currentVersion < SettingsV2Model.LatestVersion)
            {
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
            }
            return await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
        }

        public static async Task<int> GetSettingsVersion(string filePath)
        {
            string fileData = await ChannelSession.Services.FileService.ReadFile(filePath);
            if (string.IsNullOrEmpty(fileData))
            {
                return -1;
            }
            JObject settingsJObj = JObject.Parse(fileData);
            return (int)settingsJObj["Version"];
        }
    }
}
