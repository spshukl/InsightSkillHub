
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER root
RUN apt-get update && \
    apt-get install -y --no-install-recommends python3 python3-pip python3-venv && \
    python3 -m venv /opt/markitdown-venv && \
    /opt/markitdown-venv/bin/pip install --no-cache-dir "markitdown[all]" && \
    ln -s /opt/markitdown-venv/bin/markitdown /usr/local/bin/markitdown && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AgenticAIDemo-Api.csproj", "."]
RUN dotnet restore "./AgenticAIDemo-Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./AgenticAIDemo-Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AgenticAIDemo-Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgenticAIDemo-Api.dll"]