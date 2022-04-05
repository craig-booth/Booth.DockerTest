using Docker.DotNet;
using Docker.DotNet.Models;

namespace Booth.DockerTest
{
    public class BackupAgent
    {
        private DockerClient _DockerClient;

        private class ServiceDefinition
        {
            public string Id;
            public string Name;
            public int Scale;

            public ServiceDefinition(string id, string name, int scale)
            {
                Id = id; 
                Name = name;
                Scale = scale;
            }
        }

        public BackupAgent()
        {
            _DockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }


        public async Task<IEnumerable<string>> GetVolumes()
        { 
            var volumes = await _DockerClient.Volumes.ListAsync();

            return volumes.Volumes.Select(x => x.Name);
        }

        public async Task Backup(BackupDefinition backupDefinition)
        {
            var affectedServices = await GetAffectedServices(backupDefinition);

            foreach (var service in affectedServices)
                await StopService(service.Name);


            foreach (var service in affectedServices)
                await StartService(service.Name, service.Scale);          
        }

        private async Task<IEnumerable<ServiceDefinition>> GetAffectedServices(BackupDefinition backupDefinition)
        {
            var affectedServices = new List<ServiceDefinition>();

            var services = await _DockerClient.Swarm.ListServicesAsync();
            foreach (var service in services)
            {
                foreach (var mount in service.Spec.TaskTemplate.ContainerSpec.Mounts)
                {
                    if (backupDefinition.Volumes.Contains(mount.Source))
                    {
                        affectedServices.Add(new ServiceDefinition(service.ID, service.Spec.Name, (int)service.Spec.Mode.Replicated.Replicas));
                        break;
                    }
                }
            }


            return affectedServices;
        }

        private async Task StopService(string id)
        {
            var serviceParameters = new ServiceUpdateParameters();
           // serviceParameters.Service.

            await _DockerClient.Swarm.UpdateServiceAsync(id, serviceParameters);
        }

        private async Task StartService(string id, int scale)
        {

        }


    }
}
