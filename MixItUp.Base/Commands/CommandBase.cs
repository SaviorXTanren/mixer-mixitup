using MixItUp.Base.Actions;
using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public enum CommandTypeEnum
    {
        Chat = 0,
        [Name("MixPlay")]
        Interactive = 1,
        Event = 2,
        Timer = 3,
        Custom = 4,
        ActionGroup = 5,
        Game = 6,
        Remote = 7,
    }

    [DataContract]
    public class ActionListContainer
    {
        [DataMember]
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();
    }

    [DataContract]
    public abstract class CommandBase : ActionListContainer, IComparable, IComparable<CommandBase>, IEquatable<CommandBase>
    {
        public const string CommandMatchingRegexFormat = "^({0})(\\s|$)";

        private static Dictionary<Guid, long> commandUses = new Dictionary<Guid, long>();

        public static Dictionary<Guid, long> GetCommandUses()
        {
            Dictionary<Guid, long> results = new Dictionary<Guid, long>();

            try
            {
                foreach (Guid key in CommandBase.commandUses.Keys.ToList())
                {
                    results[key] = CommandBase.commandUses[key];
                    CommandBase.commandUses[key] = 0;
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            return results;
        }

        public static bool IsValidCommandString(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                return command.All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c) || Char.IsSymbol(c) || Char.IsPunctuation(c));
            }
            return false;
        }

        public event EventHandler OnCommandStart = delegate { };

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public Guid StoreID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public HashSet<string> Commands { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsBasic { get; set; }

        [DataMember]
        public bool Unlocked { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public bool IsRandomized { get; set; }

        [JsonIgnore]
        protected StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None;

        [JsonIgnore]
        private Task currentTaskRun;
        [JsonIgnore]
        private CancellationTokenSource currentCancellationTokenSource;

        public CommandBase()
        {
            this.ID = Guid.NewGuid();
            this.Commands = new HashSet<string>();
            this.Actions = new List<ActionBase>();
            this.IsEnabled = true;
        }

        public CommandBase(string name, CommandTypeEnum type) : this(name, type, new List<string>() { }) { }

        public CommandBase(string name, CommandTypeEnum type, string command) : this(name, type, new List<string>() { command }) { }

        public CommandBase(string name, CommandTypeEnum type, IEnumerable<string> commands)
            : this()
        {
            this.Name = name;
            this.Type = type;
            this.Commands = new HashSet<string>(commands);
        }

        [JsonIgnore]
        public string TypeName { get { return EnumHelper.GetEnumName(this.Type); } }

        [JsonIgnore]
        public virtual bool IsEditable { get { return true; } }

        [JsonIgnore]
        public virtual string CommandsString { get { return string.Join(" ", this.Commands); } }

        [JsonIgnore]
        public virtual HashSet<string> CommandTriggers { get { return this.Commands; } }

        [JsonIgnore]
        protected abstract SemaphoreSlim AsyncSemaphore { get; }

        public override string ToString() { return string.Format("{0} - {1}", this.ID, this.Name); }

        public async Task Perform(StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            await this.Perform(ChannelSession.GetCurrentUser(), platform, arguments, extraSpecialIdentifiers: extraSpecialIdentifiers);
        }

        public Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            if (this.IsEnabled)
            {
                Logger.Log(LogLevel.Debug, $"Starting command performing: {this.Name}");

                if (arguments == null)
                {
                    arguments = new List<string>();
                }

                if (extraSpecialIdentifiers == null)
                {
                    extraSpecialIdentifiers = new Dictionary<string, string>();
                }

                if (this.platform == StreamingPlatformTypeEnum.None && user != null)
                {
                    this.platform = user.Platform;
                }
                else
                {
                    this.platform = platform;
                }

                try
                {
                    if (this.StoreID != Guid.Empty)
                    {
                        if (!CommandBase.commandUses.ContainsKey(this.StoreID))
                        {
                            CommandBase.commandUses[this.StoreID] = 0;
                        }
                        CommandBase.commandUses[this.StoreID]++;
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }

                ChannelSession.Services.Telemetry.TrackCommand(this.Type, this.IsBasic);
                if (user != null)
                {
                    user.Data.TotalCommandsRun++;
                }

                this.OnCommandStart(this, new EventArgs());

                this.currentCancellationTokenSource = new CancellationTokenSource();
                this.currentTaskRun = Task.Run(async () =>
                {
                    bool waitOccurred = false;
                    try
                    {
                        if (!await this.PerformPreChecks(user, arguments, extraSpecialIdentifiers))
                        {
                            return;
                        }

                        if (!this.Unlocked && !ChannelSession.Settings.UnlockAllCommands)
                        {
                            await this.AsyncSemaphore.WaitAsync();
                            waitOccurred = true;
                        }
                        await this.PerformInternal(user, arguments, extraSpecialIdentifiers, this.currentCancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException) { }
                    catch (Exception ex) { Logger.Log(ex); }
                    finally
                    {
                        if (waitOccurred)
                        {
                            this.AsyncSemaphore.Release();
                        }
                    }
                }, this.currentCancellationTokenSource.Token);
            }
            return Task.FromResult(0);
        }

        public async Task PerformAndWait(UserViewModel user, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            try
            {
                await this.Perform(user, platform, arguments, extraSpecialIdentifiers);
                if (this.currentTaskRun != null && !this.currentTaskRun.IsCompleted)
                {
                    await this.currentTaskRun;
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void StopCurrentRun()
        {
            if (this.currentCancellationTokenSource != null)
            {
                this.currentCancellationTokenSource.Cancel();
            }
        }

        public CommandGroupSettings GetGroupSettings()
        {
            if (!string.IsNullOrEmpty(this.GroupName) && ChannelSession.Settings.CommandGroups.ContainsKey(this.GroupName))
            {
                return ChannelSession.Settings.CommandGroups[this.GroupName];
            }
            return null;
        }

        public virtual bool DoesTextMatchCommand(string text, out IEnumerable<string> arguments)
        {
            return this.DoesTextMatchCommand(text, CommandBase.CommandMatchingRegexFormat, out arguments);
        }

        public bool DoesTextMatchCommand(string text, string commandMatchingRegexFormat, out IEnumerable<string> arguments)
        {
            arguments = null;
            foreach (string command in this.CommandTriggers)
            {
                string regex = string.Format(commandMatchingRegexFormat, Regex.Escape(command));
                Match match = Regex.Match(text, regex, RegexOptions.IgnoreCase);
                if (match != null && match.Success)
                {
                    arguments = text.Substring(match.Index + match.Length).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return true;
                }
            }
            return false;
        }

        protected virtual Task<bool> PerformPreChecks(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(true);
        }

        protected virtual async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            List<ActionBase> actionsToRun = new List<ActionBase>();
            if (this.IsRandomized)
            {
                actionsToRun.Add(this.Actions[RandomHelper.GenerateRandomNumber(this.Actions.Count)]);
            }
            else
            {
                actionsToRun.AddRange(this.Actions);
            }

            for (int i = 0; i < actionsToRun.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                ActionBase action = actionsToRun[i];
                if (action is OverlayAction && ChannelSession.Services.Overlay.IsConnected)
                {
                    ChannelSession.Services.Overlay.StartBatching();
                }

                Logger.Log(LogLevel.Debug, $"Running action for command: {this.Name} - {action.Type}");

                await action.Perform(user, this.platform, arguments, extraSpecialIdentifiers);

                if (action is OverlayAction && ChannelSession.Services.Overlay.IsConnected)
                {
                    if (i == (actionsToRun.Count - 1) || !(actionsToRun[i + 1] is OverlayAction))
                    {
                        await ChannelSession.Services.Overlay.EndBatching();
                    }
                }
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is CommandBase)
            {
                return this.CompareTo((CommandBase)obj);
            }
            return 0;
        }

        public int CompareTo(CommandBase other) { return this.Name.CompareTo(other.Name); }

        public override bool Equals(object obj)
        {
            if (obj is CommandBase)
            {
                return this.Equals((CommandBase)obj);
            }
            return false;
        }

        public bool Equals(CommandBase other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }
}
