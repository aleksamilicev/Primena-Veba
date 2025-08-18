using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore;

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 1. Test osnovne konekcije
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (canConnect)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Konekcija sa bazom je uspešna!",
                        connectionString = _context.Database.GetConnectionString()
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Ne mogu da se povežem sa bazom" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Greška konekcije",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // 2. Test direktnog SQL upita
        [HttpGet("test-raw-sql")]
        public async Task<IActionResult> TestRawSql()
        {
            try
            {
                // Direktan SQL upit za testiranje
                var users = await _context.Database
                    .SqlQueryRaw<UserRawResult>("SELECT USER_ID, USERNAME, EMAIL FROM SKALARR.USERS")
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    users = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Greška sa raw SQL",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // 3. Test sa detaljnim logom
        [HttpGet("test-detailed")]
        public async Task<IActionResult> GetUsersDetailed()
        {
            try
            {
                _logger.LogInformation("Pokušavam da dohvatim korisnike...");

                // Proverava da li DbSet postoji
                if (_context.Users == null)
                {
                    return BadRequest("Users DbSet je null");
                }

                // Pokušava da dobije broj korisnika
                var count = await _context.Users.CountAsync();
                _logger.LogInformation("Broj korisnika u bazi: {Count}", count);

                // Pokušava da dobije korisnike
                var users = await _context.Users.ToListAsync();
                _logger.LogInformation("Dohvatil {UserCount} korisnika", users.Count);

                return Ok(new
                {
                    success = true,
                    totalCount = count,
                    retrievedCount = users.Count,
                    users = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detaljnu grešku pri dohvatanju korisnika");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Greška pri dohvatanju korisnika",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // 4. Test sa eksplicitnim nazivom tabele
        [HttpGet("test-explicit-table")]
        public async Task<IActionResult> TestExplicitTable()
        {
            try
            {
                // Eksplicitno specificiranje tabele
                var users = await _context.Users
                    .FromSqlRaw("SELECT * FROM SKALARR.USERS")
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    users = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Greška sa eksplicitnom tabelom",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // 5. Standardni endpoint
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju korisnika");
                return StatusCode(500, new
                {
                    message = "Greška pri dohvatanju korisnika",
                    error = ex.Message
                });
            }
        }

        // 6. Test informacija o bazi
        [HttpGet("database-info")]
        public IActionResult GetDatabaseInfo()
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                var providerName = _context.Database.ProviderName;

                return Ok(new
                {
                    connectionString = connectionString,
                    providerName = providerName,
                    databaseName = _context.Database.GetDbConnection().Database
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Greška pri dobijanju info o bazi",
                    error = ex.Message
                });
            }
        }
    }

    // Helper klasa za raw SQL rezultate
    public class UserRawResult
    {
        public int USER_ID { get; set; }
        public string USERNAME { get; set; } = string.Empty;
        public string EMAIL { get; set; } = string.Empty;
    }
}

// DODATNO: Dodajte u Program.cs za detaljno logovanje
/*
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Za EF Core SQL logovanje
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
});
*/