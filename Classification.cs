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
        public static void Train(string path)
        {
            var mlCtx = new MLContext(0);
            var data = mlCtx.Data.LoadFromTextFile<PixelData>(path, separatorChar: ',');


            var classifyPipeline = mlCtx.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlCtx.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                .Append(mlCtx.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            var model = classifyPipeline.Fit(data);
            var predEngine = mlCtx.Model.CreatePredictionEngine<PixelData, CategoryPrediction>(model);
            var prepro = new PreProcessor { preProcessType = PreProcessType.Pixel };

            var single = prepro.ProcessPixelSingle("assets/001.ak47/001_0010.jpg");
            System.Console.WriteLine(predEngine.Predict(single).category);
        }
    }
}