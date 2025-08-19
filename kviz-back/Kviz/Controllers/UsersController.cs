using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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
        private readonly IJwtService _jwtService;

        public UsersController(AppDbContext context, ILogger<UsersController> logger, IJwtService jwtService)
        {
            _context = context;
            _logger = logger;
            _jwtService = jwtService;
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

                // 8. Generiši JWT token odmah nakon registracije
                var token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    message = "Registracija je uspešna",
                    userId = user.User_Id,
                    username = user.Username,
                    token = token,
                    tokenType = "Bearer"
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

                // Generiši JWT token
                var token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    message = "Uspešna prijava",
                    userId = user.User_Id,
                    username = user.Username,
                    email = user.Email,
                    isAdmin = user.Is_Admin == 1,
                    token = token,
                    tokenType = "Bearer"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri prijavi korisnika");
                return StatusCode(500, new { message = "Greška pri prijavi" });
            }
        }


        // 5. Zaštićeni endpoint - potrebna autentifikacija
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "Korisnik nije pronađen" });

                return Ok(new
                {
                    userId = user.User_Id,
                    username = user.Username,
                    email = user.Email,
                    isAdmin = user.Is_Admin == 1,
                    profileImageUrl = user.Profile_Image_Url
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju profila");
                return StatusCode(500, new { message = "Greška pri dohvatanju profila" });
            }
        }


        // 6. Admin-only endpoint
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AdminGetUsers()
        {
            try
            {
                // Proveri da li je korisnik admin
                if (!IsCurrentUserAdmin())
                    return StatusCode(403, new
                    {
                        message = "Nemate dozvolu za pristup ovom resursu"
                    });

                var users = await _context.Users
                    .Select(u => new
                    {
                        u.User_Id,
                        u.Username,
                        u.Email,
                        u.Is_Admin,
                        u.Profile_Image_Url
                        // Ne vraćamo password hash iz bezbednosnih razloga
                    })
                    .ToListAsync();

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


        // 7. Endpoint za ažuriranje profila
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "Korisnik nije pronađen" });

                // Ažuriraj samo prosleđene vrednosti
                if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
                {
                    if (!IsValidEmail(dto.Email))
                        return BadRequest(new { message = "Neispravna email adresa" });

                    // Proveri da li email već postoji
                    var emailExists = await _context.Users
                        .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.User_Id != userId);

                    if (emailExists)
                        return BadRequest(new { message = "Email je već u upotrebi" });

                    user.Email = dto.Email.Trim().ToLower();
                }

                if (!string.IsNullOrWhiteSpace(dto.ProfileImageUrl))
                    user.Profile_Image_Url = dto.ProfileImageUrl.Trim();

                await _context.SaveChangesAsync();

                return Ok(new { message = "Profil je uspešno ažuriran" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju profila");
                return StatusCode(500, new { message = "Greška pri ažuriranju profila" });
            }
        }

        // 8. Logout endpoint (opciono - za blacklisting tokena)
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // U ovom slučaju, client samo treba da obriše token
            // Za kompletnu implementaciju, možete dodati token blacklisting
            return Ok(new { message = "Uspešno ste se odjavili" });
        }







        // Helper metode
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private bool IsCurrentUserAdmin()
        {
            var isAdminClaim = User.FindFirst("IsAdmin")?.Value;
            return isAdminClaim == "1";
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }


    }

}
