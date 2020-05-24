using Microsoft.ML;
using Microsoft.ML.Data;
using Preprocess;
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
            var data = mlCtx.Data.LoadFromTextFile<PixelData>(path, separatorChar: ',');


            var pipeline = mlCtx.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlCtx.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                .Append(mlCtx.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            var model = pipeline.Fit(data);

            mlCtx.Model.Save(model, data.Schema, save);
        }
    }
}