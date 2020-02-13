using MixItUp.Base.Actions;
using MixItUp.Base.Model.Overlay;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public static class StoreCommandUpgrader
    {
        public static void PerformUpgrades(List<ActionBase> actions, string version)
        {
            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.5.1.501"))
            {
                StoreCommandUpgrader.RestructureNewerOverlayActions(actions);
            }

            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.5.1.700"))
            {
                StoreCommandUpgrader.ReplaceActionGroupAction(actions);
            }

            if (StoreCommandUpgrader.IsVersionLessThan(version, "0.5.2.1110"))
            {
                StoreCommandUpgrader.UpdateConditionalAction(actions);
            }
        }

        public static void RestructureNewerOverlayActions(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
                if (action is OverlayAction)
                {
                    OverlayAction oAction = (OverlayAction)action;
#pragma warning disable CS0612 // Type or member is obsolete
                    if (oAction.Item != null)
                    {
                        oAction.OverlayItem = ConvertOverlayItem(oAction.Item);
                        oAction.OverlayItem.Position = new OverlayItemPositionModel((OverlayItemPositionType)oAction.Position.PositionType, oAction.Position.Horizontal, oAction.Position.Vertical, 0);
                        oAction.OverlayItem.Effects = new OverlayItemEffectsModel((OverlayItemEffectEntranceAnimationTypeEnum)oAction.Effects.EntranceAnimation, (OverlayItemEffectVisibleAnimationTypeEnum)oAction.Effects.VisibleAnimation,
                            (OverlayItemEffectExitAnimationTypeEnum)oAction.Effects.ExitAnimation, oAction.Effects.Duration);

                        oAction.Item = null;
                        oAction.Position = null;
                        oAction.Effects = null;
                        oAction.Effect = null;
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                }
            }
        }

#pragma warning disable CS0612 // Type or member is obsolete
        public static OverlayItemModelBase ConvertOverlayItem(OverlayItemBase item)
        {
            if (item is OverlayTextItem)
            {
                OverlayTextItem oItem = (OverlayTextItem)item;
                return new OverlayTextItemModel(oItem.Text, oItem.Color, oItem.Size, oItem.Font, oItem.Bold, oItem.Italic, oItem.Underline, oItem.ShadowColor);
            }
            else if (item is OverlayImageItem)
            {
                OverlayImageItem oItem = (OverlayImageItem)item;
                return new OverlayImageItemModel(oItem.FilePath, oItem.Width, oItem.Height);
            }
            else if (item is OverlayVideoItem)
            {
                OverlayVideoItem oItem = (OverlayVideoItem)item;
                return new OverlayVideoItemModel(oItem.FilePath, oItem.Width, oItem.Height, oItem.Volume);
            }
            else if (item is OverlayYouTubeItem)
            {
                OverlayYouTubeItem oItem = (OverlayYouTubeItem)item;
                return new OverlayYouTubeItemModel(oItem.VideoID, oItem.Width, oItem.Height, oItem.StartTime, oItem.Volume);
            }
            else if (item is OverlayWebPageItem)
            {
                OverlayWebPageItem oItem = (OverlayWebPageItem)item;
                return new OverlayWebPageItemModel(oItem.URL, oItem.Width, oItem.Height);
            }
            else if (item is OverlayHTMLItem)
            {
                OverlayHTMLItem oItem = (OverlayHTMLItem)item;
                return new OverlayHTMLItemModel(oItem.HTMLText);
            }
            return null;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        public static void ReplaceActionGroupAction(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
#pragma warning disable CS0612 // Type or member is obsolete
                if (action is ActionGroupAction)
                {
                    ActionGroupAction aaction = (ActionGroupAction)action;
                    actions[i] = new CommandAction()
                    {
                        CommandActionType = CommandActionTypeEnum.RunCommand,
                        CommandID = aaction.ActionGroupID
                    };
                }
#pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        public static void UpdateConditionalAction(List<ActionBase> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                ActionBase action = actions[i];
#pragma warning disable CS0612 // Type or member is obsolete
                if (action is ConditionalAction)
                {
                    ConditionalAction caction = (ConditionalAction)action;
                    if (caction.Clauses.Count == 0 && !string.IsNullOrEmpty(caction.Value1))
                    {
                        caction.Clauses.Add(new ConditionalClauseModel(caction.ComparisionType, caction.Value1, caction.Value2, caction.Value3));
                        caction.Value1 = null;
                        caction.Value2 = null;
                        caction.Value3 = null;
                    }
                }
#pragma warning restore CS0612 // Type or member is obsolete
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
