using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MixItUp.Desktop.Services.DeveloperAPI.Models
{
    [DataContract]
    public class ChatMessageDeveloperAPIModel
    {
        [Required]
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public bool SendAsStreamer { get; set; }
    }
}
