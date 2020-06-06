using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Database;
using Preprocess;
using RestDto;
namespace Controller
{
    [Route("api/v1")]
    [ApiController]
    public class MilvusController : ControllerBase
    {
        private readonly IDBOperator _operator;
        private readonly IPreprocessable _preprocessor;

        static string PWD = Environment.CurrentDirectory;

        Task currentTask;

        public MilvusController(IDBOperator @operator, IPreprocessable preprocessor)
        {
            _operator = @operator;
            _preprocessor = preprocessor;
        }
        
        [HttpGet("search")]
        public async Task<ActionResult<RetriveImg>> Search()
        {
            // IPreprocessable preprocessor = new HistogramPreprocessor();
            // preprocessor.ProcessFolders(Directory.GetDirectories("assets"));
            var single = _preprocessor.PreprocessSingle("assets/007.bat/007_0001.jpg");
            _preprocessor.TotalList.Add(single);
            _preprocessor.AfterAdd();
            // await _operator.InsertVectors(_preprocessor.TotalList);

            var res = await _operator.Search(_preprocessor.TotalList[0], 10);
            return res[0];
        }

        [HttpPost("train")]
        public void Train(ReqTrain req)
        {
            _preprocessor.Clear();
            _preprocessor.ProcessFolders(new string[]{Path.Combine(PWD, req.File)});
            _preprocessor.AfterAdd();
            // await _operator.InsertVectors(_preprocessor.TotalList);
            currentTask = Task.Run(async ()  => await _operator.InsertVectors(_preprocessor.TotalList));
            
        }

        [HttpGet("process")]
        public string Process()
        {
            var (cur, tot) = _operator.CurrentProcess;
            return $"{cur}/{tot}";
        }

        [HttpGet("count")]
        [HttpPost("count")]
        public int Count()
        {
            return _operator.CurrentProcess.Item2;
        }

    }
}