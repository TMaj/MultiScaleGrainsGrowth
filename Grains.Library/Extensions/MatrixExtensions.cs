﻿using Grains.Library.Enums;
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
        public static List<Cell> AddRandomGrains(this Matrix matrix, int amount)
        {
            var randomCells = new List<Cell>();
            var rnd = new Random();

            for (int i = 0; i < amount; i++)
            {
                int x = rnd.Next(matrix.Width);
                int y = rnd.Next(matrix.Height);

                var newId = i + 2;

                if (matrix.Cells[x, y] != 0)
                {
                    i--;
                    continue;
                }

                if (matrix.RestrictedIds.Contains(newId))
                {
                    continue;
                }

                var cell = new Cell(rnd.Next(matrix.Width), rnd.Next(matrix.Height), newId);
                matrix.Add(cell);
                randomCells.Add(cell);
            }

            return randomCells;
        }

        public static void AddInclusions(this Matrix matrix, int amount, int size, Inclusions type)
        {
            var borderCellsHelper = new BorderCellsHelpers();
            var inclusionsHelper = new InclusionsHelper();
            var borderCells = borderCellsHelper.GetBorderCells(matrix);
            inclusionsHelper.AddInclusions(matrix, amount, size, type, borderCells);
        }

        public static void AddStep(this Matrix matrix, Matrix referenceMatrix, Neighbourhood strategy, int x = 100)
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
                                        referenceMatrix.Cells,
                                        new Cell(i, j),
                                        strategy == Neighbourhood.ShapeControl ? random.Next(100) : 1,
                                        x);                       
                    }
                });
            });
        }

        public static void CreateSubstructure(this Matrix matrix, Substructures substructure, int grains)
        {
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
    }
}
