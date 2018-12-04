using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grains.Library.Models
{
    public class Matrix
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[,] Cells { get; set; }
        public bool[,] NotEmptyCells { get; set; }
        public BorderStyle Border { get; set; }
        public int IdsNumber { get; set; }
        public List<int> RestrictedIds { get; set; }
        public List<Cell> CellsWOId { get; set; }

        public List<Cell> CellsList
        {
            get
            {
                var cellsList = new List<Cell>();

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        cellsList.Add(new Cell(i, j, Cells[i, j]));
                    }
                }

                return cellsList;
            }
        }

        public Matrix(int size) : this(size, size)
        {
            this.RestrictedIds = new List<int>();
        }

        public Matrix(int width, int heigth)
        {
            this.Cells = new int[width, heigth];
            this.Width = width;
            this.Height = heigth;

            this.NotEmptyCells = new bool[width, heigth];
            this.RestrictedIds = new List<int>();

            this.CellsWOId = new List<Cell>();
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    this.CellsWOId.Add(new Cell(i, j, Cells[i, j]));
                }
            }
        }

        public void Add(Cell cell)
        {
            this.Cells[cell.X, cell.Y] = cell.Id;
            this.NotEmptyCells[cell.X, cell.Y] = true;
        }
    }

    public enum BorderStyle { Periodic, Closed }
}
