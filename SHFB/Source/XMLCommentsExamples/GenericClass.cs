//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : GenericClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
//
// This class is used to demonstrate the various XML comments elements related to generics.  It serves no
// useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/05/2012  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: typeparamref

using System;

namespace XMLCommentsExamples
{
    #region typeparam/typeparamref Examples

    /// <summary>
    /// This class is used to demonstrate the various XML comments elements
    /// related to generics.  It serves no useful purpose.
    /// </summary>
    /// <typeparam name="T1">This is the first generic argument.</typeparam>
    /// <typeparam name="T2">This is the second generic argument constrained to
    /// be or derive from <see cref="EventArgs"/>.</typeparam>
    /// <conceptualLink target="163cae15-9020-4095-9b9c-da134b5b496c" />
    public class GenericClass<T1, T2> where T2 : EventArgs
    {
        /// <summary>
        /// This is a property that gets or sets an instance of the type specified
        /// by the generic type argument <typeparamref name="T1"/>.
        /// </summary>
        /// <conceptualLink target="073a5ae1-828f-4bab-b0cb-438cefb5e9fb" />
        public T1 Property { get; set; }

        /// <summary>
        /// This is a method with an argument.
        /// </summary>
        /// <param name="argument"> This is an argument of the type specified by
        /// the generic type argument <typeparamref name="T1"/>.</param>
        /// <conceptualLink target="073a5ae1-828f-4bab-b0cb-438cefb5e9fb" />
        public void Method(T1 argument)
        {
        }

        /// <summary>
        /// This is a generic method that takes two other generic types
        /// </summary>
        /// <typeparam name="T3">This is a generic type argument for the method
        /// argument.</typeparam>
        /// <typeparam name="T4">This is a generic type argument for the return
        /// value.</typeparam>
        /// <param name="argument">This is an argument of the type specified by
        /// the generic type argument <typeparamref name="T3"/>.</param>
        /// <returns>The default value of the type specified by the generic type
        /// argument <typeparamref name="T4"/>.</returns>
        /// <conceptualLink target="163cae15-9020-4095-9b9c-da134b5b496c" />
        /// <conceptualLink target="073a5ae1-828f-4bab-b0cb-438cefb5e9fb" />
        public T4 GenericMethod<T3, T4>(T3 argument)
        {
            return default;
        }

        /// <summary>
        /// This is an event that takes a generic argument.
        /// </summary>
        /// <remarks>The <see cref="Delegate">delegate</see> for this event is
        /// <see cref="EventHandler{T}"/> bound to the type specified by the
        /// generic type argument <typeparamref name="T2"/>.
        /// </remarks>
        /// <conceptualLink target="073a5ae1-828f-4bab-b0cb-438cefb5e9fb" />
        public event EventHandler<T2> SomethingHappened;

        /// <summary>
        /// This is a protected virtual method used to raise the
        /// <see cref="SomethingHappened"/> event.
        /// </summary>
        /// <param name="e">Arguments for the event of the type specified by
        /// the generic type argument <typeparamref name="T2"/>.</param>
        /// <conceptualLink target="073a5ae1-828f-4bab-b0cb-438cefb5e9fb" />
        protected virtual void OnSomethingHappened(T2 e)
        {
            SomethingHappened?.Invoke(this, e);
        }
    }
    #endregion
}
