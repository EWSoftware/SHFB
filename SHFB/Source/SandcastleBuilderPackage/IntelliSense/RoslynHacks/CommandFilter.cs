namespace SandcastleBuilder.Package.IntelliSense.RoslynHacks
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;

    using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget;
    using OLECMD = Microsoft.VisualStudio.OLE.Interop.OLECMD;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;
    using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
    using VsMenus = Microsoft.VisualStudio.Shell.VsMenus;

    /// <summary>
    /// This is the base class for implementations of <see cref="IOleCommandTarget"/> in managed code.
    /// </summary>
    /// <threadsafety/>
    /// <preliminary/>
    [ComVisible(true)]
    internal abstract class CommandFilter : IOleCommandTarget, IDisposable
    {
        /// <summary>
        /// This is the backing field for the <see cref="Enabled"/> property.
        /// </summary>
        private bool _connected;

        /// <summary>
        /// This field stores the next <see cref="IOleCommandTarget"/> in the filter chain.
        /// </summary>
        private IOleCommandTarget _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandFilter"/> class.
        /// </summary>
        protected CommandFilter()
        {
        }

        /// <summary>
        /// Gets or sets whether the command filter is currently enabled.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the current instance has been disposed.</exception>
        public bool Enabled
        {
            get
            {
                ThrowIfDisposed();
                return _connected;
            }

            set
            {
                ThrowIfDisposed();
                if (_connected == value)
                    return;

                if (value)
                {
                    _next = Connect();
                    _connected = value;
                }
                else
                {
                    try
                    {
                        Disconnect();
                    }
                    catch (Exception e)
                    {
                        if (!IsDisposing || ErrorHandler.IsCriticalException(e))
                            throw;
                    }
                    finally
                    {
                        _next = null;
                        _connected = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether the current instance is disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the current instance is currently being disposed.
        /// </summary>
        protected bool IsDisposing
        {
            get;
            private set;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsDisposing)
                throw new InvalidOperationException("Detected a recursive invocation of Dispose");

            try
            {
                IsDisposing = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            finally
            {
                IsDisposing = false;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by this instance.
        /// </summary>
        /// <remarks>
        /// When <paramref name="disposing"/> is <see langword="false"/>, the default implementation
        /// sets <see cref="Enabled"/> to <see langword="false"/>.
        /// </remarks>
        /// <param name="disposing"><see langword="true"/> if this method is being called from <see cref="Dispose()"/>; otherwise, <see langword="false"/> if this method is being called from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Enabled = false;
            }
        }

        /// <summary>
        /// Enables the command filter by connecting it to a chain of command targets.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// Do not call this method directly. This method is called as necessary when <see cref="Enabled"/> is set to <see langword="true"/>.
        /// </note>
        /// </remarks>
        /// <returns>The next command target in the chain.</returns>
        protected abstract IOleCommandTarget Connect();

        /// <summary>
        /// Disables the command filter by disconnecting it from a chain of command targets.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// Do not call this method directly. This method is called as necessary when <see cref="Enabled"/> is set to <see langword="false"/>.
        /// </note>
        /// </remarks>
        protected abstract void Disconnect();

        /// <summary>
        /// Throw an <see cref="ObjectDisposedException"/> if the current instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the current instance has been disposed.</exception>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private int ExecCommand(ref Guid guidCmdGroup, uint nCmdID, OLECMDEXECOPT nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int rc = VSConstants.S_OK;

            if (!HandlePreExec(ref guidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut) && _next != null)
            {
                // Pass it along the chain.
                rc = this.InnerExec(ref guidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                if (!ErrorHandler.Succeeded(rc))
                    return rc;

                HandlePostExec(ref guidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return rc;
        }

        /// <summary>
        /// This method supports the implementation for commands which are directly implemented by this command filter.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <see langword="false"/> for all commands.
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="executionOptions">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are specific to a particular command.</param>
        /// <returns>
        /// <see langword="true"/> if this command filter handled the command; otherwise, <see langword="false"/>
        /// to call the next <see cref="IOleCommandTarget"/> in the chain.
        /// </returns>
        protected virtual bool HandlePreExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            return false;
        }

        private int InnerExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (_next != null)
                return _next.Exec(ref commandGroup, commandId, (uint)executionOptions, pvaIn, pvaOut);

            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// This method supports specialized handling in response to commands that are successfully handled by another command target.
        /// </summary>
        /// <remarks>
        /// This method is only called if <see cref="HandlePreExec"/> for the current instance returned <see langword="false"/> and
        /// the next command target in the chain returned a value indicating the command execution succeeded.
        ///
        /// <para>
        /// The default implementation is empty.
        /// </para>
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="executionOptions">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are specific to a particular command.</param>
        protected virtual void HandlePostExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
        }

        protected virtual int QueryParameterList(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Gets the current status of a particular command.
        /// </summary>
        /// <remarks>
        /// The base implementation returns 0 for all commands, indicating the command is not supported by this command filter.
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <returns>A collection of <see cref="OLECMDF"/> flags indicating the current status of a particular command, or 0 if the command is not supported by the current command filter.</returns>
        protected virtual OLECMDF QueryCommandStatus(ref Guid commandGroup, uint commandId)
        {
            return default(OLECMDF);
        }

        /// <inheritdoc/>
        int IOleCommandTarget.Exec(ref Guid guidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ushort lo = (ushort)(nCmdexecopt & (uint)0xffff);
            ushort hi = (ushort)(nCmdexecopt >> 16);

            switch (lo)
            {
            case (ushort)OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP:
                if ((nCmdexecopt >> 16) == VsMenus.VSCmdOptQueryParameterList)
                {
                    return QueryParameterList(ref guidCmdGroup, nCmdID, (OLECMDEXECOPT)nCmdexecopt, pvaIn, pvaOut);
                }
                break;

            default:
                return ExecCommand(ref guidCmdGroup, nCmdID, (OLECMDEXECOPT)nCmdexecopt, pvaIn, pvaOut);
            }

            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <inheritdoc/>
        int IOleCommandTarget.QueryStatus(ref Guid guidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            Guid cmdGroup = guidCmdGroup;
            for (uint i = 0; i < cCmds; i++)
            {
                OLECMDF status = QueryCommandStatus(ref cmdGroup, prgCmds[i].cmdID);
                if (status == default(OLECMDF) && _next != null)
                {
                    if (_next != null)
                        return _next.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
                    else
                        return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                }

                prgCmds[i].cmdf = (uint)status;
            }

            return VSConstants.S_OK;
        }
    }
}
