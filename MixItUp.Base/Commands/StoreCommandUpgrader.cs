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

            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.4.12.5"))
            {
                StoreCommandUpgrader.ChangeCounterActionsToUseSpecialIdentifiers(actions);
            }

            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.4.17.0"))
            {
                StoreCommandUpgrader.ChangeCounterActionsToUseSpecialIdentifiers(actions);
            }

            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.4.19.0"))
            {
                StoreCommandUpgrader.RestructureNewOverlayActions(actions);
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

        internal static void ChangeCounterActionsToUseSpecialIdentifiers(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
                if (action is CounterAction)
                {
                    CounterAction cAction = (CounterAction)action;
#pragma warning disable CS0612 // Type or member is obsolete
                    cAction.Amount = cAction.CounterAmount.ToString();
#pragma warning restore CS0612 // Type or member is obsolete
                }
            }
        }

        internal static void RestructureNewOverlayActions(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
                if (action is OverlayAction)
                {
                    OverlayAction oAction = (OverlayAction)action;
#pragma warning disable CS0612 // Type or member is obsolete
                    if (oAction.Effect != null)
                    {
                        if (oAction.Effect is OverlayTextEffect)
                        {
                            OverlayTextEffect effect = (OverlayTextEffect)oAction.Effect;
                            oAction.Item = new Model.Overlay.OverlayTextItem(effect.Text, effect.Color, effect.Size, string.Empty, true, false, false, string.Empty);
                        }
                        else if (oAction.Effect is OverlayImageEffect)
                        {
                            OverlayImageEffect effect = (OverlayImageEffect)oAction.Effect;
                            oAction.Item = new Model.Overlay.OverlayImageItem(effect.FilePath, effect.Width, effect.Height);
                        }
                        else if (oAction.Effect is OverlayVideoEffect)
                        {
                            OverlayVideoEffect effect = (OverlayVideoEffect)oAction.Effect;
                            oAction.Item = new Model.Overlay.OverlayVideoItem(effect.FilePath, effect.Width, effect.Height, 100);
                        }
                        else if (oAction.Effect is OverlayYoutubeEffect)
                        {
                            OverlayYoutubeEffect effect = (OverlayYoutubeEffect)oAction.Effect;
                            oAction.Item = new Model.Overlay.OverlayYouTubeItem(effect.ID, effect.StartTime, effect.Width, effect.Height, 100);
                        }
                        else if (oAction.Effect is OverlayWebPageEffect)
                        {
                            OverlayWebPageEffect effect = (OverlayWebPageEffect)oAction.Effect;
                            oAction.Item = new Model.Overlay.OverlayWebPageItem(effect.URL, effect.Width, effect.Height);
                        }
                        else if (oAction.Effect is OverlayHTMLEffect)
                        {
                            OverlayHTMLEffect effect = (OverlayHTMLEffect)oAction.Effect;
                            oAction.Item = new Model.Overlay.OverlayHTMLItem(effect.HTMLText);
                        }
                        oAction.Position = new Model.Overlay.OverlayItemPosition(Model.Overlay.OverlayEffectPositionType.Percentage, oAction.Effect.Horizontal, oAction.Effect.Vertical);
                        oAction.Effects = new Model.Overlay.OverlayItemEffects((Model.Overlay.OverlayEffectEntranceAnimationTypeEnum)oAction.Effect.EntranceAnimation,
                            (Model.Overlay.OverlayEffectVisibleAnimationTypeEnum)oAction.Effect.VisibleAnimation, (Model.Overlay.OverlayEffectExitAnimationTypeEnum)oAction.Effect.ExitAnimation,
                            oAction.Effect.Duration);
                        oAction.Effect = null;
                    }
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
