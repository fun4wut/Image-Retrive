using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Trainer
{
    public class TFTrainer
    {
        public static void TrainAndSave(string path, string save)
        {
            var mlCtx = new MLContext();
            
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