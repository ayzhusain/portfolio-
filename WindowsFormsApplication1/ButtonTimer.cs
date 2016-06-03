using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WindowsFormsApplication1
{
    class ButtonTimer
    {
        private System.Timers.Timer timer;

        public ButtonTimer(ISynchronizeInvoke synchronizingObject, System.Timers.ElapsedEventHandler callback)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.AutoReset = false;
            timer.Elapsed += callback;
            timer.SynchronizingObject = synchronizingObject;
        }

        public void Start()
        {
            timer.Start();
        }
    }
}
