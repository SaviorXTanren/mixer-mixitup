using Mixer.Base.Model.Interactive;
using MixItUp.Base.Commands;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel
{
    [DataContract]
    public class InteractiveControlViewModel : IEquatable<InteractiveControlViewModel>
    {
        [DataMember]
        public InteractiveControlModel Control { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public InteractiveCommand Command { get; set; }

        public InteractiveControlViewModel(InteractiveControlModel control)
        {
            this.Control = control;
        }

        public InteractiveControlViewModel() { }

        [JsonIgnore]
        public int Cost
        {
            get { return (this.Control is InteractiveButtonControlModel) ? ((InteractiveButtonControlModel)this.Control).cost : -1; }
            set { if (this.Control is InteractiveButtonControlModel) { ((InteractiveButtonControlModel)this.Control).cost = value; } }
        }

        public override bool Equals(object obj)
        {
            if (obj is InteractiveControlViewModel)
            {
                return this.Equals((InteractiveControlViewModel)obj);
            }
            return false;
        }

        public bool Equals(InteractiveControlViewModel other) { return this.Control.controlID.Equals(other.Control.controlID); }

        public override int GetHashCode() { return this.Control.controlID.GetHashCode(); }
    }
}
