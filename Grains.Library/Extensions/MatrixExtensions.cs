using Grains.Library.Enums;
using Grains.Library.Helpers;
using Grains.Library.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Extensions
{
    public static class MatrixExtensions
    {
        public static List<Cell> AddRandomGrains(this Matrix matrix, int amount)
        {
            var randomCells = new List<Cell>();
            var rnd = new Random();            

            for (int i=0; i<amount; i++)
            {
                int x = rnd.Next(matrix.Width);
                int y = rnd.Next(matrix.Height);

                if (matrix.Cells[x,y] != 0)
                {
                    i--;
                    continue;
                }

                var cell = new Cell(rnd.Next(matrix.Width), rnd.Next(matrix.Height), i + 1);
                matrix.Add(cell);
                randomCells.Add(cell);
            }

            return randomCells;
        }

        public static void AddStep(this Matrix matrix, Matrix referenceMatrix, Neighbourhood strategy)
        {
            foreach (var cell in matrix.NotEmptyCells)
            {
                switch (strategy)
                {
                    case Neighbourhood.Moore:
                        {
                            matrix.AddMooreNeighbourhood(referenceMatrix.Cells, cell);
                            break;
                        }
                    case Neighbourhood.VonNeumann:
                        {
                            matrix.AddVonNeumannNeighbourhood(referenceMatrix.Cells, cell);
                            break;
                        }
                }                
            }

            matrix.NotEmptyCells = new List<Cell>();

            for (int i = 0; i < matrix.Width; i++)
            {
                for (int j = 0; j < matrix.Height; j++)
                {                   
                   if(matrix.Cells[i,j] !=0) matrix.NotEmptyCells.Add(new Cell(i, j,matrix.Cells[i,j]));
                }
            }
        }
    }
}
