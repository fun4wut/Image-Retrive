using System;
using Utils;
using System.IO;
using Preprocess;
using System.Threading.Tasks;
using Database;

namespace Trainer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Timer.Reset();

            IPreprocessable preprocessor = new TFPreprocessor();
            // IPreprocessable preprocessor = new HistogramPreprocessor();
            // preprocessor.ProcessFolders(Directory.GetDirectories("assets"));
            var single = preprocessor.PreprocessSingle("assets/007.bat/007_0001.jpg");
            preprocessor.TotalList.Add(single);
            preprocessor.AfterAdd();
            var dbOperator = new DBOperator(VectorType.TF);

            if (await dbOperator.CheckExists())
            {
                await dbOperator.CreateCollection();
            }

            // await dbOperator.InsertVectors(preprocessor.TotalList);

            var res = await dbOperator.Search(preprocessor.TotalList[0], 10);

            Console.WriteLine($"Test Image: {single.Name}\n\nAnswer:");

            foreach (var item in res)
            {
                Console.WriteLine($"{item.path}\t{item.distance}");
            }

            Console.Write("Time used:   ");
            Console.WriteLine(Timer.Stop());
        }
    }
}
