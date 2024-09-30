namespace Loupedeck.MixItUpPlugin
{
    using System;

    // A helper class that enables logging from the plugin code.

    internal static class PluginLog
    {
        private static PluginLogFile _pluginLogFile;

        public static void Init(PluginLogFile pluginLogFile)
        {
            pluginLogFile.CheckNullArgument(nameof(pluginLogFile));
            PluginLog._pluginLogFile = pluginLogFile;
        }

        public static void Verbose(String text) => PluginLog._pluginLogFile?.Verbose(text);

        public static void Verbose(Exception ex, String text) => PluginLog._pluginLogFile?.Verbose(ex, text);

        public static void Info(String text) => PluginLog._pluginLogFile?.Info(text);

        public static void Info(Exception ex, String text) => PluginLog._pluginLogFile?.Info(ex, text);

        public static void Warning(String text) => PluginLog._pluginLogFile?.Warning(text);

        public static void Warning(Exception ex, String text) => PluginLog._pluginLogFile?.Warning(ex, text);

        public static void Error(String text) => PluginLog._pluginLogFile?.Error(text);

        public static void Error(Exception ex, String text) => PluginLog._pluginLogFile?.Error(ex, text);
    }
}
