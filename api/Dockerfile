# ---------------------------------------------------------
# STAGE 1 — Build & Publish (Alpine + Self-contained)
# ---------------------------------------------------------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

# Reduce overhead
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_NOLOGO=1 \
    NUGET_XMLDOC_MODE=skip

WORKDIR /src

# Copy only csproj first (better caching)
COPY ["Cashify.Api/Cashify.Api.csproj", "Cashify.Api/"]

# Restore dependencies for Alpine musl
ARG TARGETARCH
RUN dotnet restore Cashify.Api/Cashify.Api.csproj -r linux-musl-"$TARGETARCH"

# Copy entire project
COPY ["Cashify.Api/", "Cashify.Api/"]

# Framework-dependent publish for smaller size
RUN dotnet publish Cashify.Api/Cashify.Api.csproj \
    --no-restore \
    -c Release \
    -r linux-musl-"$TARGETARCH" \
    /p:PublishSingleFile=true \
    /p:SelfContained=false \
    -o /publish


# ---------------------------------------------------------
# STAGE 2 — ASP.NET Runtime (Smaller than self-contained)
# ---------------------------------------------------------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

# Install runtime dependencies for Alpine
RUN apk add --no-cache curl icu-data-full icu-libs tzdata

# Create logs directory
RUN mkdir -p /app/logs

WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy self-contained single-file output
COPY --from=build /publish .

# Expose API port
EXPOSE 8080

# Because PublishSingleFile=true, output will be "./Cashify.Api"
ENTRYPOINT ["./Cashify.Api"]