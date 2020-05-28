using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Utils.PixelUtils;
using static Preprocess.ResizeProcessor;

namespace Preprocess
{

    public enum PreProcessType{ Histogram, Pixel, Naive }

    public class PreProcessor
    {
        public PreProcessType preProcessType;
        List<IImageData> totalList = new List<IImageData>(4000);

        public HistogramData ProcessHistogramSingle(string path)
        {
            var values = new float[80];
            IterateImg(path, 256, (val, i, j) => values[val]++);
            return new HistogramData { Name = path, Values = values, Category = "None" };
        }

        public PixelData ProcessPixelSingle(string path, string category = null)
        {
            var values = new float[10000];
            IterateImg(path, 100, (val, i, j) => values[i * 100 + j] = val);
            return new PixelData { Name = path, Values = values, Category = category ?? "None" };
        }

        public PixelData ProcessNaiveSingle(string path, string category = null)
        {
            return new PixelData { Name = path, Values = new float[]{}, Category = category ?? "None" };
        }

        public List<IImageData> ProcessFolder(string path)
        {
            string category = path.Split('.')[1];
            var list = new List<IImageData>(400);
            var lockObj = new Object();
            Parallel.ForEach(Directory.GetFiles(path), item => {
                lock (lockObj)
                {
                    switch (preProcessType)
                    {
                        case PreProcessType.Histogram:
                            list.Add(ProcessHistogramSingle(item));
                            break;
                        case PreProcessType.Pixel:
                            list.Add(ProcessPixelSingle(item, category));
                            break;
                        case PreProcessType.Naive:
                            list.Add(ProcessNaiveSingle(item, category));
                            break;
                    }
                }
            });
            Console.WriteLine($"{path} done");
            return list;
        }

        public List<IImageData> ProcessFolders(string[] paths)
        {
            Array.ForEach(paths, item => totalList.AddRange(ProcessFolder(item)));
            return totalList;
        }

        public void Write2CSV(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                totalList.ForEach(item => writer.WriteLine(
                    $"{item.Name},{item.Category},{string.Join(',', item.Values)}"
                ));
            }
        }

        public static void WriteSimpleRelation(string path, string dir)
        {
            var list = new List<NaiveData>();
            Parallel.ForEach(Directory.GetDirectories(dir), sub => {
                foreach (var item in Directory.GetDirectories(sub))
                {
                    list.Add(new NaiveData { Name = item, Category = sub.Split('.')[1] });
                }
            });
        }

        public static void IterateImg(string path, int size, Action<int, int, int> func)
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