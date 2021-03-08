using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public interface IDispatcher
    {
        bool OnDispatcherThread();

        void Invoke(Action func);

        Task InvokeAsync(Func<Task> func);
    }

    public static class DispatcherHelper
    {
        public static IDispatcher Dispatcher { get; private set; }

        public static void RegisterDispatcher(IDispatcher dispatcher) { DispatcherHelper.Dispatcher = dispatcher; }
    }
}
