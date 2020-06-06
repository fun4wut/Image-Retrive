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
        Task currentTask;

        public MilvusController(IDBOperator @operator, IPreprocessable preprocessor)
        {
            _operator = @operator;
            _preprocessor = preprocessor;
        }
        
        [HttpPost("search")]
        public async Task<ActionResult<List<RetriveImg>>> Search([FromForm] int Num, [FromForm] IFormFile file)
        {
            // 先保存至磁盘，再加载出来
            var filePath = Path.GetTempFileName();
            Console.WriteLine(filePath);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            var single = _preprocessor.PreprocessSingle(filePath);
            _preprocessor.TotalList.Add(single);
            _preprocessor.AfterAdd();
            // await _operator.InsertVectors(_preprocessor.TotalList);

            var res = await _operator.Search(_preprocessor.TotalList[0], Num);
            return res;
        }

        [HttpPost("train")]
        public void Train(ReqTrain req)
        {
            _preprocessor.Clear();
            _preprocessor.ProcessFolders(new string[]{Path.Combine("assets", req.File)});
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

        [HttpGet("count")][HttpPost("count")]
        public int Count() => _operator.CurrentProcess.Item2;

        [HttpPost("delete")]
        public async Task ClearAll() => await _operator.Clear();

    }
}