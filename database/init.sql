-- PlanMorph Database Initialization Script
-- This script initializes the PostgreSQL database with required tables and initial data
-- Run this AFTER Entity Framework Core migrations are applied

-- ========================================
-- 1. Create Schemas
-- ========================================

-- Main schema for PlanMorph tables (created by EF Core)
-- Already handled by Entity Framework Core migrations

-- ========================================
-- 2. Create Extensions
-- ========================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";  -- For full-text search

-- ========================================
-- 3. Create Initial Data
-- ========================================

-- Insert default admin user (password: Admin@123456)
-- Note: This is a placeholder. The actual password hash should be generated using ASP.NET Identity
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
    "LockoutEnabled", "AccessFailedCount", "FirstName", "LastName", "Role", "IsActive"
) VALUES (
    'e0000000-0000-0000-0000-000000000001'::uuid,
    'admin@planmorph.software',
    'ADMIN@PLANMORPH.SOFTWARE',
    'admin@planmorph.software',
    'ADMIN@PLANMORPH.SOFTWARE',
    true,
    'AQAAAAIAAYagAAAAEK+...', -- This needs to be generated using ASP.NET Identity
    'PLACEHOLDER_SECURITY_STAMP',
    'PLACEHOLDER_CONCURRENCY_STAMP',
    '+254712345678',
    true,
    false,
    NULL,
    true,
    0,
    'Admin',
    'User',
    'Admin',
    true
) ON CONFLICT DO NOTHING;

-- Insert admin role
INSERT INTO "AspNetRoles" (
    "Id", "Name", "NormalizedName", "ConcurrencyStamp"
) VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'::uuid,
    'Admin',
    'ADMIN',
    'PLACEHOLDER_CONCURRENCY_STAMP'
) ON CONFLICT DO NOTHING;

-- Insert user roles for admin
INSERT INTO "AspNetUserRoles" (
    "UserId", "RoleId"
) VALUES (
    'e0000000-0000-0000-0000-000000000001'::uuid,
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'::uuid
) ON CONFLICT DO NOTHING;

-- ========================================
-- 4. Create Indexes
-- ========================================

-- User indexes
CREATE INDEX IF NOT EXISTS idx_users_email ON "AspNetUsers"("NormalizedEmail");
CREATE INDEX IF NOT EXISTS idx_users_username ON "AspNetUsers"("NormalizedUserName");
CREATE INDEX IF NOT EXISTS idx_users_role ON "AspNetUsers"("Role");

-- Design indexes
CREATE INDEX IF NOT EXISTS idx_designs_architect ON "Designs"("ArchitectId");
CREATE INDEX IF NOT EXISTS idx_designs_status ON "Designs"("Status");
CREATE INDEX IF NOT EXISTS idx_designs_created ON "Designs"("CreatedAt");

-- Order indexes
CREATE INDEX IF NOT EXISTS idx_orders_client ON "Orders"("ClientId");
CREATE INDEX IF NOT EXISTS idx_orders_design ON "Orders"("DesignId");
CREATE INDEX IF NOT EXISTS idx_orders_status ON "Orders"("Status");

-- Design file indexes
CREATE INDEX IF NOT EXISTS idx_design_files_design ON "DesignFiles"("DesignId");
CREATE INDEX IF NOT EXISTS idx_design_files_category ON "DesignFiles"("Category");

-- Construction contract indexes
CREATE INDEX IF NOT EXISTS idx_contracts_order ON "ConstructionContracts"("OrderId");
CREATE INDEX IF NOT EXISTS idx_contracts_contractor ON "ConstructionContracts"("ContractorId");
CREATE INDEX IF NOT EXISTS idx_contracts_status ON "ConstructionContracts"("Status");

-- ========================================
-- 5. Create Views (Optional)
-- ========================================

-- View for architect earnings
CREATE OR REPLACE VIEW v_architect_earnings AS
SELECT
    a."Id" as architect_id,
    a."FirstName",
    a."LastName",
    COUNT(DISTINCT d."Id") as total_designs,
    COUNT(DISTINCT o."Id") as total_sales,
    SUM(CASE WHEN o."Status" = 'Paid' THEN o."Amount" * 0.70 ELSE 0 END) as total_earnings
FROM "AspNetUsers" a
LEFT JOIN "Designs" d ON a."Id" = d."ArchitectId"
LEFT JOIN "Orders" o ON d."Id" = o."DesignId"
WHERE a."Role" = 'Architect' AND a."IsActive" = true
GROUP BY a."Id", a."FirstName", a."LastName";

-- ========================================
-- 6. Performance Settings
-- ========================================

-- Enable query parallelization
ALTER SYSTEM SET max_parallel_workers_per_gather = 4;
ALTER SYSTEM SET max_parallel_workers = 8;
ALTER SYSTEM SET max_worker_processes = 8;

-- Increase shared buffers (if server has 8GB+ RAM)
-- ALTER SYSTEM SET shared_buffers = '2GB';

-- Connection pooling recommendation: Use PgBouncer for production

-- ========================================
-- 7. Backup and Maintenance Settings
-- ========================================

-- Enable automatic vacuuming
ALTER SYSTEM SET autovacuum = on;
ALTER SYSTEM SET autovacuum_naptime = '1min';

-- Log slow queries (queries slower than 1 second)
ALTER SYSTEM SET log_min_duration_statement = 1000;

-- Reload configuration
SELECT pg_reload_conf();

-- ========================================
-- 8. Verify Installation
-- ========================================

-- Check database version
SELECT version();

-- Check extensions
SELECT * FROM pg_extension;

-- Check tables are created
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'public' ORDER BY table_name;

-- ========================================
-- 9. Cleanup Instructions
-- ========================================

-- To reset database (for development only):
-- DROP SCHEMA public CASCADE;
-- CREATE SCHEMA public;
-- Then re-run Entity Framework migrations

-- To fix permissions (if needed):
-- GRANT USAGE ON SCHEMA public TO planmorph;
-- GRANT CREATE ON SCHEMA public TO planmorph;
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO planmorph;

COMMIT;
