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
    public class TFPreprocessor : IPreprocessable
    {
        static string PWD = Environment.CurrentDirectory;
        static string PB_PATH = "inception5h/tensorflow_inception_graph.pb";
        static string SOFT2_NAME = "softmax2_pre_activation";
        private static MLContext mlCtx = new MLContext();
        List<ImageVector> totalList = new List<ImageVector>(40000);
        public List<ImageVector> TotalList { get => totalList;}
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
            var data = mlCtx.Data.LoadFromEnumerable<ImageVector>(totalList);
            data = mlCtx.Data.ShuffleRows(data);
            var pipeline = GenBasePipeline();
            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);

            totalList = mlCtx.Data.CreateEnumerable<ImageVector>(transformed, reuseRowObject: false).ToList();
            
        }

        public ImageVector PreprocessSingle(string path) => new ImageVector { Name = path, Values = new float[]{} };

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