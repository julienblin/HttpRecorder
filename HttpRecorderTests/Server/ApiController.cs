using Microsoft.AspNetCore.Mvc;

namespace HttpRecorderTests.Server
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        public const string GetJsonUri = "json";

        [HttpGet(GetJsonUri)]
        public IActionResult GetJson()
            => Ok(new JsonModel());
    }
}
