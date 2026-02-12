using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Infrastructure.Data;
using System.Reflection;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// Returns: 200 if service is running
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy" });
    }

    /// <summary>
    /// Detailed health check endpoint
    /// Returns: Health status of API, database, and version info
    /// </summary>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var timestamp = DateTime.UtcNow;
        var version = GetApplicationVersion();
        var services = new Dictionary<string, object>();

        try
        {
            // Check database connectivity
            var dbHealthy = await CheckDatabaseHealth();
            services["database"] = new { status = dbHealthy ? "healthy" : "unhealthy" };

            // Check API
            services["api"] = new { status = "healthy" };

            // Overall status
            var overallStatus = dbHealthy ? "healthy" : "degraded";
            return Ok(new { status = overallStatus, timestamp, version, services });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new { status = "unhealthy", error = ex.Message });
        }
    }

    /// <summary>
    /// Database-specific health check
    /// Returns: 200 if database is accessible
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        try
        {
            var isHealthy = await CheckDatabaseHealth();
            if (isHealthy)
            {
                return Ok(new { status = "healthy", message = "Database is accessible" });
            }
            return StatusCode(503, new { status = "unhealthy", message = "Database is not responding" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return StatusCode(503, new { status = "unhealthy", error = ex.Message });
        }
    }

    /// <summary>
    /// Readiness check - returns 200 when service is ready to accept traffic
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            // Check critical dependencies
            var dbReady = await CheckDatabaseHealth();

            if (dbReady)
            {
                return Ok(new { status = "ready" });
            }

            return StatusCode(503, new { status = "not_ready", reason = "Database not accessible" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { status = "not_ready", reason = ex.Message });
        }
    }

    /// <summary>
    /// Liveness check - returns 500 only if service should be restarted
    /// </summary>
    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        // This endpoint should return 200 as long as the process is running
        // It should only return 500 if the service is in a broken state
        return Ok(new { status = "alive" });
    }

    private async Task<bool> CheckDatabaseHealth()
    {
        try
        {
            // Try to execute a simple query
            var canConnect = await _context.Database.CanConnectAsync();
            return canConnect;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            return false;
        }
    }

    private static string GetApplicationVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
}
