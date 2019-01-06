using Grains.Library.Enums;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

           // matrix.IdsNumber = number;
        }

        public static void AddMCStep(this Matrix matrix, double jb)
        {
            var random = new Random();
            var notVisitedCells = new List<Cell>(matrix.CellsWOId);
            notVisitedCells.Shuffle(random);

            foreach (var currentCell in notVisitedCells)
            {
                currentCell.Id = matrix.Cells[currentCell.X, currentCell.Y];

                var currentEnergy = matrix.CalculateEnergy(currentCell, jb);

                if (currentEnergy == 0)
                {
                    continue;
                }

                var currentCellValue = currentCell.Id;
                var tempCellValue = random.Next(matrix.IdsNumber) + 2;

                if (tempCellValue == currentCellValue)
                {
                    continue;
                }

                currentCell.Id = tempCellValue;

                var newEnergy = matrix.CalculateEnergy(currentCell, jb);

                if (newEnergy < currentEnergy)
                {
                    matrix.Cells[currentCell.X, currentCell.Y] = tempCellValue;
                }
            }
        }

        public static void AddRXMCStep(this Matrix matrix, double jb, NucleationModuleType nucleationType, NucleationArea nucleationArea, int nucleationSize, int currentStep)
        {
            switch (nucleationType)
            {
                case NucleationModuleType.Constant:
                    {
                        matrix.AddRecrystalisedNucleons(nucleationSize, nucleationArea);
                        break;
                    }
                case NucleationModuleType.Increasing:
                    {
                        matrix.AddRecrystalisedNucleons(nucleationSize * currentStep, nucleationArea);
                        break;
                    }
            }

            var random = new Random();
            var notVisitedCells = new List<Cell>(matrix.ShuffledCells);
          
            foreach (var currentCell in notVisitedCells)
            {
                currentCell.Id = matrix.Cells[currentCell.X, currentCell.Y];

                var randomNeighbour = matrix.GetRandomNeighbour(currentCell, random);

                if (matrix.Energy[randomNeighbour.X, randomNeighbour.Y] != 0)
                {
                    continue;
                }

                var currentEnergy = matrix.CalculateEnergy(currentCell, jb) + matrix.Energy[currentCell.X, currentCell.Y];

                if (currentEnergy == 0)
                {
                    continue;
                }

                var currentCellValue = currentCell.Id;

                var tempCellValue = matrix.Cells[randomNeighbour.X, randomNeighbour.Y];

                if (tempCellValue == currentCellValue)
                {
                    continue;
                }

                currentCell.Id = tempCellValue;

                var newEnergy = matrix.CalculateEnergy(currentCell, jb);

                if (newEnergy < currentEnergy)
                {
                    matrix.Cells[currentCell.X, currentCell.Y] = tempCellValue;
                    matrix.Energy[currentCell.X, currentCell.Y] = 0;
                }
            }
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

        private static Cell GetRandomNeighbour(this Matrix matrix, Cell cell, Random random)
        {
            var coordinates = Coordinates.Coordinates.MooreCoordinates;
            var neighbours = new List<Cell>();

            foreach (var point in coordinates)
            {
                var tempCell = cell.Get(point.X, point.Y).NormalizeCell(matrix);
                neighbours.Add(tempCell);
            }

            return neighbours.ElementAt(random.Next(neighbours.Count));
        }
    }
}
