using Kviz.DTOs;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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


        // 2. Endpoint za registraciju korisnika
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            try
            {
                // 1. Validacija modela preko atributa u DTO-u
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Neispravni podaci", errors = errors });
                }

                // 2. Validacija korisničkog imena
                if (string.IsNullOrWhiteSpace(dto.Username) || dto.Username.Length < 3)
                    return BadRequest(new { message = "Korisničko ime mora imati najmanje 3 karaktera" });

                if (dto.Username.Length > 12)
                    return BadRequest(new { message = "Korisničko ime ne sme biti duže od 12 karaktera" });


                // 3. Validacija lozinke
                if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                    return BadRequest(new { message = "Lozinka mora imati najmanje 6 karaktera" });

                if (dto.Password.Length > 12)
                    return BadRequest(new { message = "Lozinka ne sme biti duža od 12 karaktera" });


                // 4. Validacija email-a (atribut [EmailAddress] već radi, ali možemo dodati još proveru)
                try
                {
                    var addr = new System.Net.Mail.MailAddress(dto.Email);
                    if (addr.Address != dto.Email.Trim())
                        return BadRequest(new { message = "Neispravna email adresa" });
                }
                catch
                {
                    return BadRequest(new { message = "Neispravna email adresa" });
                }
                if(dto.Email.Length > 30)
                    return BadRequest(new { message = "Email ne sme biti duži od 30 karaktera" });

                // 5. Provera jedinstvenosti username/email
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower() ||
                                            u.Email.ToLower() == dto.Email.ToLower());

                if (existingUser != null)
                {
                    if (existingUser.Username.ToLower() == dto.Username.ToLower())
                        return BadRequest(new { message = "Korisničko ime je već zauzeto" });

                    return BadRequest(new { message = "Email je već registrovan" });
                }



                // 6. Hash lozinke sa BCrypt
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);


                // 7. Kreiranje novog korisnika
                var user = new User
                {
                    Username = dto.Username.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    Password_Hash = hashedPassword,
                    Is_Admin = 0 // Podrazumevano nije admin, 0 = korisnik, 1 = admin
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Uspešno registrovan korisnik: {Username}", user.Username);

                return Ok(new
                {
                    message = "Registracija je uspešna",
                    userId = user.User_Id,
                    username = user.Username
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database greška pri registraciji korisnika {Username}", dto?.Username);
                return StatusCode(500, new
                {
                    message = "Greška u bazi podataka pri registraciji",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Opšta greška pri registraciji korisnika {Username}", dto?.Username);
                return StatusCode(500, new
                {
                    message = "Neočekivana greška pri registraciji",
                    error = ex.Message
                });
            }
        }


        // 3. Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Neispravni podaci" });

                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    (dto.Username != null && u.Username.ToLower() == dto.Username.ToLower()) ||
                    (dto.Email != null && u.Email.ToLower() == dto.Email.ToLower()));


                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password_Hash))
                {
                    return Unauthorized(new { message = "Neispravno korisničko ime ili lozinka" });
                }

                return Ok(new
                {
                    message = "Uspešna prijava",
                    userId = user.User_Id,
                    username = user.Username,
                    isAdmin = user.Is_Admin == 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri prijavi korisnika");
                return StatusCode(500, new { message = "Greška pri prijavi" });
            }
        }





        // Standardni endpoint /api/users za dohvatanje svih korisnika
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