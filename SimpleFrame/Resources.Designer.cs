﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SimpleFrame {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SimpleFrame.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SFPV.
        /// </summary>
        public static string AppName {
            get {
                return ResourceManager.GetString("AppName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] DefaultFrame {
            get {
                object obj = ResourceManager.GetObject("DefaultFrame", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Close All.
        /// </summary>
        public static string NotifyIcon_CloseAllWindows {
            get {
                return ResourceManager.GetString("NotifyIcon_CloseAllWindows", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Quit.
        /// </summary>
        public static string NotifyIcon_Quit {
            get {
                return ResourceManager.GetString("NotifyIcon_Quit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to not found.
        /// </summary>
        public static string PhotoWindow_Error_FileNotFound {
            get {
                return ResourceManager.GetString("PhotoWindow_Error_FileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to invalid path.
        /// </summary>
        public static string PhotoWindow_Error_InvalidFilePath {
            get {
                return ResourceManager.GetString("PhotoWindow_Error_InvalidFilePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to unknown error.
        /// </summary>
        public static string PhotoWindow_Error_Unknown {
            get {
                return ResourceManager.GetString("PhotoWindow_Error_Unknown", resourceCulture);
            }
        }
    }
}
