using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Docker.DotNet;

namespace Booth.DockerTest.Controllers
{
    [Route("api")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private IBackupAgent _BackupAgent;
        public TestController(IBackupAgent backupAgent)
        {
            _BackupAgent = backupAgent;
        }

        [HttpGet]
        [Route("volumes")]
        public async Task<IEnumerable<string>> Get()
        {
            return await _BackupAgent.GetVolumes();
        }

        [HttpGet]
        [Route("backup")]
        public async Task<string> Backup([FromQuery] string volumes)
        {
            var backupDefintion = new BackupDefinition();
            backupDefintion.Volumes.AddRange(volumes.Split(","));

            await _BackupAgent.Backup(backupDefintion);

            return "Done!!!";
        }

    }
}
