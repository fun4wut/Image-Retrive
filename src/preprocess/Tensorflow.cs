using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Data;
using System.Threading.Tasks;
namespace Preprocess
{
    public class TFPreprocessor : IPreprocessable<TFData>
    {
        static string PWD = Environment.CurrentDirectory;
        static string PB_PATH = "inception5h/tensorflow_inception_graph.pb";
        static string SOFT2_NAME = "softmax2_pre_activation";
        private static MLContext mlCtx = new MLContext();
        List<TFData> totalList = new List<TFData>(40000);
        public List<TFData> TotalList { get => totalList; }
        private static EstimatorChain<TensorFlowTransformer> GenBasePipeline()
        {
            return mlCtx.Transforms.LoadImages("input", PWD, "Name")
                .Append(mlCtx.Transforms.ResizeImages(
                    "input", 
                    InceptionSettings.ImageWidth,
                    InceptionSettings.ImageHeight
                ))
                .Append(mlCtx.Transforms.ExtractPixels(
                    "input", 
                    interleavePixelColors: InceptionSettings.ChannelsLast, 
                    offsetImage: InceptionSettings.Mean
                ))
                .Append(mlCtx.Model.LoadTensorFlowModel(PB_PATH).ScoreTensorFlowModel(
                    outputColumnName: SOFT2_NAME,
                    inputColumnName: "input",
                    addBatchDimensionInput: true
                ));
        }

        public void AfterAdd()
        {
            var data = mlCtx.Data.LoadFromEnumerable<TFData>(totalList);
            data = mlCtx.Data.ShuffleRows(data);
            var pipeline = GenBasePipeline();
            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);

            totalList = mlCtx.Data.CreateEnumerable<TFData>(transformed, reuseRowObject: false).ToList();
            
        }

        public List<TFData> ProcessFolder(string path)
        {
            string category = path.Split('.')[1];
            var lockObj = new Object();
            var list = new List<TFData>();
            Parallel.ForEach(Directory.GetFiles(path), item => {
                var data =  new TFData { Name = item, Values = new float[]{}, Category = category ?? "None" };
                lock (lockObj) { list.Add(data); }
            });
            Console.WriteLine($"{path} done");
            return list;
        }

        private struct InceptionSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const float Mean = 117;
            public const float Scale = 1;
            public const bool ChannelsLast = true;
        }
    }
}