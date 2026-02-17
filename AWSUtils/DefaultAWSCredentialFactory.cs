using Amazon.Runtime;
using CardGeneratorBackend.Environment;
using Microsoft.Extensions.Options;

namespace CardGeneratorBackend.AWSUtils
{
    public class DefaultAWSCredentialFactory(IOptions<AWSParameters> awsOptions, IWebHostEnvironment environment) : IAWSCredentialFactory
    {
        public AWSCredentials? GetCredentials()
        {
            if(environment.IsDevelopment())
            {
                var awsParams = awsOptions.Value;
                return new BasicAWSCredentials(awsParams.AccessKey, awsParams.SecretKey);
            }
            else
            {
                // In production, we will rely on IAM roles, so we can return null here
                return null;
            }
        }
    }
}