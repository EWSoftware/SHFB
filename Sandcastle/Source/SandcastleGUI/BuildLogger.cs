using Microsoft.Build.Framework;

namespace SandcastleGui
{
    public class BuildLogger : ILogger
    {
        private string parameters = string.Empty;
        private LoggerVerbosity verbosity;

        public event BuildMessageEventHandler MsgRaised;

        public event ProjectFinishedEventHandler PrjFinished;

        public event TargetStartedEventHandler TargetStarted;

        public void Initialize(IEventSource eventSource)
        {
            if (this.TargetStarted != null)
            {
                eventSource.TargetStarted += new TargetStartedEventHandler(this.TargetStarted.Invoke);
            }
            if (this.MsgRaised != null)
            {
                eventSource.MessageRaised += new BuildMessageEventHandler(this.MsgRaised.Invoke);
            }
            if (this.PrjFinished != null)
            {
                eventSource.ProjectFinished += new ProjectFinishedEventHandler(this.PrjFinished.Invoke);
            }
        }

        public void Shutdown()
        {
        }

        public string Parameters
        {
            get
            {
                return this.parameters;
            }
            set
            {
                this.parameters = value;
            }
        }

        public LoggerVerbosity Verbosity
        {
            get
            {
                return this.verbosity;
            }
            set
            {
                this.verbosity = value;
            }
        }

        public delegate void ChangedEventHandler(object sender, TargetStartedEventArgs e);
    }
}

