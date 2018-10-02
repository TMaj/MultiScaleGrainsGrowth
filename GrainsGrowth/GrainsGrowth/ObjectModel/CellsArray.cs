using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GrainsGrowth.ObjectModel
{
    public class CellsArray
    {
        private int[,] cells;
        public int Height;
        public int Width;

        public Dictionary<int, List<Point>> WrittenCells;

        public void Add(int x, int y, int id)
        {
            this.cells[x, y] = id;
            this.WrittenCells[id].Add(new Point(x, y));
        }

        public CellsArray(int height, int width)
        {
            this.Width = width;
            this.Height = height;
            this.cells = new int[height, width];
        }

        public CellsArray(int size)
        {
            this.Width = size;
            this.Height = size;
            this.cells = new int[size, size];
        }

        public CellsArray()
        {
        }
    }
}
