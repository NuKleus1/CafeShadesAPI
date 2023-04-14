using AutoMapper;
using Cafeshades.Models;
using Cafeshades.Models.Dtos;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CafeShades.Controllers
{
    // TODO : Implement Add and Update Requests for Image Handling
    // TODO : Patch for Qauntity
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IGenericRepository<Category> categoryRepository, IMapper mapper, IGenericRepository<Product> productRepo, ILogger<ProductController> logger)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _productRepo = productRepo;
            _logger = logger;
        }

        [HttpGet("getMenu")]
        public async Task<IActionResult> GetMenu()
        {
            var categoryWithProducts = await _categoryRepository.ListAllAsync(cat => cat.Products);

            if (categoryWithProducts.IsNullOrEmpty())
                return NotFound(new ErrorResponse("Menu Not found!"));

            var data = _mapper.Map<IReadOnlyList<Category>, IReadOnlyList<CategoryDto>>(categoryWithProducts);

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

            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductDto>(product));
        }

        [HttpGet()]
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

            if (products.IsNullOrEmpty()) return NotFound();

            return Ok(_mapper.Map<IEnumerable<ProductDto>>(products));
        }

        [HttpPost()]
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var record = await _productRepo.GetByIdAsync(id);

                if (record == null) return NotFound();

                _productRepo.Delete(record);
                _productRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured while Deleting Product");
                return BadRequest();
            }
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromBody]ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            try
            {
                var pro = await _productRepo.GetByIdAsync(id);

                if (pro == null) return NotFound();

                _productRepo.Delete(product);
                _productRepo.SaveChanges();
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
