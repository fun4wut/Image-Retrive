namespace RpcDto
{
    public class ReqCreateCollection 
    { 
        public string collection_name {get;set;}
        public int dimension {get;set;}
    }

    public class ReqInsertVector
    {
        public float[][] vectors {get; set;}
    }

    public class ResIds
    {
        public string[] ids {get; set;}
    }

    public class ReqSearch
    {
        public int topk {get; set;}
        public float[][] vectors {get; set;}
        public object @params {get; set;}

    }

    public class ResSearch
    {
        public int num {get; set;}
        public SingleResult[][] result {get; set;} 
    }
    public class SingleResult
    {
        public string id {get; set;}
        public string distance {get; set;}
    }
    public class ResBase
    {
        public string message {get; set;}
        public int code {get; set;}
    }

}