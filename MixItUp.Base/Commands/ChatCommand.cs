namespace MixItUp.Base.Commands
{
    public class ChatCommand : CommandBase
    {
        public string Command { get; set; }

        public ChatCommand(string name, string command)
            : base(name)
        {
            this.Command = command;
        }
    }
}
