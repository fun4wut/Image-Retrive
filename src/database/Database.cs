using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using RestSharp;
using RpcDto;
using Preprocess;
using StackExchange.Redis;
using RestSharp.Serializers.SystemTextJson;


namespace Database
{
    public class DBOperator : IDBOperator
    {
        RestClient client = new RestClient();
        string Milvus_URL = "http://127.0.0.1:19121";
        ConnectionMultiplexer conn = ConnectionMultiplexer.Connect("localhost, allowAdmin=true");

        object lockObj = new object();

        bool isTFVector;
        public DBOperator(VectorType vecType)
        {
            this.isTFVector = vecType == VectorType.TF;
            client.UseSystemTextJson();
        }

        string CollectionName {get => isTFVector ? "tf" : "histogram";}

        int Dimension {get => isTFVector ? 1001 : 80;}

        public (int, int) CurrentProcess {get; set;}

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

        public async Task UpdateCollection()
        {

            var req = new RestRequest($"{Milvus_URL}/collections/{CollectionName}/indexes")
                .AddJsonBody(new { index_type = "IVFFLAT", @params = new { nlist = 2048, nprobe = 64 } });
            
            req.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            await client.PostAsync<ResBase>(req);
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
                CurrentProcess = (0, res.ids.Length);
                // 插入至redis
                await Task.WhenAll(vectors.Zip(res.ids).Select(item => Task.Run(async () => {
                    await db.StringSetAsync(item.Second, item.First.Name);
                    Console.WriteLine(item.First.Name);
                    lock (lockObj)
                    {
                        CurrentProcess = (CurrentProcess.Item1 + 1, CurrentProcess.Item2);
                    }
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

            var json = new {
                    search = new ReqSearch { topk = topk, vectors = new float[][]{vec.Values}, @params = new { nprobe = 300 } }
                };
            var req = new RestRequest($"{Milvus_URL}/collections/{CollectionName}/vectors")
                .AddJsonBody(json);
                
            req.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            var res = await client.PutAsync<ResSearch>(req);
            var list = new List<RetriveImg>();
            if (res.num == 0)
            {
                return list;
            }
            var db = conn.GetDatabase();
            await Task.WhenAll(res.result[0].Select(item => Task.Run(async () => {
                var path = await db.StringGetAsync(item.id);
                lock (lockObj)
                {
                    list.Add(new RetriveImg { distance = double.Parse(item.distance), id = item.id, path = (string)path });
                }
            })));
            return list;
        }

        public async Task Clear()
        {
            var req = new RestRequest($"{Milvus_URL}/collections/{CollectionName}");
            client.Delete(req); // 这里不能使用async，原因未知
            await this.CreateCollection();
            await this.UpdateCollection();
            await conn.GetServer("localhost:6379").FlushDatabaseAsync();
        }

    }
}