using API.Model;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using WebApplication1.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductRepository _productRepository;
        IConfiguration configuration;
        public ProductController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _productRepository = new ProductRepository(new Data.DBConnection(), configuration);
        }

        [HttpGet("GetAllTypeProduct")]
        public async Task<IActionResult> GetAllTypeProduct()
        {
            try
            {
                List<string> list = await _productRepository.GetAllTypeProduct();
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
                        Message = "Can't get list of type product"
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
        [HttpPost("CreateProduct")]
        public IActionResult CreateProduct(Product product, string id)
        {
            try
            {
                int result = _productRepository.CreateProduct(product, id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Create product success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Create product fail"
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

        [HttpGet("pageProductByIdUser")]
        public async Task<ActionResult<Page>> GetProductPageByIdUser(string col, int pageNum, int perPage, string direction, string id)
        {
            try
            {
                Page page = await _productRepository.GetPageProductAsync(col, pageNum, perPage, direction, id);
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
                        Message = "Cann't get products"
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

        [HttpGet("searchProductOfUserByName")]
        public async Task<ActionResult<Page>> SearchProductPageOfUserByName(int pageNum, int perPage, string direction, string key, string id)
        {
            try
            {
                Page page = await _productRepository.SearchProductOfUserByName(pageNum, perPage, direction, key, id);
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
                        Message = "No product match"
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
        [HttpDelete("DeleteProductById")]
        public IActionResult DeleteProduct(string id)
        {
            try
            {
                int result = _productRepository.DeleteProductById(id);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Delete product success"
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

        [HttpGet("GetImgByIdProduct")]
        public async Task<IActionResult> GetShopByUserId(string id)
        {
            try
            {
                List<string> imgs = await _productRepository.GetImgByIdProduct(id);
                if (imgs != null)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Data = imgs
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "You don't have image"
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
        [HttpPut("ChangeProduct")]
        public IActionResult ChangeProduct(Product product)
        {
            try
            {
                int result = _productRepository.ChangeProductById(product);
                if (result == 1)
                {
                    return Ok(new APIResponse
                    {
                        Success = true,
                        Message = "Change product infor success"
                    });
                }
                else
                {
                    return Accepted(new APIResponse
                    {
                        Success = false,
                        Message = "Change product infor fail"
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

        [HttpGet("searchProductOfUserByAddress")]
        public async Task<ActionResult<Page>> SearchProductPageOfUserByAddress(int pageNum, int perPage, string direction, string key, string id)
        {
            try
            {
                Page page = await _productRepository.SearchProductOfUserByAddress(pageNum, perPage, direction, key, id);
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
                        Message = "No product match"
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

        [HttpGet("searchProductOfUserByType")]
        public async Task<ActionResult<Page>> SearchProductPageOfUserByType(int pageNum, int perPage, string direction, string key, string id)
        {
            try
            {
                Page page = await _productRepository.SearchProductOfUserByType(pageNum, perPage, direction, key, id);
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
                        Message = "No product match"
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

        [HttpGet("CountProductOfUser")]
        public async Task<IActionResult> GetCountProductByIdUser(string id)
        {
            try
            {
                int count = await _productRepository.GetCountProductOfUser(id);
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
    }
}
