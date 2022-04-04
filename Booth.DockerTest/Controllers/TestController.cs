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

            var result = new List<BackupDefinition>();
            foreach (var volume in volumes.Volumes)
            {
                result.Add(new BackupDefinition() { volume.Name });
                result.AddRange(volume.Labels.Select(x => new BackupDefinition() { Name = x.Key + "=" + x.Value }));
            }
     
            return result;
        }

    }
}
