using API.Model;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;
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

        [Authorize("Admin")]
        [HttpGet("pageUser")]
        public async Task<ActionResult<Page>> GetUserPage(int pageNum, int perPage, string direction)
        {
            try
            {
                Page page = await _userRepository.GetPageUsersAsync(pageNum, perPage, direction);
                return page;
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
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
        public IActionResult Signin(User user)
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
                string token = await _userRepository.GetRefreshToken(IdUser);
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
                string token = await _userRepository.RenewToken(refreshToken);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize("User")]
        [HttpPost("GetRole")]
        public async Task<IActionResult> GetRole(string id)
        {
            try
            {
                string role = await _userRepository.GetRoleById(id);
                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize("Admin")]
        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser(string id)
        {
            try
            {
                _userRepository.DeleteUserById(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
