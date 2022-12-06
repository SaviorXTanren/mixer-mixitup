using Loupedeck;

namespace MixItUp.LoupedeckPlugin
{
    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class MixItUpPlugin : Plugin
    {
        // Gets a value indicating whether this is an Universal plugin or an Application plugin.
        public override bool UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is an API-only plugin.
        public override bool HasNoApplication => false;

        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override void Load()
        {
            //try
            //{
            //    var allCommands = await MixItUp.API.Commands.GetAllCommandsAsync();
            //}
            //catch
            //{
            //    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Mix It Up is not running or developer APIs are not enabled.");
            //}
        }

        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload()
        {
        }
    }
}
