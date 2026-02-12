# PlanMorph Deployment Script (Windows PowerShell)
# This script helps deploy the PlanMorph application stack on Windows

param(
    [Parameter(Position=0)]
    [string]$Command = "up",

    [Parameter(Position=1)]
    [string]$Option = ""
)

# Colors for output
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "   PlanMorph Deployment Script" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Error-Custom ".env file not found!"

    if (Test-Path ".env.example") {
        Write-Info "Creating .env from .env.example..."
        Copy-Item ".env.example" ".env"
        Write-Warn "Please edit .env file with your actual configuration values"
        exit 1
    } else {
        Write-Error-Custom ".env.example not found"
        exit 1
    }
}

# Check if Docker is installed
try {
    docker --version | Out-Null
} catch {
    Write-Error-Custom "Docker is not installed. Please install Docker Desktop first."
    exit 1
}

# Check if Docker Compose is available
$dockerComposeCmd = $null
try {
    docker compose version | Out-Null
    $dockerComposeCmd = "docker compose"
} catch {
    try {
        docker-compose --version | Out-Null
        $dockerComposeCmd = "docker-compose"
    } catch {
        Write-Error-Custom "Docker Compose is not available. Please install Docker Compose."
        exit 1
    }
}

Write-Info "Using Docker Compose command: $dockerComposeCmd"

switch ($Command) {
    "up" {
        Write-Info "Starting PlanMorph services..."

        if ($Option -eq "production") {
            Write-Info "Using profile: production"
            Invoke-Expression "$dockerComposeCmd --profile production up -d"
        } else {
            Invoke-Expression "$dockerComposeCmd up -d"
        }

        Write-Info "Waiting for services to be healthy..."
        Start-Sleep -Seconds 10

        Write-Info "Checking service status..."
        Invoke-Expression "$dockerComposeCmd ps"

        Write-Host ""
        Write-Info "Services are starting up!"
        Write-Info "Access points:"
        Write-Host "  - Frontend (Client): http://localhost:3000"
        Write-Host "  - API: http://localhost:7038"
        Write-Host "  - Admin Panel: http://localhost:8080"
        Write-Host "  - Database: localhost:5432"

        if ($Option -eq "production") {
            Write-Host "  - Nginx Proxy: http://localhost (port 80)"
            Write-Host "  - Nginx Proxy (HTTPS): https://localhost (port 443)"
        }
    }

    "down" {
        Write-Info "Stopping PlanMorph services..."
        Invoke-Expression "$dockerComposeCmd down"
        Write-Info "Services stopped successfully!"
    }

    "restart" {
        Write-Info "Restarting PlanMorph services..."
        Invoke-Expression "$dockerComposeCmd restart"
        Write-Info "Services restarted successfully!"
    }

    "logs" {
        if ($Option) {
            Write-Info "Showing logs for service: $Option"
            Invoke-Expression "$dockerComposeCmd logs -f $Option"
        } else {
            Write-Info "Showing logs for all services..."
            Invoke-Expression "$dockerComposeCmd logs -f"
        }
    }

    "build" {
        Write-Info "Building Docker images..."
        Invoke-Expression "$dockerComposeCmd build --no-cache"
        Write-Info "Build completed successfully!"
    }

    "rebuild" {
        Write-Info "Rebuilding and restarting services..."
        Invoke-Expression "$dockerComposeCmd down"
        Invoke-Expression "$dockerComposeCmd build --no-cache"
        Invoke-Expression "$dockerComposeCmd up -d"
        Write-Info "Rebuild completed successfully!"
    }

    "clean" {
        Write-Warn "This will remove all containers, volumes, and images!"
        $confirm = Read-Host "Are you sure? (yes/no)"

        if ($confirm -eq "yes") {
            Write-Info "Cleaning up..."
            Invoke-Expression "$dockerComposeCmd down -v --rmi all"
            Write-Info "Cleanup completed!"
        } else {
            Write-Info "Cleanup cancelled."
        }
    }

    "migrate" {
        Write-Info "Running database migrations..."
        Invoke-Expression "$dockerComposeCmd exec api dotnet ef database update"
        Write-Info "Migrations completed!"
    }

    "backup-db" {
        Write-Info "Backing up PostgreSQL database..."
        $backupFile = "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
        Invoke-Expression "$dockerComposeCmd exec -T postgres pg_dump -U planmorph planmorph" | Out-File -FilePath $backupFile -Encoding utf8
        Write-Info "Database backed up to: $backupFile"
    }

    "restore-db" {
        if (-not $Option) {
            Write-Error-Custom "Please provide backup file path"
            Write-Host "Usage: .\deploy.ps1 restore-db <backup-file>"
            exit 1
        }

        if (-not (Test-Path $Option)) {
            Write-Error-Custom "Backup file not found: $Option"
            exit 1
        }

        Write-Warn "This will overwrite the current database!"
        $confirm = Read-Host "Are you sure? (yes/no)"

        if ($confirm -eq "yes") {
            Write-Info "Restoring database from: $Option"
            Get-Content $Option | Invoke-Expression "$dockerComposeCmd exec -T postgres psql -U planmorph planmorph"
            Write-Info "Database restored successfully!"
        } else {
            Write-Info "Restore cancelled."
        }
    }

    "status" {
        Write-Info "Service Status:"
        Invoke-Expression "$dockerComposeCmd ps"
    }

    "help" {
        Write-Host "PlanMorph Deployment Script" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Usage: .\deploy.ps1 [command] [options]"
        Write-Host ""
        Write-Host "Commands:" -ForegroundColor Yellow
        Write-Host "  up [profile]       Start all services (optional: production profile)"
        Write-Host "  down               Stop all services"
        Write-Host "  restart            Restart all services"
        Write-Host "  logs [service]     Show logs (optional: specific service)"
        Write-Host "  build              Build Docker images"
        Write-Host "  rebuild            Rebuild images and restart services"
        Write-Host "  clean              Remove all containers, volumes, and images"
        Write-Host "  migrate            Run database migrations"
        Write-Host "  backup-db          Backup PostgreSQL database"
        Write-Host "  restore-db <file>  Restore database from backup"
        Write-Host "  status             Show service status"
        Write-Host "  help               Show this help message"
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Yellow
        Write-Host "  .\deploy.ps1 up                    # Start services"
        Write-Host "  .\deploy.ps1 up production         # Start with Nginx proxy"
        Write-Host "  .\deploy.ps1 logs api              # Show API logs"
        Write-Host "  .\deploy.ps1 backup-db             # Backup database"
        Write-Host "  .\deploy.ps1 restore-db backup.sql # Restore database"
    }

    default {
        Write-Error-Custom "Unknown command: $Command"
        Write-Info "Run '.\deploy.ps1 help' for usage information"
        exit 1
    }
}

Write-Host ""
Write-Info "Done!"
