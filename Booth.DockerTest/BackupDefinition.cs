namespace Booth.DockerTest
{
    public class BackupDefinition
    {
        public string VolumeName { get; set; }

        public List<string> Services { get; } = new List<string>();

        public BackupDefinition(string volumneName)
        {
            VolumeName = volumneName;
        }
    }
}
