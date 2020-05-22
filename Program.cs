using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Preprocess;
using static Preprocess.ImageProcessor;
namespace img_search
{
    class Program
    {
        static void Main(string[] args)
        {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            var totalList = new List<ImageData>();
            Parallel.ForEach(Directory.GetDirectories("assets"), item => totalList.AddRange(ProcessFolder(item)));
            // var totalList = ProcessFolder("assets/001.ak47");
            Write2CSV(totalList, "demo.csv");
            Console.WriteLine(DateTimeOffset.Now.ToUnixTimeSeconds() - now);
        }
    }
}
