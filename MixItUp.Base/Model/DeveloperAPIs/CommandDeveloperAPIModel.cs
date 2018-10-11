using MixItUp.Base.Commands;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.DeveloperAPIs
{
    [DataContract]
    public class CommandDeveloperAPIModel
    {
        [Required]
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        public CommandDeveloperAPIModel()
        {
        }

        public CommandDeveloperAPIModel(CommandBase command)
        {
            this.ID = command.ID;
            this.Name = command.Name;
            this.IsEnabled = command.IsEnabled;
        }
    }
}
