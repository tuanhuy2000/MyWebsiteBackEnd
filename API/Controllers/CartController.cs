using API.Model;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartRepository _cartRepository;
        IConfiguration configuration;
        public CartController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _cartRepository = new CartRepository(new Data.DBConnection(), configuration);
        }

        [Authorize("User")]
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(string uId, string pId, int quantity)
        {
            try
            {
                int result = await _cartRepository.AddToCart(uId, pId, quantity);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Add success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Already in cart"
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
        [HttpPost("GetProductInCart")]
        public async Task<IActionResult> GetProductInCart(string uId)
        {
            try
            {
                List<Product> list = await _cartRepository.GetProductInCart(uId);
                if (list != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = list
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Can't get product"
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
        [HttpPut("ChangeQuantity")]
        public async Task<IActionResult> ChangeQuantity(string uId, string pId, int quantity)
        {
            try
            {
                int result = await _cartRepository.ChangeQuantity(uId, pId, quantity);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Can't change quantity"
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
        [HttpDelete("DeleteProductInCart")]
        public async Task<IActionResult> DeleteProductInCart(string uId, string pId)
        {
            try
            {
                int result = await _cartRepository.DeleteProductInCart(uId, pId);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Delete product fail"
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
