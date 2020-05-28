using Microsoft.ML.Data;
namespace Preprocess
{
    public interface IImageData
    {
        string Name {get;set;}
        string Category {get;set;}
        float[] Values {get; set;}
    }

    public class HistogramData : IImageData
    {
        [LoadColumn(0)] public string Name {get; set;}
        [ColumnName("Label")][LoadColumn(1)] public string Category {get; set;}
        [ColumnName("Features")][LoadColumn(2, 81)][VectorType(80)] public float[] Values {get; set;}
    }

    public class NaiveData : IImageData
    {
        [LoadColumn(0)] public string Name {get;set;}
        [ColumnName("Label")][LoadColumn(1)] public string Category {get;set;}
        public float[] Values {get; set;} // dummy member
    }

    public class PixelData : IImageData
    {
        [LoadColumn(0)] public string Name {get; set;}
        [ColumnName("Label")][LoadColumn(1)] public string Category {get; set;}
        [ColumnName("Features")][LoadColumn(2, 10001)][VectorType(10000)] public float[] Values {get; set;}
    }
}