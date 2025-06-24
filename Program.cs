using CardGeneratorBackend.Config;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.FileManagement;
using CardGeneratorBackend.Services;
using CardGeneratorBackend.Services.Impl;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<CardDatabaseContext>();

builder.Services.AddSingleton<IFileIOHandlerFactory, FileIOHandlerFactoryImpl>();

builder.Services.AddScoped<ITrackedFileService, TrackedFileServiceImpl>();
builder.Services.AddScoped<ICardTypeService, CardTypeServiceImpl>();

var frontendUrl = builder.Configuration["FrontendURL"] ?? throw new ArgumentException("Frontend URL is not set");

builder.Services.AddCors(cors =>
{
    cors.AddDefaultPolicy(policy => policy
        .WithOrigins(frontendUrl)
        .AllowAnyMethod()
        .AllowAnyHeader());
});

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();