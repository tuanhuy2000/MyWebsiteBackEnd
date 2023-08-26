using API.Model;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        IConfiguration configuration;
        public UserController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _userRepository = new UserRepository(new Data.DBConnection(), configuration);
        }

        [Authorize("Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUser()
        {
            try
            {
                List<User> users = await _userRepository.GetAllUsersAsync();
                return users;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(Login login)
        {
            try
            {
                User user = await _userRepository.Login(login);
                if (user == null)
                {
                    return Ok(new APIResponse
                    {
                        Success = false,
                        Message = "Wrong username or password"
                    });
                }
                else
                {
                    var refreshToken = new RefreshToken
                    {
                        Id = Guid.NewGuid().ToString(),
                        User = user,
                        RefreshTokenn = _userRepository.GenerateRefreshToken(),
                        Expire = DateTime.UtcNow.AddDays(7),
                    };
                    _userRepository.SaveRefreshToken(refreshToken);
                    user.AccessToken = _userRepository.GetToken(user);
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Login Success",
                        Data = user
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Signin")]
        public async Task<IActionResult> Signin(User user)
        {
            try
            {
                _userRepository.Signin(user);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRefreshToken")]
        public async Task<IActionResult> GetRefreshToken(string IdUser)
        {
            try
            {
                string token = _userRepository.GetRefreshToken(IdUser);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(string refreshToken)
        {
            try
            {
                string token = _userRepository.RenewToken(refreshToken);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
