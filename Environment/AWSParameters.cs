namespace CardGeneratorBackend.Environment
{
    public class AWSParameters
    {
        public const string ENV_OBJ_KEY = "AWS";

        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        public string S3BucketName { get; set; } = string.Empty;
    }
}