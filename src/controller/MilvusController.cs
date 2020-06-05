using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Database;

namespace Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MilvusController : ControllerBase
    {
        [HttpGet()]
        public async Task<ActionResult<RetriveImg>> aaa()
        {
            await Task.Delay(100);
            return new RetriveImg();
        }
    }
}