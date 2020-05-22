using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Utils.PixelUtils;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ML.Data;

#nullable enable
namespace Preprocess
{
    public class ImageData
    { 
        [LoadColumn(0)] public string name = "";
        [LoadColumn(1)] public string category = "";
        [LoadColumn(2, 81)][VectorType(80)] public int[] values = new int[80];
    }

    public static class ImageProcessor
    {
        public static ImageData ProcessSingle(string path, string? category)
        {
            var values = new int[80];
            using (var img = Image.Load<Rgb24>(path))
            {
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
            return new ImageData { name = path, values = values, category = category ?? "None" };
        }

        public static List<ImageData> ProcessFolder(string path)
        {
            string category = path.Split('.')[1];
            var list = new List<ImageData>();

            Parallel.ForEach(Directory.GetFiles(path), _ => list.Add(ProcessSingle(_, category)));
            System.Console.WriteLine($"{path} done");
            return list;
        }

        public static void Write2CSV(List<ImageData> datas, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                datas.ForEach(item => writer.WriteLine(
                    $"{item.name},{item.category},{string.Join(',', item.values)}"
                ));
            }
        }

    }
}