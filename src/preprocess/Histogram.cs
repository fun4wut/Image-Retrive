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
    public class HistogramPreprocessor : IPreprocessable<HistogramData>
    {
        List<HistogramData> totalList = new List<HistogramData>(40000);

        public List<HistogramData> TotalList { get => totalList; }

        public List<HistogramData> ProcessFolder(string path)
        {
            string category = path.Split('.')[1];
            var lockObj = new Object();
            var list = new List<HistogramData>();
            Parallel.ForEach(Directory.GetFiles(path), item => {
                var values = new float[80];
                IterateImg(path, 256, (val, i, j) => values[val]++);
                var data =  new HistogramData { Name = path, Values = values, Category = "None" };
                lock (lockObj) { list.Add(data); }
            });
            Console.WriteLine($"{path} done");
            return list;
        }

        
        public List<TFData> ProcessFolderTF(string path)
        {
            string category = path.Split('.')[1];
            var lockObj = new Object();
            var list = new List<TFData>();
            Parallel.ForEach(Directory.GetFiles(path), item => {
                var values = new float[80];
                IterateImg(path, 256, (val, i, j) => values[val]++);
                var data =  new TFData { Name = path, Values = values, Category = category ?? "None" };
                lock (lockObj) { list.Add(data); }
            });
            Console.WriteLine($"{path} done");
            return list;
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