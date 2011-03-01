using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TestDoc.Generics.MissingMembersBug
{
    #region Action delegate bug
    //=====================================================================

    /// <summary>
    /// Due to the Action delegate, members are not listed if compiled as
    /// a .NET 4.0 assembly (May 2008 release).
    /// </summary>
    public interface Intro
    {
        /// <summary>
        /// Some method.
        /// </summary>
        void Method(Action<int, int> i);
    }

    /// <summary>
    /// Due to the Action delegate, members are not listed if compiled as
    /// a .NET 4.0 assembly (May 2008 release).
    /// </summary>
    public class Class1 : Intro
    {
        // Note also that inheritdoc doesn't work here either.  Most likely it
        // is related to the issue demonstrated by Generics.InterfaceFailure.TestClass.
        /// <inheritdoc />
        public void Method(Action<int, int> i)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region LINQ bug
    //=====================================================================

    /// <summary>
    /// Another May 2008 missing member bug in .NET 4.0 assemblies
    /// </summary>
    public abstract class BugTest
    {
        /// <summary>
        /// This Public Property should show up in the help file
        /// </summary>
        public String PublicProperty { get; set; }

        /// <summary>
        /// This protected Method should also show up in the help
        /// </summary>
        protected virtual Dictionary<string, object> GetPropertyValues()
        {
            // If this piece of code is uncommented, the members of this class will not show up
            // in the generated Help file. Besides this, you cannot see the members of this class
            // when you click on APIFilters and navigate to this class.
            #region Sandcastle bug
            var dict = this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => (x.CanRead && x.CanWrite))
                .ToDictionary(x => x.Name, x => x.GetValue(this, null));
            #endregion

            // This piece of code does the same like the code above, 
            // but now Sandcastle will generate the correct help file.

            #region Workaround
            //Dictionary<string, object> dict = new Dictionary<string, object>();
            //var propertyInfos = this.GetType()
            //                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            //foreach (var item in propertyInfos)
            //    if (item.CanRead && item.CanWrite)
            //        dict.Add(item.Name, item.GetValue(this, null));
            #endregion

            return dict;
        }
    }
    #endregion

    #region Tuple bug
    //=====================================================================

    /// <summary>
    /// May 2008 generics bug
    /// </summary>
    public class TupleBug
    {
        /// <summary>
        /// String property
        /// </summary>
        public string StringProperty { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TupleBug()
        {
        }

        /// <summary>
        /// The use of the Tuple causes all members to be removed in the May
        /// 2008 release of Sandcastle.
        /// </summary>
        /// <param name="value">A value</param>
        public void MissingMembers(Tuple<string, int> value)
        {
        }
    }
    #endregion

}
