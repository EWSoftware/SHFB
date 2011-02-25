//=============================================================================
// System  : Code Colorizer Library
// File    : TextColorizerControl.cs
// Author  : Jonathan de Halleux, (c) 2003
// Updated : 02/21/2007
// Compiler: Microsoft Visual C#
//
// This is used to display syntax colorized blocks of text in an HTML page.
// The original Code Project article by Jonathan can be found at:
// http://www.codeproject.com/csharp/highlightcs.asp.
//
// Modifications by Eric Woodruff (Eric@EWoodruff.us) 11/2006:
//
//      Conditionalized code to remove it from release builds.
//
//=============================================================================

#if DEBUG

using System;

namespace ColorizerLibrary
{
	/// <summary>
	/// A timer with averaging and counting
	/// </summary>
	public class AvgTimer : BasicTimer
	{
        #region Private data members

		private long totalQuantity, runNumber;
        private double totalTime;

        #endregion

        #region Properties
        /// <summary>
        /// Returns the sum of timer runs
        /// </summary>
        /// <value>Sum of timer runs</value>
        public double TotalDuration
        {
            get { return totalTime; }
        }

        /// <summary>
        /// Returns the number of runs that the timer has done
        /// </summary>
        /// <value>Run count</value>
        public long RunCount
        {
            get { return runNumber; }
        }

        /// <summary>
        /// Returns a normalized time measurement
        /// </summary>
        /// <value>Total time divided by total quantity</value>
        public double DurationPerQuantity
        {
            get { return (double)totalTime / (double)totalQuantity; }
        }

        /// <summary>
        /// Returns average time per run
        /// </summary>
        /// <value>Total time divided by run number</value>
        public double DurationPerRun
        {
            get { return (double)totalTime / (double)runNumber; }
        }
        #endregion

		#region Constructors
		/// <summary>
		/// An averaging/normalizing timer.
		/// </summary>
		/// <example>In this example, we measure the time to parse a string. The string length will be passed to the
		/// timer to have a sec/char time measurement.
		/// <code>
		/// string str;
		/// AvgTimer timer = new AvgTimer();
		/// //Start timer
		/// timer.Start( str.Length);
		/// ... // do things
		/// timer.Stop();
		/// result = "The processing timer per character is " + timer.DurationPerQuantity;
		/// </code>
		/// </example>
		public AvgTimer()
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Starts timer
		/// </summary>
		/// <param name="quantity">Quantity that will be processed</param>
		public void Start(long quantity)
		{
			totalQuantity += quantity;
			base.Start();
		}

		/// <summary>
		/// Stops the timer
		/// </summary>
		public override void Stop()
		{
			base.Stop();
			totalTime += this.Duration;
			++runNumber;
		}

		/// <summary>
		/// Resests internal run counter and time integrator
		/// </summary>
		public void Reset()
		{
			totalQuantity = 0;
			totalTime = 0;
			runNumber = 0;
		}
		#endregion
	}
}

#endif
