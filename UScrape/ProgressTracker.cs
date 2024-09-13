using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UScrape
{
    public class ProgressTracker
    {
        public enum Statuses
        {
            Running,
            Completed,
            Error,
            Aborted,
        }

        private object Mutex { get; set; }

        // Progression is a double between 0 and 1
        private double progression;
        public double Progression
        {
            get
            {
                lock (Mutex)
                {
                    return progression;
                }
            }
            set
            {
                lock (Mutex)
                {
                    progression = value;
                    CallUpdate();
                }
            }
        }
        private string message;
        public string Message
        {
            get
            {
                lock (Mutex)
                {
                    return message;
                }
            }
            set
            {
                lock (Mutex)
                {
                    message = value;
                    CallUpdate();
                }
            }
        }
        private string data;
        public string Data
        {
            get
            {
                lock (Mutex)
                {
                    return data;
                }
            }
            set
            {
                lock (Mutex)
                {
                    data = value;
                    CallUpdate();
                }
            }
        }
        private DateTime starttime;
        public DateTime StartTime
        {
            get
            {
                lock (Mutex)
                {
                    return starttime;
                }
            }
            set
            {
                lock (Mutex)
                {
                    starttime = value;
                    CallUpdate();
                }
            }
        }
        private Statuses status;
        public Statuses Status
        {
            get
            {
                lock (Mutex)
                {
                    return status;
                }
            }
            set
            {
                lock (Mutex)
                {
                    status = value;
                    CallUpdate();

                    switch (status)
                    {
                        case Statuses.Completed:
                            CallCompletion();
                            break;
                        case Statuses.Error:
                            CallError();
                            break;
                        case Statuses.Aborted:
                            CallAbortion();
                            break;
                    }
                }
            }
        }
        private DateTime LastUpdate { get; set; }
        private List<(Action, TimeSpan)> UpdateCallbacks { get; set; }
        private List<Action> CompletionCallbacks { get; set; }
        private List<Action> ErrorCallbacks { get; set; }
        private List<Action> AbortionCallbacks { get; set; }

        public ProgressTracker()
        {
            Mutex = new object();
            progression = 0;
            message = "Initializing";
            data = "";
            starttime = DateTime.Now;
            status = Statuses.Running;
            UpdateCallbacks = new List<(Action, TimeSpan)>();
            CompletionCallbacks = new List<Action>();
            ErrorCallbacks = new List<Action>();
            AbortionCallbacks = new List<Action>();
            LastUpdate = DateTime.MinValue;
        }

        public void Abort()
        {
            if (Status == Statuses.Running)
                Status = Statuses.Aborted;
        }

        public void OnUpdate(Action callback)
        {
            OnUpdate(callback, TimeSpan.Zero);
        }

        public void OnUpdate(Action callback, TimeSpan interval)
        {
            lock (Mutex)
            {
                UpdateCallbacks.Add((callback, interval));
            }
        }

        private void CallUpdate()
        {
            lock (Mutex)
            {
                foreach ((Action callback, TimeSpan interval) elem in UpdateCallbacks)
                {
                    if (DateTime.Now - LastUpdate > elem.interval || Status != Statuses.Running)
                    {
                        elem.callback();
                        LastUpdate = DateTime.Now;
                    }
                }
            }
        }

        public void OnCompletion(Action callback)
        {
            lock (Mutex)
            {
                CompletionCallbacks.Add(callback);
            }
        }

        private void CallCompletion()
        {
            lock (Mutex)
            {
                foreach (Action callback in CompletionCallbacks)
                {
                    callback();
                }
            }
        }

        public void OnError(Action callback)
        {
            lock (Mutex)
            {
                ErrorCallbacks.Add(callback);
            }
        }

        private void CallError()
        {
            lock (Mutex)
            {
                foreach (Action callback in ErrorCallbacks)
                {
                    callback();
                }
            }
        }

        public void OnAbortion(Action callback)
        {
            lock (Mutex)
            {
                AbortionCallbacks.Add(callback);
            }
        }

        private void CallAbortion()
        {
            lock (Mutex)
            {
                foreach (Action callback in AbortionCallbacks)
                {
                    callback();
                }
            }
        }
    }
}
