using API.Model;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly ShopRepository _shopRepository;
        IConfiguration configuration;
        public ShopController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _shopRepository = new ShopRepository(new Data.DBConnection(), configuration);
        }

        [HttpGet("GetShop")]
        public async Task<IActionResult> GetShopByUserId(string id)
        {
            try
            {
                Shop shop = await _shopRepository.GetShopByUserId(id);
                if (shop != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = shop
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "You don't have shop"
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
        [HttpPost("CreateShop")]
        public IActionResult CreateShop(Shop shop, string id)
        {
            try
            {
                int result = _shopRepository.CreateShop(shop, id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Create shop success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Create shop fail"
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
        [HttpPut("ChangeShopInfor")]
        public IActionResult ChangeShop(Shop shop, string id)
        {
            try
            {
                int result = _shopRepository.ChangeShopInfor(shop, id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Change shop infor success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Change shop infor fail"
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
        [HttpDelete("DeleteShop")]
        public IActionResult DeleteShop(string id)
        {
            try
            {
                int result = _shopRepository.DeleteShopByIdUser(id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Delete shop success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Delete shop fail"
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

        [HttpGet("GetShopByProduct")]
        public async Task<IActionResult> GetShopByProductId(string id)
        {
            try
            {
                Shop shop = await _shopRepository.GetShopByProductId(id);
                if (shop != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = shop
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "You don't have shop"
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
