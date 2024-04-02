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
        public IActionResult CreateOrder(Order order, string uId, string sId, string aId)
        {
            try
            {
                bool result = _orderRepository.CreateOrder(order, uId, sId, aId);
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

        [Authorize("User")]
        [HttpGet("GetOrderByShopOrUser")]
        public async Task<ActionResult<Page>> GetOrderByShopOrUser(int pageNum, int perPage, string direction, string tId, string type)
        {
            try
            {
                Page page = await _orderRepository.GetOrderByIdShopOrUser(pageNum, perPage, direction, tId, type);
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
                        Message = "Cann't get Orders"
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
        [HttpGet("SearchOrderOfShopOrUser")]
        public async Task<ActionResult<Page>> SearchOrderOfShopOrUser(int pageNum, int perPage, string direction, string key, string tId, string type)
        {
            try
            {
                Page page = await _orderRepository.SearchOrderOfShopOrUser(pageNum, perPage, direction, key, tId, type);
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
                        Message = "Cann't get Orders"
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
        [HttpGet("CountOrderOfShop")]
        public async Task<IActionResult> CountOrderByIdShop(string id)
        {
            try
            {
                int count = await _orderRepository.CountOrderOfShop(id);
                return Ok(new APIResponse
                {
                    Success = true,
                    Data = count
                });

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
        [HttpPost("AddTransportToOrder")]
        public IActionResult AddTransportToOrder(string tId, string oId)
        {
            try
            {
                bool result = _orderRepository.AddTransportToOrder(tId, oId);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Confirm Order Success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Confirm Order Fail"
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
        [HttpGet("GetAddressOfOrderById")]
        public async Task<IActionResult> GetAddressOfOrderById(string oId)
        {
            try
            {
                Address result = await _orderRepository.GetAddressOfOrderById(oId);
                if (result != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = result
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Get Order Address Fail"
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
        [HttpGet("GetTransportOfOrderById")]
        public async Task<IActionResult> GetTransportOfOrderById(string oId)
        {
            try
            {
                Transport result = await _orderRepository.GetTransportOfOrderById(oId);
                if (result != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = result
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = result
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
        [HttpGet("GetProductOfOrderById")]
        public async Task<IActionResult> GetProductOfOrderById(string oId)
        {
            try
            {
                List<Product> list = await _orderRepository.GetProductOfOrderById(oId);
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
                        Message = "Can't get products of order"
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
        [HttpGet("GetShopOfOrderById")]
        public async Task<IActionResult> GetShopOfOrderById(string oId)
        {
            try
            {
                Shop shop = await _orderRepository.GetShopOfOrderById(oId);
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
                        Message = "Can't get shop of order"
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
        [HttpDelete("CancelOrder")]
        public IActionResult DeleteOrder(string oId)
        {
            try
            {
                bool result = _orderRepository.DeleteOrderById(oId);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Cancel order success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Cancel order fail"
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
        [HttpGet("CheckBeforeOrder")]
        public async Task<IActionResult> CheckBeforeOrder(string uId, string pId)
        {
            try
            {
                bool result = await _orderRepository.CheckBeforeOrder(uId, pId);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Can't buy products from my own store"
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
        [HttpPost("FinishOrder")]
        public IActionResult FinishOrder(string oId)
        {
            try
            {
                bool result = _orderRepository.FinishOrder(oId);
                if (result)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Thanks for your order"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Confirm finish order fail"
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
        [HttpGet("SearchOrderOfShopOrUserByStatus")]
        public async Task<ActionResult<Page>> SearchOrderOfShopOrUserByStatus(int pageNum, int perPage, string direction, string status, string tId, string type)
        {
            try
            {
                Page page = await _orderRepository.SearchOrderOfShopOrUserByStatus(pageNum, perPage, direction, status, tId, type);
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
                        Message = "Cann't get Orders"
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
