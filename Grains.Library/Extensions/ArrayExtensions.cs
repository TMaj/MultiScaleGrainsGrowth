namespace Grains.Library.Extensions
{
    public static class ArrayExtensions
    {
        public static int Max(this int[,] array)
        {
            int max = 0;

            foreach (var number in array)
            {
                if (number > max)
                {
                    max = number;
                }       
            }

            return max;
        }
    }
}
