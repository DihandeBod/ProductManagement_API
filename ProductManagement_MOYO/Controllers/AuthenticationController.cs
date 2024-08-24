using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Octokit;
using ProductManagement_MOYO.Data;
using System.Web;
using ProductManagement_MOYO.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProductManagement_MOYO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<UserAccount>> Login([FromBody] CodeDto codeDto)
        {
            var code = codeDto.Code;
            var client = new HttpClient();
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _configuration["GitHub:ClientID"] },
                { "client_secret", _configuration["GitHub:ClientSecret"] },
                { "code", code },
                { "redirect_uri", _configuration["GitHub:RedirectURL"] }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var values = HttpUtility.ParseQueryString(responseContent);
            var access_token = values["access_token"];

            if (string.IsNullOrEmpty(access_token))
            {
                return BadRequest("Invalid GitHub access token.");
            }

            var gitHubClient = new GitHubClient(new ProductHeaderValue("ProductManagement"));
            gitHubClient.Credentials = new Credentials(access_token);
            var gitHubUser = await gitHubClient.User.Current();

            // Retrieve the user's email addresses
            var emailList = await gitHubClient.User.Email.GetAll();
            var primaryEmail = emailList.FirstOrDefault(email => email.Primary)?.Email;

            // If no primary email is found, fall back to the first email in the list
            var email = primaryEmail ?? emailList.FirstOrDefault()?.Email;

            // Check if the user already exists in the database
            var user = await _context.Users.SingleOrDefaultAsync(u => u.GitHubId == gitHubUser.Id.ToString());

            if (user == null)
            {
                string role = "Product Manager";

                // Create a new user
                user = new UserAccount
                {
                    GitHubId = gitHubUser.Id.ToString(),
                    Username = gitHubUser.Login,
                    Email = email,
                    Name = gitHubUser.Name,
                    OAuthProvider = "GitHub",
                    OAuthId = gitHubUser.Id.ToString(),
                    PasswordHash = null,
                    Role = role
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Password!" , UserId = user.GitHubId});
            }
            else if(user != null && user.PasswordHash == null)
            {
                return Ok(new { Message = "Password!" , UserId = user.GitHubId });
            } else
            {
                // Update existing user details if needed
                user.Username = gitHubUser.Login;
                user.Email = email;
                user.Name = gitHubUser.Name;
                user.OAuthProvider = "GitHub";
                user.OAuthId = gitHubUser.Id.ToString();
            }

            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return Ok(new { AccessToken = token, Name = user.Name, Email = user.Email, Role = user.Role });
        }


        [HttpPost]
        [Route("LoginWithEmail")]
        public async Task<ActionResult<UserAccount>> LoginWithEmail([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { AccessToken = token, Name = user.Name, Email = user.Email, Role = user.Role });
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<UserAccount>> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("User with this email already exists.");
            }

            string role = "Product Manager";

            var user = new UserAccount
            {
                Email = registerDto.Email,
                Username = registerDto.Username,
                PasswordHash = CreatePasswordHash(registerDto.Password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, Username = user.Username });
        }

        [HttpPost]
        [Route("SetPassword")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto setPasswordDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.GitHubId == setPasswordDto.UserId);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            user.PasswordHash = CreatePasswordHash(setPasswordDto.Password);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private string GenerateJwtToken(UserAccount user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public class CodeDto
        {
            public string Code { get; set; }
        }

        public class LoginDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class RegisterDto
        {
            public string Email { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class SetPasswordDto
        {
            public string UserId { get; set; }
            public string Password { get; set; }
        }

    }
}

