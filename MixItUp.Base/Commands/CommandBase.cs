using Mixer.Base.Util;
using Mixer.Base.ViewModel;
using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MixItUp.Base.Commands
{
    public enum CommandTypeEnum
    {
        Chat,
        Interactive,
        Event,
        Timer
    }

    [DataContract]
    public class CommandBase
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public string Command { get; set; }

        [XmlIgnore]
        public List<ActionBase> Actions { get; set; }

        [DataMember]
        public List<SerializableAction> SerializedActions { get; set; }

        [XmlIgnore]
        public string TypeName { get { return EnumHelper.GetEnumName(this.Type); } }

        public CommandBase()
        {
            this.Actions = new List<ActionBase>();
        }

        public CommandBase(string name, CommandTypeEnum type, string command, IEnumerable<ActionBase> actions)
        {
            this.Name = name;
            this.Type = type;
            this.Command = command;
            this.Actions = new List<ActionBase>(actions);
        }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (arguments == null)
            {
                arguments = new List<string>();
            }

            List<Task> taskActionsToPerform = new List<Task>();
            foreach (ActionBase action in this.Actions)
            {
                taskActionsToPerform.Add(action.Perform(user, arguments));

                if (action.Type == ActionTypeEnum.Wait)
                {
                    await Task.WhenAll(taskActionsToPerform);
                    taskActionsToPerform.Clear();
                }
            }
            await Task.WhenAll(taskActionsToPerform);
        }

        public void SerializeActions()
        {
            this.SerializedActions = new List<SerializableAction>();
            foreach (ActionBase action in this.Actions)
            {
                this.SerializedActions.Add(action.Serialize());
            }
        }

        public void DeserializeActions()
        {
            foreach (SerializableAction serializedAction in this.SerializedActions)
            {
                ActionBase action = null;
                switch (serializedAction.Type)
                {
                    case ActionTypeEnum.Chat:
                        action = new ChatAction(serializedAction.Values[0], bool.Parse(serializedAction.Values[1]));
                        break;
                    case ActionTypeEnum.Cooldown:
                        break;
                    case ActionTypeEnum.Currency:
                        action = new CurrencyAction(int.Parse(serializedAction.Values[0]));
                        break;
                    case ActionTypeEnum.ExternalProgram:
                        action = new ExternalProgramAction(serializedAction.Values[0], serializedAction.Values[1], bool.Parse(serializedAction.Values[2]));
                        break;
                    case ActionTypeEnum.Giveaway:
                        break;
                    case ActionTypeEnum.Input:
                        action = new InputAction(new List<InputTypeEnum>() { EnumHelper.GetEnumValueFromString<InputTypeEnum>(serializedAction.Values[0]) });
                        break;
                    case ActionTypeEnum.Overlay:
                        action = new OverlayAction(serializedAction.Values[0], int.Parse(serializedAction.Values[1]), int.Parse(serializedAction.Values[2]), int.Parse(serializedAction.Values[3]));
                        break;
                    case ActionTypeEnum.Sound:
                        action = new SoundAction(serializedAction.Values[0], int.Parse(serializedAction.Values[1]));
                        break;
                }

                if (action != null)
                {
                    this.Actions.Add(action);
                }
            }
        }
    }
}
