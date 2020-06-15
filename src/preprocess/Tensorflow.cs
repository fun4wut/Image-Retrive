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
        static string PB_PATH = "inception5h/inception_v3_2016_08_28_frozen.pb";
        static string SOFT2_NAME = "InceptionV3/Predictions/Reshape";
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
                    offsetImage: InceptionSettings.Mean,
                    scaleImage: InceptionSettings.Scale
                ))
                .Append(mlCtx.Model.LoadTensorFlowModel(PB_PATH).ScoreTensorFlowModel(
                    outputColumnName: SOFT2_NAME,
                    inputColumnName: "input",
                    addBatchDimensionInput: false
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
            public const int ImageHeight = 299;
            public const int ImageWidth = 299;
            public const float Mean = 117;
            public const float Scale = 1/255f;
            public const bool ChannelsLast = true;
        }
    }
}