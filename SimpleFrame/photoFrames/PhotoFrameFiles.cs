using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Collections;

namespace SimpleFrame {
    internal static class PhotoFrameFiles {
        private static readonly string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) + @"\frames\";

        /// <summary>
        /// Gets the paths of all the frame files.
        /// </summary>
        public static IReadOnlyCollection<string?> GetFiles() {
            string[] loadedPaths;

            try {
                loadedPaths = Directory.GetFiles(dir, "*.pfz");
            } catch (DirectoryNotFoundException) {
                //there are lots of exceptions that this can throw and we can't recover from any of them :p
                Directory.CreateDirectory(dir);
                //let fail this time
                loadedPaths = Directory.GetFiles(dir, "*.pfz");
            }

            List<string?> paths = new List<string?>(loadedPaths.Length + 1);

            //represents default frame
            paths.Add(null);
            paths.AddRange(loadedPaths);
            return paths;
        }
    }
}
