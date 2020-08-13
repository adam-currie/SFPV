using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Collections;

namespace SimpleFrame {
    internal static class PhotoFiles {
        private static readonly string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) + @"\frames\";

        /// <summary>
        /// Gets the paths of all the frame files.
        /// </summary>
        public static string[] GetPaths() {
            try {
                return Directory.GetFiles(dir, "*.pfz");
            } catch (DirectoryNotFoundException) {
                //there are lots of exceptions that this can throw and we can't recover from any of them
                Directory.CreateDirectory(dir);
                //let fail this time
                return Directory.GetFiles(dir, "*.pfz");
            }
        }
    }
}
