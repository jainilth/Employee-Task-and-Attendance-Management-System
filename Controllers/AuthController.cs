using BCrypt.Net;
using Employee_Task_and_Attendance_Management_System.DTOs.Auth;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly EmployeeTaskAttendanceDbContext context;
        private readonly IConfiguration configuration;
        public AuthController(IConfiguration _configuration, EmployeeTaskAttendanceDbContext _context)
        {
            configuration = _configuration;
            context = _context;
        }

        #region Login
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginDto logindto)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == logindto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid Email");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(logindto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid Password");
            }

            var token = GenerateToken(user);
            return Ok(new { token, user = ToMeDto(user) });
        }
        #endregion

        #region self
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = context.Users.FirstOrDefault(item => item.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(ToMeDto(user));
        }
        #endregion

        #region GenerateToken
        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(configuration["Jwt:DurationInMinutes"])),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region ResponseMapping
        private static Me ToMeDto(User user)
        {
            return new Me
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };
        }
        #endregion
    }
}
