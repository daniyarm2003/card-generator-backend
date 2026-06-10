namespace CardGeneratorBackend.Environment
{
    public class EnvironmentHelper(IConfiguration configuration)
    {
        public string GetEnvironmentVariableOrDefault(string variableName, string defaultValue)
        {
            string? value = configuration[variableName];
            return value ?? defaultValue;
        }

        public string GetBackendURL()
        {
            return GetEnvironmentVariableOrDefault("BackendURL", "http://localhost:8080");
        }
    }
}