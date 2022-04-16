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

            await CreateContainer();
            /*
            _Logger?.LogInformation("Backup volumes: " + String.Join(", ", backupDefinition.Volumes));
            var affectedServices = await GetAffectedServices(backupDefinition);

            _Logger?.LogInformation("Stop services...");
            foreach (var service in affectedServices)
                await StopService(service);

            _Logger?.LogInformation("Waiting...");
            await Task.Delay(30000);

            _Logger?.LogInformation("Restart services...");
            foreach (var service in affectedServices)
                await StartService(service);      */    
        }

        public async Task CreateContainer()
        {
            _Logger.LogInformation("Create image");

            var imageParameters = new ImagesCreateParameters
            {
                FromImage = "ubunta",
                Tag = "latest"
            };
            await _DockerClient.Images.CreateImageAsync(imageParameters, null, new Progress<JSONMessage>());
            

            _Logger.LogInformation("Create Container");

            var createParameters = new CreateContainerParameters
            {
                Name = "DockerTest.Agent",
                Image = "ubunta",
                Cmd = new[] { "bash", "-c", "tar cvf /backup/unifi_config2.tar /source" },
                HostConfig = new HostConfig
                {
                    Binds = new[] { @"unifi_config:/source:ro" }
                }
            };

            var response = await _DockerClient.Containers.CreateContainerAsync(createParameters);
            var containerId = response.ID;

            _Logger.LogInformation($"New container id: {containerId}"); 

            var startParameters = new ContainerStartParameters();
            await _DockerClient.Containers.StartContainerAsync(containerId, startParameters);

            _Logger.LogInformation("Started Container");
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


                _Logger?.LogInformation("service tasktemplate: " + service.Spec.TaskTemplate);
                _Logger?.LogInformation("service containerspec: " + service.Spec.TaskTemplate.ContainerSpec);
                _Logger?.LogInformation("service mounts: " + service.Spec.TaskTemplate.ContainerSpec.Mounts);

                if (service.Spec.TaskTemplate.ContainerSpec.Mounts != null)
                {
                    _Logger?.LogInformation("checking mounts");

                    foreach (var mount in service.Spec.TaskTemplate.ContainerSpec.Mounts)
                    {
                        _Logger?.LogInformation("volume: " + mount.Source);

                        if (backupDefinition.Volumes.Contains(mount.Source))
                        {
                            _Logger?.LogInformation("add version: " + service.Version.Index);
                            _Logger?.LogInformation("add spec: " + service.Spec);
                            affectedServices.Add(new ServiceDefinition(service.ID, (int)service.Version.Index, service.Spec));
                            _Logger?.LogInformation("Added");
                            break;
                        }
                    }
                }
                

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
            serviceParameters.Version = service.Version + 1;

            await _DockerClient.Swarm.UpdateServiceAsync(service.Id, serviceParameters);
        }


    }
}
