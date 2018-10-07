using Grains.Library.Enums;
using Grains.Library.Helpers;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Extensions
{
    public static class MatrixExtensions
    {
        public static void AddRandomGrains(this Matrix matrix, int amount)
        {
            Random rnd = new Random();            

            for (int i=0; i<amount; i++)
            {
                int x = rnd.Next(matrix.Width);
                int y = rnd.Next(matrix.Height);

                if (matrix.Cells[x,y] != 0)
                {
                    i--;
                    continue;
                }

                matrix.Add(new Cell(rnd.Next(matrix.Width), rnd.Next(matrix.Height), i));
            }
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
        }
    }
}
