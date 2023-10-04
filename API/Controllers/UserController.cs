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

        //[Authorize("Admin")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<User>>> GetAllUser()
        //{
        //    try
        //    {
        //        List<User> users = await _userRepository.GetAllUsersAsync();
        //        return users;
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest();
        //    }
        //}

        [HttpPost("Signin")]
        public IActionResult Signin(User user)
        {
            try
            {
                int result = _userRepository.Signin(user);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Signin success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Signin fail"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
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
                    return Accepted(new APIResponse
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
                    int result = _userRepository.SaveRefreshToken(refreshToken);
                    if (result == 1)
                    {
                        user.AccessToken = _userRepository.GetToken(user);
                    }
                    else
                    {
                        return Accepted(new APIResponse
                        {
                            Success = false,
                            Message = "Save RefreshToken fail"
                        });
                    }
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
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [Authorize("User")]
        [HttpPost("GetRefreshToken")]
        public async Task<IActionResult> GetRefreshToken(string IdUser)
        {
            try
            {
                string token = await _userRepository.GetRefreshToken(IdUser);
                if (token != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = token
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Get RefreshToken fail"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [Authorize("Admin")]
        [HttpGet("pageUser")]
        public async Task<ActionResult<Page>> GetUserPage(int pageNum, int perPage, string direction)
        {
            try
            {
                Page page = await _userRepository.GetPageUsersAsync(pageNum, perPage, direction);
                if (page != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = page
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Cann't get users"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(string refreshToken)
        {
            try
            {
                string token = await _userRepository.RenewToken(refreshToken);
                if (token != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = token
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Renew token fail"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [Authorize("User")]
        [HttpPost("GetUserByRefreshToken")]
        public async Task<IActionResult> GetUserByRefreshToken(string token)
        {
            try
            {
                User user = await _userRepository.GetUserByRefreshToken(token);
                if (user == null)
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Get user by refresh token fail"
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = user
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [Authorize("Admin")]
        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser(string id)
        {
            try
            {
                int result = _userRepository.DeleteUserById(id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Delete success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Delete fail"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [Authorize("User")]
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword(string id, string pass, string newPass)
        {
            try
            {
                int result = _userRepository.ChangePasswordById(id, pass, newPass);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Change password success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Password wrong"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [Authorize("User")]
        [HttpPut("ChangeInfor")]
        public IActionResult ChangeUserInfor(User user)
        {
            try
            {
                int result = _userRepository.ChangeUserInfor(user);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Change user infor success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Change user infor fail"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }
    }
}
