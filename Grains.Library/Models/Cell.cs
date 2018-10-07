using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Models
{
    public class Cell
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Cell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Cell(int x, int y, int id) : this(x,y)
        {
            this.Id = id;
        }

        public Cell Get(int xDiff, int yDiff)
        {
            return new Cell(this.X + xDiff, this.Y + yDiff, this.Id);
        }
    }
}
