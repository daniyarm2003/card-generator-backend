using CardGeneratorBackend.CardGeneration;
using CardGeneratorBackend.Config;
using CardGeneratorBackend.DTO;
using CardGeneratorBackend.Environment;
using CardGeneratorBackend.Exceptions;
using CardGeneratorBackend.FileManagement;
using CardGeneratorBackend.Services;
using CardGeneratorBackend.Services.Impl;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

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
builder.Services.AddScoped<ICardService, CardServiceImpl>();

builder.Services.AddScoped<IFileUploadValidationService, FileExistAndSizeValidator>();

builder.Services.AddScoped<ICardImageGeneratorFactory, DefaultCardImageGeneratorFactory>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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
app.UseExceptionHandler(_ => { });

using (var serviceScope = app.Services.CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetService<CardDatabaseContext>();
    var fileService = serviceScope.ServiceProvider.GetService<ITrackedFileService>();

    if (dbContext is null || fileService is null)
    {
        return;
    }

    var firstCard = dbContext.Cards.AsQueryable().Include(card => card.Type).ThenInclude(type => type.ImageFile)
        .Include(card => card.DisplayImage).Where(card => card.Type.Name == "Electric").FirstOrDefault();

    if (firstCard is null)
    {
        Console.WriteLine("card is null");
        return;
    }

    var cardDTO = firstCard.GetDTO();
    var cardBitmap = new SKBitmap(500, 750);

    using var cardCanvas = new SKCanvas(cardBitmap);
    cardCanvas.Clear(SKColors.Transparent);

    var cardGenerator = new BaseCardImageGenerator(fileService);
    cardGenerator.GenerateCardImage(cardDTO, cardCanvas, cardBitmap.Width, cardBitmap.Height).Wait();

    using var cardImage = SKImage.FromBitmap(cardBitmap);
    using var cardImageData = cardImage.Encode(SKEncodedImageFormat.Png, 100);

    if (cardImageData is null)
    {
        Console.WriteLine("data is null");
        return;
    }

    using var stream = File.OpenWrite("testcard.png");

    cardImageData.SaveTo(stream);
}

app.Run();