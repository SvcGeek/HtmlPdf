# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ── Base: runtime image with Chromium system dependencies ────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Install shared libraries required by Chromium/PuppeteerSharp on Debian.
# These are NOT included in the minimal aspnet image and will cause a crash without them.
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2t64 \
    libpango-1.0-0 \
    libcairo2 \
    libx11-6 \
    libx11-xcb1 \
    libxcb1 \
    libxext6 \
    fonts-liberation \
    wget \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Declare persistent volumes.
# /app/chromium          → Chromium binaries (downloaded once, reused across restarts)
# /app/Templates/DynamicTemplates → custom .cshtml templates loaded at startup
VOLUME /app/chromium
VOLUME /app/Templates/DynamicTemplates


# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HtmlPdf.Service/HtmlPdf.Service.csproj", "HtmlPdf.Service/"]
COPY ["Pdf.Abstractions/Pdf.Abstractions.csproj", "Pdf.Abstractions/"]
RUN dotnet restore "./HtmlPdf.Service/HtmlPdf.Service.csproj"
COPY . .
WORKDIR "/src/HtmlPdf.Service"
RUN dotnet build "./HtmlPdf.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build


# ── Publish stage ─────────────────────────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HtmlPdf.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# ── Final stage ───────────────────────────────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HtmlPdf.Service.dll"]
