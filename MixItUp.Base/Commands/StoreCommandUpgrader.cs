using MixItUp.Base.Actions;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public static class StoreCommandUpgrader
    {
        public static void PerformUpgrades(List<ActionBase> actions, string version)
        {
            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.4.9.0"))
            {
                StoreCommandUpgrader.SeperateChatFromCurrencyActions(actions);
            }

            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.4.10.0"))
            {
                StoreCommandUpgrader.ChangeWaitActionsToUseSpecialIdentifiers(actions);
            }
        }

        internal static void SeperateChatFromCurrencyActions(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
                if (action is CurrencyAction)
                {
                    CurrencyAction cAction = (CurrencyAction)action;
#pragma warning disable CS0612 // Type or member is obsolete
                    ChatAction chatAction = new ChatAction(cAction.ChatText, sendAsStreamer: false, isWhisper: cAction.IsWhisper, whisperUserName: (cAction.IsWhisper) ? cAction.Username : null);
#pragma warning restore CS0612 // Type or member is obsolete
                    actions.Insert(i + 1, chatAction);
                }
            }
        }

        internal static void ChangeWaitActionsToUseSpecialIdentifiers(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
                if (action is WaitAction)
                {
                    WaitAction wAction = (WaitAction)action;
#pragma warning disable CS0612 // Type or member is obsolete
                    wAction.Amount = wAction.WaitAmount.ToString();
#pragma warning restore CS0612 // Type or member is obsolete
                }
            }
        }

        private static bool IsVersionLessThan(string currentVersion, string checkVersion)
        {
            Version current = Version.Parse(currentVersion.Trim());
            Version check = Version.Parse(checkVersion);
            return current.CompareTo(check) < 0;
        }
    }
}
