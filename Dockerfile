FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

RUN dotnet restore
RUN dotnet publish ./CardGeneratorBackend.csproj -c Release -o ./out

FROM public.ecr.aws/lambda/dotnet:8

WORKDIR /var/task

# Install native dependencies needed by SkiaSharp
RUN dnf install -y \
    fontconfig \
    freetype \
    glib2 \
    libX11 \
    libXext \
    libXrender \
    libxcb \
    && dnf clean all

COPY --from=build /app/out .
COPY Assets ./Assets

CMD ["CardGeneratorBackend"]