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
        public static List<Cell> AddRandomGrains(this Matrix matrix, int amount)
        {
            var randomCells = new List<Cell>();
            var rnd = new Random();

            for (int i = 0; i < amount; i++)
            {
                int x = rnd.Next(matrix.Width);
                int y = rnd.Next(matrix.Height);

                if (matrix.Cells[x, y] != 0)
                {
                    i--;
                    continue;
                }

                var cell = new Cell(rnd.Next(matrix.Width), rnd.Next(matrix.Height), i + 2);
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

        public static void AddStep(this Matrix matrix, Matrix referenceMatrix, Neighbourhood strategy)
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
            }

            Parallel.For(0, matrix.Width, (i) => {
                Parallel.For(0, matrix.Height, (j) => {
                    if (!matrix.NotEmptyCells[i, j])
                    {
                        neighbourAction(matrix, referenceMatrix.Cells, new Cell(i, j));                       
                    }
                });
            });
        }        
    }
}
