using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Library.Extensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> ts, Random random)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = random.Next(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }
}
