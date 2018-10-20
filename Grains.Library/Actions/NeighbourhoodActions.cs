using Grains.Library.Extensions;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Grains.Library.Actions
{
    public delegate void NeighbourhoodCalculation(Matrix originalMatrix, int[,] referenceArray, Cell currentCell);

    public static class NeighbourhoodActions
    {
        public static NeighbourhoodCalculation MooreAction => (Matrix originalMatrix, int[,] referenceArray, Cell currentCell) => {

            var coordinates = Coordinates.Coordinates.MooreCoordinates;

            NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates);
        };

        public static NeighbourhoodCalculation VonNeumannAction => (Matrix originalMatrix, int[,] referenceArray, Cell currentCell) => {

            var coordinates = Coordinates.Coordinates.VonNeumannCoordinates;

            NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates);
        };

        static void NeighbourhoodCalculation(Matrix originalMatrix, int[,] referenceArray, Cell currentCell, Point[] coordinates)
        {
            var neighbourhoodPoints = new List<int>();

            foreach (var point in coordinates)
            {
                var tempCell = currentCell.Get(point.X, point.Y).NormalizeCell(originalMatrix);

                if (referenceArray[tempCell.X, tempCell.Y] != 0 && referenceArray[tempCell.X, tempCell.Y] != 1)
                {
                    neighbourhoodPoints.Add(referenceArray[tempCell.X, tempCell.Y]);
                }
            }

            var mostOftenId = neighbourhoodPoints.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).FirstOrDefault();
            if (mostOftenId != 0)
            {
                originalMatrix.Cells[currentCell.X, currentCell.Y] = mostOftenId;
                originalMatrix.NotEmptyCells[currentCell.X, currentCell.Y] = true;
            }
        }
    }
}
