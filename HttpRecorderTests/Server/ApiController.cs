using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HttpRecorderTests.Server
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        public const string JsonUri = "json";
        public const string FormDataUri = "formdata";

        [HttpGet(JsonUri)]
        public IActionResult GetJson([FromQuery] string name = null)
            => Ok(new SampleModel { Name = name ?? SampleModel.DefaultName });

        [HttpPost(JsonUri)]
        public IActionResult PostJson(SampleModel model)
            => Ok(model);

        [HttpPost(FormDataUri)]
        public IActionResult PostFormData([FromForm] SampleModel model)
            => Ok(model);
    }
}
