namespace CardGeneratorBackend.Environment
{
    public class FileUploadParameters
    {
        public const string ENV_OBJ_KEY = "FileUpload";

        public string MaxFileSizeBytes { get; set; } = string.Empty;

        public int MaxFileSizeBytesParsed
        {
            get
            {
                if(int.TryParse(MaxFileSizeBytes, out int result))
                {
                    return result;
                }

                return 10_000_000;
            }
        }
    }
}
