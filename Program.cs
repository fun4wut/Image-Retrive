using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Preprocess;
using static Trainer.ClassificationTrainer;
namespace img_search
{
    class Program
    {
        static void Main(string[] args)
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();

            // var preprocessor = new PreProcessor { preProcessType = PreProcessType.Pixel };
            // preprocessor.ProcessFolders(Directory.GetDirectories("assets"));
            // preprocessor.Write2CSV("pixel.csv");

            Train("pixel.csv");

            Console.WriteLine(DateTimeOffset.Now.ToUnixTimeSeconds() - now);
        }
    }
}
