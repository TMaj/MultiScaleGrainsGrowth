using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grains.Library.Extensions
{
    public static class MonteCarloExtensions
    {
        public static void GenerateMonteCarloArea(this Matrix matrix, int number)
        {
            var random = new Random();

            for (int i = 0; i < matrix.Width; i++)
            {
                for (int j = 0; j < matrix.Height; j++)
                {
                    if (matrix.Cells[i, j] == 0)
                    {
                        matrix.Cells[i, j] = random.Next(number) + 2;
                    }
                }
            }

            matrix.IdsNumber = number;
        }

        public static void AddMCStep(this Matrix matrix, Matrix referenceMatrix, double jb)
        {
            var notVisitedCells = new List<Cell>(matrix.CellsList);

            var random = new Random();

            Parallel.For(0, matrix.Width, (i) => {
                Parallel.For(0, matrix.Height, (j) => {
                    var currentCell = new Cell(i, j, referenceMatrix.Cells[i, j]);

                    var currentEnergy = referenceMatrix.CalculateEnergy(currentCell, j);

                    if (currentEnergy == 0)
                    {
                        return;
                    }

                    var currentCellValue = currentCell.Id;
                    var tempCellValue = random.Next(referenceMatrix.IdsNumber) + 2;

                    if (tempCellValue == currentCellValue)
                    {
                        return;
                    }

                    currentCell.Id = tempCellValue;

                    var newEnergy = referenceMatrix.CalculateEnergy(currentCell, j);

                    if (newEnergy < currentEnergy)
                    {
                        matrix.Cells[currentCell.X, currentCell.Y] = tempCellValue;
                    }
                });
            });
        }

        private static int CalculateEnergy(this Matrix matrix, Cell cell, double j)
        {
            int kroeneckerDelta = 0;
            var coordinates = Coordinates.Coordinates.MooreCoordinates;

            foreach (var point in coordinates)
            {
                var tempCell = cell.Get(point.X, point.Y).NormalizeCell(matrix);
                var tempId = matrix.Cells[tempCell.X, tempCell.Y];
                if (tempId != cell.Id)
                {
                    kroeneckerDelta += 1;
                }
            }

            return kroeneckerDelta; // * j;
        }
    }
}
