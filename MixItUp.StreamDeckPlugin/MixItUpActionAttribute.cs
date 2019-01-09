using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.StreamDeckPlugin
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MixItUpActionAttribute : Attribute
    {
        public string ActionName { get; private set; }

        public MixItUpActionAttribute(string actionName)
        {
            this.ActionName = actionName;
        }
    }
}
