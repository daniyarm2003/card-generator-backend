using Amazon.Runtime;

namespace CardGeneratorBackend.AWSUtils
{
    public interface IAWSCredentialFactory
    {
        AWSCredentials? GetCredentials();
    }
}