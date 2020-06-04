using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using RestSharp;
using Dto;
using Preprocess;
using StackExchange.Redis;
using RestSharp.Serializers.SystemTextJson;

namespace Database
{
    public enum VectorType { Histogram, TF }

    public class RetriveImg
    {
        public string id {get; set;}
        public string path {get; set;}
        public string distance {get; set;}
    }
    public class DBOperator
    {
        RestClient client = new RestClient();
        string Milvus_URL = "http://127.0.0.1:19121";
        ConnectionMultiplexer conn = ConnectionMultiplexer.Connect("localhost");

        object lockObj = new object();

        bool isTFVector;
        public DBOperator(VectorType vecType)
        {
            this.isTFVector = vecType == VectorType.TF;
            client.UseSystemTextJson();
        }

        string CollectionName {get => isTFVector ? "tf" : "histogram";}

        int Dimension {get => isTFVector ? 1008 : 80;}

        public async Task<bool> CheckExists()
        {
            try
            {
                await client.GetAsync<object>(new RestRequest($"{Milvus_URL}/collections/{CollectionName}"));
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
            
        }

        public async Task CreateCollection()
        {
            try
            {
                var req = new RestRequest($"{Milvus_URL}/collections")
                    .AddJsonBody(new ReqCreateCollection { collection_name = CollectionName, dimension = Dimension });
                
                var res = await client.PostAsync<ResBase>(req);
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Error occured: {e}");
                throw;
            }
        }

        public async Task InsertVectors(List<ImageVector> vectors)
        {
            var vecs = vectors.Select(item => item.Values).ToArray();
            try
            {
                var req = new RestRequest($"{Milvus_URL}/collections/{CollectionName}/vectors")
                    .AddJsonBody(new ReqInsertVector { vectors = vecs });
                var res = await client.PostAsync<ResIds>(req);
                var db = conn.GetDatabase();
                // 插入至redis
                await Task.WhenAll(vectors.Zip(res.ids).Select(item => Task.Run(async () => {
                    await db.StringSetAsync(item.Second, item.First.Name);
                })));
                
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Error occured: {e}");
                throw;
            }
        }

        public async Task<List<RetriveImg>> Search(ImageVector vec, int topk)
        {
            var req = new RestRequest($"{Milvus_URL}/collections/{CollectionName}/vectors")
                .AddJsonBody(new {
                    search = new ReqSearch { topk = topk, vectors = new float[][]{vec.Values} }
                });
            
            var res = await client.PutAsync<ResSearch>(req);
            Console.WriteLine(res.num);
            var list = new List<RetriveImg>();
            if (res.num == 0)
            {
                return list;
            }
            var db = conn.GetDatabase();
            await Task.WhenAll(res.results[0].Select(item => Task.Run(async () => {
                var path = await db.StringGetAsync(item.id);
                lock (lockObj)
                {
                    list.Add(new RetriveImg { distance = item.distance, id = item.id, path = (string)path });
                }
            })));

            return list;
        }


    }
}