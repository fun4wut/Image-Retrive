using System;
using System.Threading.Tasks;
using RestSharp;
using Dto;
namespace Utils
{
    public static class DBUtils
    {
        static readonly RestClient client = new RestClient();
        static readonly string Milvus_URL = "http://127.0.0.1:19121";
        static string Collection_Name = "imgs";
        public static async Task<bool> CheckExists()
        {
            try
            {
                await client.GetAsync<object>(new RestRequest($"{Milvus_URL}/collections/imgs"));
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
            
        }

        public static async Task CreateCollection()
        {
            try
            {
                var req = new RestRequest($"{Milvus_URL}/collections")
                    .AddJsonBody(new ReqCreateCollection { collection_name = Collection_Name, dimension = 1080 });
                
                var res = await client.PostAsync<ResBase>(req);
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Error occured: {e}");
                throw;
            }
        }

        public static async Task InsertVector()
        {

        }


    }
}