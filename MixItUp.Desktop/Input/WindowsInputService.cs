using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace MixItUp.Input
{
    public class WindowsInputService : IInputService
    {
        private InputSimulator simulator = new InputSimulator();

        public Task SendInput(IEnumerable<InputTypeEnum> inputs)
        {
            List<VirtualKeyCode> keyCodes = new List<VirtualKeyCode>();
            foreach (InputTypeEnum input in inputs)
            {
                if (Enum.IsDefined(typeof(InputTypeEnum), input))
                {
                    VirtualKeyCode virtualCode = (VirtualKeyCode)((int)input);
                    keyCodes.Add(virtualCode);
                }
            }

            this.simulator.Keyboard.KeyPress(keyCodes.ToArray());

            return Task.FromResult(0);
        }
    }
}
