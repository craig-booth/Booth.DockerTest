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
        public async Task<IEnumerable<BackupDefinition>> Get()
        {
            var dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

            var volumes = await dockerClient.Volumes.ListAsync();

            return volumes.Volumes.Where(x => x.Labels.ContainsKey("booth.dockerbackup.enable")).Where(x => x.Labels["booth.dockerbackup.enable"] == "true").Select(x => new BackupDefinition() { Name = x.Name });

        }

    }
}
