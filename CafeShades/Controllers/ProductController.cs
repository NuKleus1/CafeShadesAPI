using AutoMapper;
using CafeShades.Models;
using CafeShades.Models.Dtos;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CafeShades.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ICategoryRepository categoryRepository, IMapper mapper, IGenericRepository<Product> productRepo, ILogger<ProductController> logger)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _productRepo = productRepo;
            _logger = logger;
        }

        [HttpGet("getMenu")]
        public async Task<IActionResult> GetMenu()
        {
            var categoryWithProducts = await _categoryRepository.GetAllWithProducts();

            if (categoryWithProducts.IsNullOrEmpty())
                return NotFound(new ErrorResponse("Menu Not found!"));

            var data = _mapper.Map<List<Category>, List<CategoryDto>>(categoryWithProducts);

            return Ok(new MenuDto(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Product product;
            try
            {
                product = await _productRepo.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while GetBYID Product");
                return BadRequest();
            }
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Product> products;
            try
            {
                products = await _productRepo.ListAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while GetAll Product");
                return BadRequest(ex);
            }
            var productDto = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(productDto);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            try
            {
                _productRepo.Add(product);
                _productRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Adding Product");
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProduct([FromBody] ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            try
            {
                _productRepo.Delete(product);
                _productRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Product");
                return BadRequest();
            }
            return Ok();
        }

        [HttpPut("update")]
        public async Task<IActionResult> PutProduct([FromBody]ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            try
            {
                _productRepo.SaveChanges();
                _productRepo.Delete(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Updating Product");
                return BadRequest();
            }
            return Ok();
        }

    }
}
