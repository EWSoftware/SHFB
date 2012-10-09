using System;
using System.Runtime.InteropServices;

using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;

namespace SandcastleGui
{
    public class MsBuildRunner
    {
        private string _errorMsg = string.Empty;
        private bool _hasError = false;
        private string _projectFile;
        private string _projectName;

        public event BuildMessageEventHandler MsgRaised;

        public event ProjectFinishedEventHandler PrjFinished;

        public event TargetStartedEventHandler TargetStarted;

        public void Run()
        {
            Engine engine = new Engine();
            Project project = new Project(engine);
            try
            {
                project.Load(this._projectFile);
            }
            catch (InvalidProjectFileException)
            {
                this._hasError = true;
                this._errorMsg = "The project file is invalid: " + this._projectFile;
                return;
            }
            BuildLogger logger = new BuildLogger {
                Verbosity = LoggerVerbosity.Diagnostic
            };
            if (this.TargetStarted != null)
            {
                logger.TargetStarted += new TargetStartedEventHandler(this.TargetStarted.Invoke);
            }
            if (this.MsgRaised != null)
            {
                logger.MsgRaised += new BuildMessageEventHandler(this.MsgRaised.Invoke);
            }
            if (this.PrjFinished != null)
            {
                logger.PrjFinished += new ProjectFinishedEventHandler(this.PrjFinished.Invoke);
            }
            engine.RegisterLogger(logger);
            string[] targetNames = project.DefaultTargets.Split(new char[] { ';', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                project.Build(targetNames);
            }
            finally
            {
                engine.UnregisterAllLoggers();
            }
        }

        public string ErrorMsg
        {
            get
            {
                return this._errorMsg;
            }
        }

        public bool HasError
        {
            get
            {
                return this._hasError;
            }
        }

        public string ProjectFile
        {
            get
            {
                return this._projectFile;
            }
            set
            {
                this._projectFile = value;
            }
        }

        public string ProjectName
        {
            get
            {
                return this._projectName;
            }
            set
            {
                this._projectName = value;
            }
        }
    }
}

