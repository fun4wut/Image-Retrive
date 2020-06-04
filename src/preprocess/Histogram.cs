using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Utils.PixelUtils;
using static Preprocess.ResizeProcessor;

namespace Preprocess
{
    public class HistogramPreprocessor : IPreprocessable
    {
        List<ImageVector> totalList = new List<ImageVector>(40000);

        public List<ImageVector> TotalList { get => totalList; }

        public ImageVector PreprocessSingle(string path)
        {
            var values = new float[80];
            IterateImg(path, 224, (val, i, j) => values[val]++);
            return new ImageVector { Name = path, Values = values };
        }

        static void IterateImg(string path, int size, Action<int, int, int> func)
        {
            using (var img = Image.Load<Rgb24>(path))
            {
                Resize(img, size);
                for (int i = 0; i < img.Height; ++i)
                {
                    var row = img.GetPixelRowSpan(i);
                    for (int j = 0; j < img.Width; j++)
                    {
                        var pixel = RGB2HSV(row[j]);
                        var value = Quantilize(pixel);
                        func(value, i, j);
                    }
                }
            }
        }
    }
}