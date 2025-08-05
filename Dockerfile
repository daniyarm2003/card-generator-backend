FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

RUN dotnet restore
RUN dotnet publish ./CardGeneratorBackend.csproj -c Release -r linux-x64 --self-contained false -o ./out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Install native dependencies needed by SkiaSharp
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libglib2.0-0 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libxcb1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .
COPY Assets ./Assets

EXPOSE 8080

CMD ["dotnet", "CardGeneratorBackend.dll"]