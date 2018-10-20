using Grains.Library.Enums;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Extensions.Helpers
{
    public class InclusionsHelper
    {
        public void AddInclusions(Matrix matrix, int amount, int size, Inclusions type, IList<Cell> borderCells)
        {
            var rnd = new Random();

            for (int i = 0; i < amount; i++)
            {
                int inclusionX = 0;
                int inclusionY = 0;

                if (borderCells.Count == 0)
                {
                    inclusionX = rnd.Next(matrix.Width);
                    inclusionY = rnd.Next(matrix.Height);
                }
                else
                {
                    var borderCell = borderCells[rnd.Next(borderCells.Count)] ;
                    
                    inclusionX = borderCell.X;
                    inclusionY = borderCell.Y;

                    borderCells.Remove(borderCell);
                }

                switch (type)
                {
                    case Inclusions.Square:
                        {
                            DrawSquareInclusion(matrix, inclusionX, inclusionY, size);
                            break;
                        }
                    case Inclusions.Circular:
                        {
                            DrawCircularInclusion(matrix, inclusionX, inclusionY, size);
                            break;
                        }
                }
            }
        }

        private void DrawSquareInclusion(Matrix matrix, int x, int y, int size)
        {
            x = x - size;
            y = y - size;

            for (int i = 0; i < size * 2; i++)
            {
                for (int j = 0; j < size * 2; j++)
                {
                    var cell = new Cell(x + i, y + j, 1).NormalizeCell(matrix);
                    matrix.Add(cell);
                }
            }
        }

        private void DrawCircularInclusion(Matrix matrix, int x, int y, int size)
        {
            int circleA = x;
            int circleB = y;

            x = x - size;
            y = y - size;

            for (int i = 0; i < size * 2; i++)
            {
                for (int j = 0; j < size * 2; j++)
                {
                    if ( (x + i - circleA)*(x + i - circleA) + (y + j - circleB)*(y + j - circleB) <= size*size)
                    {
                        var cell = new Cell(x + i, y + j, 1).NormalizeCell(matrix);
                        matrix.Add(cell);
                    }                  
                }
            }
        }
    }
}
