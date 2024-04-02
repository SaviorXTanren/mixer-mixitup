using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MixItUp.WPF.Util
{
    public class WindowsDispatcher : IDispatcher
    {
        private Dispatcher dispatcher;

        public WindowsDispatcher(Dispatcher dispatcher) { this.dispatcher = dispatcher; }

        public bool OnDispatcherThread() { return this.dispatcher.CheckAccess(); }

        public void Invoke(Action func)
        {
            if (this.OnDispatcherThread())
            {
                func();
            }
            else
            {
                this.dispatcher.Invoke(func);
            }
        }

        public async Task InvokeAsync(Func<Task> func)
        {
            if (this.OnDispatcherThread())
            {
                await func();
            }
            else
            {
                await this.dispatcher.Invoke(func);
            }
        }
    }
}
