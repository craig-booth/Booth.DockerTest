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
            var result = volumes.Volumes.Select(x => new BackupDefinition(x.Name)).ToList();

            var services = await dockerClient.Swarm.ListServicesAsync();
            foreach (var service in services)
            {
                foreach (var mount in service.Spec.TaskTemplate.ContainerSpec.Mounts)
                {
                    foreach (var backup in result.Where(x => x.VolumeName == mount.Source))
                        backup.Services.Add(service.Spec.Name);
                }
            }

                 
            return result;
        }

    }
}
