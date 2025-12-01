namespace CardGeneratorBackend.Environment
{
    public class GoogleServiceParameters
    {
        public const string ENV_OBJ_KEY = "Google";

        public string GoogleServiceAccountCredentialsFileName { get; set; } = "";
        public string GoogleDriveStorageFolderId { get; set; } = "";
        public string GoogleDriveLinkedUserEmail { get; set; } = "";
    }
}
