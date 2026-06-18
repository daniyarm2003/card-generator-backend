using CardGeneratorBackend.CardGeneration;
using CardGeneratorBackend.Config;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.FileManagement;
using CardGeneratorBackend.Services;
using CardGeneratorBackend.Services.Impl;
using Amazon.S3;
using Amazon.Runtime;
using CardGeneratorBackend.AWSUtils;
using Amazon;
using Microsoft.Extensions.Options;
using CardGeneratorBackend.DTO.Mappers;
using CardGeneratorBackend.AI.Embeddings;
using CardGeneratorBackend;

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

builder.Services.AddSingleton<EnvironmentHelper>();

builder.Services.AddDbContext<CardDatabaseContext>();

builder.Services.AddSingleton<IDefaultFileMethodRetriever, DefaultFileMethodRetrieverImpl>();
builder.Services.AddSingleton<IFileIOHandlerFactory, FileIOHandlerFactoryImpl>();

// Gemini setup
builder.Services.AddSingleton(serviceProvider =>
{
    var geminiOptions = serviceProvider.GetService<IOptions<GoogleServiceParameters>>();
    ArgumentNullException.ThrowIfNull(geminiOptions);

    return new Google.GenAI.Client(apiKey: geminiOptions.Value.GeminiAPIKey);
});

// AWS utils setup
builder.Services.AddSingleton<IAWSCredentialFactory, DefaultAWSCredentialFactory>();

builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
{
    var awsCredentialFactory = serviceProvider.GetService<IAWSCredentialFactory>();
    var awsOptions = serviceProvider.GetService<IOptions<AWSParameters>>();

    ArgumentNullException.ThrowIfNull(awsCredentialFactory);

    AWSCredentials? awsCredentials = awsCredentialFactory.GetCredentials();
    RegionEndpoint awsRegion = RegionEndpoint.GetBySystemName(awsOptions?.Value.Region);

    return awsCredentials != null ? new AmazonS3Client(awsCredentials, awsRegion) : new AmazonS3Client(awsRegion);
});

builder.Services.AddScoped<ScopedStartupProcedures>();

builder.Services.AddKeyedScoped<IEmbeddingCachingStrategy, DatabaseEmbeddingCachingStrategy>("embedding_db_cache");
builder.Services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();

builder.Services.AddScoped<ITrackedFileService, TrackedFileServiceImpl>();
builder.Services.AddScoped<ICardTypeService, CardTypeServiceImpl>();
builder.Services.AddScoped<ICardService, CardServiceImpl>();
builder.Services.AddScoped<ICardEmbeddingService, CardEmbeddingServiceImpl>();

builder.Services.AddScoped<IFileUploadValidationService, FileExistAndSizeValidator>();

builder.Services.AddScoped<ICardImageGeneratorFactory, DefaultCardImageGeneratorFactory>();

// DTO Mappers
builder.Services.AddScoped<CardTypeDTOMapper>();
builder.Services.AddScoped<CardDTOMapper>();

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

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var startupProcedures = scope.ServiceProvider.GetRequiredService<ScopedStartupProcedures>();
    await startupProcedures.RunMainProcedure();
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

app.MapControllers();

app.UseCors();
app.UseExceptionHandler(_ => { });

app.Run();