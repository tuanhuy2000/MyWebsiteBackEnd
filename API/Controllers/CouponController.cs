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
    public class CouponController : ControllerBase
    {
        private readonly CouponRepository _couponRepository;
        IConfiguration configuration;
        public CouponController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _couponRepository = new CouponRepository(new Data.DBConnection(), configuration);
        }

        [HttpGet("GetAllTypeCoupon")]
        public async Task<IActionResult> GetAllTypeCoupon()
        {
            try
            {
                List<string> list = await _couponRepository.GetAllTypeCoupon();
                if (list != null && list.Count > 0)
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
                        Message = "Can't get list of type coupon"
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
        [HttpPost("CreateShopCoupon")]
        public IActionResult CreateShopCoupon(Coupon coupon, string id)
        {
            try
            {
                int result = _couponRepository.CreateShopCoupon(coupon, id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Create coupon success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Create coupon fail"
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
        [HttpGet("pageCouponByIdUser")]
        public async Task<ActionResult<Page>> GetCouponPageByIdUser(int pageNum, int perPage, string direction, string id)
        {
            try
            {
                Page page = await _couponRepository.GetPageCouponAsync(pageNum, perPage, direction, id);
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
                        Message = "Cann't get coupons"
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
        [HttpDelete("DeleteCouponById")]
        public IActionResult DeleteCoupon(string id)
        {
            try
            {
                int result = _couponRepository.DeleteCouponById(id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Delete coupon success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Delete coupon fail"
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
        [HttpPut("ChangeCoupon")]
        public IActionResult ChangeCoupon(Coupon coupon)
        {
            try
            {
                int result = _couponRepository.ChangeCouponById(coupon);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Change coupon infor success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Change coupon infor fail"
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
        [HttpGet("searchCouponOfUserByType")]
        public async Task<ActionResult<Page>> SearchCouponPageOfUserByType(int pageNum, int perPage, string direction, string key, string id)
        {
            try
            {
                Page page = await _couponRepository.SearchCouponOfUserByType(pageNum, perPage, direction, key, id);
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
                        Message = "No coupon match"
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
        [HttpGet("searchCouponOfUserByProductType")]
        public async Task<ActionResult<Page>> SearchCouponPageOfUserByProductType(int pageNum, int perPage, string direction, string key, string id)
        {
            try
            {
                Page page = await _couponRepository.SearchCouponOfUserByProductType(pageNum, perPage, direction, key, id);
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
                        Message = "No coupon match"
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
        [HttpGet("searchCouponOfUserByDate")]
        public async Task<ActionResult<Page>> SearchCouponPageOfUserByDate(int pageNum, int perPage, string direction, DateTime from, DateTime to, string id)
        {
            try
            {
                Page page = await _couponRepository.SearchCouponOfUserByDate(pageNum, perPage, direction, from, to, id);
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
                        Message = "No coupon match"
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
        [HttpPost("CreateAdminCoupon")]
        public IActionResult CreateAdminCoupon(Coupon coupon)
        {
            try
            {
                int result = _couponRepository.CreateAdminCoupon(coupon);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Create coupon success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Create coupon fail"
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
        [HttpGet("pageAdminCoupon")]
        public async Task<ActionResult<Page>> GetAdminCouponPage(int pageNum, int perPage, string direction)
        {
            try
            {
                Page page = await _couponRepository.GetPageAdminCouponAsync(pageNum, perPage, direction);
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
                        Message = "Cann't get coupons"
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
        [HttpGet("searchAdminCouponByType")]
        public async Task<ActionResult<Page>> SearchAdminCouponPageByType(int pageNum, int perPage, string direction, string key)
        {
            try
            {
                Page page = await _couponRepository.SearchAdminCouponByType(pageNum, perPage, direction, key);
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
                        Message = "No coupon match"
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
