using Microsoft.ML;
using Microsoft.ML.Data;
using Preprocess;
using System.Linq;
using System;

namespace Trainer
{
    public class ClusterPrediction
    {
        [ColumnName("PredictedLabel")] public uint PredictedId;
        [ColumnName("Score")] public float[] Distances;
    }
    public class ClusterTrainer
    {
        public static void TrainAndSave(string path, string save, int numOfClusters = 8)
        {
            var mlCtx = new MLContext();
            var data = mlCtx.Data.LoadFromTextFile<PixelData>(path, separatorChar: ',');
            var pipeline = mlCtx.Clustering.Trainers.KMeans(numberOfClusters: numOfClusters);

            var cvRes = mlCtx.Clustering.CrossValidate(data, pipeline);

            var paramRes = cvRes.Select(fold => fold.Metrics.AverageDistance);
            
            foreach (var item in paramRes)
            {
                Console.WriteLine(item);
            }
            
            
            var model = cvRes
                .OrderByDescending(_ => _.Metrics.AverageDistance)
                .Select(_ => _.Model)
                .ToArray()[0];

            // var model = pipeline.Fit(data);
            
            mlCtx.Model.Save(model, data.Schema, save);
        }
    }
}