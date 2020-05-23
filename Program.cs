using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Preprocess;

namespace img_search
{
    class Program
    {
        static void Main(string[] args)
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            var preprocessor = new PreProcessor { preProcessType = PreProcessType.Histogram };
            preprocessor.ProcessFolders(Directory.GetDirectories("assets"));
            // var totalList = ProcessFolder("assets/001.ak47");
            preprocessor.Write2CSV("demo.csv");
            Console.WriteLine(DateTimeOffset.Now.ToUnixTimeSeconds() - now);
        }
    }
}
