using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Preprocess
{
    public class ImageVector
    {
        public string Name {get;set;}
        [ColumnName("Label")] public string Category = "None";
        [ColumnName("softmax2_pre_activation")][VectorType(1008)] public float[] Values {get; set;}
    }

    public interface IPreprocessable
    {
        List<ImageVector> TotalList {get;}
        ImageVector PreprocessSingle(string path);
        List<ImageVector> ProcessFolder(string dir)
        {
            var lockObj = new Object();
            var list = new List<ImageVector>();
            Parallel.ForEach(Directory.GetFiles(dir), item => {
                var data = PreprocessSingle(item);
                lock (lockObj) { list.Add(data); }
            });
            Console.WriteLine($"{dir} done");
            return list;
        }
        List<ImageVector> ProcessFolders(string[] dirs)
        {
            Array.ForEach(dirs, item => TotalList.AddRange(ProcessFolder(item)));
            return TotalList;
        }
        void AfterAdd(){}
        void Write2CSV(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                TotalList.ForEach(item => writer.WriteLine(
                    $"{item.Name},{item.Category},{string.Join(',', item.Values)}"
                ));
            }
        }

        void Clear()
        {
            TotalList.Clear();
        }
        // void Write2DB();
    }


}