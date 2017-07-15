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
        public List<SerializableAction> SerializedActions
        {
            get
            {
                List<SerializableAction> serializedActions = new List<SerializableAction>();
                foreach (ActionBase action in this.Actions)
                {
                    serializedActions.Add(action.Serialize());
                }
                return serializedActions;
            }
            set
            {
                if (value != null)
                {
                    foreach (SerializableAction serializedAction in value)
                    {
                        ActionBase action = null;
                        switch (action.Type)
                        {
                            case ActionTypeEnum.Chat:
                                action = new ChatAction(serializedAction.Values[0]);
                                break;
                            case ActionTypeEnum.Cooldown:
                                break;
                            case ActionTypeEnum.Currency:
                                break;
                            case ActionTypeEnum.ExternalProgram:
                                action = new ExternalProgramAction(serializedAction.Values[0], serializedAction.Values[1], bool.Parse(serializedAction.Values[2]));
                                break;
                            case ActionTypeEnum.Giveaway:
                                break;
                            case ActionTypeEnum.Input:
                                break;
                            case ActionTypeEnum.Overlay:
                                break;
                            case ActionTypeEnum.Sound:
                                action = new SoundAction(serializedAction.Values[0], float.Parse(serializedAction.Values[1]));
                                break;
                            case ActionTypeEnum.Whisper:
                                action = new WhisperAction(serializedAction.Values[0]);
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

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (arguments == null)
            {
                arguments = new List<string>();
            }

            foreach (ActionBase action in this.Actions)
            {
                await action.Perform(user, arguments);
            }
        }
    }
}
