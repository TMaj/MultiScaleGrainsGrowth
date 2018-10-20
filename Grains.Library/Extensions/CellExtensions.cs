using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Extensions
{
    public static class CellExtensions
    {
        public static Cell NormalizeCell(this Cell cell, Matrix matrix)
        {
            switch (matrix.Border)
            {
                case BorderStyle.Closed:
                    {
                        if (cell.X >= matrix.Width)
                        {
                            cell.X = matrix.Width - 1;
                        }

                        if (cell.Y >= matrix.Height)
                        {
                            cell.Y = matrix.Height - 1;
                        }

                        if (cell.X < 0)
                        {
                            cell.X = 0;
                        }

                        if (cell.Y < 0)
                        {
                            cell.Y = 0;
                        }

                        break;
                    }

                case BorderStyle.Transient:
                    {
                        if (cell.X >= matrix.Width)
                        {
                            cell.X = Math.Abs(matrix.Width - cell.X);
                        }

                        if (cell.Y >= matrix.Height)
                        {
                            cell.Y = Math.Abs(matrix.Height - cell.Y);
                        }

                        if (cell.X < 0)
                        {
                            cell.X = matrix.Width + cell.X;
                        }

                        if (cell.Y < 0)
                        {
                            cell.Y = matrix.Height + cell.Y;
                        }

                        break;
                    }
                default:
                    {
                        return cell;
                    }
            }

            return cell;
        }
    }
}
