﻿using System;
using Utils;
using System.IO;
using Preprocess;
using Trainer;

namespace img_search
{
    class Program
    {
        static void Main(string[] args)
        {
            Timer.Reset();

            // IPreprocessable<TFData> preprocessor = new TFPreprocessor();
            IPreprocessable<HistogramData> preprocessor = new HistogramPreprocessor();
            preprocessor.ProcessFolders(Directory.GetDirectories("assets"));
            preprocessor.AfterAdd();
            preprocessor.Write2CSV("histogram_feat.csv");
            // ClassificationTrainer.TrainAndSave("pixel.csv", "classify.zip");

            // ClusterTrainer.TrainAndSave("pixel.csv", "cluster.zip");

            // TFTrainer.TrainAndSave("naive.csv", "tf.zip");


            Console.Write("Time used:   ");
            Console.WriteLine(Timer.Stop());
        }
    }
}
