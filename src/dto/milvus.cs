namespace Dto
{
    public class ReqCreateCollection 
    { 
        public string collection_name {get;set;}
        public int dimension {get;set;}
    }

    public class ReqInsertVector
    {
        
    }

    public class ResBase
    {
        public string message {get; set;}
        public int code {get; set;}
    }

}