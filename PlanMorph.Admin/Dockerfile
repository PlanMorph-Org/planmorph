# Multi-stage build for Blazor Admin
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["PlanMorph.Admin/PlanMorph.Admin.csproj", "PlanMorph.Admin/"]
COPY ["PlanMorph.Application/PlanMorph.Application.csproj", "PlanMorph.Application/"]
COPY ["PlanMorph.Core/PlanMorph.Core.csproj", "PlanMorph.Core/"]
COPY ["PlanMorph.Infrastructure/PlanMorph.Infrastructure.csproj", "PlanMorph.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "PlanMorph.Admin/PlanMorph.Admin.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/PlanMorph.Admin"
RUN dotnet build "PlanMorph.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlanMorph.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PlanMorph.Admin.dll"]
