using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OrderAPI.Classes
{
    // ==================== JWT Token Helper ====================
    public class JwtToken
    {
        public static readonly byte[] _key = Encoding.UTF8.GetBytes("dsdgkdfgjd;sj;sedhjlgbalkfqwh134141221!!!!!!!!!");

        public static string Generate(User user)
        {
            JwtSecurityTokenHandler handler = new();
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public static int? GetUserIdFromToken(string token)
        {
            try
            {
                JwtSecurityTokenHandler handler = new();
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                JwtSecurityToken jwtSecurityToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtSecurityToken.Claims.FirstOrDefault(t => t.Type == "UserId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    return userId;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    // ==================== Response Models ====================
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static ApiResponse Ok(object data = null, string message = "Success")
        {
            return new ApiResponse { Success = true, Message = message, Data = data };
        }

        public static ApiResponse Error(string message, object data = null)
        {
            return new ApiResponse { Success = false, Message = message, Data = data };
        }
    }

    // ==================== Database Context ====================
    public class DatabaseManager : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<CandidateComment> CandidateComments { get; set; }
        public DbSet<CandidateStatus> CandidateStatuses { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<RemoveStatus> RemoveStatuses { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<VacancyFile> VacancyFiles { get; set; }

        public DatabaseManager() =>
            Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;uid=root;pwd=1234;database=apinew",
                new MySqlServerVersion(new Version(8, 0, 11)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Login).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Login).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Firstname).HasMaxLength(100);
                entity.Property(e => e.Lastname).HasMaxLength(100);
            });

            modelBuilder.Entity<Candidate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.IdStatus);
                entity.HasOne(e => e.City).WithMany().HasForeignKey(e => e.IdCity);
                entity.HasOne(e => e.Vacancy).WithMany().HasForeignKey(e => e.IdVacancy);
                entity.HasOne(e => e.HR).WithMany().HasForeignKey(e => e.IdHR);
            });

            modelBuilder.Entity<CandidateComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Candidate).WithMany().HasForeignKey(e => e.IdCandidate);
            });

            modelBuilder.Entity<CandidateStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.IdDepartment);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<RemoveStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Vacancy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SalaryRange).HasMaxLength(100);
                entity.Property(e => e.Experience).HasMaxLength(100);
                entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.IdDepartment);
                entity.HasOne(e => e.Position).WithMany().HasForeignKey(e => e.IdPosition);
                entity.HasOne(e => e.City).WithMany().HasForeignKey(e => e.IdCity);
                entity.HasOne(e => e.Specialist).WithMany().HasForeignKey(e => e.IdSpecialist);
            });

            modelBuilder.Entity<VacancyFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).HasMaxLength(500);
                entity.HasOne(e => e.Vacancy).WithMany().HasForeignKey(e => e.IdVacancy);
            });
        }
    }

    // ==================== Models ====================
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public byte[] Photo { get; set; }
        public string Token { get; set; }
    }

    public class Candidate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? IdDepartment { get; set; }
        public int? IdStatus { get; set; }
        public int? IdCity { get; set; }
        public int? IdVacancy { get; set; }
        public int? IdHR { get; set; }

        [ForeignKey("IdStatus")]
        public virtual CandidateStatus Status { get; set; }

        [ForeignKey("IdCity")]
        public virtual City City { get; set; }

        [ForeignKey("IdVacancy")]
        public virtual Vacancy Vacancy { get; set; }

        [ForeignKey("IdHR")]
        public virtual Employee HR { get; set; }
    }

    public class CandidateComment
    {
        public int Id { get; set; }
        public int IdCandidate { get; set; }
        public string Comment { get; set; }

        [ForeignKey("IdCandidate")]
        public virtual Candidate Candidate { get; set; }
    }

    public class CandidateStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? IdDepartment { get; set; }

        [ForeignKey("IdDepartment")]
        public virtual Department Department { get; set; }
    }

    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RemoveStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Vacancy
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? IdDepartment { get; set; }
        public DateTime? OpeningDate { get; set; }
        public int? IdPosition { get; set; }
        public string SalaryRange { get; set; }
        public int? RequiredQuantity { get; set; }
        public string Experience { get; set; }
        public int? IdCity { get; set; }
        public bool? IsPublic { get; set; }
        public string Description { get; set; }
        public string TeamOfEmployment { get; set; }
        public string Requirements { get; set; }
        public string TechnicalQuestions { get; set; }
        public string TestTask { get; set; }
        public int? IdSpecialist { get; set; }

        [ForeignKey("IdDepartment")]
        public virtual Department Department { get; set; }

        [ForeignKey("IdPosition")]
        public virtual Position Position { get; set; }

        [ForeignKey("IdCity")]
        public virtual City City { get; set; }

        [ForeignKey("IdSpecialist")]
        public virtual Employee Specialist { get; set; }
    }

    public class VacancyFile
    {
        public int Id { get; set; }
        public int IdVacancy { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }

        [ForeignKey("IdVacancy")]
        public virtual Vacancy Vacancy { get; set; }
    }

    // ==================== DTOs with Validation ====================
    public class AuthRequest
    {
        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 100 символов")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [StringLength(255, ErrorMessage = "Email не может быть длиннее 255 символов")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Имя должно быть от 1 до 100 символов")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Фамилия должна быть от 1 до 100 символов")]
        public string Lastname { get; set; }

        public IFormFile Photo { get; set; }
    }

    public class CandidateUpdateDTO
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Имя должно быть от 1 до 255 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [StringLength(255, ErrorMessage = "Email не может быть длиннее 255 символов")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Отдел обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID отдела")]
        public int IdDepartment { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [StringLength(50, ErrorMessage = "Телефон не может быть длиннее 50 символов")]
        public string Phone { get; set; }
    }

    public class CandidateStatusUpdateDTO
    {
        [Required(ErrorMessage = "ID кандидата обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID кандидата")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID статуса обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID статуса")]
        public int IdStatus { get; set; }
    }

    public class CandidateDeleteDTO
    {
        [Required(ErrorMessage = "ID статуса удаления обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID статуса удаления")]
        public int IdStatusRemoved { get; set; }

        [StringLength(1000, ErrorMessage = "Примечание не может быть длиннее 1000 символов")]
        public string Note { get; set; }
    }

    public class CommentDTO
    {
        [Required(ErrorMessage = "Комментарий обязателен")]
        [StringLength(2000, ErrorMessage = "Комментарий не может быть длиннее 2000 символов")]
        public string Comment { get; set; }
    }

    public class VacancyDTO
    {
        [Required(ErrorMessage = "Название вакансии обязательно")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 500 символов")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Отдел обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID отдела")]
        public int IdDepartment { get; set; }

        [Required(ErrorMessage = "Дата открытия обязательна")]
        public DateTime OpeningDate { get; set; }

        [Required(ErrorMessage = "Должность обязательна")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID должности")]
        public int IdPosition { get; set; }

        [StringLength(100, ErrorMessage = "Зарплатная вилка не может быть длиннее 100 символов")]
        public string SalaryRange { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int RequiredQuantity { get; set; }

        [StringLength(100, ErrorMessage = "Опыт не может быть длиннее 100 символов")]
        public string Experience { get; set; }

        [Required(ErrorMessage = "Город обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID города")]
        public int IdCity { get; set; }

        public bool IsPublic { get; set; }

        [StringLength(5000, ErrorMessage = "Описание не может быть длиннее 5000 символов")]
        public string Description { get; set; }

        [StringLength(1000, ErrorMessage = "Команда не может быть длиннее 1000 символов")]
        public string TeamOfEmployment { get; set; }

        [StringLength(5000, ErrorMessage = "Требования не могут быть длиннее 5000 символов")]
        public string Requirements { get; set; }

        [StringLength(5000, ErrorMessage = "Технические вопросы не могут быть длиннее 5000 символов")]
        public string TechnicalQuestions { get; set; }

        [StringLength(5000, ErrorMessage = "Тестовое задание не может быть длиннее 5000 символов")]
        public string TestTask { get; set; }

        [Required(ErrorMessage = "Специалист обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID специалиста")]
        public int IdSpecialist { get; set; }
    }

    // ==================== Base Controller with Helpers ====================
    public class BaseController : ControllerBase
    {
        protected User GetUserByToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            using var db = new DatabaseManager();
            return db.Users.FirstOrDefault(u => u.Token == token);
        }

        protected IActionResult ValidateAndExecute(string token, Func<DatabaseManager, IActionResult> action)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(ApiResponse.Error("Токен не предоставлен"));

                using var db = new DatabaseManager();
                var user = db.Users.FirstOrDefault(u => u.Token == token);

                if (user == null)
                    return Unauthorized(ApiResponse.Error("Недействительный токен"));

                return action(db);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse.Error($"Ошибка базы данных: {ex.InnerException?.Message ?? ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }

        protected IActionResult ValidateModelAndExecute<T>(T model, string token, Func<DatabaseManager, T, IActionResult> action) where T : class
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.Error("Ошибка валидации", errors));
            }

            return ValidateAndExecute(token, db => action(db, model));
        }
    }

    // ==================== Controllers ====================

    [ApiController]
    public class UserController : BaseController
    {
        [HttpPost("auth")]
        public IActionResult Auth([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.Error("Ошибка валидации", errors));
            }

            try
            {
                using var db = new DatabaseManager();
                var user = db.Users.FirstOrDefault(u => u.Login == request.Login && u.Password == request.Password);

                if (user == null)
                    return Unauthorized(ApiResponse.Error("Неверный логин или пароль"));

                user.Token = JwtToken.Generate(user);
                db.SaveChanges();

                return Ok(ApiResponse.Ok(new { token = user.Token }, "Авторизация успешна"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Ошибка авторизации: {ex.Message}"));
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.Error("Ошибка валидации", errors));
            }

            try
            {
                using var db = new DatabaseManager();

                // Проверка уникальности логина
                if (db.Users.Any(u => u.Login == request.Email))
                    return Conflict(ApiResponse.Error("Пользователь с таким логином уже существует"));

                // Проверка уникальности email
                if (db.Users.Any(u => u.Email == request.Email))
                    return Conflict(ApiResponse.Error("Пользователь с таким email уже существует"));

                // Проверка размера фото (максимум 5MB)
                if (request.Photo != null && request.Photo.Length > 5 * 1024 * 1024)
                    return BadRequest(ApiResponse.Error("Размер фото не должен превышать 5MB"));

                // Проверка формата фото
                if (request.Photo != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(request.Photo.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                        return BadRequest(ApiResponse.Error("Недопустимый формат фото. Разрешены: jpg, jpeg, png, gif"));
                }

                var user = new User
                {
                    Login = request.Email,
                    Email = request.Email,
                    Password = request.Password,
                    Firstname = request.Firstname,
                    Lastname = request.Lastname
                };

                if (request.Photo != null && request.Photo.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await request.Photo.CopyToAsync(ms);
                    user.Photo = ms.ToArray();
                }

                user.Token = JwtToken.Generate(user);
                db.Users.Add(user);
                db.SaveChanges();

                return Ok(ApiResponse.Ok(new { token = user.Token }, "Регистрация успешна"));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ApiResponse.Error($"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Ошибка регистрации: {ex.Message}"));
            }
        }

        [HttpGet("auth/profileByToken")]
        public IActionResult GetProfileByToken([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var user = db.Users.FirstOrDefault(u => u.Token == token);
                return Ok(ApiResponse.Ok(new
                {
                    user.Id,
                    user.Login,
                    user.Email,
                    user.Firstname,
                    user.Lastname
                }));
            });
        }

        [HttpGet("logout")]
        public IActionResult Logout([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var user = db.Users.FirstOrDefault(u => u.Token == token);
                user.Token = null;
                db.SaveChanges();
                return Ok(ApiResponse.Ok(null, "Выход выполнен успешно"));
            });
        }

        [HttpGet("userAvatar/{id}")]
        public IActionResult GetUserAvatar([FromHeader] string token, int id)
        {
            return ValidateAndExecute(token, db =>
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID пользователя"));

                var targetUser = db.Users.FirstOrDefault(u => u.Id == id);
                if (targetUser == null)
                    return NotFound(ApiResponse.Error("Пользователь не найден"));

                if (targetUser.Photo != null && targetUser.Photo.Length > 0)
                    return File(targetUser.Photo, "image/jpeg");

                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "URL", "img", $"{id}.jpg");
                if (System.IO.File.Exists(imagePath))
                    return File(System.IO.File.ReadAllBytes(imagePath), "image/jpeg");

                return NotFound(ApiResponse.Error("Аватар не найден"));
            });
        }

        [HttpPut("userAvatar/{id}")]
        public async Task<IActionResult> UpdateUserAvatar([FromHeader] string token, int id, IFormFile file)
        {
            if (string.IsNullOrEmpty(token))
                return Unauthorized(ApiResponse.Error("Токен не предоставлен"));

            if (id <= 0)
                return BadRequest(ApiResponse.Error("Некорректный ID пользователя"));

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.Error("Файл не предоставлен"));

            // Проверка размера файла
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(ApiResponse.Error("Размер фото не должен превышать 5MB"));

            // Проверка формата файла
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse.Error("Недопустимый формат фото. Разрешены: jpg, jpeg, png, gif"));

            try
            {
                using var db = new DatabaseManager();
                var user = db.Users.FirstOrDefault(u => u.Token == token);

                if (user == null)
                    return Unauthorized(ApiResponse.Error("Недействительный токен"));

                if (user.Id != id)
                    return Forbid();

                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "URL", "img");
                Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, $"{id}.jpg");
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var dbUser = db.Users.First(u => u.Id == id);
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                dbUser.Photo = ms.ToArray();
                db.SaveChanges();

                return Ok(ApiResponse.Ok(null, "Аватар обновлен успешно"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Ошибка обновления аватара: {ex.Message}"));
            }
        }
    }

    [ApiController]
    [Route("candidates")]
    public class CandidatesController : BaseController
    {
        [HttpGet("vacancies")]
        public IActionResult GetCandidatesVacancies([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var candidates = db.Candidates
                    .Include(c => c.Status)
                    .Include(c => c.City)
                    .Include(c => c.Vacancy)
                    .Include(c => c.HR)
                    .ToList();

                return Ok(ApiResponse.Ok(candidates));
            });
        }

        [HttpGet("card/{cardId}")]
        public IActionResult GetCandidateCard([FromHeader] string token, int cardId)
        {
            return ValidateAndExecute(token, db =>
            {
                if (cardId <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID кандидата"));

                var candidate = db.Candidates
                    .Include(c => c.Status)
                    .Include(c => c.City)
                    .Include(c => c.Vacancy)
                    .Include(c => c.HR)
                    .FirstOrDefault(c => c.Id == cardId);

                if (candidate == null)
                    return NotFound(ApiResponse.Error("Кандидат не найден"));

                return Ok(ApiResponse.Ok(candidate));
            });
        }

        [HttpGet("card/comment/{cardId}")]
        public IActionResult GetCandidateComment([FromHeader] string token, int cardId)
        {
            return ValidateAndExecute(token, db =>
            {
                if (cardId <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID кандидата"));

                if (!db.Candidates.Any(c => c.Id == cardId))
                    return NotFound(ApiResponse.Error("Кандидат не найден"));

                var comment = db.CandidateComments.FirstOrDefault(c => c.IdCandidate == cardId);
                return Ok(ApiResponse.Ok(comment?.Comment ?? ""));
            });
        }

        [HttpPut("card/comment/{cardId}")]
        public IActionResult UpdateCandidateComment([FromHeader] string token, int cardId, [FromBody] CommentDTO commentDTO)
        {
            return ValidateModelAndExecute(commentDTO, token, (db, model) =>
            {
                if (cardId <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID кандидата"));

                if (!db.Candidates.Any(c => c.Id == cardId))
                    return NotFound(ApiResponse.Error("Кандидат не найден"));

                var comment = db.CandidateComments.FirstOrDefault(c => c.IdCandidate == cardId);

                if (comment == null)
                {
                    comment = new CandidateComment { IdCandidate = cardId, Comment = model.Comment };
                    db.CandidateComments.Add(comment);
                }
                else
                {
                    comment.Comment = model.Comment;
                }

                db.SaveChanges();
                return Ok(ApiResponse.Ok(null, "Комментарий обновлен успешно"));
            });
        }

        [HttpPut("card/hr/{cardId}")]
        public IActionResult UpdateCandidateHR([FromHeader] string token, int cardId, [FromBody] CandidateUpdateDTO updateDTO)
        {
            return ValidateModelAndExecute(updateDTO, token, (db, model) =>
            {
                if (cardId <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID кандидата"));

                var candidate = db.Candidates.FirstOrDefault(c => c.Id == cardId);
                if (candidate == null)
                    return NotFound(ApiResponse.Error("Кандидат не найден"));

                // Проверка существования отдела
                if (!db.Departments.Any(d => d.Id == model.IdDepartment))
                    return BadRequest(ApiResponse.Error("Указанный отдел не существует"));

                // Проверка уникальности email (если меняется)
                if (candidate.Email != model.Email && db.Candidates.Any(c => c.Email == model.Email && c.Id != cardId))
                    return Conflict(ApiResponse.Error("Кандидат с таким email уже существует"));

                candidate.Name = model.Name;
                candidate.Email = model.Email;
                candidate.IdDepartment = model.IdDepartment;
                candidate.Phone = model.Phone;

                db.SaveChanges();
                return Ok(ApiResponse.Ok(candidate, "Карточка кандидата обновлена успешно"));
            });
        }

        [HttpPut("status")]
        public IActionResult UpdateCandidateStatus([FromHeader] string token, [FromBody] CandidateStatusUpdateDTO statusDTO)
        {
            return ValidateModelAndExecute(statusDTO, token, (db, model) =>
            {
                var candidate = db.Candidates.FirstOrDefault(c => c.Id == model.Id);
                if (candidate == null)
                    return NotFound(ApiResponse.Error("Кандидат не найден"));

                // Проверка существования статуса
                if (!db.CandidateStatuses.Any(s => s.Id == model.IdStatus))
                    return BadRequest(ApiResponse.Error("Указанный статус не существует"));

                candidate.IdStatus = model.IdStatus;
                db.SaveChanges();
                return Ok(ApiResponse.Ok(null, "Статус обновлен успешно"));
            });
        }

        [HttpDelete("card/{cardId}")]
        public IActionResult DeleteCandidateCard([FromHeader] string token, int cardId, [FromBody] CandidateDeleteDTO deleteDTO)
        {
            return ValidateModelAndExecute(deleteDTO, token, (db, model) =>
            {
                if (cardId <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID кандидата"));

                var candidate = db.Candidates.FirstOrDefault(c => c.Id == cardId);
                if (candidate == null)
                    return NotFound(ApiResponse.Error("Кандидат не найден"));

                // Проверка существования статуса удаления
                if (!db.RemoveStatuses.Any(s => s.Id == model.IdStatusRemoved))
                    return BadRequest(ApiResponse.Error("Указанный статус удаления не существует"));

                candidate.IdStatus = model.IdStatusRemoved;

                if (!string.IsNullOrEmpty(model.Note))
                {
                    db.CandidateComments.Add(new CandidateComment
                    {
                        IdCandidate = cardId,
                        Comment = $"Удален: {model.Note}"
                    });
                }

                db.SaveChanges();
                return Ok(ApiResponse.Ok(null, "Карточка кандидата удалена успешно"));
            });
        }
    }

    [ApiController]
    [Route("dictionary")]
    public class DictionaryController : BaseController
    {
        [HttpGet("status")]
        public IActionResult GetStatuses([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var statuses = db.CandidateStatuses.ToList();
                return Ok(ApiResponse.Ok(statuses));
            });
        }

        [HttpGet("city")]
        public IActionResult GetCities([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var cities = db.Cities.ToList();
                return Ok(ApiResponse.Ok(cities));
            });
        }
    }

    [ApiController]
    [Route("employee")]
    public class EmployeeController : BaseController
    {
        [HttpPost("applicantListBy")]
        public IActionResult GetApplicantListBy([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var candidates = db.Candidates
                    .Include(c => c.Status)
                    .Include(c => c.City)
                    .Include(c => c.Vacancy)
                    .ToList();

                return Ok(ApiResponse.Ok(candidates));
            });
        }

        [HttpGet("shortStaff")]
        public IActionResult GetShortStaff([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var employees = db.Employees
                    .Select(e => new { e.Id, e.Name, e.Email, e.Phone })
                    .ToList();

                return Ok(ApiResponse.Ok(employees));
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployeeById([FromHeader] string token, int id)
        {
            return ValidateAndExecute(token, db =>
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID сотрудника"));

                var employee = db.Employees
                    .Include(e => e.Department)
                    .FirstOrDefault(e => e.Id == id);

                if (employee == null)
                    return NotFound(ApiResponse.Error("Сотрудник не найден"));

                return Ok(ApiResponse.Ok(employee));
            });
        }
    }

    [ApiController]
    [Route("hiring")]
    public class HiringController : BaseController
    {
        [HttpGet("departments")]
        public IActionResult GetDepartments([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var departments = db.Departments.ToList();
                return Ok(ApiResponse.Ok(departments));
            });
        }

        [HttpGet("positions")]
        public IActionResult GetPositions([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var positions = db.Positions.ToList();
                return Ok(ApiResponse.Ok(positions));
            });
        }

        [HttpGet("remove_status")]
        public IActionResult GetRemoveStatuses([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var statuses = db.RemoveStatuses.ToList();
                return Ok(ApiResponse.Ok(statuses));
            });
        }

        [HttpGet("vacancies")]
        public IActionResult GetHRVacancies([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var vacancies = db.Vacancies
                    .Include(v => v.Department)
                    .Include(v => v.Position)
                    .Include(v => v.City)
                    .Include(v => v.Specialist)
                    .ToList();

                return Ok(ApiResponse.Ok(vacancies));
            });
        }
    }

    [ApiController]
    [Route("vacancy")]
    public class VacancyController : BaseController
    {
        [HttpGet("short")]
        public IActionResult GetShortVacancies([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var vacancies = db.Vacancies
                    .Select(v => new { v.Id, v.Title, v.IdDepartment, v.IdPosition })
                    .ToList();

                return Ok(ApiResponse.Ok(vacancies));
            });
        }

        [HttpGet]
        public IActionResult GetVacancies([FromHeader] string token)
        {
            return ValidateAndExecute(token, db =>
            {
                var vacancies = db.Vacancies
                    .Include(v => v.Department)
                    .Include(v => v.Position)
                    .Include(v => v.City)
                    .Include(v => v.Specialist)
                    .ToList();

                return Ok(ApiResponse.Ok(vacancies));
            });
        }

        [HttpPost]
        public IActionResult CreateVacancy([FromHeader] string token, [FromBody] VacancyDTO vacancyDTO)
        {
            return ValidateModelAndExecute(vacancyDTO, token, (db, model) =>
            {
                // Проверка существования отдела
                if (!db.Departments.Any(d => d.Id == model.IdDepartment))
                    return BadRequest(ApiResponse.Error("Указанный отдел не существует"));

                // Проверка существования должности
                if (!db.Positions.Any(p => p.Id == model.IdPosition))
                    return BadRequest(ApiResponse.Error("Указанная должность не существует"));

                // Проверка существования города
                if (!db.Cities.Any(c => c.Id == model.IdCity))
                    return BadRequest(ApiResponse.Error("Указанный город не существует"));

                // Проверка существования специалиста
                if (!db.Employees.Any(e => e.Id == model.IdSpecialist))
                    return BadRequest(ApiResponse.Error("Указанный специалист не существует"));

                // Проверка уникальности названия вакансии
                if (db.Vacancies.Any(v => v.Title == model.Title))
                    return Conflict(ApiResponse.Error("Вакансия с таким названием уже существует"));

                var vacancy = new Vacancy
                {
                    Title = model.Title,
                    IdDepartment = model.IdDepartment,
                    OpeningDate = model.OpeningDate,
                    IdPosition = model.IdPosition,
                    SalaryRange = model.SalaryRange,
                    RequiredQuantity = model.RequiredQuantity,
                    Experience = model.Experience,
                    IdCity = model.IdCity,
                    IsPublic = model.IsPublic,
                    Description = model.Description,
                    TeamOfEmployment = model.TeamOfEmployment,
                    Requirements = model.Requirements,
                    TechnicalQuestions = model.TechnicalQuestions,
                    TestTask = model.TestTask,
                    IdSpecialist = model.IdSpecialist
                };

                db.Vacancies.Add(vacancy);
                db.SaveChanges();

                return Ok(ApiResponse.Ok(vacancy, "Вакансия создана успешно"));
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetVacancyById([FromHeader] string token, int id)
        {
            return ValidateAndExecute(token, db =>
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID вакансии"));

                var vacancy = db.Vacancies
                    .Include(v => v.Department)
                    .Include(v => v.Position)
                    .Include(v => v.City)
                    .Include(v => v.Specialist)
                    .FirstOrDefault(v => v.Id == id);

                if (vacancy == null)
                    return NotFound(ApiResponse.Error("Вакансия не найдена"));

                return Ok(ApiResponse.Ok(vacancy));
            });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateVacancy([FromHeader] string token, int id, [FromBody] VacancyDTO vacancyDTO)
        {
            return ValidateModelAndExecute(vacancyDTO, token, (db, model) =>
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID вакансии"));

                var vacancy = db.Vacancies.FirstOrDefault(v => v.Id == id);
                if (vacancy == null)
                    return NotFound(ApiResponse.Error("Вакансия не найдена"));

                // Проверка существования отдела
                if (!db.Departments.Any(d => d.Id == model.IdDepartment))
                    return BadRequest(ApiResponse.Error("Указанный отдел не существует"));

                // Проверка существования должности
                if (!db.Positions.Any(p => p.Id == model.IdPosition))
                    return BadRequest(ApiResponse.Error("Указанная должность не существует"));

                // Проверка существования города
                if (!db.Cities.Any(c => c.Id == model.IdCity))
                    return BadRequest(ApiResponse.Error("Указанный город не существует"));

                // Проверка существования специалиста
                if (!db.Employees.Any(e => e.Id == model.IdSpecialist))
                    return BadRequest(ApiResponse.Error("Указанный специалист не существует"));

                // Проверка уникальности названия (исключая текущую вакансию)
                if (db.Vacancies.Any(v => v.Title == model.Title && v.Id != id))
                    return Conflict(ApiResponse.Error("Вакансия с таким названием уже существует"));

                vacancy.Title = model.Title;
                vacancy.IdDepartment = model.IdDepartment;
                vacancy.OpeningDate = model.OpeningDate;
                vacancy.IdPosition = model.IdPosition;
                vacancy.SalaryRange = model.SalaryRange;
                vacancy.RequiredQuantity = model.RequiredQuantity;
                vacancy.Experience = model.Experience;
                vacancy.IdCity = model.IdCity;
                vacancy.IsPublic = model.IsPublic;
                vacancy.Description = model.Description;
                vacancy.TeamOfEmployment = model.TeamOfEmployment;
                vacancy.Requirements = model.Requirements;
                vacancy.TechnicalQuestions = model.TechnicalQuestions;
                vacancy.TestTask = model.TestTask;
                vacancy.IdSpecialist = model.IdSpecialist;

                db.SaveChanges();
                return Ok(ApiResponse.Ok(vacancy, "Вакансия обновлена успешно"));
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteVacancy([FromHeader] string token, int id)
        {
            return ValidateAndExecute(token, db =>
            {
                if (id <= 0)
                    return BadRequest(ApiResponse.Error("Некорректный ID вакансии"));

                var vacancy = db.Vacancies.FirstOrDefault(v => v.Id == id);
                if (vacancy == null)
                    return NotFound(ApiResponse.Error("Вакансия не найдена"));

                // Проверка, есть ли кандидаты связанные с этой вакансией
                if (db.Candidates.Any(c => c.IdVacancy == id))
                    return BadRequest(ApiResponse.Error("Невозможно удалить вакансию, так как с ней связаны кандидаты"));

                db.Vacancies.Remove(vacancy);
                db.SaveChanges();
                return Ok(ApiResponse.Ok(null, "Вакансия удалена успешно"));
            });
        }

        [HttpPost("{id}/upload-files")]
        public async Task<IActionResult> UploadVacancyFiles([FromHeader] string token, int id, IFormFile file)
        {
            if (string.IsNullOrEmpty(token))
                return Unauthorized(ApiResponse.Error("Токен не предоставлен"));

            if (id <= 0)
                return BadRequest(ApiResponse.Error("Некорректный ID вакансии"));

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.Error("Файл не предоставлен"));

            // Проверка размера файла (максимум 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(ApiResponse.Error("Размер файла не должен превышать 10MB"));

            // Проверка расширения файла
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse.Error($"Недопустимый формат файла. Разрешены: {string.Join(", ", allowedExtensions)}"));

            try
            {
                using var db = new DatabaseManager();
                var user = db.Users.FirstOrDefault(u => u.Token == token);

                if (user == null)
                    return Unauthorized(ApiResponse.Error("Недействительный токен"));

                if (!db.Vacancies.Any(v => v.Id == id))
                    return NotFound(ApiResponse.Error("Вакансия не найдена"));

                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "URL", "documents");
                Directory.CreateDirectory(uploadPath);

                string fileName = $"{id}_{DateTime.Now:yyyyMMddHHmmss}_{file.FileName}";
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                db.VacancyFiles.Add(new VacancyFile
                {
                    IdVacancy = id,
                    FileName = fileName,
                    FileData = ms.ToArray()
                });
                db.SaveChanges();

                return Ok(ApiResponse.Ok(new { fileName }, "Файл загружен успешно"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Ошибка загрузки файла: {ex.Message}"));
            }
        }
    }
}