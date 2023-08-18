using API.Model;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        [Authorize]
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
                    return BadRequest("Invalid");
                }
                else
                {
                    user.AccessToken = await _userRepository.GetToken(user);
                    return Ok(user);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
