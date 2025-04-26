/********************************************************************************************

Copyright (c) Microsoft Corporation 
All rights reserved. 

Microsoft Public License: 

This license governs use of the accompanying software. If you use the software, you 
accept this license. If you do not accept the license, do not use the software. 

1. Definitions 
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the 
same meaning here as under U.S. copyright law. 
A "contribution" is the original software, or any additions or changes to the software. 
A "contributor" is any person that distributes its contribution under this license. 
"Licensed patents" are a contributor's patent claims that read directly on its contribution. 

2. Grant of Rights 
(A) Copyright Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free copyright license to reproduce its contribution, prepare derivative works of 
its contribution, and distribute its contribution or any derivative works that you create. 
(B) Patent Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free license under its licensed patents to make, have made, use, sell, offer for 
sale, import, and/or otherwise dispose of its contribution in the software or derivative 
works of the contribution in the software. 

3. Conditions and Limitations 
(A) No Trademark License- This license does not grant you rights to use any contributors' 
name, logo, or trademarks. 
(B) If you bring a patent claim against any contributor over patents that you claim are 
infringed by the software, your patent license from such contributor to the software ends 
automatically. 
(C) If you distribute any portion of the software, you must retain all copyright, patent, 
trademark, and attribution notices that are present in the software. 
(D) If you distribute any portion of the software in source code form, you may do so only 
under this license by including a complete copy of this license with your distribution. 
If you distribute any portion of the software in compiled or object code form, you may only 
do so under a license that complies with this license. 
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give 
no express warranties, guarantees or conditions. You may have additional consumer rights 
under your local laws which this license cannot change. To the extent permitted under your 
local laws, the contributors exclude the implied warranties of merchantability, fitness for 
a particular purpose and non-infringement.

********************************************************************************************/

namespace Microsoft.VisualStudio.Project
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class UIThread : IDisposable
    {
        private WindowsFormsSynchronizationContext synchronizationContext;
        private Thread uithread; 

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static readonly UIThread instance = new UIThread();

        internal UIThread()
        {
            this.Initialize();
        }

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static UIThread Instance => instance;

        /// <summary>
        /// Checks whether this is the UI thread.
        /// </summary>
        public bool IsUIThread => this.uithread == Thread.CurrentThread;

        #region IDisposable Members
        /// <summary>
        /// Dispose implementation.
        /// </summary>
        public void Dispose()
        {
            if (this.synchronizationContext != null)
            {
                this.synchronizationContext.Dispose();
            }
        }

        #endregion

        [Conditional("DEBUG")]
        internal void MustBeCalledFromUIThread()
        {
            Debug.Assert(this.uithread == System.Threading.Thread.CurrentThread, "This must be called from the GUI thread");
        }

        /// <summary>
        /// Runs an action asynchronously on an associated forms synchronization context.
        /// </summary>
        /// <param name="a">The action to run</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void Run(Action a)
        {
            Debug.Assert(this.synchronizationContext != null, "The SynchronizationContext must be captured before calling this method");
#if DEBUG
            StackTrace stackTrace = new StackTrace(true);
#endif
            this.synchronizationContext.Post(delegate(object ignore)
            {
                try
                {
                    this.MustBeCalledFromUIThread();
                    a();
                }
#if DEBUG
                catch (Exception e)
                {
                    // swallow, random exceptions should not kill process
                    Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, "UIThread.Run caught and swallowed exception: {0}\n\noriginally invoked from stack:\n{1}", e.ToString(), stackTrace.ToString()));
                }
#else
                catch (Exception)
                {
                    // swallow, random exceptions should not kill process
                }
#endif
            }, null);

        }

        /// <summary>
        /// Performs a callback on the UI thread, blocking until the action completes
        /// </summary>
        internal static T DoOnUIThread<T>(Func<T> callback)
        {
            // Switch to the UI thread to execute the callback.  This looks rather odd but it's the recommended
            // way to switch contexts in Visual Studio now.  Since we aren't asynchronous here, this runs an
            // asynchronous delegate and waits for it to finish.
            var result = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                return callback();
            });

            return result;
        }

        /// <summary>
        /// Performs a callback on the UI thread, blocking until the action completes
        /// </summary>
        internal static void DoOnUIThread(Action callback)
        {
            // Switch to the UI thread to execute the callback.  This looks rather odd but it's the recommended
            // way to switch contexts in Visual Studio now.  Since we aren't asynchronous here, this runs an
            // asynchronous delegate and waits for it to finish.
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                callback();
            });
        }

        /// <summary>
        /// Initializes this object.
        /// </summary>
        private void Initialize()
        {
            this.uithread = Thread.CurrentThread;

            if (this.synchronizationContext == null)
            {
#if DEBUG
                 // This is a handy place to do this, since the product and all interesting unit tests
                 // must go through this code path.
                 AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(delegate(object sender, UnhandledExceptionEventArgs args)
                 {
                     if (args.IsTerminating)
                     {
                         string s = String.Format(CultureInfo.InvariantCulture, "An unhandled exception is about to terminate the process.  Exception info:\n{0}", args.ExceptionObject.ToString());
                         Debug.Assert(false, s);
                     }
                 });
#endif
                this.synchronizationContext = new WindowsFormsSynchronizationContext();
            }
            else
            {
                 // Make sure we are always capturing the same thread.
                 Debug.Assert(this.uithread == Thread.CurrentThread);
            }
        }       
    }
}
