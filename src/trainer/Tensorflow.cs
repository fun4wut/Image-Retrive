using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Preprocess;
using Microsoft.ML.Data;

namespace Trainer
{
    public class TFPrediction : NaiveData
    {
        public float[] Score;
        [ColumnName("PredictedLabel")] public string category;
    }

    public class TFVector
    {
        public string Name;
        [ColumnName("softmax2_pre_activation")] public float[] FeaturesVec {get; set;}
    }

    public static class TFTrainer
    {
        static string PWD = Environment.CurrentDirectory;
        static string PB_PATH = "inception5h/tensorflow_inception_graph.pb";
        static string SOFT2_NAME = "softmax2_pre_activation";
        private static MLContext mlCtx = new MLContext();
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

        public static void ProcessFeatures(string path, string save)
        {
            var data = mlCtx.Data.LoadFromTextFile<NaiveData>(path, separatorChar: ',');
            data = mlCtx.Data.ShuffleRows(data);
            var pipeline = GenBasePipeline();
            var model = pipeline.Fit(data);
            var transformed = model.Transform(data);
            
            var outScores = mlCtx.Data.CreateEnumerable<TFVector>(transformed, reuseRowObject: false);

            using (var writer = new StreamWriter(save))
            {
                foreach (var item in outScores)
                {
                    writer.WriteLine($"{item.Name}\t{string.Join(',', item.FeaturesVec)}");
                }
            }
        }

        public static void TrainAndSave(string path, string save)
        {
            var data = mlCtx.Data.LoadFromTextFile<NaiveData>(path, separatorChar: ',');

            data = mlCtx.Data.ShuffleRows(data);

            var pipeline = GenBasePipeline()
                .Append(mlCtx.Transforms.Conversion.MapValueToKey("Label"))
                .Append(mlCtx.MulticlassClassification.Trainers.LbfgsMaximumEntropy(
                    featureColumnName: SOFT2_NAME
                ))
                .Append(mlCtx.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                .AppendCacheCheckpoint(mlCtx);

                var cvRes = mlCtx.MulticlassClassification.CrossValidate(data, pipeline, 3);
                
                Console.WriteLine("Cross Validate Result:(Macro Accuracy)");
                foreach (var item in cvRes)
                {
                    Console.WriteLine($"{item.Metrics.MacroAccuracy}");
                }
            
                var model = cvRes
                    .OrderByDescending(fold => fold.Metrics.MacroAccuracy)
                    .Select(_ => _.Model)
                    .ToArray()[0];
                
                mlCtx.Model.Save(model, data.Schema, save);    
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