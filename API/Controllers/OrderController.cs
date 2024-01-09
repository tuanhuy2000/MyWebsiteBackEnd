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
    public class OrderController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        IConfiguration configuration;
        public OrderController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _orderRepository = new OrderRepository(new Data.DBConnection(), configuration);
        }

        [Authorize("User")]
        [HttpPost("CreateTransport")]
        public IActionResult CreateTransport(Transport transport)
        {
            try
            {
                bool result = _orderRepository.CreateTransport(transport);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Create transport success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Create transport fail"
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
        [HttpPost("CreateOrder")]
        public IActionResult CreateOrder(Order order, string uId, string tId, string aId)
        {
            try
            {
                bool result = _orderRepository.CreateOrder(order, uId, tId, aId);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Order success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Order fail"
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
        [HttpPost("AddCouponToOrder")]
        public IActionResult AddCouponToOrder(string oId, string cId)
        {
            try
            {
                bool result = _orderRepository.AddCouponToOrder(oId, cId);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Add Coupon To Order Success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Add Coupon To Order Fail"
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
        [HttpPost("AddProductToOrder")]
        public IActionResult AddProductToOrder(string oId, string pId, int quantity)
        {
            try
            {
                bool result = _orderRepository.AddProductToOrder(oId, pId, quantity);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Add Product To Order Success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Add Product To Order Fail"
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
