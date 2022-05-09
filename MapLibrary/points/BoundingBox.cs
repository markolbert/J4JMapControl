using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.MapLibrary
{
    public record BoundingBox
    {
        public const double MinDelta = 1e-9;

        public BoundingBox(
            DoublePoint upperLeft,
            DoublePoint lowerRight
        )
        {
            var minX = upperLeft.X < lowerRight.X ? upperLeft.X : lowerRight.X;
            var minY = upperLeft.Y < lowerRight.Y ? upperLeft.Y : lowerRight.Y;
            var maxX = upperLeft.X > lowerRight.X ? upperLeft.X : lowerRight.X;
            var maxY = upperLeft.Y > lowerRight.Y ? upperLeft.Y : lowerRight.Y;

            UpperLeft = new DoublePoint( minX, minY );
            LowerRight = new DoublePoint( maxX, maxY );
        }

        public BoundingBox(
            DoublePoint center,
            double width,
            double height
        )
        {
            UpperLeft = new DoublePoint(center.X - width/2, center.Y - height/2);
            LowerRight = new DoublePoint( center.X + width / 2, center.Y + height / 2 );
        }

        public DoublePoint UpperLeft { get; }
        public DoublePoint LowerRight { get; }

        public bool IsCollapsed =>
            Math.Abs( UpperLeft.X - LowerRight.X ) < MinDelta
         || Math.Abs( UpperLeft.Y - LowerRight.Y ) < MinDelta;
    }
}
