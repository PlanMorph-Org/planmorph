# Multi-stage build for .NET API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["PlanMorph.Api/PlanMorph.Api.csproj", "PlanMorph.Api/"]
COPY ["PlanMorph.Application/PlanMorph.Application.csproj", "PlanMorph.Application/"]
COPY ["PlanMorph.Core/PlanMorph.Core.csproj", "PlanMorph.Core/"]
COPY ["PlanMorph.Infrastructure/PlanMorph.Infrastructure.csproj", "PlanMorph.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "PlanMorph.Api/PlanMorph.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/PlanMorph.Api"
RUN dotnet build "PlanMorph.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlanMorph.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PlanMorph.Api.dll"]
