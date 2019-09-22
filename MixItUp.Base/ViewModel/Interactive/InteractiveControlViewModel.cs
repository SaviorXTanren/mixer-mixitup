using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Commands;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.Interactive
{
    [DataContract]
    public class InteractiveControlViewModel : IEquatable<InteractiveControlViewModel>
    {
        [DataMember]
        public MixPlayControlModel Control { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public MixPlayCommand Command { get; set; }

        public InteractiveControlViewModel(MixPlayControlModel control)
        {
            this.Control = control;
        }

        public InteractiveControlViewModel() { }

        [JsonIgnore]
        public int Cost
        {
            get { return (this.Control is MixPlayButtonControlModel) ? ((MixPlayButtonControlModel)this.Control).cost.GetValueOrDefault() : 0; }
            set { if (this.Control is MixPlayButtonControlModel) { ((MixPlayButtonControlModel)this.Control).cost = value; } }
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
