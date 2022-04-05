using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Docker.DotNet;

namespace Booth.DockerTest.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var backupAgent = new BackupAgent();

            return await backupAgent.GetVolumes();
        }

        [HttpGet]
        public void Backup(string volumes)
        {
            var backupDefintion = new BackupDefinition();

            backupDefintion.Volumes.AddRange(volumes.Split(","));

            var backupAgent = new BackupAgent();

            backupAgent.Backup(backupDefintion);
        }

    }
}
