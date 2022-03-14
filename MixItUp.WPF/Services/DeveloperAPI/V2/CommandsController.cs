using MixItUp.API.V2.Models;
using MixItUp.Base;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    [RoutePrefix("api/v2/commands")]
    public class CommandsController : ApiController
    {
        // Get Command
        // Must have all command data (triggers, costs, restrictions, etc)
        // Run Command
        // Update Command
        // Delete Command?
        // Create Command?

        [Route("{commandId:guid}")]
        [HttpGet]
        public IHttpActionResult GetCommandById(Guid commandId)
        {
            if (!ChannelSession.Settings.Commands.TryGetValue(commandId, out var command) || command == null)
            {
                return NotFound();
            }

            return Ok(new GetSingleCommandResponse { Command = CommandMapper.ToCommand(command) });
        }

        [Route]
        [HttpGet]
        public IHttpActionResult GetAllCommands(int skip = 0, int pageSize = 25)
        {
            var allCommands = GetAllCommands();
            var commands = allCommands
                .OrderBy(c => c.ID)
                .Skip(skip)
                .Take(pageSize);

            var result = new GetListOfCommandsResponse();
            result.TotalCount = allCommands.Count;
            foreach (var command in commands)
            {
                result.Commands.Add(CommandMapper.ToCommand(command));
            }

            return Ok(result);
        }

        [Route("{commandId:guid}/state/{state:int}")]
        [HttpPatch]
        public async Task<IHttpActionResult> UpdateCommandState(Guid commandId, CommandStateOptions state)
        {
            if (!ChannelSession.Settings.Commands.TryGetValue(commandId, out var command) || command == null)
            {
                return NotFound();
            }

            if (state == CommandStateOptions.Disable)
            {
                command.IsEnabled = false;
            }
            else if (state == CommandStateOptions.Enable)
            {
                command.IsEnabled = true;
            }
            else if (state == CommandStateOptions.Toggle)
            {
                command.IsEnabled = !command.IsEnabled;
            }
            else
            {
                return BadRequest();
            }

            if (command is ChatCommandModel)
            {
                ServiceManager.Get<ChatService>().RebuildCommandTriggers();
            }
            else if (command is TimerCommandModel)
            {
                await ServiceManager.Get<TimerService>().RebuildTimerGroups();
            }

            return Ok(new GetSingleCommandResponse { Command = CommandMapper.ToCommand(command) });
        }

        private List<CommandModelBase> GetAllCommands()
        {
            return new List<CommandModelBase>(ServiceManager.Get<CommandService>().AllCommands);
        }
    }

    public static class CommandMapper
    {
        private static Dictionary<Type, MethodInfo> ToArgumentRequirementMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> ToActionBaseMap = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> ToOverlayItemMap = new Dictionary<Type, MethodInfo>();
        static CommandMapper()
        {
            var methods = typeof(CommandMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "ToArgumentRequirement");
            foreach (var method in methods)
            {
                var argType = method.GetParameters().FirstOrDefault();
                if (argType != null)
                {
                    ToArgumentRequirementMap[argType.ParameterType] = method;
                }
            }

            methods = typeof(CommandMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "ToActionBase");
            foreach (var method in methods)
            {
                var argType = method.GetParameters().FirstOrDefault();
                if (argType != null)
                {
                    ToActionBaseMap[argType.ParameterType] = method;
                }
            }

            methods = typeof(CommandMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "ToOverlayItem");
            foreach (var method in methods)
            {
                var argType = method.GetParameters().FirstOrDefault();
                if (argType != null)
                {
                    ToOverlayItemMap[argType.ParameterType] = method;
                }
            }
        }

        public static Command ToCommand(CommandModelBase command)
        {
            return new Command
            {
                ID = command.ID,
                Name = command.Name,
                Type = command.Type.ToString(),
                IsEnabled = command.IsEnabled,
                Unlocked = command.Unlocked,
                IsEmbedded = command.IsEmbedded,
                GroupName = command.GroupName,
                Triggers = new HashSet<string>(command.Triggers),
                Requirements = ToCommandRequirements(command.Requirements),
                Actions = ToActionBase(command.Actions),
            };
        }

        #region ActionBase Mappers
        private static List<ActionBase> ToActionBase(List<ActionModelBase> actions)
        {
            var results = new List<ActionBase>();

            foreach (var action in actions)
            {
                if (ToActionBaseMap.TryGetValue(action.GetType(), out var method))
                {
                    results.Add(method.Invoke(null, new object[] { action }) as ActionBase);
                }
                else
                {
                    // Unknown type? What should we do here?
                }
            }

            return results;
        }

        private static ChatAction ToActionBase(ChatActionModel action)
        {
            return new ChatAction
            {
                ActionBaseType = action.GetType().ToString(),

                ChatText = action.ChatText,
                IsWhisper = action.IsWhisper,
                SendAsStreamer = action.SendAsStreamer,
                WhisperUserName = action.WhisperUserName,
            };
        }

        private static CommandAction ToActionBase(CommandActionModel action)
        {
            return new CommandAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                Arguments = action.Arguments,
                CommandGroupName = action.CommandGroupName,
                CommandID = action.CommandID,
                PreMadeType = action.PreMadeType.ToString(),
                WaitForCommandToFinish = action.WaitForCommandToFinish,
            };
        }

        private static ConditionalAction ToActionBase(ConditionalActionModel action)
        {
            return new ConditionalAction
            {
                ActionBaseType = action.GetType().ToString(),

                Actions = ToActionBase(action.Actions),
                CaseSensitive = action.CaseSensitive,
                Clauses = ToConditionalClause(action.Clauses),
                CommandID = action.CommandID,
                Operator = action.Operator.ToString(),
                RepeatWhileTrue = action.RepeatWhileTrue,
            };
        }

        private static List<ConditionalClause> ToConditionalClause(List<ConditionalClauseModel> clauses)
        {
            var results = new List<ConditionalClause>();

            foreach (var clause in clauses)
            {
                results.Add(ToConditionalClause(clause));
            }

            return results;
        }

        private static ConditionalClause ToConditionalClause(ConditionalClauseModel clause)
        {
            return new ConditionalClause
            {
                ComparisionType = clause.ComparisionType.ToString(),
                Value1 = clause.Value1,
                Value2 = clause.Value2,
                Value3 = clause.Value3,
            };
        }

        private static ConsumablesAction ToActionBase(ConsumablesActionModel action)
        {
            return new ConsumablesAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                Amount = action.Amount,
                CurrencyID = action.CurrencyID,
                DeductFromUser = action.DeductFromUser,
                InventoryID = action.InventoryID,
                ItemName = action.ItemName,
                StreamPassID = action.StreamPassID,
                Username = action.Username,
                UserRoleToApplyTo = action.UserRoleToApplyTo.ToString(),
                UsersMustBePresent = action.UsersMustBePresent,
            };
        }

        private static CounterAction ToActionBase(CounterActionModel action)
        {
            return new CounterAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                Amount = action.Amount,
                CounterName = action.CounterName,
            };
        }

        private static DiscordAction ToActionBase(DiscordActionModel action)
        {
            return new DiscordAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                ChannelID = action.ChannelID,
                FilePath = action.FilePath,
                MessageText = action.MessageText,
                ShouldMuteDeafen = action.ShouldMuteDeafen,
            };
        }

        private static ExternalProgramAction ToActionBase(ExternalProgramActionModel action)
        {
            return new ExternalProgramAction
            {
                ActionBaseType = action.GetType().ToString(),

                Arguments = action.Arguments,
                FilePath = action.FilePath,
                SaveOutput = action.SaveOutput,
                ShowWindow = action.ShowWindow,
                WaitForFinish = action.WaitForFinish,
            };
        }

        private static FileAction ToActionBase(FileActionModel action)
        {
            return new FileAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                FilePath = action.FilePath,
                LineIndex = action.LineIndex,
                TransferText = action.TransferText,
            };
        }

        private static GameQueueAction ToActionBase(GameQueueActionModel action)
        {
            return new GameQueueAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                RoleRequirement = action.RoleRequirement.ToString(),
                TargetUsername = action.TargetUsername,
            };
        }

        private static IFTTTAction ToActionBase(IFTTTActionModel action)
        {
            return new IFTTTAction
            {
                ActionBaseType = action.GetType().ToString(),

                EventName = action.EventName,
                EventValue1 = action.EventValue1,
                EventValue2 = action.EventValue2,
                EventValue3 = action.EventValue3,
            };
        }

        private static InputAction ToActionBase(InputActionModel action)
        {
            return new InputAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                Alt = action.Alt,
                Control = action.Control,
                Key = action.VirtualKey?.ToString(),
                Mouse = action.Mouse?.ToString(),
                Shift = action.Shift,
            };
        }

        private static ModerationAction ToActionBase(ModerationActionModel action)
        {
            return new ModerationAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                ModerationReason = action.ModerationReason,
                TargetUsername = action.TargetUsername,
                TimeoutAmount = action.TimeoutAmount,
            };
        }

        private static OverlayAction ToActionBase(OverlayActionModel action)
        {
            return new OverlayAction
            {
                ActionBaseType = action.GetType().ToString(),

                OverlayItem = ToOverlayItem(action.OverlayItem),
                OverlayName = action.OverlayName,
                ShowWidget = action.ShowWidget,
                WidgetID = action.WidgetID,
            };
        }

        private static OverlayItem ToOverlayItem(OverlayItemModelBase overlayItem)
        {
            OverlayItem item;
            if (ToOverlayItemMap.TryGetValue(overlayItem.GetType(), out var method))
            {
                item = method.Invoke(null, new object[] { overlayItem }) as OverlayItem;
            }
            else
            {
                // What to do if no map?
                item = new OverlayItem();
            }

            item.OverlayItemType = overlayItem.GetType().ToString();
            item.Effects = ToOverlayItemEffects(overlayItem.Effects);
            item.ID = overlayItem.ID;
            item.ItemType = overlayItem.ItemType.ToString();
            item.Position = ToOverlayItemPosition(overlayItem.Position);

            return item;
        }

        private static OverlayFileItem ToOverlayItem(OverlayImageItemModel overlayItem)
        {
            return new OverlayFileItem
            {
                FileID = overlayItem.FileID,
                FilePath = overlayItem.FilePath,
                FileType = overlayItem.FileType,
                Height = overlayItem.Height,
                Width = overlayItem.Width,
            };
        }

        private static OverlayHTMLItem ToOverlayItem(OverlayHTMLItemModel overlayItem)
        {
            return new OverlayHTMLItem
            {
                HTML = overlayItem.HTML,
            };
        }

        private static OverlayHTMLItem ToOverlayItem(OverlayHTMLTemplateItemModelBase overlayItem)
        {
            return new OverlayHTMLItem
            {
                HTML = overlayItem.HTML,
            };
        }

        private static OverlayTextItem ToOverlayItem(OverlayTextItemModel overlayItem)
        {
            return new OverlayTextItem
            {
                Bold = overlayItem.Bold,
                Color = overlayItem.Color,
                Font = overlayItem.Font,
                Italic = overlayItem.Italic,
                ShadowColor = overlayItem.ShadowColor,
                Size = overlayItem.Size,
                Text = overlayItem.Text,
                Underline = overlayItem.Underline,
            };
        }

        private static OverlayItemPosition ToOverlayItemPosition(OverlayItemPositionModel position)
        {
            return new OverlayItemPosition
            {
                Horizontal = position.Horizontal,
                Layer = position.Layer,
                PositionType = position.PositionType.ToString(),
                Vertical = position.Vertical,
            };
        }

        private static OverlayItemEffects ToOverlayItemEffects(OverlayItemEffectsModel effects)
        {
            return new OverlayItemEffects
            {
                Duration = effects.Duration,
                EntranceAnimation = effects.EntranceAnimation.ToString(),
                ExitAnimation = effects.ExitAnimation.ToString(),
                VisibleAnimation = effects.VisibleAnimation.ToString(),
            };
        }

        private static OvrStreamAction ToActionBase(OvrStreamActionModel action)
        {
            return new OvrStreamAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                TitleName = action.TitleName,
                Variables = new Dictionary<string, string>(action.Variables),
            };
        }

        private static PixelChatAction ToActionBase(PixelChatActionModel action)
        {
            return new PixelChatAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                ComponentID = action.ComponentID,
                OverlayID = action.OverlayID,
                SceneComponentVisible = action.SceneComponentVisible,
                SceneID = action.SceneID,
                TargetUsername = action.TargetUsername,
                TimeAmount = action.TimeAmount,
            };
        }

        private static SerialAction ToActionBase(SerialActionModel action)
        {
            return new SerialAction
            {
                ActionBaseType = action.GetType().ToString(),

                Message = action.Message,
                PortName = action.PortName,
            };
        }

        private static SoundAction ToActionBase(SoundActionModel action)
        {
            return new SoundAction
            {
                ActionBaseType = action.GetType().ToString(),

                FilePath = action.FilePath,
                OutputDevice = action.OutputDevice,
                VolumeScale = action.VolumeScale,
            };
        }

        private static SpecialIdentifierAction ToActionBase(SpecialIdentifierActionModel action)
        {
            return new SpecialIdentifierAction
            {
                ActionBaseType = action.GetType().ToString(),

                MakeGloballyUsable = action.MakeGloballyUsable,
                ReplacementText = action.ReplacementText,
                ShouldProcessMath = action.ShouldProcessMath,
                SpecialIdentifierName = action.SpecialIdentifierName,
            };
        }

        private static StreamingSoftwareAction ToActionBase(StreamingSoftwareActionModel action)
        {
            return new StreamingSoftwareAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                ItemName = action.ItemName,
                ParentName = action.ParentName,
                SourceDimensions = ToSourceDimensions(action.SourceDimensions),
                SourceText = action.SourceText,
                SourceTextFilePath = action.SourceTextFilePath,
                SourceURL = action.SourceURL,
                StreamingSoftwareType = action.StreamingSoftwareType.ToString(),
                Visible = action.Visible,
            };
        }

        private static StreamingSoftwareSourceDimensions ToSourceDimensions(StreamingSoftwareSourceDimensionsModel sourceDimensions)
        {
            return new StreamingSoftwareSourceDimensions
            {
                Rotation = sourceDimensions.Rotation,
                X = sourceDimensions.X,
                XScale = sourceDimensions.XScale,
                Y = sourceDimensions.Y,
                YScale = sourceDimensions.YScale,
            };
        }

        private static StreamlabsAction ToActionBase(StreamlabsActionModel action)
        {
            return new StreamlabsAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
            };
        }

        private static TextToSpeechAction ToActionBase(TextToSpeechActionModel action)
        {
            return new TextToSpeechAction
            {
                ActionBaseType = action.GetType().ToString(),

                Pitch = action.Pitch,
                Rate = action.Rate,
                Text = action.Text,
                Voice = action.Voice,
                Volume = action.Volume,
            };
        }

        private static TrovoAction ToActionBase(TrovoActionModel action)
        {
            return new TrovoAction
            {
                ActionBaseType = action.GetType().ToString(),

                Actions = ToActionBase(action.Actions),
                ActionType = action.ActionType.ToString(),
                Amount = action.Amount,
                RoleName = action.RoleName,
                Username = action.Username,
            };
        }

        private static TwitchAction ToActionBase(TwitchActionModel action)
        {
            return new TwitchAction
            {
                ActionBaseType = action.GetType().ToString(),

                Actions = ToActionBase(action.Actions),
                ActionType=action.ActionType.ToString(),
                AdLength = action.AdLength,
                ChannelPointRewardCostString = action.ChannelPointRewardCostString,
                ChannelPointRewardGlobalCooldownString = action.ChannelPointRewardGlobalCooldownString,
                ChannelPointRewardID = action.ChannelPointRewardID,
                ChannelPointRewardMaxPerStreamString = action.ChannelPointRewardMaxPerStreamString,
                ChannelPointRewardMaxPerUserString = action.ChannelPointRewardMaxPerUserString,
                ChannelPointRewardState = action.ChannelPointRewardState,
                ChannelPointRewardUpdateCooldownsAndLimits = action.ChannelPointRewardUpdateCooldownsAndLimits,
                ClipIncludeDelay = action.ClipIncludeDelay,
                PollBitsCost = action.PollBitsCost,
                PollChannelPointsCost = action.PollChannelPointsCost,
                PollChoices = new List<string>(action.PollChoices),
                PollDurationSeconds = action.PollDurationSeconds,
                PollTitle = action.PollTitle,
                PredictionDurationSeconds = action.PredictionDurationSeconds,
                PredictionOutcomes = new List<string>(action.PredictionOutcomes),
                PredictionTitle = action.PredictionTitle,
                ShowInfoInChat = action.ShowInfoInChat,
                StreamMarkerDescription = action.StreamMarkerDescription,
                TimeLength = action.TimeLength,
                Username = action.Username,
            };
        }

        private static TwitterAction ToActionBase(TwitterActionModel action)
        {
            return new TwitterAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                ImagePath = action.ImagePath,
                NameUpdate = action.NameUpdate,
                TweetText = action.TweetText,
            };
        }

        private static VoicemodAction ToActionBase(VoicemodActionModel action)
        {
            return new VoicemodAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType = action.ActionType.ToString(),
                RandomVoiceType = action.RandomVoiceType.ToString(),
                SoundFileName = action.SoundFileName,
                State = action.State,
                VoiceID = action.VoiceID,
            };
        }

        private static VTubeStudioAction ToActionBase(VTubeStudioActionModel action)
        {
            return new VTubeStudioAction
            {
                ActionBaseType = action.GetType().ToString(),

                ActionType= action.ActionType.ToString(),
                HotKeyID = action.HotKeyID,
                ModelID = action.ModelID,
                MovementRelative = action.MovementRelative,
                MovementTimeInSeconds = action.MovementTimeInSeconds,
                MovementX = action.MovementX,
                MovementY = action.MovementY,
                Rotation = action.Rotation,
                Size = action.Size,
            };
        }

        private static WaitAction ToActionBase(WaitActionModel action)
        {
            return new WaitAction
            {
                ActionBaseType = action.GetType().ToString(),

                Amount = action.Amount,
            };
        }

        private static WebRequestAction ToActionBase(WebRequestActionModel action)
        {
            return new WebRequestAction
            {
                ActionBaseType = action.GetType().ToString(),

                JSONToSpecialIdentifiers = new Dictionary<string, string>(action.JSONToSpecialIdentifiers),
                ResponseType = action.ResponseType.ToString(),
                Url = action.Url,
            };
        }

        private static YouTubeAction ToActionBase(YouTubeActionModel action)
        {
            return new YouTubeAction
            {
                ActionBaseType = action.GetType().ToString(),

                Actions = ToActionBase(action.Actions),
                ActionType = action.ActionType.ToString(),
                AdBreakLength = action.AdBreakLength,
            };
        }
        #endregion

        #region Requirements Mappers
        private static List<CommandRequirement> ToCommandRequirements(RequirementsSetModel requirements)
        {
            var results = new List<CommandRequirement>();

            foreach (var requirement in requirements.Requirements)
            {
                if (ToArgumentRequirementMap.TryGetValue(requirement.GetType(), out var method))
                {
                    results.Add(method.Invoke(null, new object[] { requirement }) as CommandRequirement);
                }
                else
                {
                    // Unknown type? What should we do here?
                }
            }

            return results;
        }

        private static ThresholdRequirement ToArgumentRequirement(ThresholdRequirementModel requirement)
        {
            return new ThresholdRequirement
            {
                CommandRequirementType = requirement.GetType().ToString(),

                Amount = requirement.Amount,
                RunForEachUser = requirement.RunForEachUser,
                TimeSpan = requirement.TimeSpan,
            };
        }

        private static SettingsRequirement ToArgumentRequirement(SettingsRequirementModel requirement)
        {
            return new SettingsRequirement
            {
                CommandRequirementType = requirement.GetType().ToString(),

                DeleteChatMessageWhenRun = requirement.DeleteChatMessageWhenRun,
                DontDeleteChatMessageWhenRun = requirement.DontDeleteChatMessageWhenRun,
                ShowOnChatContextMenu = requirement.ShowOnChatContextMenu,
            };
        }

        private static RoleRequirement ToArgumentRequirement(RoleRequirementModel requirement)
        {
            return new RoleRequirement
            {
                CommandRequirementType = requirement.GetType().ToString(),

                PatreonBenefitID = requirement.PatreonBenefitID,
                StreamingPlatform = requirement.StreamingPlatform.ToString(),
                SubscriberTier = requirement.SubscriberTier,
                UserRole = requirement.UserRole.ToString(),
            };
        }

        private static RankRequirement ToArgumentRequirement(RankRequirementModel requirement)
        {
            return new RankRequirement
            {
                CommandRequirementType = requirement.GetType().ToString(),

                MatchType = requirement.MatchType.ToString(),
                RankName = requirement.RankName,
                RankSystemID = requirement.RankSystemID,
            };
        }

        private static InventoryRequirement ToArgumentRequirement(InventoryRequirementModel requirement)
        {
            return new InventoryRequirement
            {
                CommandRequirementType = requirement.GetType().ToString(),

                InventoryID = requirement.InventoryID,
                ItemID = requirement.ItemID,
                Amount = requirement.Amount,
            };
        }

        private static CurrencyRequirement ToArgumentRequirement(CurrencyRequirementModel requirement)
        {
            return new CurrencyRequirement
            {
                CommandRequirementType = requirement.GetType().ToString(),

                CurrencyID = requirement.CurrencyID,
                MaxAmount = requirement.MaxAmount,
                MinAmount = requirement.MinAmount,
                RequirementType = requirement.RequirementType.ToString(),
            };
        }

        private static ArgumentsRequirement ToArgumentsRequirement(ArgumentsRequirementModel requirement)
        {
            return new ArgumentsRequirement
            {
                Items = ToArgumentsRequirementItems(requirement.Items),
            };
        }

        private static CooldownRequirement ToArgumentRequirement(CooldownRequirementModel requirement)
        {
            return new CooldownRequirement
            {
                CommandRequirementType = requirement.GetType().ToString(),

                GroupName = requirement.GroupName,
                IndividualAmount = requirement.IndividualAmount,
                Type = requirement.Type.ToString(),
            };
        }

        private static List<ArgumentsRequirementItem> ToArgumentsRequirementItems(List<ArgumentsRequirementItemModel> requirementItems)
        {
            var results = new List<ArgumentsRequirementItem>();

            foreach (var item in requirementItems)
            {
                results.Add(ToArgumentsRequirementItem(item));
            }

            return results;
        }

        private static ArgumentsRequirementItem ToArgumentsRequirementItem(ArgumentsRequirementItemModel item)
        {
            return new ArgumentsRequirementItem
            {
                Name = item.Name,
                Optional = item.Optional,
                Type = item.Type.ToString(),
            };
        }
        #endregion
    }
}
