using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;

namespace PhotoFrames {
    internal class ResizeOperation {
        private readonly Point originalPoint;
        private readonly Rect originalRect;
        private readonly bool fromTop;
        private readonly bool fromBottom;
        private readonly bool fromLeft;
        private readonly bool fromRight;

        /// <summary>
        /// Creates and starts a resize operation.
        /// </summary>
        /// <param name="originalPoint">anchor point </param>
        /// <param name="originalRect"> the starting rect before the resize </param>
        /// <param name="fromTop">      if dragging from a top side handle </param>
        /// <param name="fromBottom">   if dragging from a bottom side handle </param>
        /// <param name="fromLeft">     if dragging from a left side handle </param>
        /// <param name="fromRight">    if dragging from a right side handle </param>
        internal ResizeOperation(Point originalPoint, Rect originalRect, bool fromTop, bool fromBottom, bool fromLeft, bool fromRight) {
            this.originalPoint = originalPoint;
            this.originalRect = originalRect;
            this.fromTop = fromTop;
            this.fromBottom = fromBottom;
            this.fromLeft = fromLeft;
            this.fromRight = fromRight;
        }

        internal Rect Evaluate(Point newPoint) {
            double xDiff = newPoint.X - originalPoint.X;
            double yDiff = newPoint.Y - originalPoint.Y;

            //need to flip if dragging from the other side
            if (fromLeft) xDiff *= -1;
            if (fromTop) yDiff *= -1;

            Rect rect = originalRect;
            if (fromLeft | fromRight) rect.Width = Math.Max(0, rect.Width + xDiff);
            if (fromTop | fromBottom) rect.Height = Math.Max(0, rect.Height + yDiff);
            if (fromLeft) rect.X -= xDiff;
            if (fromTop) rect.Y -= yDiff;
            return rect;
        }

    }

}