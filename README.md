# PlanMorph - Architectural Design Marketplace

A comprehensive platform connecting architects with clients worldwide, facilitating the sale of architectural designs. Construction services are currently available in Kenya only.

## System Architecture

### Backend (.NET 8)
- **PlanMorph.Api** - REST API for client marketplace
- **PlanMorph.Admin** - Blazor admin panel
- **PlanMorph.Core** - Domain entities and interfaces
- **PlanMorph.Application** - Business logic and services
- **PlanMorph.Infrastructure** - Database and external services

### Frontend (Next.js 14)
- **planmorph-client** - Client and architect web portals

### Database
- PostgreSQL (Neon serverless)

### File Storage
- DigitalOcean Spaces (S3-compatible)

## Features

### Client Features
- Browse and filter architectural designs
- Purchase design files (drawings, BOQ, renders)
- Request design modifications
- Optional construction services with contractor matching
- Order tracking and file downloads

### Architect Features
- Apply for verified architect status
- Upload designs with multiple file types
- Track sales and earnings (70% commission)
- Manage design portfolio
- Handle client modification requests

### Admin Features
- Approve architect applications
- Review and approve uploaded designs
- Manage users (clients, architects, contractors)
- Assign contractors to construction projects
- Monitor orders and contracts
- View platform analytics

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- PostgreSQL (or Neon account)
- DigitalOcean Spaces account (optional for file storage)

### Backend Setup

1. Clone the repository
```bash
git clone <repository-url>
cd planmorph
```

2. Update connection strings in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_NEON_CONNECTION_STRING"
  }
}
```

3. Run migrations:
```bash
dotnet ef database update --project PlanMorph.Infrastructure --startup-project PlanMorph.Api
```

4. Run the API:
```bash
cd PlanMorph.Api
dotnet run
```

5. Run the Admin panel (separate terminal):
```bash
cd PlanMorph.Admin
dotnet run
```

Default admin credentials:
- Email: admin@planmorph.com
- Password: Admin@123

### Frontend Setup

1. Navigate to client folder:
```bash
cd planmorph-client
```

2. Install dependencies:
```bash
npm install
```

3. Create `.env.local`:
```env
NEXT_PUBLIC_API_URL=https://localhost:7038/api
```

4. Run development server:
```bash
npm run dev
```

## Project Structure
```
PlanMorph/
├── PlanMorph.Api/              # REST API
├── PlanMorph.Admin/            # Admin panel (Blazor)
├── PlanMorph.Core/             # Domain models
├── PlanMorph.Application/      # Business logic
├── PlanMorph.Infrastructure/   # Data access
└── planmorph-client/           # Next.js frontend
    ├── src/
    │   ├── app/                # Next.js pages
    │   │   ├── architect/      # Architect portal
    │   │   ├── designs/        # Client marketplace
    │   │   └── my-orders/      # Client orders
    │   ├── components/         # Reusable components
    │   ├── lib/                # API client
    │   ├── store/              # State management
    │   └── types/              # TypeScript types
```

## API Endpoints

### Authentication
- POST `/api/auth/register` - Register new user
- POST `/api/auth/login` - Login

### Designs
- GET `/api/designs` - Get approved designs
- GET `/api/designs/{id}` - Get design details
- POST `/api/designs` - Create design (architect only)
- POST `/api/designs/filter` - Filter designs
- GET `/api/designs/my-designs` - Get architect's designs
- PUT `/api/designs/{id}/approve` - Approve design (admin)
- POST `/api/designs/{id}/files` - Upload design files

### Orders
- POST `/api/orders` - Create order
- GET `/api/orders/my-orders` - Get user orders
- GET `/api/orders` - Get all orders (admin)
- POST `/api/orders/{id}/mark-paid` - Mark order as paid

## Deployment

### Backend Deployment (DigitalOcean App Platform / Azure)
1. Build the application
2. Configure environment variables
3. Set up PostgreSQL database
4. Deploy API and Admin as separate services

### Frontend Deployment (Vercel)
1. Connect GitHub repository
2. Configure build settings
3. Set environment variables
4. Deploy

## Technology Stack

- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core
- **Frontend**: Next.js 14, React, TypeScript, Tailwind CSS
- **Admin**: Blazor Server
- **Database**: PostgreSQL
- **Storage**: DigitalOcean Spaces
- **Authentication**: JWT, ASP.NET Identity
- **State Management**: Zustand

## Contributing

This is a private project. Contact the repository owner for contribution guidelines.

## License

Proprietary - All rights reserved

## Contact

For support or inquiries, contact: support@planmorph.com
