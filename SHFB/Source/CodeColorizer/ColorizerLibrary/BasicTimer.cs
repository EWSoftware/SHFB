//=============================================================================
// System  : Code Colorizer Library
// File    : TextColorizerControl.cs
// Updated : 11/17/2006
// Compiler: Microsoft Visual C#
//
// This is used to display syntax colorized blocks of text in an HTML page.
// The original Code Project article by Jonathan can be found at:
// http://www.codeproject.com/csharp/highlightcs.asp.
//
//=============================================================================

#if DEBUG

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace ColorizerLibrary
{
	/// <summary>
	/// A basic timer class
	/// </summary>
	public class BasicTimer
	{
        #region Win32Imports
		[DllImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);  

		[DllImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);
		#endregion
		
		#region Internals
		private long StartTime;
		private long StopTime;
		private long Freq;
		#endregion
		
		/// <summary>
		/// Constructor
		/// </summary>
		public BasicTimer()
		{
			if(QueryPerformanceFrequency(out Freq) == false)
			{
				// high-performance counter not supported 
				throw new Win32Exception(); 
			}
		}
		
		/// <summary>
		/// Starts the timer
		/// </summary>
		public virtual void Start()
		{
			// Lets the waiting threads there work
			Thread.Sleep(0);  

			QueryPerformanceCounter(out StartTime);
		}
		
		/// <summary>
		/// Stops the timer
		/// </summary>
		public virtual void Stop()
		{
			QueryPerformanceCounter(out StopTime);
		}
		
		/// <summary>
		/// Returns the duration of the last call
		/// </summary>
		public double Duration
		{
			get { return (double)(StopTime - StartTime) / (double) Freq; }
		}
	}
}

#endif
