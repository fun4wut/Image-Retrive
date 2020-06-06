namespace RestDto
{

    public static class ExtensionRes
    {
        public static ResBase ToRes(this object data) => new ResBase { data = data, status = 200 };
    }

    public class ReqTrain
    {
        public string File {get; set;}
    }

    public interface ResData {}

    public class ResBase
    {
        public object data {get; set;}
        public int status {get; set;} = 200;

    }

    public class ResProcess : ResData
    {
        public int _current {get; set;}
        public int _total {get; set;}
    }

}