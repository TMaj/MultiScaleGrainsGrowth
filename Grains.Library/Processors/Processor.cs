using Grains.Library.Enums;
using Grains.Library.Extensions;
using Grains.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grains.Library.Processors
{
    public class Processor
    {
        private Matrix matrix1;
        private Matrix matrix2;

        private int width;
        private int height;

        public List<Cell> UpdatedCells;

        public int[,] Array => matrix1.Cells;

        private Neighbourhood neighbourhood;

        public Processor(int width, int height)
        {
            matrix1 = new Matrix(width, height);
            matrix2 = new Matrix(width, height);

            this.width = width;
            this.height = height;
            UpdatedCells = new List<Cell>();
        }

        public async Task AddRandomGrains(int amount)
        {
            await Task.Run(() => matrix1.AddRandomGrains(amount));
            await CloneMatrix(matrix1, matrix2);
        }

        public async Task AddInclusions(int amount, int size, Inclusions type)
        {
            await Task.Run(() => matrix1.AddInclusions(amount, size, type));
            await CloneMatrix(matrix1, matrix2);
        }

        public void SetNeighbourhood(Neighbourhood neighbourhood)
        {
            this.neighbourhood = neighbourhood;
        }

        public void SetBorderStyle(BorderStyle borderStyle)
        {
            matrix1.Border = borderStyle;
            matrix2.Border = borderStyle;
        }

        public async Task Clear()
        {
            await Task.Run(() =>
            {
                matrix1 = new Matrix(width, height);
                matrix2 = new Matrix(width, height);
            });
        }

        public void ResetGrowth()
        {
            this.matrix1.NotEmptyCells = new bool[matrix1.Width, matrix1.Height];
            this.matrix2.NotEmptyCells = new bool[matrix2.Width, matrix2.Height];
        }

        public void CreateSubstructure(Substructures substructure, int grains)
        {
            this.matrix1.CreateSubstructure(substructure, grains);
            CloneMatrix(matrix1, matrix2);
        }

        public void MakeStep(int x)
        {
            matrix2.AddStep(matrix1, neighbourhood, x);
            CloneMatrix(matrix2, matrix1);
        }

        private async Task CloneMatrix(Matrix source, Matrix target)
        {
            target.RestrictedIds = source.RestrictedIds;

            Parallel.For(0, source.Width, i =>
            {
                Parallel.For(0, source.Height, j =>
                {
                    target.Cells[i, j] = source.Cells[i, j];
                    target.NotEmptyCells[i, j] = source.NotEmptyCells[i, j];
                });
            });
        }
    }
}
