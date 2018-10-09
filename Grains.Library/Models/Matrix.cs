using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Models
{
    public class Matrix
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[,] Cells { get; set; }
        public bool[,] NotEmptyCells { get; set; }
        public BorderStyle Border { get; set; }

        public Matrix(int size) : this(size, size)
        {
        }

        public Matrix(int width, int heigth)
        {
            this.Cells = new int[width, heigth];
            this.Width = width;
            this.Height = heigth;

            this.NotEmptyCells = new bool[width, heigth];
        }

        public void Add(Cell cell)
        {
            this.Cells[cell.X, cell.Y] = cell.Id;
            this.NotEmptyCells[cell.X, cell.Y] = true;
        }
    }

    public enum BorderStyle { Transient, Closed }
}
