using Grains.Library.Extensions;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Grains.Library.Actions
{
    public delegate void NeighbourhoodCalculation(Matrix originalMatrix, Cell currentCell, int[,] referenceArray = null, int randomNumber = 1, int x = 100);

    public static class NeighbourhoodActions
    {
        public static NeighbourhoodCalculation MooreAction => (Matrix originalMatrix, Cell currentCell, int[,] referenceArray, int randomNumber, int x) =>
        { 
            var coordinates = Coordinates.Coordinates.MooreCoordinates;

            NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 0);
        };

        public static NeighbourhoodCalculation VonNeumannAction => (Matrix originalMatrix, Cell currentCell, int[,] referenceArray, int randomNumber, int x) =>
        {
            var coordinates = Coordinates.Coordinates.VonNeumannCoordinates;

            NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 0);
        };

        public static NeighbourhoodCalculation ShapeControlAction => (Matrix originalMatrix, Cell currentCell, int[,] referenceArray, int randomNumber, int x) =>
        {
            var coordinates = Coordinates.Coordinates.MooreCoordinates;

            if (NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 5))
            {
                return;
            };

            coordinates = Coordinates.Coordinates.VonNeumannCoordinates;

            if (NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 3))
            {
                return;
            };

            coordinates = Coordinates.Coordinates.InvertedVonNeumannCoordinates;

            if (NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 3))
            {
                return;
            };
            
            if (randomNumber <= x)
            {
                coordinates = Coordinates.Coordinates.MooreCoordinates;
                NeighbourhoodCalculation(originalMatrix, referenceArray, currentCell, coordinates, 0);
            }
        };

        static bool NeighbourhoodCalculation(Matrix originalMatrix, int[,] referenceArray, Cell currentCell, Point[] coordinates, int treshold)
        {
            var neighbourhoodPoints = new List<int>();

            foreach (var point in coordinates)
            {
                var tempCell = currentCell.Get(point.X, point.Y).NormalizeCell(originalMatrix);
                var tempId = referenceArray[tempCell.X, tempCell.Y];

                if (tempId != 0 && tempId != 1 && !originalMatrix.RestrictedIds.Contains(tempId))
                {
                    neighbourhoodPoints.Add(referenceArray[tempCell.X, tempCell.Y]);
                }
            }

            if (neighbourhoodPoints.Count() == 0)
            {
                return false;
            }

            var groupedIds = neighbourhoodPoints.GroupBy(i => i).OrderByDescending(grp => grp.Count());
            var mostOftenId = groupedIds.Select(grp => grp.Key).FirstOrDefault();
            var occurenceCount = groupedIds.FirstOrDefault().Count();

            if (mostOftenId != 0 && occurenceCount >= treshold)
            {
                originalMatrix.Cells[currentCell.X, currentCell.Y] = mostOftenId;
                originalMatrix.NotEmptyCells[currentCell.X, currentCell.Y] = true;
                return true;
            }

            return false;
        }
        
    }
}
