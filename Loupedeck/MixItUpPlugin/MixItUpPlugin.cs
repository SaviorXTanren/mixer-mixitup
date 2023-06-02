using Loupedeck;

namespace MixItUp.LoupedeckPlugin
{
    public class MixItUpPlugin : Plugin
    {
        // More info: https://github.com/Loupedeck/PluginSdk/wiki/Plugin-Capabilities

        // Gets a value indicating whether this is an Universal plugin or an Application plugin.
        public override bool UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is an API-only plugin.
        public override bool HasNoApplication => true;

        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override async void Load()
        {
            try
            {
                var allCommands = await MixItUp.API.Commands.GetAllCommandsAsync();
            }
            catch
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Mix It Up is not running or developer APIs are not enabled.", "https://wiki.mixitupapp.com/en/services/loupedeck", "Loupedeck Plug-In Details" );
            }
        }

        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload()
        {
        }
    }
}
