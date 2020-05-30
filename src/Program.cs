using System;
using Utils;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Preprocess;
using Trainer;
namespace img_search
{
    class Program
    {
        static void Main(string[] args)
        {
            Timer.Reset();

            // var preprocessor = new PreProcessor { preProcessType = PreProcessType.Histogram };
            // preprocessor.ProcessFolders(Directory.GetDirectories("assets"));
            // preprocessor.Write2CSV("demo.csv");

            // ClassificationTrainer.TrainAndSave("pixel.csv", "classify.zip");

            // ClusterTrainer.TrainAndSave("pixel.csv", "cluster.zip");

            TFTrainer.TrainAndSave("naive.csv", "tf.zip");

            Console.Write("Time used:   ");
            Console.WriteLine(Timer.Stop());
        }
    }
}
