using PhotoFrames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleFrame {
    internal static class DefaultFrame {
        internal static readonly FrameData Value =
            new FrameReader(
                new MemoryStream(Resources.DefaultFrame),
                null
            ).ReadFrame();
    }
}
