namespace CardGeneratorBackend.Environment
{
    public class GoogleServiceParameters
    {
        public const string ENV_OBJ_KEY = "Google";

        public string GeminiAPIKey { get; set; } = "";
        public string GeminiAPIFreeTier { get; set; } = "";

        public bool IsGeminiAPIFreeTier => !bool.TryParse(GeminiAPIFreeTier.ToLower(), out bool result) || result;
    }
}
