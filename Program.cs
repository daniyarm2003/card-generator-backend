using CardGeneratorBackend.CardGeneration;
using CardGeneratorBackend.Config;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.FileManagement;
using CardGeneratorBackend.GoogleUtils;
using CardGeneratorBackend.Services;
using CardGeneratorBackend.Services.Impl;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Amazon.S3;
using Amazon.Runtime;
using CardGeneratorBackend.AWSUtils;
using Amazon;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"The current environment is: {builder.Environment.EnvironmentName}");

builder.Configuration.AddEnvironmentVariables();

var awsRegionName = builder.Configuration.GetValue<string>("AWS:Region");
var awsRegion = RegionEndpoint.GetBySystemName(awsRegionName);

Console.WriteLine($"AWS Region from configuration: {awsRegion.DisplayName}");

// In production, retrieve environment variables from AWS SSM Parameter Store
if(builder.Environment.IsProduction())
{
    var ssmClient = new AmazonSimpleSystemsManagementClient(awsRegion);
    var parameterName = builder.Configuration.GetValue<string>("AWS:SsmEnvParameterName");

    try
    {
        var ssmRequest = new GetParameterRequest
        {
            Name = parameterName,
            WithDecryption = true
        };

        var ssmResponse = ssmClient.GetParameterAsync(ssmRequest).Result;
        var parameterValue = ssmResponse.Parameter.Value;

        // Base64 is used as a little hack to store multiple environment variables in a single SSM parameter, since SSM parameters are just key-value pairs
        byte[] base64DecodedValueBytes = Convert.FromBase64String(parameterValue);
        string envString = System.Text.Encoding.UTF8.GetString(base64DecodedValueBytes);

        string[] envLines = envString.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in envLines)
        {
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex > 0)
            {
                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Error while retrieving environment variables from SSM Parameter Store: {ex.Message}");
        throw;
    }
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PostgresConnectionParameters>(
    builder.Configuration.GetSection(PostgresConnectionParameters.ENV_OBJECT_KEY));

builder.Services.Configure<DiskFileStorageParameters>(
    builder.Configuration.GetSection(DiskFileStorageParameters.ENV_OBJ_KEY));

builder.Services.Configure<FileUploadParameters>(
    builder.Configuration.GetSection(FileUploadParameters.ENV_OBJ_KEY));

builder.Services.Configure<GoogleServiceParameters>(
    builder.Configuration.GetSection(GoogleServiceParameters.ENV_OBJ_KEY));

builder.Services.Configure<AWSParameters>(
    builder.Configuration.GetSection(AWSParameters.ENV_OBJ_KEY));

builder.Services.AddDbContext<CardDatabaseContext>();

builder.Services.AddSingleton<IDefaultFileMethodRetriever, DefaultFileMethodRetrieverImpl>();
builder.Services.AddSingleton<IFileIOHandlerFactory, FileIOHandlerFactoryImpl>();

// Google service account utils setup
builder.Services.AddSingleton<IGoogleCredentialFactory, DefaultGoogleServiceAccountCredentialFactory>();

// AWS utils setup
builder.Services.AddSingleton<IAWSCredentialFactory, DefaultAWSCredentialFactory>();

builder.Services.AddSingleton(serviceProvider =>
{
    var credentialFactory = serviceProvider.GetService<IGoogleCredentialFactory>();
    ArgumentNullException.ThrowIfNull(credentialFactory);

    try
    {
        var credentialTask = credentialFactory.GetCredentials();
        credentialTask.Wait();

        return credentialTask.Result;
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Error while getting Google credentials: {ex.Message}");
        throw;
    }
});

builder.Services.AddSingleton(serviceProvider =>
{
    GoogleCredential? credentials = null;

    try
    {
        credentials = serviceProvider.GetService<GoogleCredential>();
        ArgumentNullException.ThrowIfNull(credentials);
    }
    catch(Exception)
    {
        Console.WriteLine("Error while getting Google credentials for DriveService initialization. Check previous logs for details. DriveService will be initialized with null credentials, which will likely cause errors when used.");
    }

    return new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
    {
        HttpClientInitializer = credentials,
        ApplicationName = "DFA Card Generator"
    });
});

builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
{
    var awsCredentialFactory = serviceProvider.GetService<IAWSCredentialFactory>();
    var awsOptions = serviceProvider.GetService<IOptions<AWSParameters>>();

    ArgumentNullException.ThrowIfNull(awsCredentialFactory);

    AWSCredentials? awsCredentials = awsCredentialFactory.GetCredentials();
    RegionEndpoint awsRegion = RegionEndpoint.GetBySystemName(awsOptions?.Value.Region);

    return awsCredentials != null ? new AmazonS3Client(awsCredentials, awsRegion) : new AmazonS3Client(awsRegion);
});

builder.Services.AddScoped<ITrackedFileService, TrackedFileServiceImpl>();
builder.Services.AddScoped<ICardTypeService, CardTypeServiceImpl>();
builder.Services.AddScoped<ICardService, CardServiceImpl>();

builder.Services.AddScoped<IFileUploadValidationService, FileExistAndSizeValidator>();

builder.Services.AddScoped<ICardImageGeneratorFactory, DefaultCardImageGeneratorFactory>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(cors =>
    {
        cors.AddDefaultPolicy(policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    });
}
else
{
    var frontendUrlSection = builder.Configuration.GetSection("FrontendURLs");

    if (!frontendUrlSection.Exists())
    {
        throw new ArgumentException("Frontend URL is not set");
    }

    var frontendUrls = frontendUrlSection.GetChildren().Where(url => url.Value is not null).Select(url => url.Value!);

    builder.Services.AddCors(cors =>
    {
        cors.AddDefaultPolicy(policy => policy
            .WithOrigins([.. frontendUrls])
            .AllowAnyMethod()
            .AllowAnyHeader());
    });
}

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CardDatabaseContext>();
    db.Database.Migrate();

    Console.WriteLine("Database migrations applied successfully");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
    });

    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
// app.UseAuthorization();
app.MapControllers();

app.UseCors();
app.UseExceptionHandler(_ => { });

app.Run();