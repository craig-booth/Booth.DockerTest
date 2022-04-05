using Docker.DotNet;
using Docker.DotNet.Models;

namespace Booth.DockerTest
{

    public interface IBackupAgent
    {
        Task<IEnumerable<string>> GetVolumes();
        Task Backup(BackupDefinition backupDefinition);
    }

    public class BackupAgent : IBackupAgent
    {
        private DockerClient _DockerClient;
        private ILogger _Logger;

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

        public BackupAgent(ILogger<BackupAgent> logger)
        {
            _Logger = logger;
            _DockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }


        public async Task<IEnumerable<string>> GetVolumes()
        {
            _Logger?.LogInformation("Get Volumes");
            var volumes = await _DockerClient.Volumes.ListAsync();

            return volumes.Volumes.Select(x => x.Name);
        }

        public async Task Backup(BackupDefinition backupDefinition)
        {
            _Logger?.LogInformation("Backup volumes: " + String.Join(", ", backupDefinition.Volumes));
            var affectedServices = await GetAffectedServices(backupDefinition);

            _Logger?.LogInformation("Stop services...");
            foreach (var service in affectedServices)
                await StopService(service);

            _Logger?.LogInformation("Waiting...");
            await Task.Delay(30000);

            _Logger?.LogInformation("Restart services...");
            foreach (var service in affectedServices)
                await StartService(service);          
        }

        private async Task<IEnumerable<ServiceDefinition>> GetAffectedServices(BackupDefinition backupDefinition)
        {
            _Logger?.LogInformation("Get affected services");
            var affectedServices = new List<ServiceDefinition>();

            var services = await _DockerClient.Swarm.ListServicesAsync();
            _Logger?.LogInformation("Found services");
            foreach (var service in services)
            {
                _Logger?.LogInformation("service id: " + service.ID);
                _Logger?.LogInformation("service name: " + service.Spec.Name);

              /*  foreach (var mount in service.Spec.TaskTemplate.ContainerSpec.Mounts)
                {
                    if (backupDefinition.Volumes.Contains(mount.Source))
                    {
                        affectedServices.Add(new ServiceDefinition(service.ID, (int)service.Version.Index, service.Spec));
                        break;
                    }
                } */

                _Logger?.LogInformation("done" );
            }

            _Logger?.LogInformation($"Found {affectedServices.Count} services");

            return affectedServices;
        }

        private async Task StopService(ServiceDefinition service)
        {
            var serviceParameters = new ServiceUpdateParameters();
            serviceParameters.Service = service.Spec;
            serviceParameters.Service.Mode.Replicated.Replicas = 0;
            serviceParameters.Version = service.Version;

            _Logger?.LogInformation("Update service " + service.Spec.Name);
            await _DockerClient.Swarm.UpdateServiceAsync(service.Id, serviceParameters);
            _Logger?.LogInformation("Updated");
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
