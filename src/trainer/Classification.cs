using Microsoft.ML;
using Microsoft.ML.Data;
using Preprocess;
using System.Linq;
using System;
namespace Trainer
{
    public class CategoryPrediction
    {
        [ColumnName("PredictedLabel")] public string category;
    }

    public class ClassificationTrainer
    {
        public static void TrainAndSave(string path, string save)
        {
            var mlCtx = new MLContext(0);
            var data = mlCtx.Data.LoadFromTextFile<ImageVector>(path, separatorChar: ',');

            data = mlCtx.Data.ShuffleRows(data);

            var pipeline = mlCtx.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlCtx.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                .Append(mlCtx.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var cvRes = mlCtx.MulticlassClassification.CrossValidate(data, pipeline, 3);

            var paramRes = cvRes.Select(fold => fold.Metrics);
            
            Console.WriteLine("Cross Validate Result(Macro Accuracy):");

            foreach (var item in paramRes)
            {
                Console.WriteLine($"{item.MacroAccuracy}");
            }
            
            
            var model = cvRes
                .OrderByDescending(_ => _.Metrics.MacroAccuracy)
                .Select(_ => _.Model)
                .ToArray()[0];

            // var model = pipeline.Fit(data);

            mlCtx.Model.Save(model, data.Schema, save);
        }
    }
}