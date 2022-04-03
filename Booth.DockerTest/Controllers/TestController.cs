using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booth.DockerTest.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpGet]
        public IEnumerable<BackupDefinition> Get()
        {
            return new BackupDefinition[] { new BackupDefinition() { Name = "volume 1" }, new BackupDefinition() { Name = "volume2" } };
        }

    }
}
