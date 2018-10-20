using System.Drawing;

namespace Grains.Library.Coordinates
{
    public static class Coordinates
    {
        public static Point[] MooreCoordinates =>
           new Point[]
           {
                new Point(-1, -1),
                new Point(0, -1),
                new Point(1, -1),
                new Point(-1, 0),
                new Point(1, 0),
                new Point(-1, 1),
                new Point(0, 1),
                new Point(1, 1),
           };

        public static Point[] VonNeumannCoordinates =>
          new Point[]
          {
                new Point(0, -1),
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, 1)
          };
    }
}
