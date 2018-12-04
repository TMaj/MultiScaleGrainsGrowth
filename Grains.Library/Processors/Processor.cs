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

        public int IdsNumber => this.matrix1.Cells.Max();

        public int[,] Array => matrix1.Cells;

        public Neighbourhood Neighbourhood { get; set; }

        public SimulationType SimulationType { get; set; }

        public Processor(int width, int height)
        {
            matrix1 = new Matrix(width, height);
            matrix2 = new Matrix(width, height);

            this.width = width;
            this.height = height;

            SimulationType = SimulationType.MonteCarlo;
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

        public async Task AddBorders(int size)
        {
            await Task.Run(() => matrix1.AddBorders(size));
            await CloneMatrix(matrix1, matrix2);
        }

        public async Task AddSingleBorder(int size, int x, int y)
        {
            await Task.Run(() => matrix1.AddSingleBorder(size, x, y));
            await CloneMatrix(matrix1, matrix2);
        }

        public async Task ClearAllButBorders()
        {
            await Task.Run(() => matrix1.ClearAllButBorders());
            await CloneMatrix(matrix1, matrix2);
        }

        public async Task<double> GetBordersPercentage()
        {
            double percentage = 0;
            await Task.Run(() => percentage = matrix1.GetBordersPercentage());
            return percentage;
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

        public async Task CreateSubstructure(Substructures substructure, int grains)
        {
            await Task.Run(()=>this.matrix1.CreateSubstructure(substructure, grains));
            await CloneMatrix(matrix1, matrix2);
        }

        public void MakeStep(int x, double j)
        {
            switch (SimulationType)
            {
                case SimulationType.CellularAutomata:
                    {
                        matrix2.AddCAStep(matrix1, Neighbourhood, x);
                        break;
                    }
                case SimulationType.MonteCarlo:
                    {
                        matrix2.AddMCStep(j);
                        break;
                    }
            }
            CloneMatrix(matrix2, matrix1);
        }

        // Monte Carlo

        public async Task GenerateMonteCarloArea(int number)
        {
           await Task.Run(() => this.matrix1.GenerateMonteCarloArea(number));

           CloneMatrix(matrix1, matrix2);
        }

        private async Task CloneMatrix(Matrix source, Matrix target)
        {
            target.RestrictedIds = source.RestrictedIds;
            target.IdsNumber = source.IdsNumber;

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
