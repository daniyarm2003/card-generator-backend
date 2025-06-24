namespace CardGeneratorBackend.Environment
{
    public class DiskFileStorageParameters
    {
        public const string ENV_OBJ_KEY = "DiskFiles";

        public string BaseDirectoryPath { get; set; } = ".";
    }
}
