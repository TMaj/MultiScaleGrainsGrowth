using Grains.Library.Enums;
using Grains.Library.Extensions;
using Grains.Library.Models;
using System.Threading.Tasks;

namespace Grains.Library.Processors
{
    public class Processor
    {
        private Matrix matrix1;
        private Matrix matrix2;

        private int width;
        private int height;

        public int CurrentRXStep = 0;
        public bool EnergyDistributed;
        public bool Recrystalisation;

        public int IdsNumber => this.matrix1.Cells.Max();

        public int[,] Array => matrix1.Cells;
        public int[,] Energy => matrix1.Energy;

        public Neighbourhood Neighbourhood { get; set; }

        // Recrystalisation
        public SimulationType SimulationType { get; set; }
        public NucleationModuleType NucleationType { get; set; }
        public NucleationArea NucleationArea { get; set; }
        public int NucleationSize { get; set; }
        public int StepsLimit { get; set; }


        public delegate void StepIncrementedDelegate(int stepnumber);
        public event StepIncrementedDelegate StepIncremented;

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
            CloneMatrix(matrix1, matrix2);
        }

        public async Task AddInclusions(int amount, int size, Inclusions type)
        {
            await Task.Run(() => matrix1.AddInclusions(amount, size, type));
            CloneMatrix(matrix1, matrix2);
        }

        public async Task AddBorders(int size)
        {
            await Task.Run(() => matrix1.AddBorders(size));
            CloneMatrix(matrix1, matrix2);
        }

        public async Task AddSingleBorder(int size, int x, int y)
        {
            await Task.Run(() => matrix1.AddSingleBorder(size, x, y));
            CloneMatrix(matrix1, matrix2);
        }

        public async Task ClearAllButBorders()
        {
            await Task.Run(() => matrix1.ClearAllButBorders());
            CloneMatrix(matrix1, matrix2);
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
            this.CurrentRXStep = 0;

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
            CloneMatrix(matrix1, matrix2);
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
                case SimulationType.RXMonteCarlo:
                    {
                        matrix2.AddRXMCStep(j, NucleationType, NucleationArea, NucleationSize, CurrentRXStep);
                        CurrentRXStep++;
                        StepIncremented(CurrentRXStep);
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

        public async Task DistributeEnergy(EnergyDistributionType energyDistributionType)
        {
            await Task.Run(() => this.matrix1.DistributeEnergy(energyDistributionType));
            EnergyDistributed = true;
            CloneMatrix(matrix1, matrix2);
        }

        public async Task ClearEnergy()
        {
            await Task.Run(() => this.matrix1.ClearEnergy());            
            CloneMatrix(matrix1, matrix2);
        }

        public async Task AddRecrystalisedNucleons()
        {
            await Task.Run(() => this.matrix1.AddRecrystalisedNucleons(NucleationSize, NucleationArea));
            CloneMatrix(matrix1, matrix2);
        }

        private void CloneMatrix(Matrix source, Matrix target)
        {
            target.RestrictedIds = source.RestrictedIds;
          //  target.IdsNumber = source.IdsNumber;

            Parallel.For(0, source.Width, i =>
            {
                Parallel.For(0, source.Height, j =>
                {
                    target.Cells[i, j] = source.Cells[i, j];
                    target.Energy[i, j] = source.Energy[i, j];
                    target.NotEmptyCells[i, j] = source.NotEmptyCells[i, j];
                });
            });
        }
    }
}
