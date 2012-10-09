/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Text;

namespace Microsoft.VisualStudio.Shell
{
    /// <summary>
    /// This attribute registers a path that should be probed for candidate assemblies at assembly load time.
    /// 
    /// For example:
    ///   [...\VisualStudio\10.0\BindingPaths\{5C48C732-5C7F-40f0-87A7-05C4F15BC8C3}]
    ///     "$PackageFolder$"=""
    ///     
    /// This would register the "PackageFolder" (i.e. the location of the pkgdef file) as a directory to be probed
    /// for assemblies to load.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideBindingPathAttribute : RegistrationAttribute
    {
        /// <summary>
        /// An optional SubPath to set after $PackageFolder$. This should be used
        /// if the assemblies to be probed reside in a different directory than
        /// the pkgdef file.
        /// </summary>
        public string SubPath { get; set; }

        private static string GetPathToKey(RegistrationContext context)
        {
            return string.Concat(@"BindingPaths\", context.ComponentType.GUID.ToString("B").ToUpperInvariant());
        }

        /// <inheritdoc />
        public override void Register(RegistrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            using (Key childKey = context.CreateKey(GetPathToKey(context)))
            {
                StringBuilder keyName = new StringBuilder(context.ComponentPath); 
                if (!string.IsNullOrEmpty(SubPath))
                {
                    keyName.Append("\\");
                    keyName.Append(SubPath);
                }

                childKey.SetValue(keyName.ToString(), string.Empty);
            }
        }

        /// <inheritdoc />
        public override void Unregister(RegistrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.RemoveKey(GetPathToKey(context));
        }
    }
}
