using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Docker.DotNet;

namespace Booth.DockerTest.Controllers
{
    [Route("api")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpGet]
        [Route("volumes")]
        public async Task<IEnumerable<string>> Get()
        {
            var backupAgent = new BackupAgent();

            return await backupAgent.GetVolumes();
        }

        [HttpGet]
        [Route("backup")]
        public async Task<string> Backup([FromQuery] string volumes)
        {
            var backupDefintion = new BackupDefinition();

            backupDefintion.Volumes.AddRange(volumes.Split(","));

            var backupAgent = new BackupAgent();

            await backupAgent.Backup(backupDefintion);

            return "Done!!!";
        }

    }
}
