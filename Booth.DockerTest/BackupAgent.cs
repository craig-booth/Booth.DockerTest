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
            public int Version;
            string Name;
            public ServiceSpec Spec;
            public int Scale;

            public ServiceDefinition(string id, int version, ServiceSpec spec)
            {
                Id = id; 
                Version = version;  
                Name = spec.Name;
                Scale = (int)spec.Mode.Replicated.Replicas;

                Spec = spec;
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
                await StopService(service);

            await Task.Delay(30000);

            foreach (var service in affectedServices)
                await StartService(service);          
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
                        affectedServices.Add(new ServiceDefinition(service.ID, (int)service.Version.Index, service.Spec));
                        break;
                    }
                }
            }


            return affectedServices;
        }

        private async Task StopService(ServiceDefinition service)
        {
            var serviceParameters = new ServiceUpdateParameters();
            serviceParameters.Service = service.Spec;
            serviceParameters.Service.Mode.Replicated.Replicas = 0;
            serviceParameters.Version = service.Version;

            await _DockerClient.Swarm.UpdateServiceAsync(service.Id, serviceParameters);
        }

        private async Task StartService(ServiceDefinition service)
        {
            var serviceParameters = new ServiceUpdateParameters();
            serviceParameters.Service = service.Spec;
            serviceParameters.Service.Mode.Replicated.Replicas = (ulong)service.Scale;
            serviceParameters.Version = service.Version;

            await _DockerClient.Swarm.UpdateServiceAsync(service.Id, serviceParameters);
        }


    }
}
