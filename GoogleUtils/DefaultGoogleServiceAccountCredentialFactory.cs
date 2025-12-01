using CardGeneratorBackend.Environment;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.GoogleUtils
{
    public class DefaultGoogleServiceAccountCredentialFactory(IOptions<GoogleServiceParameters> googleOptions, ILogger<DefaultGoogleServiceAccountCredentialFactory> logger) : IGoogleCredentialFactory
    {
        private static readonly string[] SCOPES = [ DriveService.Scope.Drive ];

        private readonly string mCredentialsFilePath = googleOptions.Value.GoogleServiceAccountCredentialsFileName;

        private readonly ILogger<DefaultGoogleServiceAccountCredentialFactory> mLogger = logger;

        public Task<GoogleCredential> GetCredentials()
        {
            using var fileStream = File.OpenRead(mCredentialsFilePath);
            var credentials = GoogleCredential.FromStream(fileStream).CreateScoped(SCOPES);

            mLogger.LogInformation("Retrieved credentials from {}", mCredentialsFilePath);

            return Task.FromResult(credentials);
        }
    }
}
