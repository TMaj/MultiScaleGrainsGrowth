﻿using Grains.Library.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grains.Library.Extensions.Helpers
{
    public class BorderCellsHelpers
    {
        public IList<Cell> GetBorderCells(Matrix matrix)
        {
            var borderCells = new ConcurrentBag<Cell>();

            Parallel.For(0, matrix.Width, (i) =>
            {
                Parallel.For(0, matrix.Height, (j) =>
                {
                    var currentCell = new Cell(i, j, matrix.Cells[i, j]);
                    if (CheckForDifference(currentCell, matrix))
                    {
                        borderCells.Add(currentCell);
                    }
                });
            });

            return new List<Cell>(borderCells);
        }

        private bool CheckForDifference(Cell currentCell, Matrix matrix)
        {
            foreach (var point in Coordinates.Coordinates.MooreCoordinates)
            {
                var tempCell = currentCell.Get(point.X, point.Y).NormalizeCell(matrix);

                if (tempCell.Id != currentCell.Id && tempCell.Id != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
