using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Helpers
{
    public static class CommonHelper
    {
        public static void TrySetValue(this int[,] array, int[,] referenceArray, Cell cell, int treshold)
        {
            if (referenceArray[cell.X, cell.Y] != cell.Id && referenceArray[cell.X, cell.Y] <= treshold)
            {
                array[cell.X, cell.Y] = cell.Id;
            }
        }
    }
}
