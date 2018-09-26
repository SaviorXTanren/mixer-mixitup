using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public enum CommandTypeEnum
    {
        Chat,
        Interactive,
        Event,
        Timer,
        Custom,
        [Name("Action Group")]
        ActionGroup,
        Game,
        Remote,
    }

    [DataContract]
    public abstract class CommandBase
    {
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
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }

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

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public Guid StoreID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public List<string> Commands { get; set; }

        [DataMember]
        public List<ActionBase> Actions { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsBasic { get; set; }

        [DataMember]
        public bool Unlocked { get; set; }

        [DataMember]
        public bool IsRandomized { get; set; }

        [JsonIgnore]
        private Random random = new Random();
        [JsonIgnore]
        private Task currentTaskRun;
        [JsonIgnore]
        private CancellationTokenSource currentCancellationTokenSource;

        public CommandBase()
        {
            this.ID = Guid.NewGuid();
            this.Commands = new List<string>();
            this.Actions = new List<ActionBase>();
            this.IsEnabled = true;
        }

        public CommandBase(string name, CommandTypeEnum type, string command) : this(name, type, new List<string>() { command }) { }

        public CommandBase(string name, CommandTypeEnum type, IEnumerable<string> commands)
            : this()
        {
            this.Name = name;
            this.Type = type;
            this.Commands.AddRange(commands);
        }

        [JsonIgnore]
        public string TypeName { get { return EnumHelper.GetEnumName(this.Type); } }

        [JsonIgnore]
        public virtual bool IsEditable { get { return true; } }

        [JsonIgnore]
        public string CommandsString
        {
            get
            {
                if (this.Commands.Count > 0 && this.Commands.Any(s => s.Contains(" ")))
                {
                    if (this.Commands.Count > 1)
                    {
                        return string.Join(";", this.Commands);
                    }
                    return this.Commands.First() + ";";
                }
                return string.Join(" ", this.Commands);
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> CommandTriggers { get { return this.Commands; } }

        public bool ContainsCommand(string command) { return this.CommandTriggers.Count() > 0 && this.CommandTriggers.Any(c => command.StartsWith(c, StringComparison.InvariantCultureIgnoreCase)); }

        public async Task Perform() { await this.Perform(null); }

        public async Task Perform(IEnumerable<string> arguments) { await this.Perform(await ChannelSession.GetCurrentUser(), arguments); }

        public Task Perform(UserViewModel user, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            if (this.IsEnabled)
            {
                if (arguments == null)
                {
                    arguments = new List<string>();
                }

                if (extraSpecialIdentifiers == null)
                {
                    extraSpecialIdentifiers = new Dictionary<string, string>();
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
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }

                this.currentCancellationTokenSource = new CancellationTokenSource();
                this.currentTaskRun = Task.Run(async () =>
                {
                    try
                    {
                        if (!this.Unlocked && !ChannelSession.Settings.UnlockAllCommands)
                        {
                            await this.AsyncSemaphore.WaitAsync();
                        }

                        await SpecialIdentifierStringBuilder.AssignRandomUserSpecialIdentifierGroup(this.ID);
                        foreach (ActionBase action in this.Actions)
                        {
                            action.AssignRandomUserSpecialIdentifierGroup(this.ID);
                        }

                        await this.PerformInternal(user, arguments, extraSpecialIdentifiers, this.currentCancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException) { }
                    catch (Exception ex) { Util.Logger.Log(ex); }
                    finally
                    {
                        if (!this.Unlocked && !ChannelSession.Settings.UnlockAllCommands)
                        {
                            this.AsyncSemaphore.Release();
                        }
                    }
                }, this.currentCancellationTokenSource.Token);
            }
            return Task.FromResult(0);
        }

        public async Task PerformAndWait(UserViewModel user, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            await this.Perform(user, arguments, extraSpecialIdentifiers);
            if (this.currentTaskRun != null && !this.currentTaskRun.IsCompleted)
            {
                await this.currentTaskRun;
            }
        }

        public void StopCurrentRun()
        {
            if (this.currentCancellationTokenSource != null)
            {
                this.currentCancellationTokenSource.Cancel();
            }
        }

        protected virtual async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            List<ActionBase> actionsToRun = new List<ActionBase>();
            if (this.IsRandomized)
            {
                actionsToRun.Add(this.Actions[this.random.Next(this.Actions.Count)]);
            }
            else
            {
                actionsToRun.AddRange(this.Actions);
            }

            for (int i = 0; i < actionsToRun.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                if (actionsToRun[i] is OverlayAction && ChannelSession.Services.OverlayServer != null)
                {
                    ChannelSession.Services.OverlayServer.StartBatching();
                }

                await actionsToRun[i].Perform(user, arguments, extraSpecialIdentifiers);

                if (actionsToRun[i] is OverlayAction && ChannelSession.Services.OverlayServer != null)
                {
                    if (i == (actionsToRun.Count - 1) || !(actionsToRun[i + 1] is OverlayAction))
                    {
                        await ChannelSession.Services.OverlayServer.EndBatching();
                    }
                }
            }
        }

        protected abstract SemaphoreSlim AsyncSemaphore { get; }
    }
}
