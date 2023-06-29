using FormulaOneApp.Configurations;
using FormulaOneApp.Models;
using FormulaOneApp.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FormulaOneApp.Controllers
{

    [Route("api/[controller]")] // api/authentication
    [ApiController]
    //As ControllerBase does not contain view => this controller will return API (Http Status: 200, 400, ... ) instead of views like usual
    public class AuthenticationController : ControllerBase
    {
        // IdentityUser is a default user to interact with identity
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IConfiguration _configuration;
        
        // Pass Jwt config to authenticate the request and ensure the key is encrypted and decrypted in the correct way
        //private readonly JwtConfig _jwtConfig;

        public AuthenticationController(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration
            //JwtConfig jwtConfig
            )
        {
            _userManager = userManager;
            //_jwtConfig = jwtConfig;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            // Validate the incoming request
            if (ModelState.IsValid)
            {
                // We need to check if the email already exist
                var user_exist = await _userManager.FindByEmailAsync(requestDto.Email);

                if (user_exist != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Email already exist"
                        }
                    });
                }

                // create a user
                var new_user = new IdentityUser()
                {
                    Email = requestDto.Email,
                    UserName = requestDto.Email
                };

                var is_created = await _userManager.CreateAsync(new_user, requestDto.Password);

                if (is_created.Succeeded)
                {
                    // Generate the token
                    var token = GenerateJwtToken(new_user);

                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest(new AuthResult()
                {
                    Errors = new List<string>()
                    {
                        "Server Error!"
                    },
                    Result = false,
                });
            }

            return BadRequest();
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequest)
        {
            if (ModelState.IsValid) // both username and password has been provided
            {
                //Check if the user exist
                var existing_user = await _userManager.FindByEmailAsync(loginRequest.Email);

                if (existing_user == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid Payload!"
                        },
                        Result = false,
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existing_user, loginRequest.Password);

                // Case for not correct
                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid Credentials!"
                        },
                        Result = false,
                    });
                }


                // Case for correct => Need to generate token
                var jwtToken = GenerateJwtToken(existing_user);

                return Ok(new AuthResult()
                {
                    Token = jwtToken,
                    Result = true,
                });
            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                {
                    "Invalid Payload!"
                },
                Result = false,
            });
        }

        // Method to create token
        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            //var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            // Token descriptor
            var tokenDiscriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new []
                {    
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),

                Expires = DateTime.Now.AddHours(1), // Each Hour create new token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256) // Verify signature
            };

            var token = jwtTokenHandler.CreateToken(tokenDiscriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken; 
        }
    }
}
