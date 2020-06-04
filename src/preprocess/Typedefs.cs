using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
namespace Preprocess
{
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

    public class TFData : IImageData
    {
        [LoadColumn(0)] public string Name {get;set;}
        [ColumnName("Label")][LoadColumn(1)] public string Category {get;set;}

        [ColumnName("softmax2_pre_activation")]
        [LoadColumn(2, 1009)] 
        [VectorType(1008)]
        public float[] Values {get; set;} // dummy member
    }

    public interface IPreprocessable<R> where R : IImageData
    {
        List<R> TotalList {get;}
        List<R> ProcessFolder(string dir);
        List<R> ProcessFolders(string[] dirs)
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
        void Write2DB();
    }


}