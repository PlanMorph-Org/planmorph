#!/bin/bash

# PlanMorph Deployment Script
# This script helps deploy the PlanMorph application stack

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}   PlanMorph Deployment Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Function to print colored messages
info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if .env file exists
if [ ! -f .env ]; then
    error ".env file not found!"
    info "Creating .env from .env.example..."

    if [ -f .env.example ]; then
        cp .env.example .env
        warn "Please edit .env file with your actual configuration values"
        exit 1
    else
        error ".env.example not found"
        exit 1
    fi
fi

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    error "Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Determine docker compose command
if docker compose version &> /dev/null 2>&1; then
    DOCKER_COMPOSE="docker compose"
else
    DOCKER_COMPOSE="docker-compose"
fi

info "Using Docker Compose command: $DOCKER_COMPOSE"

# Parse command line arguments
COMMAND=${1:-"up"}
PROFILE=${2:-""}

case $COMMAND in
    up)
        info "Starting PlanMorph services..."
        if [ -n "$PROFILE" ]; then
            info "Using profile: $PROFILE"
            $DOCKER_COMPOSE --profile $PROFILE up -d
        else
            $DOCKER_COMPOSE up -d
        fi

        info "Waiting for services to be healthy..."
        sleep 10

        info "Checking service status..."
        $DOCKER_COMPOSE ps

        echo ""
        info "Services are starting up!"
        info "Access points:"
        echo "  - Frontend (Client): http://localhost:3000"
        echo "  - API: http://localhost:7038"
        echo "  - Admin Panel: http://localhost:8080"
        echo "  - Database: localhost:5432"

        if [ "$PROFILE" = "production" ]; then
            echo "  - Nginx Proxy: http://localhost (port 80)"
            echo "  - Nginx Proxy (HTTPS): https://localhost (port 443)"
        fi
        ;;

    down)
        info "Stopping PlanMorph services..."
        $DOCKER_COMPOSE down
        info "Services stopped successfully!"
        ;;

    restart)
        info "Restarting PlanMorph services..."
        $DOCKER_COMPOSE restart
        info "Services restarted successfully!"
        ;;

    logs)
        SERVICE=${2:-""}
        if [ -n "$SERVICE" ]; then
            info "Showing logs for service: $SERVICE"
            $DOCKER_COMPOSE logs -f $SERVICE
        else
            info "Showing logs for all services..."
            $DOCKER_COMPOSE logs -f
        fi
        ;;

    build)
        info "Building Docker images..."
        $DOCKER_COMPOSE build --no-cache
        info "Build completed successfully!"
        ;;

    rebuild)
        info "Rebuilding and restarting services..."
        $DOCKER_COMPOSE down
        $DOCKER_COMPOSE build --no-cache
        $DOCKER_COMPOSE up -d
        info "Rebuild completed successfully!"
        ;;

    clean)
        warn "This will remove all containers, volumes, and images!"
        read -p "Are you sure? (yes/no): " confirm
        if [ "$confirm" = "yes" ]; then
            info "Cleaning up..."
            $DOCKER_COMPOSE down -v --rmi all
            info "Cleanup completed!"
        else
            info "Cleanup cancelled."
        fi
        ;;

    migrate)
        info "Running database migrations..."
        $DOCKER_COMPOSE exec api dotnet ef database update
        info "Migrations completed!"
        ;;

    backup-db)
        info "Backing up PostgreSQL database..."
        BACKUP_FILE="backup_$(date +%Y%m%d_%H%M%S).sql"
        $DOCKER_COMPOSE exec -T postgres pg_dump -U planmorph planmorph > $BACKUP_FILE
        info "Database backed up to: $BACKUP_FILE"
        ;;

    restore-db)
        BACKUP_FILE=${2}
        if [ -z "$BACKUP_FILE" ]; then
            error "Please provide backup file path"
            echo "Usage: ./deploy.sh restore-db <backup-file>"
            exit 1
        fi

        if [ ! -f "$BACKUP_FILE" ]; then
            error "Backup file not found: $BACKUP_FILE"
            exit 1
        fi

        warn "This will overwrite the current database!"
        read -p "Are you sure? (yes/no): " confirm
        if [ "$confirm" = "yes" ]; then
            info "Restoring database from: $BACKUP_FILE"
            cat $BACKUP_FILE | $DOCKER_COMPOSE exec -T postgres psql -U planmorph planmorph
            info "Database restored successfully!"
        else
            info "Restore cancelled."
        fi
        ;;

    status)
        info "Service Status:"
        $DOCKER_COMPOSE ps
        echo ""
        info "Container Health:"
        $DOCKER_COMPOSE ps --format json | jq -r '.[] | "\(.Name): \(.State) - \(.Health)"' 2>/dev/null || $DOCKER_COMPOSE ps
        ;;

    help)
        echo "PlanMorph Deployment Script"
        echo ""
        echo "Usage: ./deploy.sh [command] [options]"
        echo ""
        echo "Commands:"
        echo "  up [profile]       Start all services (optional: production profile)"
        echo "  down               Stop all services"
        echo "  restart            Restart all services"
        echo "  logs [service]     Show logs (optional: specific service)"
        echo "  build              Build Docker images"
        echo "  rebuild            Rebuild images and restart services"
        echo "  clean              Remove all containers, volumes, and images"
        echo "  migrate            Run database migrations"
        echo "  backup-db          Backup PostgreSQL database"
        echo "  restore-db <file>  Restore database from backup"
        echo "  status             Show service status"
        echo "  help               Show this help message"
        echo ""
        echo "Examples:"
        echo "  ./deploy.sh up                    # Start services"
        echo "  ./deploy.sh up production         # Start with Nginx proxy"
        echo "  ./deploy.sh logs api              # Show API logs"
        echo "  ./deploy.sh backup-db             # Backup database"
        echo "  ./deploy.sh restore-db backup.sql # Restore database"
        ;;

    *)
        error "Unknown command: $COMMAND"
        info "Run './deploy.sh help' for usage information"
        exit 1
        ;;
esac

echo ""
info "Done!"
