using System.Threading.Tasks;
using System.Collections.Generic;
using Preprocess;

namespace Database
{
    public enum VectorType { Histogram, TF }

    public class RetriveImg
    {
        public string id {get; set;}
        public string path {get; set;}
        public string distance {get; set;}
    }

    public interface IDBOperator
    {
        Task<bool> CheckExists();
        Task CreateCollection();
        Task InsertVectors(List<ImageVector> vectors);
        Task<List<RetriveImg>> Search(ImageVector vec, int topk);
        (int, int) CurrentProcess {get; set;}
    }
}