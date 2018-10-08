using Grains.Library.Enums;
using Grains.Library.Extensions;
using Grains.Library.Models;
using System;
using System.Collections.Generic;
using System.Text;
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
            this.matrix1 = new Matrix(width, height);
            this.matrix2 = new Matrix(width, height);

            this.width = width;
            this.height = height;
            this.UpdatedCells = new List<Cell>();
        }
               
        public void AddRandomGrains(int amount)
        {
            this.matrix1.AddRandomGrains(amount);
            this.UpdatedCells = this.matrix1.NotEmptyCells;
            CloneMatrix(this.matrix1, this.matrix2);
        }

        public void SetNeighbourhood(Neighbourhood neighbourhood)
        {
            this.neighbourhood = neighbourhood;
        }

        public void SetBorderStyle(BorderStyle borderStyle)
        {
            this.matrix1.Border = borderStyle;
            this.matrix2.Border = borderStyle;
        }

        public void Clear()
        {
            this.matrix1 = new Matrix(this.width, this.height);
            this.matrix2 = new Matrix(this.width, this.height);
            this.UpdatedCells = new List<Cell>();
        }

        public void StartGrowth()
        {

        }

        public async void MakeStep()
        {
            await Task.Run(() => this.matrix2.AddStep(this.matrix1, this.neighbourhood));
            await CloneMatrix(this.matrix2, this.matrix1);
            this.UpdatedCells = this.matrix1.NotEmptyCells;
        }

        private async Task CloneMatrix(Matrix source, Matrix target)
        {
            target.NotEmptyCells = source.NotEmptyCells;
            Parallel.For(0, source.Width, i => {
                for(int j=0; j< source.Height; j++)
                {
                    target.Cells[i, j] = source.Cells[i,j];
                }
            });
        }
    }
}
