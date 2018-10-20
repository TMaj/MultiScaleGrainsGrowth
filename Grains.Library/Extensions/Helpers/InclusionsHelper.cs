using Grains.Library.Enums;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Extensions.Helpers
{
    public class InclusionsHelper
    {
        public void AddInclusions(Matrix matrix, int amount, int size, Inclusions type, IList<Cell> borderCells)
        {
            var rnd = new Random();

            for (int i = 0; i < amount; i++)
            {
                int inclusionX = 0;
                int inclusionY = 0;

                if (borderCells.Count == 0)
                {
                    inclusionX = rnd.Next(matrix.Width);
                    inclusionY = rnd.Next(matrix.Height);
                }
                else
                {
                    var borderCell = borderCells[rnd.Next(borderCells.Count)];

                    inclusionX = borderCell.X;
                    inclusionY = borderCell.Y;

                    borderCells.Remove(borderCell);
                }


            }
        }
    }
}
