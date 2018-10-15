using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Grains.Utilities.ImageHandler
{
    public static class ImageHandler
    {
        public static void ExportToImage(Canvas canvas, string path)
        {
            var rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width+15,
           (int)canvas.RenderSize.Height+15, 96d, 96d, PixelFormats.Default);

            rtb.Render(canvas);

            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, 300, 300));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = File.OpenWrite(path))
            {
                pngEncoder.Save(fs);
            }
        }

        public static void ImportFromImage(int[,] array, int xDimension, int yDimension, string path)
        {
            var originBitmap = new Bitmap(path);
            var bitmap = new Bitmap(originBitmap, xDimension, yDimension);

            int id = 0;
            var ids = new Dictionary<string, int>();

            int x = bitmap.Size.Width;
            int y = bitmap.Size.Height;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    var pixelColor = bitmap.GetPixel(i, j).ToString();

                    if (ids.ContainsKey(pixelColor))
                    {
                        array[i, j] = ids[pixelColor];
                    }
                    else
                    {
                        ids.Add(pixelColor, id++);
                    }
                }
            }
        }
    }
}
