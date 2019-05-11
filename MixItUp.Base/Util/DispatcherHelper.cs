using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class DispatcherHelper
    {
        private static Func<Func<Task>, Task> dispatcher;

        public static void RegisterDispatcher(Func<Func<Task>, Task> dispatcher) { DispatcherHelper.dispatcher = dispatcher; }

        public static async Task InvokeDispatcher(Func<Task> func) { await DispatcherHelper.dispatcher(func); }
    }
}
