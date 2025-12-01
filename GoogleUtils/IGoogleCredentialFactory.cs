using Google.Apis.Auth.OAuth2;

namespace CardGeneratorBackend.GoogleUtils
{
    public interface IGoogleCredentialFactory
    {
        Task<GoogleCredential> GetCredentials();
    }
}
