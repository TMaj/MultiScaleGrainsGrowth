using Grains.Library.Extensions;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Helpers
{
    public static class VonNeumannHelper
    {
        public static void AddVonNeumannNeighbourhood(this Matrix matrix, int[,] referenceArray, Cell cell)
        {
            matrix.Cells.TrySetValue(referenceArray, cell.Get(0, -1).NormalizeCell(matrix), 0);
            matrix.Cells.TrySetValue(referenceArray, cell.Get(-1, 0).NormalizeCell(matrix), 0);
            matrix.Cells.TrySetValue(referenceArray, cell.Get(1, 0).NormalizeCell(matrix), 0);
            matrix.Cells.TrySetValue(referenceArray, cell.Get(0, 1).NormalizeCell(matrix), 0);
        }
    }
}
