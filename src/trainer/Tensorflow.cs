using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Preprocess;
using Microsoft.ML.Data;

namespace Trainer
{
    public class TFPrediction : NaiveData
    {
        public float[] Score;
        [ColumnName("PredictedLabel")] public string category;
    }
    public class TFTrainer
    {
        static string PWD = Environment.CurrentDirectory;
        static string PB_PATH = "inception5h/tensorflow_inception_graph.pb";
        static string SOFT2_NAME = "softmax2_pre_activation";
        public static void TrainAndSave(string path, string save)
        {
            var mlCtx = new MLContext();

            var data = mlCtx.Data.LoadFromTextFile<NaiveData>(path, separatorChar: ',');

            data = mlCtx.Data.ShuffleRows(data);

            var pipeline = mlCtx.Transforms.LoadImages("input", PWD, "Name")
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
                ))
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

        private static void DisplayResults(IEnumerable<TFPrediction> imagePredictionData)
        {
            foreach (var prediction in imagePredictionData)
            {
                Console.WriteLine($"Image: {Path.GetFileName(prediction.Name)} predicted as: {prediction.Category} with score: {prediction.Score.Max()} ");
            }
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