using AutoMapper;
using Cafeshades.Models;
using Cafeshades.Models.Dtos;
using CafeShades.Models.Request;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CafeShades.Controllers
{
    // TODO : Patch for Qauntity
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IGenericRepository<Category> categoryRepository,
            IMapper mapper,
            IGenericRepository<Product> productRepo,
            ILogger<ProductController> logger,
            IWebHostEnvironment env)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _productRepo = productRepo;
            _logger = logger;
            _env = env;
        }

        // TODO : Check Total Mapping
        [HttpGet("getMenu")]
        public async Task<IActionResult> GetMenu()
        {
            var categoryWithProducts = await _categoryRepository.ListAllAsync(cat => cat.Products);

            if (categoryWithProducts.IsNullOrEmpty())
                return NotFound(new { responseStatus = false, responseMessage = "Menu Not found!" });

            var data = _mapper.Map<IReadOnlyList<Category>, IReadOnlyList<CategoryDto>>(categoryWithProducts);

            return Ok(new {responseStatus = true, categoryList = data});
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Product product;
            try
            {
                product = await _productRepo.GetByIdAsync(id, pro => pro.Category);

                if (product == null)
                    return NotFound(new { responseStatus = false, responseMessage = "Product Not Found!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while GetBYID Product");
                return BadRequest(new { responseStatus = false, responseMessage = "Unkown Server Error Occurred" });
            }

            if (product == null) return NotFound(new { responseStatus = false, responseMessage = "Not Found" });

            return Ok(new { responseStatus = true, product = _mapper.Map<ProductDto>(product)});
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Product> products;
            try
            {
                products = await _productRepo.ListAllAsync(pro => pro.Category);

                if (products.IsNullOrEmpty())
                    return NotFound(new { responseStatus = false, responseMessage = "Products Not found!" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while GetAll Product");
                return BadRequest(new { responseStatus = false, responseMessage = "Unkown Server Error Occurred" });
            }

            if (products.IsNullOrEmpty()) return NotFound();

            return Ok(new { responseStatus = true, productList = _mapper.Map<IEnumerable<ProductDto>>(products)});
        }

        [HttpPost()]
        public async Task<IActionResult> AddProduct([FromForm] ProductRequest productRequest)
        {
            Product product = new Product
            {
                Name = productRequest.productName,
                ImageUrl = productRequest.productImage.FileName,
                Price = productRequest.productPrice,
                CategoryId = productRequest.productCategoryId,
            };

            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);

            if (category == null)
                return UnprocessableEntity(new {responseStatus = false, responseMessage = "Category does not Exist !"});

            try
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequest.productImage.FileName);

                string filePath = Path.Combine(_env.WebRootPath, "Product", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    productRequest.productImage.CopyTo(stream);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Product Image");
                return BadRequest(new { responseStatus = false, responseMessage = "Error while saving Image!" });
            }

            try
            {
                _productRepo.Add(product);
                _productRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Adding Product");
                return BadRequest(new { responseStatus = false, responseMessage = "Error while adding Product!" });
            }

            return Ok(new { responseStatus = true, responseMessage = "Product Added Successfully !" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var record = await _productRepo.GetByIdAsync(id);

                if (record == null) return NotFound(new { responseStatus = false, responseMessage = "Product Not Found!" });

                _productRepo.Delete(record);
                _productRepo.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Deleting Product");
                return BadRequest(new { responseStatus = false, responseMessage = "Error while Deleting Product!" });
            }
            return Ok(new { responseStatus = false, responseMessage = "Product Deleted Successfuly!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromBody] ProductRequest productRequest)
        {
            Product oldProduct = await _productRepo.GetByIdAsync(id);

            if (oldProduct == null)
                return NotFound(new { responseStatus = false, responseMessage = "Product Not found!" });

            if (productRequest.productImage == null || productRequest.productImage.Length <= 0)
                return BadRequest(new { responseStatus = false, responseMessage = "No image file was provided." });

           var category = await _categoryRepository.GetByIdAsync(oldProduct.CategoryId);

            if (category == null)
                return UnprocessableEntity(new { responseStatus = false, responseMessage = "Category does not Exist !" });
            
            string oldImagePath = Path.Combine(_env.WebRootPath, "Product", oldProduct.ImageUrl);

            string newImageFileName;
            string newImageFilePath;

            try
            {
                newImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequest.productImage.FileName);

                newImageFilePath = Path.Combine(_env.WebRootPath, "Product", newImageFileName);

                using (var stream = new FileStream(newImageFilePath, FileMode.Create))
                    productRequest.productImage.CopyTo(stream);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Product Image");
                return BadRequest(new { responseStatus = false, responseMessage = "Error while saving Image!" });
            }
            
            oldProduct.Name = productRequest.productName;
            oldProduct.ImageUrl = newImageFileName;
            oldProduct.Price = productRequest.productPrice;
            oldProduct.CategoryId = productRequest.productCategoryId;
            
            try
            {
                //_productRepo.Update(newProduct);
                _productRepo.SaveChanges();

                System.IO.File.Replace(oldImagePath, newImageFilePath, oldProduct.Id + "_" + oldProduct.Name);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred while Adding Product");
                return BadRequest(new { responseStatus = false, responseMessage = "Error while updating Product!" });
            }

            return Ok(new { responseStatus = true, responseMessage = "Product Updated Successfully !" });
        }

        //[HttpPatch("{id}")]
        //public async Task<IActionResult> UpdateProductQuantity(int id, [FromForm] int productQuantity, [FromForm] int productPrice)
        //{
        //    try
        //    {
        //        var product = await _productRepo.GetByIdAsync(id);

        //        if (product == null)
        //            return NotFound(new { responseStatus = false, responseMessage = "Product Not Found!" });

        //        product.Quantity = productQuantity;
        //        product.Price = productPrice;

        //        _productRepo.Update(product);
        //        _productRepo.SaveChanges();

        //        return Ok(new { responseStatus = true, responseMessage = "Product Quantity Updated Successfully !" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error Occurred while Updating Product Quantity");
        //        return BadRequest(new { responseStatus = false, responseMessage = "Error while Updating Product Quantity!" });
        //    }
        //}
    }
}
