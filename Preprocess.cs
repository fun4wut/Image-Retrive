using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ML.Data;
using static Utils.PixelUtils;
using static Preprocess.ResizeProcessor;

namespace Preprocess
{

   public enum PreProcessType{ Histogram, Pixel }
    public interface IImageData
    {
        string Name {get;set;}
        string Category {get;set;}
        float[] Values {get; set;}
    }

    public class HistogramData : IImageData
    {
        [LoadColumn(0)] public string Name {get; set;}
        [ColumnName("Label")][LoadColumn(1)] public string Category {get; set;}
        [ColumnName("Features")][LoadColumn(2, 81)][VectorType(80)] public float[] Values {get; set;}
    }

    public class PixelData : IImageData
    {
        [LoadColumn(0)] public string Name {get; set;}
        [ColumnName("Label")][LoadColumn(1)] public string Category {get; set;}
        [ColumnName("Features")][LoadColumn(2, 10001)][VectorType(10000)] public float[] Values {get; set;}
    }

    public class PreProcessor
    {
        public PreProcessType preProcessType;
        
        List<IImageData> totalList = new List<IImageData>();

        public HistogramData ProcessHistogramSingle(string path)
        {
            var values = new float[80];
            using (var img = Image.Load<Rgb24>(path))
            {
                Resize(img, 256);
                for (int i = 0; i < img.Height; ++i)
                {
                    var row = img.GetPixelRowSpan(i);
                    for (int j = 0; j < img.Width; j++)
                    {
                        var pixel = RGB2HSV(row[j]);
                        var value = Quantilize(pixel);
                        values[value]++;
                    }
                }
            }
            return new HistogramData { Name = path, Values = values, Category = "None" };
        }

        public PixelData ProcessPixelSingle(string path, string category = null)
        {
            var values = new float[10000];
            using (var img = Image.Load<Rgb24>(path))
            {
                Resize(img, 100);
                for (int i = 0; i < img.Height; ++i)
                {
                    var row = img.GetPixelRowSpan(i);
                    for (int j = 0; j < img.Width; j++)
                    {
                        var pixel = RGB2HSV(row[j]);
                        var value = Quantilize(pixel);
                        values[i * img.Width + j] = value;
                    }
                }
            }
            return new PixelData { Name = path, Values = values, Category = category ?? "None" };
        }

        public List<IImageData> ProcessFolder(string path)
        {
            string category = path.Split('.')[1];
            var list = new List<IImageData>();
            Parallel.ForEach(Directory.GetFiles(path), item => {
                if (preProcessType == PreProcessType.Histogram) {
                    list.Add(ProcessHistogramSingle(item));
                } else {
                    list.Add(ProcessPixelSingle(item, category));
                }
            });
            System.Console.WriteLine($"{path} done");
            return list;
        }

        public List<IImageData> ProcessFolders(string[] paths)
        {
            Parallel.ForEach(paths, item => totalList.AddRange(ProcessFolder(item)));
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

    }
}