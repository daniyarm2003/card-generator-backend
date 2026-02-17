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

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"The current environment is: {builder.Environment.EnvironmentName}");

builder.Configuration.AddEnvironmentVariables();

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

var app = builder.Build();


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