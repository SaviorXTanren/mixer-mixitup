using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class DelayedCountSemaphore
    {
        public event EventHandler Completed = delegate { };

        private int delay;

        private int count;
        private object lockObject = new object();

        public bool Available { get { return this.count == 0; } }

        public DelayedCountSemaphore(int delay)
        {
            this.delay = delay;
        }

        public void Add()
        {
            lock (this.lockObject)
            {
                this.count++;
            }
            Task.Run(() => this.Remove());
        }

        private async Task Remove()
        {
            await Task.Delay(this.delay);

            bool available = false;
            lock (this.lockObject)
            {
                this.count--;
                if (this.count == 0)
                {
                    available = true;
                }
            }

            if (available)
            {
                this.Completed(this, new EventArgs());
            }
        }
    }
}
