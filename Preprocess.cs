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
        int[] Values {get; set;}
    }

    public class HistogramData : IImageData
    { 
        [LoadColumn(0)] public string Name {get; set;}
        [LoadColumn(1)] public string Category {get; set;}
        [LoadColumn(2, 81)][VectorType(80)] public int[] Values {get; set;}
    }

    public class PreProcessor
    {
        public PreProcessType preProcessType;
        
        List<IImageData> totalList = new List<IImageData>();

        public HistogramData ProcessSingle(string path, string category)
        {
            var values = new int[80];
            using (var img = Image.Load<Rgb24>(path))
            {
                Resize(img);
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
            return new HistogramData { Name = path, Values = values, Category = category ?? "None" };
        }

        public List<IImageData> ProcessFolder(string path)
        {
            string category = path.Split('.')[1];
            var list = new List<IImageData>();

            Parallel.ForEach(Directory.GetFiles(path), item => list.Add(ProcessSingle(item, category)));
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