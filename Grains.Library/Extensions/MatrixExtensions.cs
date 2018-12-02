using Grains.Library.Enums;
using Grains.Library.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grains.Library.Actions;
using System.Collections.Concurrent;
using Grains.Library.Extensions.Helpers;

namespace Grains.Library.Extensions
{
    public static class MatrixExtensions
    {
        public static void AddRandomGrains(this Matrix matrix, int amount)
        {
            var emptyCells = new List<Cell>();

            for (int i = 0; i < matrix.Width; i++)
            {
                for (int j = 0; j < matrix.Height; j++)
                {
                    if (matrix.Cells[i, j] != 0)
                    {
                        continue;
                    }

                    emptyCells.Add(new Cell(i, j));
                }
            }

            var rnd = new Random();

            for (int i = 0; i < amount; i++)
            {
                int x = rnd.Next(emptyCells.Count);

                var newId = i + 2;

                if (matrix.RestrictedIds.Contains(newId))
                {
                    continue;
                }

                emptyCells[x].Id = newId;
                matrix.Add(emptyCells[x]);
                emptyCells.RemoveAt(x);
            }
        }

        public static void AddInclusions(this Matrix matrix, int amount, int size, Inclusions type)
        {
            var borderCellsHelper = new BorderCellsHelpers();
            var inclusionsHelper = new InclusionsHelper();
            var borderCells = borderCellsHelper.GetBorderCells(matrix);
            inclusionsHelper.AddInclusions(matrix, amount, size, type, borderCells);
        }

        public static void AddCAStep(this Matrix matrix, Matrix referenceMatrix, Neighbourhood strategy, int x = 100)
        {
            NeighbourhoodCalculation neighbourAction = NeighbourhoodActions.MooreAction;

            switch (strategy)
            {
                case Neighbourhood.Moore:
                    {
                        neighbourAction = NeighbourhoodActions.MooreAction;
                        break;
                    }
                case Neighbourhood.VonNeumann:
                    {
                        neighbourAction = NeighbourhoodActions.VonNeumannAction;
                        break;
                    }
                case Neighbourhood.ShapeControl:
                    {
                        neighbourAction = NeighbourhoodActions.ShapeControlAction;
                        break;
                    }
            }

            var random = new Random();

            Parallel.For(0, matrix.Width, (i) => {
                Parallel.For(0, matrix.Height, (j) => {
                    if (!matrix.NotEmptyCells[i, j])
                    {
                        neighbourAction(matrix, 
                                        new Cell(i, j),
                                        referenceMatrix.Cells,
                                        strategy == Neighbourhood.ShapeControl ? random.Next(100) : 1,
                                        x);                       
                    }
                });
            });
        }

        public static void CreateSubstructure(this Matrix matrix, Substructures substructure, int grains)
        {
            if (matrix.NotEmptyCells.Length == 0)
            {
                return;
            }

            var random = new Random();
            var chosenIds = new List<int>();

            for (int i = 0; i < grains; i++)
            {
                var x = random.Next(matrix.Width);
                var y = random.Next(matrix.Height);

                var id = matrix.Cells[x, y];

                if (!chosenIds.Contains(id) && id != 0 && id != 1)
                {
                    chosenIds.Add(id);
                }
                else
                {
                    i--;
                }
            }

            matrix.NotEmptyCells = new bool[matrix.Width, matrix.Height]; 

            switch (substructure)
            {
                case Substructures.Substructure:
                    {
                        Parallel.For(0, matrix.Width, (i) => {
                            Parallel.For(0, matrix.Height, (j) => {
                                if (!chosenIds.Contains(matrix.Cells[i, j]))
                                {
                                    matrix.Cells[i, j] = 0;
                                }
                                else
                                {
                                    matrix.NotEmptyCells[i, j] = true;
                                }
                            });
                        });

                        matrix.RestrictedIds.AddRange(chosenIds);
                        break;
                    }
                case Substructures.DualPhase:
                    {
                        Parallel.For(0, matrix.Width, (i) => {
                            Parallel.For(0, matrix.Height, (j) => {
                                if (!chosenIds.Contains(matrix.Cells[i, j]))
                                {
                                    matrix.Cells[i, j] = 0;
                                }
                                else
                                {
                                    matrix.Cells[i, j] = chosenIds.FirstOrDefault();
                                    matrix.NotEmptyCells[i, j] = true;
                                }
                            });
                        });
                        matrix.RestrictedIds.Add(chosenIds.FirstOrDefault());
                        break;
                    }
            }
        }

        public static void AddBorders(this Matrix matrix, int size)
        {
            for (int x = 0; x < size; x++)
            {
                var coordinates = Coordinates.Coordinates.WidenMooreCoordinates(x);

                for (int i = 0; i < matrix.Width; i++)
                {
                    for (int j = 0; j < matrix.Height; j++)
                    {
                        var currentCell = new Cell(i, j, matrix.Cells[i, j]);

                        foreach (var point in coordinates)
                        {
                            var tempCell = currentCell.Get(point.X, point.Y).NormalizeCell(matrix);
                            var tempId = matrix.Cells[tempCell.X, tempCell.Y];

                            if (currentCell.Id == 1)
                            {
                                continue;
                            }

                            if (tempId != currentCell.Id && tempId != 1)
                            {
                                matrix.Cells[tempCell.X, tempCell.Y] = 1;
                                matrix.NotEmptyCells[tempCell.X, tempCell.Y] = true;
                            }
                        }
                    }
                }
            }            
        }

        public static void AddSingleBorder(this Matrix matrix, int size, int x, int y)
        {
            var desiredId = matrix.Cells[x, y];

            for (int s = 0; s < size; s++)
            {
                var coordinates = Coordinates.Coordinates.WidenMooreCoordinates(s);

                for (int i = 0; i < matrix.Width; i++)
                {
                    for (int j = 0; j < matrix.Height; j++)
                    {
                        if (matrix.Cells[i, j] != desiredId)
                        {
                            continue;
                        }

                        var currentCell = new Cell(i, j, matrix.Cells[i, j]);

                        foreach (var point in coordinates)
                        {
                            var tempCell = currentCell.Get(point.X, point.Y).NormalizeCell(matrix);
                            var tempId = matrix.Cells[tempCell.X, tempCell.Y];

                            if (currentCell.Id == 1)
                            {
                                continue;
                            }

                            if (tempId != currentCell.Id && tempId != 1)
                            {
                                matrix.Cells[tempCell.X, tempCell.Y] = 1;
                            }
                        }
                    }
                }
            }
        }

        public static void ClearAllButBorders(this Matrix matrix)
        {
            Parallel.For(0, matrix.Width, (i) => {
                Parallel.For(0, matrix.Height, (j) => {
                    if (matrix.Cells[i, j] != 1)
                    {
                        matrix.Cells[i, j] = 0;
                        matrix.NotEmptyCells[i, j] = false;
                    }
                });
            });
        }

        public static double GetBordersPercentage(this Matrix matrix)
        {
            int bordersNumber = 0;

            foreach (var id in matrix.Cells)
            {
                if (id == 1)
                {
                    bordersNumber++;
                }
            }

            double percentage = ((double)bordersNumber /(double)(matrix.Width * matrix.Height)) * 100;

            return percentage;
        }
    }
}
